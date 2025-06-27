using CsvHelper;
using CsvHelper.Configuration;
using OilGasETL.Models;
using Serilog;
using System.Globalization;

namespace OilGasETL.Services
{
    public class CsvProcessingService
    {
        private readonly string _inputPath;
        private readonly string _processedPath;
        private readonly string _errorPath;

        public CsvProcessingService(string inputPath, string processedPath, string errorPath)
        {
            _inputPath = inputPath;
            _processedPath = processedPath;
            _errorPath = errorPath;
            
            // Ensure directories exist
            Directory.CreateDirectory(_inputPath);
            Directory.CreateDirectory(_processedPath);
            Directory.CreateDirectory(_errorPath);
        }

        public async Task<List<ProductionData>> ProcessCsvFilesAsync()
        {
            var allData = new List<ProductionData>();
            var csvFiles = Directory.GetFiles(_inputPath, "*.csv");

            if (csvFiles.Length == 0)
            {
                Log.Warning("No CSV files found in input directory: {InputPath}", _inputPath);
                return allData;
            }

            foreach (var csvFile in csvFiles)
            {
                try
                {
                    Log.Information("Processing file: {FileName}", Path.GetFileName(csvFile));
                    var data = await ReadCsvFileAsync(csvFile);
                    allData.AddRange(data);
                    
                    // Move processed file
                    var processedFile = Path.Combine(_processedPath, Path.GetFileName(csvFile));
                    File.Move(csvFile, processedFile);
                    Log.Information("Moved {FileName} to processed folder", Path.GetFileName(csvFile));
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error processing file: {FileName}", Path.GetFileName(csvFile));
                    
                    // Move error file
                    var errorFile = Path.Combine(_errorPath, Path.GetFileName(csvFile));
                    File.Move(csvFile, errorFile);
                }
            }

            return allData;
        }

        private async Task<List<ProductionData>> ReadCsvFileAsync(string filePath)
        {
            var data = new List<ProductionData>();
            
            using var reader = new StreamReader(filePath);
            using var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HeaderValidated = null,
                MissingFieldFound = null,
                BadDataFound = null
            });

            var records = csv.GetRecordsAsync<ProductionDataCsv>();
            
            await foreach (var record in records)
            {
                try
                {
                    var productionData = new ProductionData
                    {
                        WellId = record.WellId?.Trim() ?? string.Empty,
                        ProductionDate = DateTime.Parse(record.ProductionDate ?? string.Empty),
                        OilProduction = double.Parse(record.OilProduction ?? "0"),
                        GasProduction = double.Parse(record.GasProduction ?? "0"),
                        WaterProduction = double.Parse(record.WaterProduction ?? "0"),
                        WellheadPressure = double.Parse(record.WellheadPressure ?? "0"),
                        Temperature = double.Parse(record.Temperature ?? "0"),
                        Status = record.Status?.Trim() ?? "Active",
                        Comments = record.Comments?.Trim()
                    };

                    if (productionData.IsValid())
                    {
                        data.Add(productionData);
                    }
                    else
                    {
                        Log.Warning("Invalid data row for Well {WellId} on {Date}", productionData.WellId, productionData.ProductionDate);
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "Error parsing CSV row: {Record}", record);
                }
            }

            return data;
        }
    }

    // Helper class for CSV mapping
    public class ProductionDataCsv
    {
        public string? WellId { get; set; }
        public string? ProductionDate { get; set; }
        public string? OilProduction { get; set; }
        public string? GasProduction { get; set; }
        public string? WaterProduction { get; set; }
        public string? WellheadPressure { get; set; }
        public string? Temperature { get; set; }
        public string? Status { get; set; }
        public string? Comments { get; set; }
    }
} 