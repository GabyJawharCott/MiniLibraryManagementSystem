using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace MiniLibraryManagementSystem.Data;

/// <summary>
/// Used by EF Core tools (e.g. dotnet ef migrations add). Uses the same provider as the configured connection string
/// so migrations get the right types (PostgreSQL types for Neon, SQL Server types for LocalDB). Set PostgreSQL in
/// appsettings.Development.json when generating migrations for Neon.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();
        if (basePath.EndsWith("MiniLibraryManagementSystem", StringComparison.OrdinalIgnoreCase) == false)
            basePath = Path.Combine(basePath, "src", "MiniLibraryManagementSystem");

        var config = new ConfigurationBuilder()
            .SetBasePath(basePath)
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .Build();

        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? "Server=(localdb)\\mssqllocaldb;Database=MiniLibraryManagementSystem;Trusted_Connection=True;MultipleActiveResultSets=true";
        var databaseProvider = config.GetValue<string>("DatabaseProvider") ?? "SqlServer";
        var usePostgres = string.Equals(databaseProvider, "PostgreSQL", StringComparison.OrdinalIgnoreCase)
            || connectionString.TrimStart().StartsWith("Host=", StringComparison.OrdinalIgnoreCase);

        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        if (usePostgres)
            optionsBuilder.UseNpgsql(connectionString);
        else
            optionsBuilder.UseSqlServer(connectionString);

        return new ApplicationDbContext(optionsBuilder.Options);
    }
}
