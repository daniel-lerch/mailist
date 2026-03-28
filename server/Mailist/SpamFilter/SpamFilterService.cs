using Mailist.EmailRelay.Entities;
using Microsoft.Extensions.AI;
using MimeKit;
using Mistral.SDK;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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

    public async Task ClassifyMessage(InboxEmail email, CancellationToken cancellationToken)
    {
        if (email.Body == null)
            throw new ArgumentException("Body must not be null for message classification", nameof(email));

        ChatOptions chatOptions = new()
        {
            ModelId = ModelDefinitions.MistralSmall,
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
            // TODO: Reject email with no text content
            return;
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

        List<ChatMessage> messages = [
            new(ChatRole.System, ""),
            new(ChatRole.User, userPrompt.ToString())
        ];

        var response = await chatClient.GetResponseAsync(messages, chatOptions, cancellationToken);
    }
}
