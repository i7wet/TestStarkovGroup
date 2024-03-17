using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace TestStarkovGroup.Database;

public class StarkovDbContextFactory : IDesignTimeDbContextFactory<StarkovDbContext>
{
    public StarkovDbContext CreateDbContext(string[] args)
    {
        var fileSettingsName = "appsettings.json";
        if(Path.Exists(Directory.GetCurrentDirectory() + "\\appsettings.Development.json"))
            fileSettingsName = "appsettings.Development.json";
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(fileSettingsName, optional: false);

        IConfiguration config = builder.Build();

        var connectionString = config.GetConnectionString("Default");
        var optionsBuilder = new DbContextOptionsBuilder<StarkovDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new StarkovDbContext(optionsBuilder.Options);
    }
}