using Microsoft.Extensions.Configuration;
using OilGasETL.Services;
using Serilog;

namespace OilGasETL
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Configure Serilog
            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            try
            {
                Log.Information("=== Oil & Gas Production Data ETL Pipeline ===");
                Log.Information("Starting ETL application...");

                // Get configuration settings
                var inputPath = configuration["ETLSettings:InputPath"] ?? "./data/input";
                var processedPath = configuration["ETLSettings:ProcessedPath"] ?? "./data/processed";
                var errorPath = configuration["ETLSettings:ErrorPath"] ?? "./data/error";
                var useDatabaseMode = bool.Parse(configuration["ETLSettings:UseDatabaseMode"] ?? "false");

                // Determine connection string based on environment
                var connectionString = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true"
                    ? configuration.GetConnectionString("DockerConnection")
                    : configuration.GetConnectionString("DefaultConnection");

                if (string.IsNullOrEmpty(connectionString))
                {
                    Log.Warning("No connection string found, running in file-only mode");
                    useDatabaseMode = false;
                }

                Log.Information("Configuration loaded:");
                Log.Information("- Input Path: {InputPath}", inputPath);
                Log.Information("- Processed Path: {ProcessedPath}", processedPath);
                Log.Information("- Error Path: {ErrorPath}", errorPath);
                Log.Information("- Database Mode: {DatabaseMode}", useDatabaseMode);

                // Initialize services
                var csvService = new CsvProcessingService(inputPath, processedPath, errorPath);
                var databaseService = new DatabaseService(connectionString ?? string.Empty);
                var etlOrchestrator = new ETLOrchestrator(csvService, databaseService, useDatabaseMode);

                // Run ETL Pipeline
                var result = await etlOrchestrator.RunETLPipelineAsync();

                if (result.Success)
                {
                    Log.Information("ETL Pipeline completed successfully!");
                    LogBusinessMetrics(result);
                    return 0; // Success exit code
                }
                else
                {
                    Log.Error("ETL Pipeline failed: {ErrorMessage}", result.ErrorMessage);
                    return 1; // Error exit code
                }
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application terminated unexpectedly");
                return 1; // Error exit code
            }
            finally
            {
                Log.Information("ETL Application shutting down...");
                await Log.CloseAndFlushAsync();
            }
        }

        private static void LogBusinessMetrics(Services.ETLResult result)
        {
            Log.Information("=== Business Impact Metrics ===");
            
            if (result.SummaryStats.ContainsKey("TotalOilProduction"))
            {
                var totalOil = Convert.ToDouble(result.SummaryStats["TotalOilProduction"]);
                var totalGas = Convert.ToDouble(result.SummaryStats["TotalGasProduction"]);
                var avgOilGasRatio = result.SummaryStats.ContainsKey("AvgOilGasRatio") 
                    ? Convert.ToDouble(result.SummaryStats["AvgOilGasRatio"]) : 0;

                Log.Information("Total Oil Production: {Oil:N0} barrels", totalOil);
                Log.Information("Total Gas Production: {Gas:N0} MCF", totalGas);
                Log.Information("Average Oil/Gas Ratio: {Ratio:N2}", avgOilGasRatio);

                // Estimate monetary value (rough industry averages)
                var estimatedOilValue = totalOil * 75; // $75/barrel estimate
                var estimatedGasValue = totalGas * 3;  // $3/MCF estimate
                
                Log.Information("Estimated Production Value:");
                Log.Information("- Oil Value: ${Value:N0}", estimatedOilValue);
                Log.Information("- Gas Value: ${Value:N0}", estimatedGasValue);
                Log.Information("- Total Estimated Value: ${Value:N0}", estimatedOilValue + estimatedGasValue);
            }

            if (result.SummaryStats.ContainsKey("ActiveWells"))
            {
                Log.Information("Well Status Summary:");
                Log.Information("- Active Wells: {Count}", result.SummaryStats["ActiveWells"]);
                if (result.SummaryStats.ContainsKey("ShutdownWells"))
                    Log.Information("- Shutdown Wells: {Count}", result.SummaryStats["ShutdownWells"]);
                if (result.SummaryStats.ContainsKey("MaintenanceWells"))
                    Log.Information("- Maintenance Wells: {Count}", result.SummaryStats["MaintenanceWells"]);
            }

            Log.Information("Data Quality Metrics:");
            Log.Information("- Success Rate: {Rate:P2}", 
                result.RecordsTransformed / (double)Math.Max(result.RecordsExtracted, 1));
            Log.Information("- Processing Speed: {Speed:N0} records/second", 
                result.RecordsExtracted / Math.Max(result.ProcessingTime.TotalSeconds, 1));
        }
    }
}
