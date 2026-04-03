using MailKit;
using MailKit.Net.Imap;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Mailist.Utilities;

public static class ImapClientExtensions
{
    public static async IAsyncEnumerable<IMessageSummary> IdleFetchAsync(this IImapClient client, IMailFolder folder, MessageSummaryItems items, [EnumeratorCancellation] CancellationToken cancellationToken)
    {
        const int FetchBatchSize = 32;

        if (!folder.IsOpen) throw new InvalidOperationException("The IMAP folder must be open to use IdleFetchAsync.");

        // Initial fetch
        for (int min = 0; min < folder.Count; min += FetchBatchSize)
        {
            int max = Math.Min(min + FetchBatchSize - 1, folder.Count - 1);

            IList<IMessageSummary> buffer = await folder.FetchAsync(min, max, items, cancellationToken);
            foreach (IMessageSummary item in buffer)
            {
                yield return item;
            }
        }

        CancellationTokenSource? done = null;
        int messageCount = folder.Count;
        bool messagesArrived = false;

        folder.CountChanged += onCountChanged;
        folder.MessageExpunged += onMessageExpunged;

        try
        {
            while (true)
            {
                await waitForMessages();

                for (int min = messageCount; min < folder.Count; min += FetchBatchSize)
                {
                    int max = Math.Min(min + FetchBatchSize - 1, folder.Count - 1);

                    IList<IMessageSummary> buffer = await folder.FetchAsync(min, max, items, cancellationToken);
                    foreach (IMessageSummary item in buffer)
                    {
                        yield return item;
                    }
                }

                messageCount = folder.Count;
            }
        }
        finally
        {
            folder.MessageExpunged -= onMessageExpunged;
            folder.CountChanged -= onCountChanged;
            done?.Dispose();
        }

        async Task waitForMessages()
        {
            while (!messagesArrived)
            {
                if (client.Capabilities.HasFlag(ImapCapabilities.Idle))
                {
                    // Note: IMAP servers are only supposed to drop the connection after 30 minutes, so normally
                    // we'd IDLE for a max of, say, ~29 minutes... but GMail seems to drop idle connections after
                    // about 10 minutes, so we'll only idle for 9 minutes.
                    done = new(TimeSpan.FromMinutes(9));
                    try
                    {
                        await client.IdleAsync(done.Token, cancellationToken);
                    }
                    finally
                    {
                        done.Dispose();
                        done = null;
                    }
                }
                else
                {
                    // Note: we don't want to spam the IMAP server with NOOP commands, so lets wait a minute
                    // between each NOOP command.
                    await Task.Delay(new TimeSpan(0, 1, 0), cancellationToken);
                    await client.NoOpAsync(cancellationToken);
                }
            }
        }

        void onCountChanged(object? sender, EventArgs e)
        {
            var folder = (IMailFolder)sender!;

            int arrived = folder.Count - messageCount;
            if (arrived > 0)
            {
                messagesArrived = true;
                done?.Cancel();
            }
        }

        void onMessageExpunged(object? sender, MessageEventArgs e)
        {
            messageCount--;
        }
    }
}
