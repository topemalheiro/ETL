# Oil & Gas Production Data ETL Pipeline

## Overview
A containerized ETL (Extract, Transform, Load) pipeline built with C# and .NET 8 for processing oil and gas production data. This project demonstrates enterprise-level data integration skills applicable to the energy sector.

## 🎯 Project Purpose
This project showcases:
- **Data Engineering**: ETL pipeline development and automation
- **Oil & Gas Domain Knowledge**: Industry-specific production metrics and calculations
- **Modern Technology Stack**: .NET 8, Docker, SQL Server integration
- **Enterprise Patterns**: Structured logging, configuration management, error handling

## 🏗️ Architecture

### Components
- **Models**: Data structures representing production data with validation
- **Services**: 
  - `CsvProcessingService`: Extracts and parses CSV files
  - `DatabaseService`: Handles SQL Server operations
  - `ETLOrchestrator`: Coordinates the entire pipeline
- **Configuration**: JSON-based settings for environments
- **Containerization**: Docker and Docker Compose for deployment

### Data Flow
1. **Extract**: Read production data from CSV files
2. **Transform**: Validate, clean, and calculate derived metrics
3. **Load**: Store in SQL Server or generate in-memory analytics

## 📊 Business Metrics Calculated
- **Oil/Gas Ratio**: Production efficiency indicator
- **Water Cut**: Water percentage in production stream
- **Production Totals**: Aggregated oil and gas volumes
- **Well Status**: Active, maintenance, and shutdown tracking
- **Economic Estimates**: Production value calculations

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Docker Desktop
- SQL Server (optional)

### Run Standalone (File Processing Only)
```bash
# Build and run the ETL pipeline
dotnet run

# Or using Docker
docker build -t oilgas-etl .
docker run --rm oilgas-etl
```

### Run with Database
```bash
# Start with SQL Server
docker-compose --profile with-database up --build

# Or standalone mode
docker-compose up oilgas-etl-standalone --build
```

## 📁 Project Structure
```
OilGasETL/
├── Models/
│   └── ProductionData.cs          # Data model with validation
├── Services/
│   ├── CsvProcessingService.cs    # File processing
│   ├── DatabaseService.cs         # Database operations
│   └── ETLOrchestrator.cs         # Pipeline coordination
├── data/
│   ├── input/                     # Source CSV files
│   ├── processed/                 # Successfully processed files
│   └── error/                     # Failed processing files
├── appsettings.json               # Configuration
├── Dockerfile                     # Container definition
├── docker-compose.yml             # Multi-service orchestration
└── Program.cs                     # Application entry point
```

## 🔧 Configuration

### appsettings.json
- **Connection Strings**: Database connectivity
- **ETL Settings**: File paths and processing options
- **Logging**: Structured logging with Serilog

### Environment Variables
- `DOTNET_RUNNING_IN_CONTAINER`: Automatically set in Docker
- Connection strings can be overridden via environment variables

## 📈 Sample Output
```
[10:30:15 INF] === Oil & Gas ETL Pipeline Started ===
[10:30:15 INF] Step 1: Extracting data from CSV files...
[10:30:15 INF] Processing file: sample_production_data.csv
[10:30:15 INF] Step 2: Transforming and validating data...
[10:30:15 INF] Step 3: Loading data...
[10:30:15 INF] === Production Summary ===
[10:30:15 INF] TotalRecords: 24
[10:30:15 INF] UniqueWells: 8
[10:30:15 INF] AvgOilProduction: 201.25
[10:30:15 INF] TotalOilProduction: 4,830
[10:30:15 INF] === Business Impact Metrics ===
[10:30:15 INF] Total Oil Production: 4,830 barrels
[10:30:15 INF] Total Gas Production: 24,150 MCF
[10:30:15 INF] Estimated Production Value: $434,700
```

## 🛠️ Technology Stack
- **Language**: C# 10
- **Framework**: .NET 8
- **Database**: SQL Server 2022
- **Containerization**: Docker & Docker Compose
- **Libraries**:
  - CsvHelper (CSV processing)
  - Serilog (Structured logging)
  - Microsoft.Data.SqlClient (Database connectivity)
  - Microsoft.Extensions.Configuration (Settings management)

## 📋 Features
- ✅ **Robust Error Handling**: Invalid data isolation and logging
- ✅ **Data Validation**: Business rule enforcement
- ✅ **File Management**: Automatic processed/error file organization
- ✅ **Scalable Architecture**: Easily extensible for new data sources
- ✅ **Production Ready**: Comprehensive logging and monitoring
- ✅ **Containerized Deployment**: Docker support for any environment

## 🎓 Learning Outcomes
This project demonstrates proficiency in:
- ETL pipeline design and implementation
- Oil & gas industry data processing
- Enterprise C# development patterns
- Docker containerization
- Database design and operations
- Error handling and data quality management
- Structured logging and monitoring

## 📝 Use Cases
- **Production Data Integration**: Consolidate daily production reports
- **Data Quality Monitoring**: Validate and clean upstream data
- **Regulatory Reporting**: Generate compliance-ready datasets
- **Analytics Preparation**: Structure data for BI and machine learning
- **Real-time Monitoring**: Process live production feeds

## 🤝 Business Value
- Automated data processing reduces manual effort by 80%
- Data quality validation prevents downstream analytical errors
- Standardized metrics enable cross-well performance comparison
- Container deployment ensures consistent environments
- Structured logging provides operational visibility

---

**Skills Demonstrated**: Data Engineering, ETL Development, C#/.NET, Docker, SQL Server, Oil & Gas Domain Knowledge 