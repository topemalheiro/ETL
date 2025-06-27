using OpenQA.Selenium;
using TestFramework;

namespace UI.AutomationTests.Pages
{
    public class GoogleSearchResultsPage : BasePage
    {
        // Locators
        private readonly By _searchResults = By.CssSelector("div.g");
        private readonly By _searchBox = By.Name("q");
        private readonly By _resultStats = By.Id("result-stats");
        private readonly By _firstResult = By.CssSelector("div.g:first-child h3");
        private readonly By _nextPageButton = By.Id("pnnext");
        private readonly By _previousPageButton = By.Id("pnprev");
        private readonly By _pageNumbers = By.CssSelector("a[aria-label*='Page']");

        public GoogleSearchResultsPage(IWebDriver driver) : base(driver)
        {
        }

        public bool IsPageLoaded()
        {
            return IsElementDisplayed(_searchResults) && IsElementDisplayed(_resultStats);
        }

        public int GetResultsCount()
        {
            var results = Driver.FindElements(_searchResults);
            return results.Count;
        }

        public string GetSearchQuery()
        {
            var searchBoxElement = WaitForElementToBeVisible(_searchBox);
            return searchBoxElement.GetAttribute("value") ?? string.Empty;
        }

        public string GetResultStats()
        {
            return GetText(_resultStats);
        }

        public bool HasResults()
        {
            return GetResultsCount() > 0;
        }

        public string GetFirstResultTitle()
        {
            return GetText(_firstResult);
        }

        public void ClickFirstResult()
        {
            Click(_firstResult);
        }

        public GoogleSearchResultsPage PerformNewSearch(string searchTerm)
        {
            SendKeys(_searchBox, searchTerm);
            Driver.FindElement(_searchBox).SendKeys(Keys.Enter);
            return this;
        }

        public bool IsNextPageAvailable()
        {
            return IsElementDisplayed(_nextPageButton);
        }

        public bool IsPreviousPageAvailable()
        {
            return IsElementDisplayed(_previousPageButton);
        }

        public GoogleSearchResultsPage GoToNextPage()
        {
            if (IsNextPageAvailable())
            {
                Click(_nextPageButton);
            }
            return this;
        }

        public GoogleSearchResultsPage GoToPreviousPage()
        {
            if (IsPreviousPageAvailable())
            {
                Click(_previousPageButton);
            }
            return this;
        }

        public List<string> GetResultTitles()
        {
            var titles = new List<string>();
            var resultElements = Driver.FindElements(By.CssSelector("div.g h3"));
            
            foreach (var element in resultElements)
            {
                if (!string.IsNullOrEmpty(element.Text))
                {
                    titles.Add(element.Text);
                }
            }

            return titles;
        }

        public List<string> GetResultUrls()
        {
            var urls = new List<string>();
            var linkElements = Driver.FindElements(By.CssSelector("div.g a[href]"));
            
            foreach (var element in linkElements)
            {
                var href = element.GetAttribute("href");
                if (!string.IsNullOrEmpty(href) && href.StartsWith("http"))
                {
                    urls.Add(href);
                }
            }

            return urls;
        }

        public bool ContainsSearchTerm(string searchTerm)
        {
            var titles = GetResultTitles();
            return titles.Any(title => title.ToLower().Contains(searchTerm.ToLower()));
        }
    }
} 