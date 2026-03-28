using AngleSharp;
using AngleSharp.Html.Dom;
using AngleSharp.Html.Parser;
using MimeKit;
using System;
using System.Diagnostics.CodeAnalysis;

namespace Mailist.SpamFilter;

public class MimeTextExtractionService
{
    public bool TryExtractText(MimeEntity entity, [NotNullWhen(true)] out string? text)
    {
        if (entity is Multipart multipart)
        {
            if (multipart.TryGetValue(MimeKit.Text.TextFormat.Plain, out TextPart? plainPart))
            {
                text = ExtractPlainText(plainPart);
                return true;
            }
            else if (multipart.TryGetValue(MimeKit.Text.TextFormat.Html, out TextPart? htmlPart))
            {
                text = ExtractHtmlText(htmlPart);
                return text != null;
            }
        }

        if (entity is TextPart textPart)
        {
            if (textPart.Format == MimeKit.Text.TextFormat.Plain)
            {
                text = ExtractPlainText(textPart);
                return true;
            }
            else if (textPart.Format == MimeKit.Text.TextFormat.Html)
            {
                text = ExtractHtmlText(textPart);
                return text != null;
            }
        }

        text = null;
        return false;
    }

    public string ExtractPlainText(TextPart textPart)
    {
        return textPart.Text;
    }

    public string? ExtractHtmlText(TextPart htmlPart)
    {
        IBrowsingContext context = BrowsingContext.New();
        IHtmlParser parser = context.GetService<IHtmlParser>()
            ?? throw new ApplicationException("Failed to get HTML parser from AngleSharp context.");

        IHtmlDocument document = parser.ParseDocument(htmlPart.Text);

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
