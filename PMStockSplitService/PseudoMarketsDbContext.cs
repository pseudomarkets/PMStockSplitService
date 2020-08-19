using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Dynamic;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;


/*
 * Pseudo Markets Database Context
 * Author: Shravan Jambukesan <shravan@shravanj.com>
 * Date: 8/18/20
 */


namespace PMStockSplitService
{
    public class PseudoMarketsDbContext : DbContext
    {
        public DbSet<Positions> Positions { get; set; }
        public DbSet<StockSplits> StockSplits { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Grab the connection string from appsettings.json
            var connectionString = Program.configuration.GetConnectionString("PMDB");

            // Use the SQL Server Entity Framework Core connector
            optionsBuilder.UseSqlServer(connectionString);
        }
    }

    // Data model for the Positions table
    public class Positions
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public int OrderId { get; set; }
        public double Value { get; set; }
        public string Symbol { get; set; }
        public int Quantity { get; set; }
    }

    // Data model for the StockSplits table
    public class StockSplits
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public int Ratio { get; set; }
        public DateTime ExDate { get; set; }
    }
}