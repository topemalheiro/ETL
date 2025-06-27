using System.ComponentModel.DataAnnotations;

namespace OilGasETL.Models
{
    public class ProductionData
    {
        [Required]
        public string WellId { get; set; } = string.Empty;
        
        [Required]
        public DateTime ProductionDate { get; set; }
        
        [Range(0, double.MaxValue)]
        public double OilProduction { get; set; } // barrels per day
        
        [Range(0, double.MaxValue)]
        public double GasProduction { get; set; } // thousand cubic feet per day
        
        [Range(0, double.MaxValue)]
        public double WaterProduction { get; set; } // barrels per day
        
        [Range(0, 100)]
        public double WellheadPressure { get; set; } // psi
        
        [Range(-50, 200)]
        public double Temperature { get; set; } // Fahrenheit
        
        public string Status { get; set; } = "Active"; // Active, Maintenance, Shutdown
        
        public string? Comments { get; set; }
        
        // Calculated properties
        public double OilGasRatio => GasProduction > 0 ? OilProduction / (GasProduction / 1000) : 0;
        public double WaterCut => (OilProduction + WaterProduction) > 0 ? WaterProduction / (OilProduction + WaterProduction) * 100 : 0;
        
        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(WellId) &&
                   ProductionDate != default(DateTime) &&
                   OilProduction >= 0 &&
                   GasProduction >= 0 &&
                   WaterProduction >= 0 &&
                   WellheadPressure >= 0 &&
                   Temperature >= -50 && Temperature <= 200 &&
                   !string.IsNullOrWhiteSpace(Status);
        }
    }
} 