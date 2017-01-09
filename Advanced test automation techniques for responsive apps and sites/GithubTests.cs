namespace Applitools
{
    using System;
    using System.Drawing;
    using Applitools;
    using NUnit.Framework;
    using OpenQA.Selenium;
    using OpenQA.Selenium.Chrome;

    [TestFixture(1000)]
    [TestFixture(800)]
    public class GithubTests
    {
        private static BatchInfo batch_ = new BatchInfo("Responsive");

        private IWebDriver driver_;
        private int width_;
        private Eyes eyes_;

        public GithubTests(int width)
        {
            width_ = width;
        }

        [SetUp]
        public void Setup()
        {
            driver_ = new ChromeDriver();

            eyes_ = new Eyes(new Uri("https://demo.applitools.com"));
            eyes_.ApiKey = Environment.GetEnvironmentVariable("DEMO_APIKEY");
            eyes_.ForceFullPageScreenshot = true;
            eyes_.Batch = batch_;

            driver_ = eyes_.Open(
                driver_, "Github", TestContext.CurrentContext.Test.Name, new Size(width_, 600));
        }

        [TearDown]
        public void TearDown()
        {
            driver_.Quit();
            eyes_.Close();
        }

        [Test]
        public void TestResponsiveness()
        {
            driver_.Url = "https://github.com";

            var homePage = new HomePage(driver_, desc => eyes_.CheckWindow(desc));
            eyes_.CheckWindow(homePage.Name);

            var personalPage = homePage.GoToPersonalPage();
            eyes_.CheckWindow(personalPage.Name);

            var openSourcePage = personalPage.GoToOpenSourcePage();
            eyes_.CheckWindow(openSourcePage.Name);
        }

        public abstract class BasePage
        {
            protected readonly IWebDriver driver;
            protected readonly Action<string> uiStateChangedHandler_;

            protected readonly By personalLocator = By.ClassName("nav-item-personal");
            protected readonly By openSourceLocator = By.ClassName("nav-item-opensource");
            protected readonly By navMenuLocator = By.ClassName("octicon-three-bars");

            public BasePage(string pageName, IWebDriver driver, Action<string> uiStateChangedHandler)
            {
                Name = pageName;
                this.driver = driver;
                uiStateChangedHandler_ = uiStateChangedHandler;
            }

            public string Name { get; private set; }

            public PersonalPage GoToPersonalPage()
            {
                ClickNavButton(personalLocator);
                return new PersonalPage(driver, uiStateChangedHandler_);
            }

            public OpenSourcePage GoToOpenSourcePage()
            {
                ClickNavButton(openSourceLocator);
                return new OpenSourcePage(driver, uiStateChangedHandler_);
            }

            protected void ClickNavButton(By locator)
            {
                var navMenu = driver.FindElement(navMenuLocator);
                if (navMenu.Displayed)
                {
                    navMenu.Click();
                    FireUIStateChanged("Navigation Menu");
                }

                driver.FindElement(locator).Click();
            }

            protected void FireUIStateChanged(string description)
            {
                if (uiStateChangedHandler_ != null)
                {
                    uiStateChangedHandler_(Name + " + " + description);
                }
            }
        }

        public class HomePage : BasePage
        {
            public HomePage(IWebDriver driver, Action<string> uiStateChangedHandler = null)
                : base("Home", driver, uiStateChangedHandler)
            {
            }
        }

        public class PersonalPage : BasePage
        {
            public PersonalPage(IWebDriver driver, Action<string> uiStateChangedHandler = null)
                : base("Personal", driver, uiStateChangedHandler)
            {
            }
        }

        public class OpenSourcePage : BasePage
        {
            public OpenSourcePage(IWebDriver driver, Action<string> uiStateChangedHandler = null)
                : base("Open Source", driver, uiStateChangedHandler)
            {
            }
        }
    }
}
