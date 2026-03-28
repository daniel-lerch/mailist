using Mailist.EmailRelay.Entities;
using Microsoft.Extensions.AI;
using MimeKit;
using Mistral.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Mailist.SpamFilter;

public class SpamFilterService
{
    private const int EmailMaxContextLength = 5000;

    private readonly IChatClient chatClient;
    private readonly MimeTextExtractionService extractionService;

    public SpamFilterService(IChatClient chatClient, MimeTextExtractionService extractionService)
    {
        this.chatClient = chatClient;
        this.extractionService = extractionService;
    }

    public async Task<ClassificationResult> ClassifyMessage(InboxEmail email, CancellationToken cancellationToken)
    {
        if (email.Body == null)
            throw new ArgumentException("Body must not be null for message classification", nameof(email));

        ChatOptions chatOptions = new()
        {
            ModelId = ModelDefinitions.MistralSmall,
            MaxOutputTokens = 500,
        };

        StringBuilder userPrompt = new();
        userPrompt.Append("From: ");
        userPrompt.AppendLine(email.From);
        userPrompt.Append("Reply-To: ");
        userPrompt.AppendLine(email.ReplyTo);
        userPrompt.Append("To: ");
        userPrompt.AppendLine(email.To);
        userPrompt.Append("Subject: ");
        userPrompt.AppendLine(email.Subject);
        userPrompt.AppendLine();
        userPrompt.AppendLine();

        MimeEntity body;
        using (MemoryStream bodyStream = new(email.Body))
            body = MimeEntity.Load(bodyStream);

        if (!extractionService.TryExtractText(body, out string? text))
        {
            return new ClassificationResult { Category = SpamCategory.NoTextContent, Justification = string.Empty };
        }

        if (text.Length > EmailMaxContextLength)
        {
            userPrompt.Append(text.AsSpan()[..EmailMaxContextLength]);
            userPrompt.AppendLine();
            userPrompt.Append(text.Length - EmailMaxContextLength);
            userPrompt.AppendLine(" more characters truncated.");
        }
        else
        {
            userPrompt.AppendLine(text);
        }

        string systemPrompt = """
            ﻿Du bist ein intelligenter Agent, der E-Mails sortiert. Du arbeitest für die Christuskirche Bensheim-Auerbach, eine evangelisch freikirche Baptistengemeinde in Hessen in Deutschland.
            Jede E-Mail musst du in eine der drei Kategorien einsortieren:

            - legitime E-Mails `Legitimate`. Das sind z.B. Anfragen, die die Kirchengemeinde betreffen oder Newsletter von christlichen Organisationen und Verbünden.
            - irrelevante E-Mails `Irrelevant`. Das sind z.B. Verkaufsangebote für Arbeitskleidung oder Angebote für Webseiten-Entwicklung.
            - potenziell gefährliche E-Mails `Dangerous`. Das sind z.B. Phishing E-Mails bei denen Links nicht zur Domain der vorgetäuschten Firma passen oder zu URL Shortenern führen und dadurch nicht erkannt werden können. Dazu gehören aber auch versuchte SQL Injection oder XSS-Angriffe.

            Ordne diese E-Mail in eine der drei oben genannten Kategorien ein und antworte in folgendem JSON Schema:
            ```json
            {
              "justification": "<SHORT JUSTIFICATION IN PLAIN TEXT>",
              "category": "Legitimate | Irrelevant | Dangerous"
            }
            ```
            """;

        List<ChatMessage> messages = [
            new(ChatRole.System, systemPrompt),
            new(ChatRole.User, userPrompt.ToString())
        ];

        var response = await chatClient.GetResponseAsync(messages, chatOptions, cancellationToken);
        string json = response.Text.Replace("```json", "").Replace("```", "");
        JsonSerializerOptions jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
        return JsonSerializer.Deserialize<ClassificationResult>(json, jsonOptions)
            ?? new ClassificationResult { Category = SpamCategory.ClassificationFailed, Justification = "Failed to parse JSON: " + json };
    }

    public class ClassificationResult
    {
        public required SpamCategory Category { get; init; }
        public required string Justification { get; init; }
    }
}
