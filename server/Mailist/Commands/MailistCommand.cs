using McMaster.Extensions.CommandLineUtils;

#pragma warning disable IDE0051 // Remove unused private members

namespace Mailist.Commands;

[Command("mailist", Description = "Mailist server management console")]
[Subcommand(typeof(DatabaseCommand))]
public class MailistCommand
{
    private int OnExecute(CommandLineApplication app)
    {
        app.ShowHint();
        return 1;
    }
}
