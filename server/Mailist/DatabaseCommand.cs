using ConsoleAppFramework;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Mailist;

public class DatabaseCommand
{
    private readonly DatabaseContext database;

    public DatabaseCommand(DatabaseContext database)
    {
        this.database = database;
    }

    // Command: mailist database migrate --to <migration>
    public async Task Migrate([HideDefaultValue] string? to = null)
    {
        if (to == null || AskYesNo("Do you really want to apply a specific migration? This will result in significant data loss for many migrations", false))
        {
            IMigrator migrator = database.GetInfrastructure().GetRequiredService<IMigrator>();
            await migrator.MigrateAsync(to);
        }
    }

    // Command: mailist database create [--force]
    public async Task Create([HideDefaultValue] bool force = false)
    {
        if (force) await DeleteDatabase();

        await database.Database.EnsureCreatedAsync();
    }

    // Command: mailist database delete
    public Task Delete() => DeleteDatabase();

    private async Task DeleteDatabase()
    {
        if (AskYesNo("Do you really want to delete the Mailist database?", false))
        {
            bool deleted = await database.Database.EnsureDeletedAsync();
            if (!deleted) Console.Error.WriteLine("Notice: No database found to delete.");
        }
    }

    private static bool AskYesNo(string question, bool defaultAnswer)
    {
        while (true)
        {
            Console.Write(question + (defaultAnswer ? " [Y/n]: " : " [y/N]: "));
            var line = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(line)) return defaultAnswer;
            line = line.Trim().ToLowerInvariant();
            if (line == "y" || line == "yes") return true;
            if (line == "n" || line == "no") return false;
        }
    }
}
