# Test Automation Framework

A comprehensive C# test automation framework demonstrating modern automation engineering practices including Selenium WebDriver UI testing, REST API testing, cross-browser compatibility, containerization, and CI/CD integration.

## 🚀 Features

### Core Capabilities
- **Cross-Browser Testing**: Chrome, Firefox, Edge support with automatic driver management
- **API Testing**: RESTful API testing with authentication, validation, and error handling
- **Page Object Model**: Scalable UI test architecture with reusable page components
- **Parallel Execution**: Concurrent test execution for faster feedback
- **CI/CD Integration**: GitHub Actions pipeline with comprehensive reporting
- **Docker Support**: Containerized test execution for consistent environments
- **Structured Logging**: Detailed test execution logs with Serilog
- **Screenshot Capture**: Automatic screenshots on test failures
- **Test Reporting**: Multiple output formats (TRX, JSON, HTML)

### Technology Stack
- **.NET 8**: Modern C# development platform
- **NUnit**: Testing framework with rich assertion library
- **Selenium WebDriver**: Browser automation and UI testing
- **HttpClient**: REST API testing and validation
- **Docker**: Containerized test execution
- **GitHub Actions**: CI/CD pipeline automation
- **Serilog**: Structured logging and diagnostics

## 📁 Project Structure

```
TestAutomationFramework/
├── TestFramework/                 # Core framework components
│   ├── DriverFactory.cs          # WebDriver factory with cross-browser support
│   ├── BasePage.cs               # Base page object model class
│   └── ApiClient.cs              # REST API client with authentication
├── UI.AutomationTests/           # User interface test suite
│   ├── Pages/                    # Page object model implementations
│   │   ├── GoogleHomePage.cs     # Google home page interactions
│   │   └── GoogleSearchResultsPage.cs # Search results page
│   └── Tests/                    # UI test implementations
│       └── GoogleSearchTests.cs  # Search functionality tests
├── API.AutomationTests/          # API test suite
│   └── Tests/
│       └── JsonPlaceholderApiTests.cs # REST API CRUD tests
├── .github/workflows/            # CI/CD pipeline definitions
│   └── test-automation.yml      # GitHub Actions workflow
├── Dockerfile                    # Container configuration
├── appsettings.json             # Framework configuration
└── README.md                    # This documentation
```

## 🛠️ Setup Instructions

### Prerequisites
- .NET 8 SDK
- Chrome/Firefox/Edge browsers
- Docker (optional for containerized execution)
- Git

### Manual Setup (Step-by-Step)

1. **Create Solution Structure**
   ```powershell
   # Create main directory
   mkdir TestAutomationFramework
   cd TestAutomationFramework
   
   # Create solution
   dotnet new sln -n "TestAutomationFramework"
   
   # Create projects
   dotnet new classlib -n "TestFramework" -f net8.0
   dotnet new nunit -n "UI.AutomationTests" -f net8.0
   dotnet new nunit -n "API.AutomationTests" -f net8.0
   
   # Add projects to solution
   dotnet sln add TestFramework/TestFramework.csproj
   dotnet sln add UI.AutomationTests/UI.AutomationTests.csproj
   dotnet sln add API.AutomationTests/API.AutomationTests.csproj
   ```

2. **Install NuGet Packages**
   ```powershell
   # Framework packages
   cd TestFramework
   dotnet add package Selenium.WebDriver
   dotnet add package Selenium.WebDriver.ChromeDriver
   dotnet add package Selenium.Support
   dotnet add package DotNetSeleniumExtras.WaitHelpers
   dotnet add package Microsoft.Extensions.Configuration.Json
   dotnet add package Serilog
   dotnet add package Newtonsoft.Json
   
   # Add project references
   cd ../UI.AutomationTests
   dotnet add reference ../TestFramework/TestFramework.csproj
   
   cd ../API.AutomationTests
   dotnet add reference ../TestFramework/TestFramework.csproj
   ```

3. **Build Solution**
   ```powershell
   cd ..
   dotnet build --configuration Release
   ```

## 🎯 Running Tests

### Local Execution

#### Run All Tests
```powershell
dotnet test --configuration Release
```

#### Run Specific Test Suites
```powershell
# API tests only
dotnet test API.AutomationTests/API.AutomationTests.csproj

# UI tests only
dotnet test UI.AutomationTests/UI.AutomationTests.csproj
```

#### Cross-Browser Testing
```powershell
# Chrome (default)
$env:BROWSER="Chrome"
dotnet test UI.AutomationTests/UI.AutomationTests.csproj

# Firefox
$env:BROWSER="Firefox"
dotnet test UI.AutomationTests/UI.AutomationTests.csproj

# Edge
$env:BROWSER="Edge"
dotnet test UI.AutomationTests/UI.AutomationTests.csproj
```

#### Test Categories
```powershell
# Smoke tests only
dotnet test --filter "Category=Smoke"

# Performance tests
dotnet test --filter "Category=Performance"

# Cross-browser tests
dotnet test --filter "Category=Cross-Browser"
```

### Docker Execution

#### Build and Run Container
```powershell
# Build Docker image
docker build -t test-automation-framework:latest .

# Run tests in container
docker run --rm test-automation-framework:latest

# Run with volume mounts for results
docker run --rm \
  -v ${PWD}/TestResults:/app/TestResults \
  -v ${PWD}/logs:/app/logs \
  -v ${PWD}/Screenshots:/app/Screenshots \
  test-automation-framework:latest
```

## 📊 Test Reporting

### Generated Artifacts
- **TRX Files**: Visual Studio test result format
- **Coverage Reports**: Code coverage analysis
- **Screenshots**: Failure evidence for UI tests
- **Logs**: Detailed execution traces
- **HTML Reports**: Human-readable test summaries

### Viewing Results
```powershell
# View test results
Start-Process ./TestResults/test-results.trx

# Open log files
Get-Content ./logs/test-log-*.txt | Out-GridView

# Browse screenshots
Invoke-Item ./Screenshots/
```

## 🔧 Configuration

### Browser Configuration
Modify `appsettings.json` to customize browser behavior:
```json
{
  "BrowserSettings": {
    "Chrome": {
      "Arguments": [
        "--no-sandbox",
        "--disable-dev-shm-usage",
        "--window-size=1920,1080"
      ]
    }
  }
}
```

### Test Environment Settings
Configure different environments for test execution:
```json
{
  "Environment": {
    "CurrentEnvironment": "Development",
    "Environments": {
      "Development": {
        "BaseUrl": "https://dev.example.com"
      },
      "Staging": {
        "BaseUrl": "https://staging.example.com"
      }
    }
  }
}
```

## 🔄 CI/CD Pipeline

### GitHub Actions Features
- **Multi-Browser Testing**: Parallel execution across Chrome and Firefox
- **API Testing**: Comprehensive REST API validation
- **Container Testing**: Docker-based test execution
- **Artifact Collection**: Test results, logs, and screenshots
- **Scheduled Runs**: Daily regression testing
- **Performance Monitoring**: Baseline performance validation

### Pipeline Triggers
- Push to main/develop branches
- Pull requests to main
- Daily scheduled runs (6 AM UTC)
- Manual execution with `[perf]` commit message

### Viewing Pipeline Results
1. Navigate to GitHub repository
2. Click "Actions" tab
3. Select latest workflow run
4. Download artifacts for detailed results

## 📈 Test Categories

### Smoke Tests
Quick validation of core functionality
```csharp
[Test]
[Category("Smoke")]
public void GoogleHomePage_ShouldLoadSuccessfully()
```

### Functional Tests
Comprehensive feature testing
```csharp
[Test]
[Category("Functional")]
[TestCase("Selenium WebDriver")]
public void GoogleSearch_ShouldReturnRelevantResults(string searchTerm)
```

### Performance Tests
Response time and load validation
```csharp
[Test]
[Category("Performance")]
public void GoogleSearch_ShouldCompleteWithinTimeLimit()
```

### Cross-Browser Tests
Multi-browser compatibility validation
```csharp
[Test]
[Category("Cross-Browser")]
public void GoogleSearch_ShouldWorkAcrossBrowsers()
```

## 🐛 Troubleshooting

### Common Issues

#### WebDriver Issues
```powershell
# Update Chrome driver
dotnet add package Selenium.WebDriver.ChromeDriver --prerelease

# Check Chrome version compatibility
chrome --version
```

#### Network Issues
```powershell
# Test API connectivity
curl https://jsonplaceholder.typicode.com/posts/1

# Check proxy settings
$env:HTTP_PROXY=""
$env:HTTPS_PROXY=""
```

#### Permission Issues
```powershell
# Set execution policy for scripts
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

## 📚 Best Practices

### Page Object Model
- Encapsulate page interactions in dedicated classes
- Use meaningful element locators
- Implement wait strategies for dynamic content

### API Testing
- Validate both positive and negative scenarios
- Test error handling and edge cases
- Verify response schemas and data integrity

### Test Design
- Keep tests atomic and independent
- Use descriptive test names and categories
- Implement proper setup and teardown

### CI/CD Integration
- Run tests in multiple environments
- Collect comprehensive artifacts
- Set up notifications for failures

## 🤝 Contributing

1. Fork the repository
2. Create feature branch: `git checkout -b feature/new-test-suite`
3. Commit changes: `git commit -am 'Add new test suite'`
4. Push to branch: `git push origin feature/new-test-suite`
5. Submit pull request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 📞 Support

- Create GitHub issues for bugs and feature requests
- Check existing documentation and troubleshooting guides
- Review CI/CD pipeline logs for execution details

---

**Built with ❤️ for Quality Engineering**

This framework demonstrates modern test automation practices suitable for enterprise-level applications and serves as a foundation for scalable, maintainable test automation solutions. 