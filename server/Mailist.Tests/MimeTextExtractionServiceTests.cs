using Mailist.SpamFilter;
using MimeKit;
using System.IO;
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
    public void ExtractionShortensHtml()
    {
        using Stream bodyStream = LoadManifestResourceStream("23437.body");
        MimeEntity body = MimeEntity.Load(bodyStream, TestContext.Current.CancellationToken);
        Assert.True(extractionService.TryExtractText(body, out string? text));
        double expectedCompressionRatio = 0.8;
        double actualCompressionRatio = 1.0 - (double)text.Length / bodyStream.Length;
        Assert.True(expectedCompressionRatio <= actualCompressionRatio,
            $"Extracted text is {100 * actualCompressionRatio:0.##}% shorter than source, expected {100 * expectedCompressionRatio:0.##}% or better");
    }

    private static Stream LoadManifestResourceStream(string resourceFileName)
    {
        string resourceName = $"Mailist.Tests.Resources.{resourceFileName}";

        Stream? stream = typeof(SpamFilterServiceTests).Assembly.GetManifestResourceStream(resourceName)
            ?? throw new FileNotFoundException($"Manifest resource stream '{resourceName}' was not found.");

        return stream;
    }
}
