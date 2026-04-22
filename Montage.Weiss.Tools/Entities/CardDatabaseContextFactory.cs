using Microsoft.EntityFrameworkCore.Design;

namespace Montage.Weiss.Tools.Entities;

public class CardDatabaseContextFactory : IDesignTimeDbContextFactory<CardDatabaseContext>
{
    public CardDatabaseContext CreateDbContext(string[] args)
    {
        //var optionsBuilder = new DbContextOptionsBuilder<CardDatabaseContext>();
        //optionsBuilder.UseSqlite("Data Source=blog.db");
        return new CardDatabaseContext(new AppConfig { DbName = "cards.db" });
    }
}
