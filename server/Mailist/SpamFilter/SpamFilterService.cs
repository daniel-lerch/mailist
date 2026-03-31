using Mailist.EmailRelay.Entities;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
    private readonly IChatClient chatClient;
    private readonly MimeTextExtractionService extractionService;
    private readonly IOptions<SpamFilterOptions> options;
    private readonly ChatOptions chatOptions;
    private readonly JsonSerializerOptions jsonOptions;
    private readonly ILogger<SpamFilterService> logger;

    public SpamFilterService(IChatClient chatClient, MimeTextExtractionService extractionService, IOptions<SpamFilterOptions> options, ILogger<SpamFilterService> logger)
    {
        this.chatClient = chatClient;
        this.extractionService = extractionService;
        this.options = options;
        this.logger = logger;

        chatOptions = new()
        {
            ModelId = ModelDefinitions.MistralSmall,
            MaxOutputTokens = 500,
        };

        jsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    public async Task<ClassificationResult> ClassifyMessage(InboxEmail email, CancellationToken cancellationToken)
    {
        if (email.Body == null)
            throw new ArgumentException("Body must not be null for message classification", nameof(email));

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
            body = MimeEntity.Load(bodyStream, cancellationToken);

        string? text = await extractionService.ExtractText(body);
        if (text == null)
        {
            logger.LogInformation("Email #{Id} from {From} has no text content.", email.Id, email.From);
            return new ClassificationResult { Category = SpamCategory.NoTextContent, Justification = string.Empty };
        }

        int remaining = Math.Max(0, options.Value.MaxInputLength - userPrompt.Length);
        if (text.Length > remaining)
        {
            userPrompt.Append(text.AsSpan()[..remaining]);
            userPrompt.AppendLine();
            userPrompt.Append(text.Length - remaining);
            userPrompt.AppendLine(" more characters truncated.");
        }
        else
        {
            userPrompt.AppendLine(text);
        }

        List<ChatMessage> messages = [
            new(ChatRole.System, options.Value.SystemPrompt),
            new(ChatRole.User, userPrompt.ToString())
        ];

        ChatResponse response;
        try
        {
            response = await chatClient.GetResponseAsync(messages, chatOptions, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Exception while calling chat client for email #{Id} from {From}.", email.Id, email.From);
            return new ClassificationResult { Category = SpamCategory.ClassificationFailed, Justification = "Exception while calling chat client: " + ex.Message };
        }

        string json = response.Text.Replace("```json", "").Replace("```", "");

        try
        {
            ClassificationResult? result = JsonSerializer.Deserialize<ClassificationResult>(json, jsonOptions);
            if (result == null)
            {
                logger.LogError("JSON deserialization returned null for email #{Id} from {From}. Response: {Response}", email.Id, email.From, json);
                return new ClassificationResult { Category = SpamCategory.ClassificationFailed, Justification = "JSON deserialization returned null. Response: " + json };
            }
            else
            {
                logger.LogInformation("Email #{Id} from {From} classified as {Category}.", email.Id, email.From, result.Category);
                return result;
            }
        }
        catch (JsonException ex)
        {
            logger.LogError(ex, "JSON deserialization error for email #{Id} from {From}. Response: {Response}", email.Id, email.From, json);
            return new ClassificationResult { Category = SpamCategory.ClassificationFailed, Justification = "JSON deserialization error: " + ex.Message + ". Response: " + json };
        }
    }

    public class ClassificationResult
    {
        public required SpamCategory Category { get; init; }
        public required string Justification { get; init; }
    }
}
