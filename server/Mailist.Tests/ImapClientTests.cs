using Mailist.Utilities;
using MailKit;
using MailKit.Net.Imap;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Mailist.Tests;

public class ImapClientTests
{
    [Fact]
    public async Task IdleFetch_ReturnsExistingMessages()
    {
        var cancel = new CancellationTokenSource();
        var client = new Mock<IImapClient>();
        var folder = new Mock<IMailFolder>();
        var existingMessage = new Mock<IMessageSummary>();
        client.Setup(c => c.Capabilities).Returns(ImapCapabilities.Idle);
        client.Setup(c => c.IdleAsync(It.IsAny<CancellationToken>(), It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken doneToken, CancellationToken cancellationToken) =>
            {
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(doneToken, cancellationToken);
                    await Task.Delay(-1, cts.Token);
                }
                catch (OperationCanceledException) when (doneToken.IsCancellationRequested)
                {
                }
            });
        folder.Setup(f => f.IsOpen).Returns(true);
        folder.Setup(f => f.Count).Returns(1);
        folder.Setup(f => f.FetchAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<IFetchRequest>(), It.IsAny<CancellationToken>()).Result)
            .Returns([existingMessage.Object]);
        existingMessage.Setup(m => m.UniqueId).Returns(new UniqueId(1));

        IAsyncEnumerator<IMessageSummary> enumerator = client.Object
            .IdleFetchAsync(folder.Object, MessageSummaryItems.UniqueId, cancel.Token)
            .GetAsyncEnumerator(CancellationToken.None);

        Assert.True(await enumerator.MoveNextAsync());
        Assert.Same(existingMessage.Object, enumerator.Current);

        cancel.CancelAfter(100);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => enumerator.MoveNextAsync().AsTask());
    }

    [Fact]
    public async Task IdleFetch_ReturnsNewlyArrivedMessages()
    {
        var cancel = new CancellationTokenSource();
        var client = new Mock<IImapClient>();
        var folder = new Mock<IMailFolder>();
        var firstMessage = new Mock<IMessageSummary>();
        var secondMessage = new Mock<IMessageSummary>();
        client.Setup(c => c.Capabilities).Returns(ImapCapabilities.Idle);
        client.Setup(c => c.IdleAsync(It.IsAny<CancellationToken>(), It.IsAny<CancellationToken>()))
            .Returns(async (CancellationToken doneToken, CancellationToken cancellationToken) =>
            {
                try
                {
                    using var cts = CancellationTokenSource.CreateLinkedTokenSource(doneToken, cancellationToken);
                    await Task.Delay(-1, cts.Token);
                }
                catch (OperationCanceledException) when (doneToken.IsCancellationRequested)
                {
                }
            });
        folder.Setup(f => f.IsOpen).Returns(true);
        folder.Setup(f => f.Count).Returns(0);
        firstMessage.Setup(m => m.UniqueId).Returns(new UniqueId(1));
        secondMessage.Setup(m => m.UniqueId).Returns(new UniqueId(2));

        IAsyncEnumerator<IMessageSummary> enumerator = client.Object
            .IdleFetchAsync(folder.Object, MessageSummaryItems.UniqueId, cancel.Token)
            .GetAsyncEnumerator(CancellationToken.None);

        ValueTask<bool> task1 = enumerator.MoveNextAsync();
        await Task.Delay(100, TestContext.Current.CancellationToken);
        folder.Setup(f => f.FetchAsync(0, 0, It.IsAny<IFetchRequest>(), It.IsAny<CancellationToken>()).Result)
            .Returns([firstMessage.Object]);
        folder.Setup(f => f.Count).Returns(1);
        folder.Raise(f => f.CountChanged += null, EventArgs.Empty);

        Assert.True(await task1);
        Assert.Same(firstMessage.Object, enumerator.Current);

        ValueTask<bool> task2 = enumerator.MoveNextAsync();
        await Task.Delay(100, TestContext.Current.CancellationToken);
        folder.Setup(f => f.FetchAsync(1, 1, It.IsAny<IFetchRequest>(), It.IsAny<CancellationToken>()).Result)
            .Returns([secondMessage.Object]);
        folder.Setup(f => f.Count).Returns(2);
        folder.Raise(f => f.CountChanged += null, EventArgs.Empty);

        Assert.True(await task2);
        Assert.Same(secondMessage.Object, enumerator.Current);

        cancel.CancelAfter(100);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => enumerator.MoveNextAsync().AsTask());
    }
}
