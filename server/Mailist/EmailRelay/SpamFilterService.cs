using Mailist.EmailRelay.Entities;
using Microsoft.Extensions.AI;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Mailist.EmailRelay;

public class SpamFilterService
{
    private readonly IChatClient chatClient;

    public async Task ClassifyMessage(InboxEmail email, CancellationToken cancellationToken)
    {
        ChatOptions chatOptions = new()
        {

        };

        List<ChatMessage> messages = [
            new(ChatRole.System, ""),
            new(ChatRole.User, "")
        ];

        var response = await chatClient.GetResponseAsync(messages, chatOptions, cancellationToken);
    }
}
