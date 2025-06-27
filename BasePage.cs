using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using Serilog;

namespace TestFramework
{
    public abstract class BasePage
    {
        protected readonly IWebDriver Driver;
        protected readonly WebDriverWait Wait;
        protected readonly string BaseUrl;

        protected BasePage(IWebDriver driver, string baseUrl = "")
        {
            Driver = driver;
            BaseUrl = baseUrl;
            Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
        }

        // Common element operations
        protected virtual void Click(By locator)
        {
            Log.Information("Clicking element: {Locator}", locator);
            WaitForElementToBeClickable(locator).Click();
        }

        protected virtual void SendKeys(By locator, string text)
        {
            Log.Information("Typing '{Text}' into element: {Locator}", text, locator);
            var element = WaitForElementToBeVisible(locator);
            element.Clear();
            element.SendKeys(text);
        }

        protected virtual string GetText(By locator)
        {
            var text = WaitForElementToBeVisible(locator).Text;
            Log.Information("Retrieved text '{Text}' from element: {Locator}", text, locator);
            return text;
        }

        protected virtual bool IsElementDisplayed(By locator)
        {
            try
            {
                var element = Driver.FindElement(locator);
                var isDisplayed = element.Displayed;
                Log.Information("Element {Locator} displayed: {IsDisplayed}", locator, isDisplayed);
                return isDisplayed;
            }
            catch (NoSuchElementException)
            {
                Log.Information("Element {Locator} not found", locator);
                return false;
            }
        }

        protected virtual void ScrollToElement(By locator)
        {
            var element = Driver.FindElement(locator);
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            Log.Information("Scrolled to element: {Locator}", locator);
        }

        // Wait methods
        protected virtual IWebElement WaitForElementToBeVisible(By locator)
        {
            return Wait.Until(ExpectedConditions.ElementIsVisible(locator));
        }

        protected virtual IWebElement WaitForElementToBeClickable(By locator)
        {
            return Wait.Until(ExpectedConditions.ElementToBeClickable(locator));
        }

        protected virtual bool WaitForElementToDisappear(By locator, int timeoutSeconds = 10)
        {
            var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(timeoutSeconds));
            return wait.Until(ExpectedConditions.InvisibilityOfElementLocated(locator));
        }

        protected virtual void WaitForPageToLoad()
        {
            Wait.Until(driver => ((IJavaScriptExecutor)driver).ExecuteScript("return document.readyState").Equals("complete"));
            Log.Information("Page loaded completely");
        }

        // Navigation
        protected virtual void NavigateTo(string url)
        {
            var fullUrl = string.IsNullOrEmpty(BaseUrl) ? url : $"{BaseUrl.TrimEnd('/')}/{url.TrimStart('/')}";
            Log.Information("Navigating to: {Url}", fullUrl);
            Driver.Navigate().GoToUrl(fullUrl);
            WaitForPageToLoad();
        }

        protected virtual void RefreshPage()
        {
            Log.Information("Refreshing current page");
            Driver.Navigate().Refresh();
            WaitForPageToLoad();
        }

        // Screenshot capability
        public virtual string TakeScreenshot(string testName)
        {
            try
            {
                var screenshot = ((ITakesScreenshot)Driver).GetScreenshot();
                var fileName = $"{testName}_{DateTime.Now:yyyyMMdd_HHmmss}.png";
                var filePath = Path.Combine("Screenshots", fileName);
                
                Directory.CreateDirectory("Screenshots");
                screenshot.SaveAsFile(filePath);
                
                Log.Information("Screenshot saved: {FilePath}", filePath);
                return filePath;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to take screenshot for test: {TestName}", testName);
                return string.Empty;
            }
        }

        // Dropdown operations
        protected virtual void SelectFromDropdown(By locator, string visibleText)
        {
            var element = WaitForElementToBeClickable(locator);
            var dropdown = new SelectElement(element);
            dropdown.SelectByText(visibleText);
            Log.Information("Selected '{Text}' from dropdown: {Locator}", visibleText, locator);
        }

        protected virtual void SelectFromDropdownByValue(By locator, string value)
        {
            var element = WaitForElementToBeClickable(locator);
            var dropdown = new SelectElement(element);
            dropdown.SelectByValue(value);
            Log.Information("Selected value '{Value}' from dropdown: {Locator}", value, locator);
        }

        // Alert handling
        protected virtual void AcceptAlert()
        {
            Wait.Until(ExpectedConditions.AlertIsPresent());
            Driver.SwitchTo().Alert().Accept();
            Log.Information("Alert accepted");
        }

        protected virtual string GetAlertText()
        {
            Wait.Until(ExpectedConditions.AlertIsPresent());
            var alertText = Driver.SwitchTo().Alert().Text;
            Log.Information("Alert text: {AlertText}", alertText);
            return alertText;
        }
    }
} 