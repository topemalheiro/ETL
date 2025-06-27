using Microsoft.Data.SqlClient;
using OilGasETL.Models;
using Serilog;

namespace OilGasETL.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        public DatabaseService(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task InitializeDatabaseAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                
                var createTableSql = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ProductionData' AND xtype='U')
                    CREATE TABLE ProductionData (
                        Id INT IDENTITY(1,1) PRIMARY KEY,
                        WellId NVARCHAR(50) NOT NULL,
                        ProductionDate DATE NOT NULL,
                        OilProduction FLOAT NOT NULL,
                        GasProduction FLOAT NOT NULL,
                        WaterProduction FLOAT NOT NULL,
                        WellheadPressure FLOAT NOT NULL,
                        Temperature FLOAT NOT NULL,
                        Status NVARCHAR(20) NOT NULL,
                        Comments NVARCHAR(500),
                        OilGasRatio FLOAT,
                        WaterCut FLOAT,
                        ProcessedDate DATETIME2 DEFAULT GETDATE(),
                        INDEX IX_ProductionData_WellId_Date (WellId, ProductionDate)
                    )";

                using var command = new SqlCommand(createTableSql, connection);
                await command.ExecuteNonQueryAsync();
                
                Log.Information("Database table initialized successfully");
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to initialize database");
                throw;
            }
        }

        public async Task<int> BulkInsertProductionDataAsync(List<ProductionData> data)
        {
            if (data.Count == 0) return 0;

            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var insertSql = @"
                    INSERT INTO ProductionData 
                    (WellId, ProductionDate, OilProduction, GasProduction, WaterProduction, 
                     WellheadPressure, Temperature, Status, Comments, OilGasRatio, WaterCut)
                    VALUES 
                    (@WellId, @ProductionDate, @OilProduction, @GasProduction, @WaterProduction, 
                     @WellheadPressure, @Temperature, @Status, @Comments, @OilGasRatio, @WaterCut)";

                var insertedCount = 0;
                
                foreach (var item in data)
                {
                    using var command = new SqlCommand(insertSql, connection);
                    command.Parameters.AddWithValue("@WellId", item.WellId);
                    command.Parameters.AddWithValue("@ProductionDate", item.ProductionDate);
                    command.Parameters.AddWithValue("@OilProduction", item.OilProduction);
                    command.Parameters.AddWithValue("@GasProduction", item.GasProduction);
                    command.Parameters.AddWithValue("@WaterProduction", item.WaterProduction);
                    command.Parameters.AddWithValue("@WellheadPressure", item.WellheadPressure);
                    command.Parameters.AddWithValue("@Temperature", item.Temperature);
                    command.Parameters.AddWithValue("@Status", item.Status);
                    command.Parameters.AddWithValue("@Comments", (object?)item.Comments ?? DBNull.Value);
                    command.Parameters.AddWithValue("@OilGasRatio", item.OilGasRatio);
                    command.Parameters.AddWithValue("@WaterCut", item.WaterCut);

                    await command.ExecuteNonQueryAsync();
                    insertedCount++;
                }

                Log.Information("Successfully inserted {Count} production records", insertedCount);
                return insertedCount;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to insert production data into database");
                throw;
            }
        }

        public async Task<bool> TestConnectionAsync()
        {
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();
                Log.Information("Database connection test successful");
                return true;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Database connection test failed - will use in-memory processing");
                return false;
            }
        }

        public async Task<Dictionary<string, object>> GetSummaryStatsAsync()
        {
            var stats = new Dictionary<string, object>();
            
            try
            {
                using var connection = new SqlConnection(_connectionString);
                await connection.OpenAsync();

                var sql = @"
                    SELECT 
                        COUNT(*) as TotalRecords,
                        COUNT(DISTINCT WellId) as UniqueWells,
                        AVG(OilProduction) as AvgOilProduction,
                        AVG(GasProduction) as AvgGasProduction,
                        MAX(ProductionDate) as LatestDate,
                        MIN(ProductionDate) as EarliestDate
                    FROM ProductionData";

                using var command = new SqlCommand(sql, connection);
                using var reader = await command.ExecuteReaderAsync();
                
                if (await reader.ReadAsync())
                {
                    stats["TotalRecords"] = reader["TotalRecords"];
                    stats["UniqueWells"] = reader["UniqueWells"];
                    stats["AvgOilProduction"] = Math.Round(Convert.ToDouble(reader["AvgOilProduction"]), 2);
                    stats["AvgGasProduction"] = Math.Round(Convert.ToDouble(reader["AvgGasProduction"]), 2);
                    stats["LatestDate"] = reader["LatestDate"];
                    stats["EarliestDate"] = reader["EarliestDate"];
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to retrieve summary statistics");
            }

            return stats;
        }
    }
} 