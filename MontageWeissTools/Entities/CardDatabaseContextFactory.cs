using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace Montage.Weiss.Tools.Entities
{
    public class CardDatabaseContextFactory : IDesignTimeDbContextFactory<CardDatabaseContext>
    {
        public CardDatabaseContext CreateDbContext(string[] args)
        {
            //var optionsBuilder = new DbContextOptionsBuilder<CardDatabaseContext>();
            //optionsBuilder.UseSqlite("Data Source=blog.db");
            return new CardDatabaseContext(new AppConfig { DbName = "cards.db" });
        }
    }
}
