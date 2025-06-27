using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Edge;
using Serilog;

namespace TestFramework
{
    public enum BrowserType
    {
        Chrome,
        Firefox,
        Edge
    }

    public class DriverFactory
    {
        private static readonly Dictionary<BrowserType, Func<IWebDriver>> DriverCreators = new()
        {
            { BrowserType.Chrome, CreateChromeDriver },
            { BrowserType.Firefox, CreateFirefoxDriver },
            { BrowserType.Edge, CreateEdgeDriver }
        };

        public static IWebDriver CreateDriver(BrowserType browserType, bool headless = false)
        {
            Log.Information("Creating {BrowserType} driver with headless={Headless}", browserType, headless);
            
            if (!DriverCreators.TryGetValue(browserType, out var creator))
            {
                throw new ArgumentException($"Unsupported browser type: {browserType}");
            }

            var driver = creator();
            
            // Set implicit wait
            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
            
            // Maximize window for better element visibility
            driver.Manage().Window.Maximize();
            
            Log.Information("{BrowserType} driver created successfully", browserType);
            return driver;
        }

        private static IWebDriver CreateChromeDriver()
        {
            var options = new ChromeOptions();
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--window-size=1920,1080");
            
            // For CI/CD environments
            if (Environment.GetEnvironmentVariable("CI") == "true")
            {
                options.AddArgument("--headless");
            }

            return new ChromeDriver(options);
        }

        private static IWebDriver CreateFirefoxDriver()
        {
            var options = new FirefoxOptions();
            options.AddArgument("--width=1920");
            options.AddArgument("--height=1080");

            // For CI/CD environments
            if (Environment.GetEnvironmentVariable("CI") == "true")
            {
                options.AddArgument("--headless");
            }

            return new FirefoxDriver(options);
        }

        private static IWebDriver CreateEdgeDriver()
        {
            var options = new EdgeOptions();
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--window-size=1920,1080");

            // For CI/CD environments
            if (Environment.GetEnvironmentVariable("CI") == "true")
            {
                options.AddArgument("--headless");
            }

            return new EdgeDriver(options);
        }

        public static void QuitDriver(IWebDriver driver)
        {
            try
            {
                if (driver != null)
                {
                    Log.Information("Closing browser driver");
                    driver.Quit();
                    driver.Dispose();
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Error while closing driver");
            }
        }
    }
} 