using Mailist.EmailRelay.Entities;
using Mailist.SpamFilter;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mailist.Tests;

public class SpamFilterServiceTests
{
    private const string ClassificationJson = """
        {"justification":"Looks legitimate","containsDangerousContent":false,"containsMeaningfulContent":true,"contentIsRelevant":true}
        """;

    [Fact]
    public async Task TransientErrorsAreRetriedUntilSuccess()
    {
        Mock<IChatClient> chatClient = new();
        chatClient
            .SetupSequence(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("API unavailable"))
            .ThrowsAsync(new HttpRequestException("API unavailable"))
            .ReturnsAsync(CreateResponse());

        SpamFilterService service = CreateService(chatClient.Object, maxRetryDuration: TimeSpan.FromMinutes(2));

        var result = await service.ClassifyMessage(CreateEmail(), TestContext.Current.CancellationToken);

        Assert.Equal(SpamCategory.Legitimate, result.Category);
        chatClient.Verify(
            c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()),
            Times.Exactly(3));
    }

    [Fact]
    public async Task RetriesStopAfterMaxRetryDuration()
    {
        Mock<IChatClient> chatClient = new();
        chatClient
            .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("API unavailable"));

        SpamFilterService service = CreateService(chatClient.Object, maxRetryDuration: TimeSpan.Zero);

        var result = await service.ClassifyMessage(CreateEmail(), TestContext.Current.CancellationToken);

        Assert.Equal(SpamCategory.ClassificationFailed, result.Category);
        chatClient.Verify(
            c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task NonTransientErrorsAreNotRetried()
    {
        Mock<IChatClient> chatClient = new();
        chatClient
            .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Not transient"));

        SpamFilterService service = CreateService(chatClient.Object, maxRetryDuration: TimeSpan.FromMinutes(2));

        var result = await service.ClassifyMessage(CreateEmail(), TestContext.Current.CancellationToken);

        Assert.Equal(SpamCategory.ClassificationFailed, result.Category);
        chatClient.Verify(
            c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CancellationIsNotRetried()
    {
        using CancellationTokenSource cancellationTokenSource = new();
        Mock<IChatClient> chatClient = new();
        chatClient
            .Setup(c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()))
            .Callback(() => cancellationTokenSource.Cancel())
            .ThrowsAsync(new OperationCanceledException());

        SpamFilterService service = CreateService(chatClient.Object, maxRetryDuration: TimeSpan.FromMinutes(2));

        var result = await service.ClassifyMessage(CreateEmail(), cancellationTokenSource.Token);

        Assert.Equal(SpamCategory.ClassificationFailed, result.Category);
        chatClient.Verify(
            c => c.GetResponseAsync(It.IsAny<IEnumerable<ChatMessage>>(), It.IsAny<ChatOptions>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    private static SpamFilterService CreateService(IChatClient chatClient, TimeSpan maxRetryDuration) =>
        new(
            chatClient,
            new MimeTextExtractionService(),
            Options.Create(new SpamFilterOptions
            {
                ApiKey = "test",
                SystemPrompt = "system prompt",
                MaxInputLength = 5000,
                MaxRetryDuration = maxRetryDuration
            }),
            NullLogger<SpamFilterService>.Instance);

    private static InboxEmail CreateEmail() =>
        new(
            uniqueId: null,
            subject: "Einladung",
            from: "sender@example.com",
            sender: null,
            replyTo: null,
            to: "receiver@example.com",
            receiver: "receiver@example.com",
            header: null,
            body: Encoding.UTF8.GetBytes("Content-Type: text/plain; charset=utf-8\r\n\r\nHello church!"));

    private static ChatResponse CreateResponse() =>
        new(new ChatMessage(ChatRole.Assistant, ClassificationJson));
}
