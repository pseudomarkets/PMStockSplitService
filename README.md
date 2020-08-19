# PMStockSplitService
A .NET Core application designed to be run using Task Scheduler or cron that performs a stock split check at SOD and updates positions according to the split ratio for a particular security

# Requirements
* .NET Core 3.1
* Pseudo Markets instance with the latest database updates

# Usage
The PMStockSplitService is meant to be run as an SOD proccess that can be scheduled through Task Scheduler on Windows or cron on Linux. You'll need to configure the DB connection string in appsettings.config to point it to your Pseudo Markets database. Once setup, stock split data must be manually inserted into the StockSplits table until I can figure out a source to grab this data with an API. At SOD when the proccess runs, the service will check if the current date matches the execution date for any splits, then performs the split for any position that has the symbol specified in the split data against the split ratio. For example, the AAPL 4 to 1 split on 8/24/2020 will multiply the shares owned quantity by 4 for any accounts that hold that stock as a current position. 
