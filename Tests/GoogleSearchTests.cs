using NUnit.Framework;
using OpenQA.Selenium;
using TestFramework;
using UI.AutomationTests.Pages;
using Serilog;

namespace UI.AutomationTests.Tests
{
    [TestFixture]
    [Parallelizable(ParallelScope.Children)]
    public class GoogleSearchTests
    {
        private IWebDriver? _driver;
        private GoogleHomePage? _homePage;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // Configure Serilog for test logging
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/test-log-.txt", rollingInterval: RollingInterval.Day)
                .CreateLogger();

            Log.Information("=== Google Search Tests Suite Started ===");
        }

        [SetUp]
        public void SetUp()
        {
            // Get browser type from environment variable or default to Chrome
            var browserTypeString = Environment.GetEnvironmentVariable("BROWSER") ?? "Chrome";
            Enum.TryParse<BrowserType>(browserTypeString, true, out var browserType);

            Log.Information("Starting test with {Browser} browser", browserType);

            _driver = DriverFactory.CreateDriver(browserType);
            _homePage = new GoogleHomePage(_driver);
        }

        [TearDown]
        public void TearDown()
        {
            try
            {
                // Take screenshot on test failure
                if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
                {
                    var testName = TestContext.CurrentContext.Test.Name;
                    _homePage?.TakeScreenshot($"FAILED_{testName}");
                    Log.Warning("Test {TestName} failed - screenshot captured", testName);
                }
            }
            finally
            {
                DriverFactory.QuitDriver(_driver!);
                Log.Information("Test completed and browser closed");
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Log.Information("=== Google Search Tests Suite Completed ===");
            Log.CloseAndFlush();
        }

        [Test]
        [Category("Smoke")]
        [Description("Verify Google home page loads correctly")]
        public void GoogleHomePage_ShouldLoadSuccessfully()
        {
            // Arrange & Act
            _homePage!.NavigateToGoogle();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(_homePage.IsPageLoaded(), Is.True, "Google home page should load successfully");
                Assert.That(_homePage.IsSearchBoxDisplayed(), Is.True, "Search box should be displayed");
                Assert.That(_homePage.IsGoogleLogoDisplayed(), Is.True, "Google logo should be displayed");
            });

            Log.Information("✓ Google home page loaded successfully");
        }

        [Test]
        [Category("Functional")]
        [Description("Verify search functionality returns relevant results")]
        [TestCase("Selenium WebDriver")]
        [TestCase("Test Automation")]
        [TestCase("C# Programming")]
        public void GoogleSearch_ShouldReturnRelevantResults(string searchTerm)
        {
            // Arrange
            _homePage!.NavigateToGoogle();

            // Act
            var resultsPage = _homePage.PerformSearch(searchTerm);

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(resultsPage.IsPageLoaded(), Is.True, "Search results page should load");
                Assert.That(resultsPage.HasResults(), Is.True, "Search should return results");
                Assert.That(resultsPage.GetSearchQuery(), Is.EqualTo(searchTerm), "Search query should match input");
                Assert.That(resultsPage.ContainsSearchTerm(searchTerm), Is.True, 
                    $"Results should contain the search term '{searchTerm}'");
            });

            var resultCount = resultsPage.GetResultsCount();
            Assert.That(resultCount, Is.GreaterThan(0), "Should return multiple search results");

            Log.Information("✓ Search for '{SearchTerm}' returned {ResultCount} results", searchTerm, resultCount);
        }

        [Test]
        [Category("Functional")]
        [Description("Verify search result navigation and pagination")]
        public void GoogleSearchResults_ShouldSupportNavigation()
        {
            // Arrange
            _homePage!.NavigateToGoogle();
            var resultsPage = _homePage.PerformSearch("automation testing");

            // Act & Assert - First page
            Assert.That(resultsPage.IsPageLoaded(), Is.True, "Results page should load");
            
            var firstPageTitles = resultsPage.GetResultTitles();
            Assert.That(firstPageTitles.Count, Is.GreaterThan(5), "Should have multiple results on first page");

            // Act & Assert - Navigation
            if (resultsPage.IsNextPageAvailable())
            {
                resultsPage.GoToNextPage();
                var secondPageTitles = resultsPage.GetResultTitles();
                
                Assert.That(secondPageTitles, Is.Not.EqualTo(firstPageTitles), 
                    "Second page should have different results");
                
                if (resultsPage.IsPreviousPageAvailable())
                {
                    resultsPage.GoToPreviousPage();
                    Assert.That(resultsPage.IsPageLoaded(), Is.True, "Should navigate back to previous page");
                }
            }

            Log.Information("✓ Search result navigation working correctly");
        }

        [Test]
        [Category("Performance")]
        [Description("Verify search performance is within acceptable limits")]
        public void GoogleSearch_ShouldCompleteWithinTimeLimit()
        {
            // Arrange
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            _homePage!.NavigateToGoogle();

            // Act
            var resultsPage = _homePage.PerformSearch("performance testing");
            stopwatch.Stop();

            // Assert
            Assert.That(resultsPage.IsPageLoaded(), Is.True, "Results should load successfully");
            Assert.That(stopwatch.ElapsedMilliseconds, Is.LessThan(5000), 
                "Search should complete within 5 seconds");

            Log.Information("✓ Search completed in {ElapsedMs}ms", stopwatch.ElapsedMilliseconds);
        }

        [Test]
        [Category("Edge Cases")]
        [Description("Verify search handles special characters and edge cases")]
        [TestCase("C#")]
        [TestCase("Test @#$%")]
        [TestCase("123456")]
        [TestCase("")]
        public void GoogleSearch_ShouldHandleSpecialCharacters(string searchTerm)
        {
            // Arrange
            _homePage!.NavigateToGoogle();

            try
            {
                // Act
                if (string.IsNullOrEmpty(searchTerm))
                {
                    _homePage.EnterSearchTerm(searchTerm);
                    // Empty search should not submit
                    Assert.That(_homePage.IsSearchBoxDisplayed(), Is.True, 
                        "Should remain on home page for empty search");
                }
                else
                {
                    var resultsPage = _homePage.PerformSearch(searchTerm);
                    
                    // Assert
                    Assert.That(resultsPage.IsPageLoaded(), Is.True, 
                        $"Should handle search term '{searchTerm}' gracefully");
                }

                Log.Information("✓ Successfully handled search term: '{SearchTerm}'", searchTerm);
            }
            catch (Exception ex)
            {
                Log.Warning("Search term '{SearchTerm}' caused issue: {Error}", searchTerm, ex.Message);
                throw;
            }
        }

        [Test]
        [Category("Cross-Browser")]
        [Description("Verify core functionality works across different browsers")]
        public void GoogleSearch_ShouldWorkAcrossBrowsers()
        {
            // This test can be run with different browser configurations
            // by setting the BROWSER environment variable

            // Arrange
            _homePage!.NavigateToGoogle();

            // Act
            var resultsPage = _homePage.PerformSearch("cross browser testing");

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(resultsPage.IsPageLoaded(), Is.True, "Should work in current browser");
                Assert.That(resultsPage.HasResults(), Is.True, "Should return search results");
                Assert.That(resultsPage.GetResultsCount(), Is.GreaterThan(0), "Should have multiple results");
            });

            var browserType = Environment.GetEnvironmentVariable("BROWSER") ?? "Chrome";
            Log.Information("✓ Cross-browser test passed for {Browser}", browserType);
        }
    }
} 