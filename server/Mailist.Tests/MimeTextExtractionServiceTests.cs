using Mailist.SpamFilter;
using MimeKit;
using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Mailist.Tests;

public class MimeTextExtractionServiceTests
{
    private readonly MimeTextExtractionService extractionService;

    public MimeTextExtractionServiceTests()
    {
        extractionService = new();
    }

    [Fact]
    public async Task ExtractionShortensHtml()
    {
        using Stream bodyStream = LoadManifestResourceStream("23437.body");
        MimeEntity body = MimeEntity.Load(bodyStream, TestContext.Current.CancellationToken);
        string? text = await extractionService.ExtractText(body);
        Assert.NotNull(text);
        double expectedCompressionRatio = 0.8;
        double actualCompressionRatio = 1.0 - (double)text.Length / bodyStream.Length;
        Assert.True(expectedCompressionRatio <= actualCompressionRatio,
            $"Extracted text is {100 * actualCompressionRatio:0.##}% shorter than source, expected {100 * expectedCompressionRatio:0.##}% or better");
    }

    [Fact]
    public async Task ExtractionTimesOutFor8MiBHtml()
    {
        StringBuilder html = new();
        html.Append("<html><body>");
        while (html.Length < 8 * 1024 * 1024)
        {
            html.Append("<div>");
        }
        html.Append("Test</body></html>");

        MimeEntity body = new TextPart(MimeKit.Text.TextFormat.Html)
        {
            Text = html.ToString()
        };
        Stopwatch stopwatch = Stopwatch.StartNew();
        string? text = await extractionService.ExtractText(body);
        stopwatch.Stop();
        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(15), $"Text extraction took longer than 15 seconds: {stopwatch.Elapsed}");
    }

    private static Stream LoadManifestResourceStream(string resourceFileName)
    {
        string resourceName = $"Mailist.Tests.Resources.{resourceFileName}";

        Stream? stream = typeof(MimeTextExtractionServiceTests).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Manifest resource stream '{resourceName}' was not found.");

        return stream;
    }
}
