using System;
using System.Collections.ObjectModel;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace CrossTest
{
    public static class DriverHelper
    {
        public static IWebElement WaitForElement(this IWebDriver driver, By by)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(by));
            var element = wait.Until(drv => drv.FindElement(by));
            return element;
        }

        public static ReadOnlyCollection<IWebElement> WaitForElements(this IWebDriver driver, By by)
        {
            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
            var elements = wait.Until(drv => drv.FindElements(by));
            return elements;
        }

        public static void Write(this IWebDriver driver, IWebElement element, string text)
        {
            element.Clear();
            element.SendKeys(text);
            // ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].value='" + text + "';", element);
        }
    }
}
