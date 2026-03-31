using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using MimeKit;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Mailist.SpamFilter;

public class MimeTextExtractionService
{
    public async ValueTask<string?> ExtractText(MimeEntity entity)
    {
        if (entity is Multipart multipart)
        {
            if (multipart.TryGetValue(MimeKit.Text.TextFormat.Plain, out TextPart? plainPart))
            {
                return ExtractPlainText(plainPart);
            }
            else if (multipart.TryGetValue(MimeKit.Text.TextFormat.Html, out TextPart? htmlPart))
            {
                return await ExtractHtmlText(htmlPart);
            }
        }

        if (entity is TextPart textPart)
        {
            if (textPart.Format == MimeKit.Text.TextFormat.Plain)
            {
                return ExtractPlainText(textPart);
            }
            else if (textPart.Format == MimeKit.Text.TextFormat.Html)
            {
                return await ExtractHtmlText(textPart);
            }
        }

        return null;
    }

    private static string ExtractPlainText(TextPart textPart)
    {
        return textPart.Text;
    }

    private static async ValueTask<string?> ExtractHtmlText(TextPart htmlPart)
    {
        IBrowsingContext context = BrowsingContext.New();
        IHtmlParser parser = context.GetService<IHtmlParser>()
            ?? throw new ApplicationException("Failed to get HTML parser from AngleSharp context.");

        using CancellationTokenSource cts = new();
        cts.CancelAfter(TimeSpan.FromSeconds(10));

        IHtmlDocument document;
        try
        {
            document = await parser.ParseDocumentAsync(htmlPart.Text, cts.Token);
        }
        catch (OperationCanceledException)
        {
            return null;
        }

        foreach (var element in document.QuerySelectorAll("a"))
        {
            // Replace anchor text with href value
            string? href = element.GetAttribute("href");
            if (!string.IsNullOrEmpty(href))
            {
                element.TextContent = href;
            }
        }

        return document.Body?.TextContent;
    }
}
