namespace TarbikMap.Tests.Browser
{
    using System;
    using System.Diagnostics;
    using System.Drawing;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;
    using OpenQA.Selenium.Interactions;
    using OpenQA.Selenium.Remote;
    using TarbikMap.Common;

    internal class DriverWrapper : IDisposable
    {
        private IWebDriver driver;
        private int x;
        private int y;
        private int waitingScreenshotCounter;

        public DriverWrapper(int x, int y)
        {
            ChromeOptions chromeOptions = new ChromeOptions();
            chromeOptions.AddArgument("--ignore-certificate-errors");

            var driver = CreateDriver(chromeOptions);

            driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(2);
            driver.Manage().Window.Position = new Point(x * 600, y * 800);
            driver.Manage().Window.Size = new Size(600, 800);

            this.driver = driver;
            this.x = x;
            this.y = y;
        }

        public void GoToPath(string path)
        {
            Uri uri = new Uri(GetBaseAddress(), path);

            this.driver.Navigate().GoToUrl(uri);
        }

        public string GetUrl()
        {
            return this.driver.Url;
        }

        public void ClickButtonByText(string text)
        {
            this.WithRetries(() =>
            {
                this.driver.FindElement(By.XPath($"//button[contains(.,'{text}')]")).Click();
            });
        }

        public void ClickLinkByText(string text)
        {
            this.WithRetries(() =>
            {
                this.driver.FindElement(By.XPath($"//a[contains(.,'{text}')]")).Click();
            });
        }

        public void ClickSpanByText(string text)
        {
            this.WithRetries(() =>
            {
                this.driver.FindElement(By.XPath($"//span[contains(.,'{text}')]")).Click();
            });
        }

        public void FillInputByPlaceholder(string placeholder, string value)
        {
            this.WithRetries(() =>
            {
                this.driver.FindElement(By.XPath($"//input[@placeholder='{placeholder}']")).SendKeys(value);
            });
        }

        public void WaitForText(string text)
        {
            this.WithRetries(() =>
            {
                string xpath = string.Format(CultureInfo.InvariantCulture, "/html/body//*[contains(text(), '{0}')]", text);
                this.driver.FindElement(By.XPath(xpath));
            });
        }

        public void ClickCoordinates(int x, int y)
        {
            var element = this.driver.FindElement(By.TagName("body"));
            new Actions(this.driver).MoveToElement(element, 0, 0).MoveByOffset(x, y).Click().Perform();
        }

        public void Dispose()
        {
            this.TakeScreenshot("end");

            this.driver.Dispose();
        }

        private static string GetTestMethodName()
        {
            StackTrace stackTrace = new StackTrace();
            foreach (StackFrame frame in stackTrace.GetFrames())
            {
                var method = frame.GetMethod();
                if (method != null && method.DeclaringType != null && method.Name == "Test")
                {
                    return method.DeclaringType.Name;
                }
            }

            throw new InvalidOperationException("Unable to get test method name");
        }

        private static WebDriver CreateDriver(DriverOptions options)
        {
            string seleniumUrl = EnvironmentVariables.GetValue("TARBIKMAP_SELENIUM_URL");

            return new RemoteWebDriver(new Uri(seleniumUrl), options);
        }

        private static Uri GetBaseAddress()
        {
            string appUrl = EnvironmentVariables.GetValue("TARBIKMAP_APP_URL");

            return new Uri(appUrl);
        }

        private void WithRetries(Action action)
        {
            int count = 100;
            for (int i = 0; i < count; ++i)
            {
                if (i > 0)
                {
                    Thread.Sleep(TimeSpan.FromMilliseconds(100));
                }

                if (i > 0 && i % 20 == 0)
                {
                    this.TakeScreenshot("waiting_" + (this.waitingScreenshotCounter++));
                }

                try
                {
                    action();
                    return;
                }
                catch (Exception e)
                {
                    if (i < count - 1)
                    {
                        if (e is StaleElementReferenceException || e is NoSuchElementException)
                        {
                            continue;
                        }
                    }

                    throw;
                }
            }
        }

        private void TakeScreenshot(string name)
        {
            var screenshot = ((ITakesScreenshot)this.driver).GetScreenshot();
            string fileName = $"s_{GetTestMethodName()}_{this.x}_{this.y}_{name}.png";

            string screenshotsDirectory = EnvironmentVariables.GetValue("TARBIKMAP_SCREENSHOTS_DIRECTORY");

            Directory.CreateDirectory(screenshotsDirectory);
            screenshot.SaveAsFile(Path.Combine(screenshotsDirectory, fileName), ScreenshotImageFormat.Png);
        }
    }
}
