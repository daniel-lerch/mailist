using MimeKit;
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
                return true;
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
                return true;
            }
        }

        text = null;
        return false;
    }

    public string ExtractPlainText(TextPart textPart)
    {
        return textPart.Text;
    }

    public string ExtractHtmlText(TextPart htmlPart)
    {
        // For simplicity, we just return the raw HTML here. In a real implementation, you might want to use an HTML parser
        // to extract the visible text content and ignore tags, scripts, styles, etc.
        return htmlPart.Text;
    }
}
