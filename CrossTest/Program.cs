using Xunit;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Linq;
using System;
using System.Collections.Generic;
using System.IO;

namespace CrossTest
{
    public class Program : IDisposable
    {
        public IWebDriver Driver { get; set; }

        public string filePath = @"../../../companiesAndDirectors.txt";

        [Fact]
        public void CrossTest()
        {
            RunBrowser("http://www.justice.cz");
            DeleteOldLines();
            var companies = FindCompanies();
            foreach (var company in companies)
            {
                string id = GetId(company);
                List<string> directors = GetDirectors(company);
                WriteToTxt(company, id, directors);
            }
        }

        public string GetId(string company)
        {
            var companies = Driver.WaitForElement(By.ClassName("search-results")).FindElements(By.TagName("li")).Where(e => e.GetAttribute("class").Contains("result"));
            var companyElement = companies.FirstOrDefault(e => e.FindElement(By.CssSelector("strong[class='left']")).Text.Equals(company));
            return companyElement.FindElement(By.CssSelector("span[class='nowrap']")).Text;
        }

        public void DeleteOldLines()
        {
            if (File.Exists(filePath))
                File.WriteAllText(filePath, String.Empty);
        }

        public void WriteToTxt(string company, string id, List<string> directors)
        {
            using (var sw = File.AppendText(filePath))
            {
                sw.WriteLine("Company: {0}\nICO: {1} \nDirectors:", company, id);
                foreach (var director in directors)
                    sw.WriteLine(director);
                sw.WriteLine("\n\n");
            }
        }

        public List<string> FindCompanies()
        {
            List<string> companiesNames = new List<string>();
            var column = Driver.WaitForElements(By.ClassName("expert-form")).FirstOrDefault(e => e.FindElement(By.TagName("a")).GetAttribute("href").Contains("or"));
            if (column != null)
            {
                var input = column.FindElement(By.Id("cse-search-input-isvr"));
                Driver.Write(input, "s. r. o.");
                column.FindElement(By.ClassName("btn-search")).Click();

                var companies = Driver.WaitForElement(By.ClassName("search-results")).FindElements(By.TagName("li")).Where(e => e.GetAttribute("class").Contains("result"));
                Assert.NotNull(companies);
                int index = 0;
                foreach (var company in companies)
                {
                    var companyName = companies.ElementAt(index).FindElement(By.CssSelector("strong[class='left']")).Text;
                    if (companyName.EndsWith("s.r.o."))
                        companiesNames.Add(companyName);
                    index++;
                }
            }
            return companiesNames;
        }

        public List<string> GetDirectors(string company)
        {
            List<string> directors = new List<string>();
            var companies = Driver.WaitForElement(By.ClassName("search-results")).FindElements(By.TagName("li")).Where(e => e.GetAttribute("class").Contains("result"));
            var companyElement = companies.FirstOrDefault(e => e.FindElement(By.CssSelector("strong[class='left']")).Text.Equals(company));

            var infoButton = companyElement.FindElements(By.TagName("a")).FirstOrDefault(e => e.GetAttribute("href").Contains("UPLNY"));
            infoButton.Click();

            IWebElement[] rows = Driver.WaitForElements(By.ClassName("div-table")).Where(e => e.Text.ToLower().StartsWith("jednatel")).ToArray();
            foreach (var row in rows)
            {
                var director = row.Text.Split(new string[] { "\r\n" }, StringSplitOptions.None)[1];
                if (director.Contains(","))
                    director = director.Remove(director.IndexOf(","));

                if (!DuplicityTest(directors, director) && !director.StartsWith("zapsáno"))
                    directors.Add(director);
            }

            while (Driver.Url.EndsWith("UPLNY"))
                Driver.WaitForElement(By.CssSelector("a[href*='./rejstrik-$firma?']")).Click();

            return directors;
        }

        public bool DuplicityTest(List<string> directors, string director)
        {
            var duplicity = false;
            foreach (var dir in directors)
            {
                if (dir == director)
                {
                    duplicity = true;
                    break;
                }
            }
            return duplicity;
        }

        public void RunBrowser(string url)
        {
            var chromeOptions = new ChromeOptions();
            chromeOptions.AddArguments("headless");
            chromeOptions.AddArgument("--window-size=1920,1080");
            Driver = new ChromeDriver(chromeOptions);
            Driver.Manage().Window.Maximize();
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            Driver.Url = url;
        }

        public void Dispose()
        {
            Driver.Close();
            Driver.Quit();
        }
    }
}
