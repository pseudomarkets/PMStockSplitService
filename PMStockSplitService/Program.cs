using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;

/*
 * PMStockSplitService - Start of Day Stock Split Service Module for Pseudo Markets
 * Author: Shravan Jambukesan <shravan@shravanj.com>
 * Date: 8/18/20
 */

namespace PMStockSplitService
{
    public class Program
    {
        public static IConfigurationRoot configuration;
        static void Main(string[] args)
        {
            // Setup the Serilog logger
            string logFileName = "PMStockSplitService-" + DateTime.Now.ToString("yyyy-MM-dd") + ".log";
            Log.Logger = new LoggerConfiguration()
                .WriteTo.File(logFileName)
                .CreateLogger();
            Log.Information("Starting PMStockSplitService");

            // Perform stock split and get the number of records (positions) affected
            int recordsAffected = PerformSplits().GetAwaiter().GetResult();
            Log.Information("Records affected: " + recordsAffected);
        }

        private static void ConfigureServices(IServiceCollection serviceCollection)
        {
            // Inject Serilog
            serviceCollection.AddSingleton(LoggerFactory.Create(builder =>
            {
                builder.AddSerilog(dispose: true);
            }));

            serviceCollection.AddLogging();

            // Setup our application config
            configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetParent(AppContext.BaseDirectory).FullName)
                .AddJsonFile("appsettings.json", false)
                .Build();

            serviceCollection.AddSingleton<IConfigurationRoot>(configuration);

            serviceCollection.AddTransient<Program>();
        }

        public static async Task<int> PerformSplits()
        {
            int numRecordsAffected = 0;
            ServiceCollection serviceCollection = new ServiceCollection();

            // Configure the config service so we can fetch the connection string from it
            ConfigureServices(serviceCollection);

            try
            {
                await using (var db = new PseudoMarketsDbContext())
                {
                    Log.Information("Connected to database");

                    // Find the splits that are happening today
                    var splits = await db.StockSplits.Where(x => x.ExDate == DateTime.Today).ToListAsync();

                    foreach (StockSplits stockSplit in splits)
                    {
                        Log.Information("Found split for symbol " + stockSplit.Symbol + " for date " + stockSplit.ExDate);
                        int ratio = stockSplit.Ratio;

                        // Find the affected positions by the symbol
                        var affectedPositions = await db.Positions.Where(x => x.Symbol == stockSplit.Symbol).ToListAsync();
                        foreach (Positions position in affectedPositions)
                        {
                            // Update the quantity based on the split ratio
                            position.Quantity = position.Quantity * ratio;
                            db.Entry(position).State = EntityState.Modified;
                            numRecordsAffected++;
                            Log.Information("Updated positions for symbol " + position.Symbol + " with a ratio of " + ratio);
                        }
                    }

                    // Save changes back to DB
                    await db.SaveChangesAsync();
                }

                return numRecordsAffected;
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
                throw;
            }

        }
    }
}
