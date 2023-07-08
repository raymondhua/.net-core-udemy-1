using Serilog;
using Serilog.Events;

namespace BulkyBook.CloudStorage.Common
{
    public class StaticLogger
    {
        public static void EnsureInitialized()
        {
            if (Log.Logger is not Serilog.Core.Logger)
            {
                Log.Logger = new LoggerConfiguration()
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    //.WriteTo.AzureTableStorage(connectionString, LogEventLevel.Information)
                    .CreateLogger();
            }
        }
    }
}