using OpenQA.Selenium;
using TestFramework;

namespace UI.AutomationTests.Pages
{
    public class GoogleHomePage : BasePage
    {
        private const string BaseUrl = "https://www.google.com";

        // Locators
        private readonly By _searchBox = By.Name("q");
        private readonly By _searchButton = By.Name("btnK");
        private readonly By _luckyButton = By.Name("btnI");
        private readonly By _googleLogo = By.Id("hplogo");
        private readonly By _acceptCookiesButton = By.XPath("//div[text()='Accept all']");

        public GoogleHomePage(IWebDriver driver) : base(driver, BaseUrl)
        {
        }

        public GoogleHomePage NavigateToGoogle()
        {
            NavigateTo(string.Empty);
            
            // Handle cookies popup if present
            try
            {
                if (IsElementDisplayed(_acceptCookiesButton))
                {
                    Click(_acceptCookiesButton);
                }
            }
            catch
            {
                // Ignore if cookies popup is not present
            }

            return this;
        }

        public bool IsPageLoaded()
        {
            return IsElementDisplayed(_googleLogo) && IsElementDisplayed(_searchBox);
        }

        public GoogleHomePage EnterSearchTerm(string searchTerm)
        {
            SendKeys(_searchBox, searchTerm);
            return this;
        }

        public GoogleSearchResultsPage ClickSearchButton()
        {
            Click(_searchButton);
            return new GoogleSearchResultsPage(Driver);
        }

        public GoogleSearchResultsPage PerformSearch(string searchTerm)
        {
            EnterSearchTerm(searchTerm);
            return ClickSearchButton();
        }

        public bool IsSearchBoxDisplayed()
        {
            return IsElementDisplayed(_searchBox);
        }

        public bool IsGoogleLogoDisplayed()
        {
            return IsElementDisplayed(_googleLogo);
        }

        public void ClickImFeelingLucky()
        {
            Click(_luckyButton);
        }

        public string GetSearchBoxPlaceholder()
        {
            var element = WaitForElementToBeVisible(_searchBox);
            return element.GetAttribute("placeholder") ?? string.Empty;
        }
    }
} 