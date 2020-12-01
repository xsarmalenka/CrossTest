using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Linq;
using System;

namespace CrossTest
{
    public class Program : IDisposable
    {
        public IWebDriver Driver { get; set; }

        public string company = "CROSS Zlín, a.s.";

        [Fact]
        public void CrossTest()
        {
            RunBrowser("http://www.justice.cz");
            FindCompany(company);
            var ico = GetICO();
            Console.WriteLine("Company: {0} ICO: {1}", company, ico);
            Assert.Equal(ico, "60715286");
        }

        public string GetICO()
        {
            var row = Driver.WaitForElements(By.ClassName("div-row")).FirstOrDefault(e => e.Text.Contains("Identifikační číslo")).Text;
            return row.Split(new string[] { "\r\n" }, StringSplitOptions.None)[1];
        }

        public void FindCompany(string companyName)
        {
            var column = Driver.WaitForElements(By.ClassName("expert-form")).FirstOrDefault(e=>e.FindElement(By.TagName("a")).GetAttribute("href").Contains("or"));
            if (column != null)
            {
                var input = column.FindElement(By.Id("cse-search-input-isvr"));
                Driver.Write(input, companyName);
                column.FindElement(By.ClassName("btn-search")).Click();

                var result = Driver.WaitForElement(By.ClassName("search-results")).FindElements(By.TagName("li")).FirstOrDefault();
                Assert.NotNull(result);
                var infoButton = result.FindElements(By.TagName("a")).FirstOrDefault(e => e.GetAttribute("href").Contains("UPLNY"));
                infoButton.Click();
            }
        }

        public void RunBrowser(string url)
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            chromeOptions.AddArgument("--window-size=1920,1080");
            Driver = new ChromeDriver(chromeOptions);
            Driver.Manage().Window.Maximize();
            Driver.Url = url;
        }

        public void Dispose()
        {
            Driver.Close();
            Driver.Quit();
        }
    }
}
