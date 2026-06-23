using HorseEntryNotifier.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace HorseEntryNotifier.Tests;

public sealed class DatabaseInitializerTests
{
    [Fact]
    public async Task InitializeAsync_AddsNominatorColumnToExistingDatabase()
    {
        var path = Path.Combine(Path.GetTempPath(), $"horse-entry-{Guid.NewGuid():N}.db");
        try
        {
            await CreateLegacyDatabaseAsync(path);
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite($"Data Source={path}")
                .Options;

            var columns = new List<string>();
            await using (var context = new AppDbContext(options))
            {
                await new DatabaseInitializer(context).InitializeAsync(CancellationToken.None);
            }

            await using (var connection = new SqliteConnection($"Data Source={path}"))
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = "PRAGMA table_info(\"RegisteredHorses\");";
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    columns.Add(reader.GetString(1));
                }
            }

            Assert.Contains("NominatorName", columns);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            File.Delete(path);
        }
    }

    private static async Task CreateLegacyDatabaseAsync(string path)
    {
        await using var connection = new SqliteConnection($"Data Source={path}");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = """
            CREATE TABLE "RegisteredHorses" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_RegisteredHorses" PRIMARY KEY AUTOINCREMENT,
                "HorseName" TEXT NOT NULL,
                "NormalizedHorseName" TEXT NOT NULL,
                "Memo" TEXT NULL,
                "Enabled" INTEGER NOT NULL,
                "CreatedAt" TEXT NOT NULL,
                "UpdatedAt" TEXT NOT NULL
            );
            """;
        await command.ExecuteNonQueryAsync();
    }
}
