using OilGasETL.Models;
using Serilog;

namespace OilGasETL.Services
{
    public class ETLOrchestrator
    {
        private readonly CsvProcessingService _csvService;
        private readonly DatabaseService _databaseService;
        private readonly bool _useDatabaseMode;

        public ETLOrchestrator(CsvProcessingService csvService, DatabaseService databaseService, bool useDatabaseMode = false)
        {
            _csvService = csvService;
            _databaseService = databaseService;
            _useDatabaseMode = useDatabaseMode;
        }

        public async Task<ETLResult> RunETLPipelineAsync()
        {
            var result = new ETLResult();
            var startTime = DateTime.UtcNow;
            
            Log.Information("=== Oil & Gas ETL Pipeline Started ===");
            Log.Information("Mode: {Mode}", _useDatabaseMode ? "Database" : "In-Memory");

            try
            {
                // Step 1: Extract - Process CSV files
                Log.Information("Step 1: Extracting data from CSV files...");
                var extractedData = await _csvService.ProcessCsvFilesAsync();
                result.RecordsExtracted = extractedData.Count;
                
                if (extractedData.Count == 0)
                {
                    Log.Warning("No data extracted. ETL pipeline completed with no processing.");
                    result.Success = true;
                    return result;
                }

                // Step 2: Transform - Validate and calculate derived fields
                Log.Information("Step 2: Transforming and validating data...");
                var validData = TransformData(extractedData);
                result.RecordsTransformed = validData.Count;
                result.RecordsRejected = extractedData.Count - validData.Count;

                // Step 3: Load - Store data (database or in-memory analysis)
                Log.Information("Step 3: Loading data...");
                if (_useDatabaseMode)
                {
                    var connectionTest = await _databaseService.TestConnectionAsync();
                    if (connectionTest)
                    {
                        await _databaseService.InitializeDatabaseAsync();
                        result.RecordsLoaded = await _databaseService.BulkInsertProductionDataAsync(validData);
                        
                        // Generate database summary
                        var dbStats = await _databaseService.GetSummaryStatsAsync();
                        result.SummaryStats = dbStats;
                    }
                    else
                    {
                        Log.Warning("Database connection failed, switching to in-memory analysis");
                        result.RecordsLoaded = validData.Count;
                        result.SummaryStats = GenerateInMemoryStats(validData);
                    }
                }
                else
                {
                    result.RecordsLoaded = validData.Count;
                    result.SummaryStats = GenerateInMemoryStats(validData);
                }

                result.Success = true;
                result.ProcessingTime = DateTime.UtcNow - startTime;
                
                LogETLSummary(result);
                
            }
            catch (Exception ex)
            {
                Log.Error(ex, "ETL Pipeline failed");
                result.Success = false;
                result.ErrorMessage = ex.Message;
                result.ProcessingTime = DateTime.UtcNow - startTime;
            }

            Log.Information("=== Oil & Gas ETL Pipeline Completed ===");
            return result;
        }

        private List<ProductionData> TransformData(List<ProductionData> rawData)
        {
            var validData = new List<ProductionData>();
            var rejectedCount = 0;

            foreach (var record in rawData)
            {
                if (record.IsValid())
                {
                    // Apply business rules and transformations
                    if (record.Status.ToLower() == "inactive") record.Status = "Shutdown";
                    if (record.OilProduction < 0) record.OilProduction = 0;
                    if (record.GasProduction < 0) record.GasProduction = 0;
                    if (record.WaterProduction < 0) record.WaterProduction = 0;
                    
                    validData.Add(record);
                }
                else
                {
                    rejectedCount++;
                    Log.Warning("Rejected invalid record for Well {WellId} on {Date}", 
                               record.WellId, record.ProductionDate);
                }
            }

            Log.Information("Data transformation completed: {Valid} valid, {Rejected} rejected", 
                           validData.Count, rejectedCount);
            
            return validData;
        }

        private Dictionary<string, object> GenerateInMemoryStats(List<ProductionData> data)
        {
            if (data.Count == 0) return new Dictionary<string, object>();

            return new Dictionary<string, object>
            {
                ["TotalRecords"] = data.Count,
                ["UniqueWells"] = data.Select(d => d.WellId).Distinct().Count(),
                ["AvgOilProduction"] = Math.Round(data.Average(d => d.OilProduction), 2),
                ["AvgGasProduction"] = Math.Round(data.Average(d => d.GasProduction), 2),
                ["AvgWaterCut"] = Math.Round(data.Average(d => d.WaterCut), 2),
                ["AvgOilGasRatio"] = Math.Round(data.Where(d => d.OilGasRatio > 0).Average(d => d.OilGasRatio), 2),
                ["LatestDate"] = data.Max(d => d.ProductionDate),
                ["EarliestDate"] = data.Min(d => d.ProductionDate),
                ["TotalOilProduction"] = Math.Round(data.Sum(d => d.OilProduction), 2),
                ["TotalGasProduction"] = Math.Round(data.Sum(d => d.GasProduction), 2),
                ["ActiveWells"] = data.Count(d => d.Status.ToLower() == "active"),
                ["ShutdownWells"] = data.Count(d => d.Status.ToLower() == "shutdown"),
                ["MaintenanceWells"] = data.Count(d => d.Status.ToLower() == "maintenance")
            };
        }

        private void LogETLSummary(ETLResult result)
        {
            Log.Information("=== ETL Pipeline Summary ===");
            Log.Information("Success: {Success}", result.Success);
            Log.Information("Processing Time: {ProcessingTime}", result.ProcessingTime);
            Log.Information("Records Extracted: {Extracted}", result.RecordsExtracted);
            Log.Information("Records Transformed: {Transformed}", result.RecordsTransformed);
            Log.Information("Records Loaded: {Loaded}", result.RecordsLoaded);
            Log.Information("Records Rejected: {Rejected}", result.RecordsRejected);
            
            if (result.SummaryStats.Any())
            {
                Log.Information("=== Production Summary ===");
                foreach (var stat in result.SummaryStats)
                {
                    Log.Information("{Key}: {Value}", stat.Key, stat.Value);
                }
            }
        }
    }

    public class ETLResult
    {
        public bool Success { get; set; }
        public string? ErrorMessage { get; set; }
        public TimeSpan ProcessingTime { get; set; }
        public int RecordsExtracted { get; set; }
        public int RecordsTransformed { get; set; }
        public int RecordsLoaded { get; set; }
        public int RecordsRejected { get; set; }
        public Dictionary<string, object> SummaryStats { get; set; } = new();
    }
} 