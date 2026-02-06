using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Chromium;
using OpenQA.Selenium.Safari;
using OpenQA.Selenium.Support.UI;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support;
using SeleniumExtras.WaitHelpers;

namespace SeleniumWebTesting
{
    class SeliniumTest
    {
        IWebDriver driver;



        [SetUp]
        public void startBroswer()
        {
            string browser = "firefox";

            switch (browser)
            {
                case "chrome":
                    driver = new ChromeDriver("C:\\Program Files\\Google\\Chrome\\Application\\chrome.exe");
                    break;
                case "firefox":
                    FirefoxOptions options = new FirefoxOptions();
                    options.AddArgument("--disable-popup-blocking");
                    driver = new FirefoxDriver("C:\\Program Files\\Mozilla Firefox\\firefox.exe", options);
                    
                    break;
            }
        }

        [Test]
        public void test()
        {
            //Declaring the URL
            driver.Url = "http://shoesensation.com";
            driver.Manage().Window.Maximize();

            //Hover over Navbar element for "Women"
            JustMouseHover(driver, "//a[@id='ui-id-3']", 10);

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            //Selecting Nav Element for "Athletic Running Shoes"
            IWebElement navLink = driver.FindElement(By.XPath("//a[@id='ui-id-6']"));
            navLink.Click();

            //Selecting the first shoe on the Product List Page
            navLink = driver.FindElement(By.CssSelector("li.product:nth-child(1) > div:nth-child(1) > div:nth-child(2) > strong:nth-child(1) > a:nth-child(1)"));
            navLink.Click();

            string cartCounter = "0";

            //while (cartCounter == "0")
            //{

                wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("#option-label-size-190-item-190")));
                //Selecting the size of shoe
                navLink = driver.FindElement(By.CssSelector("#option-label-size-190-item-190"));
                navLink.Click();

                //Adding To Cart
                navLink = driver.FindElement(By.XPath("//*[@id='product-addtocart-button']"));
                navLink.Click();

                //Waiting for cart to reflect that product was added
                //JustMouseHover(driver, "/html/body/div[5]/header/div[2]/div/div[3]/a", 100);

                cartCounter = driver.FindElement(By.XPath("/html/body/div[5]/header/div[2]/div/div[3]/a/span[2]/span[1]")).GetAttribute("/html/body/div[5]/header/div[2]/div/div[3]/a/span[2]/span[1]");


            //}

            //JustMouseHover(driver, "//*[@id='product-addtocart-button", 100);
            //Clicking on the cart button
            navLink = driver.FindElement(By.XPath("/html/body/div[5]/header/div[2]/div/div[3]/a"));
            navLink.Click();


            //wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("/html/body/div[3]/main/div[3]/div/div[2]/div[1]/ul/li[1]/button")));
            navLink = driver.FindElement(By.CssSelector("div.minicart-wrapper:nth-child(5) > a:nth-child(1)"));
            navLink.Click();

            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("/html/body/div[3]/main/div[3]/div/div[2]/div[1]/ul/li[1]/button")));
            navLink = driver.FindElement(By.XPath("/html/body/div[3]/main/div[3]/div/div[2]/div[1]/ul/li[1]/button"));
            navLink.Click();

            //Define Checkout Elements to be filled out
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//input[@id='customer-email']")));

            IWebElement emailTextBox = driver.FindElement(By.XPath("//input[@id='customer-email']"));
            IWebElement firstNameTextBox = driver.FindElement(By.XPath("//input[@name='firstname']"));
            IWebElement lastNameTextBox = driver.FindElement(By.XPath("//input[@name='lastname']"));
            IWebElement street1TextBox  = driver.FindElement(By.XPath("//input[@name='street[0]']"));
            IWebElement cityTextBox     = driver.FindElement(By.XPath("//input[@name='city']"));
            IWebElement zipTextBox      = driver.FindElement(By.XPath("//input[@name='postcode']"));
            IWebElement phoneTextBox = driver.FindElement(By.XPath("//input[@name='telephone']"));
            IWebElement stateDropDown = driver.FindElement(By.XPath("//*[@name='region_id']"));

            //Filling out Checkout Elements with text
            emailTextBox.SendKeys("ewright@shoesensation.com");
            firstNameTextBox.SendKeys("Eric");
            lastNameTextBox.SendKeys("Wright");
            street1TextBox.SendKeys("253 America Place Dr");
            cityTextBox.SendKeys("Jeffersonville");
            zipTextBox.SendKeys("47130");
            phoneTextBox.SendKeys("8129200950");

            //Selecting State from Dropdwon Box
            wait.Until(ExpectedConditions.ElementToBeSelected(By.XPath("//*[@name='region_id']")));
            var selector = new SelectElement(stateDropDown);
            selector.SelectByValue("24");
            


            //Clicking the Next Step Button
            //JustMouseHover(driver, "//*[@id='step-trigger']", 100);
            navLink = driver.FindElement(By.CssSelector("div.actions-toolbar-trigger:nth-child(4) > button:nth-child(1)"));
            navLink.Click();

            
            wait.Until(ExpectedConditions.ElementToBeClickable(By.XPath("//*[@id='braintree']")));

            //Selecting Credit Card as the Payment Type
            navLink = driver.FindElement(By.XPath("//*[@id='braintree']"));
            navLink.Click();

            //Wait For Credit Card Form To Load
            wait.Until(ExpectedConditions.ElementToBeClickable(By.CssSelector("#credit-card-number")));

            //Entering In Credit Card Information
            IWebElement cardNumberTextBox = driver.FindElement(By.XPath("//input[@name='credit-card-number']"));
            IWebElement expirationTextBox = driver.FindElement(By.XPath("//input[@id='expiration']"));
            IWebElement cvvTextBox = driver.FindElement(By.XPath("//input[@id='cvv']"));

            //Filling in Credit Card Information
            cardNumberTextBox.SendKeys("4111111111111111");
            expirationTextBox.SendKeys("122025");
            cvvTextBox.SendKeys("123");

            //Final Checkout Button Push
            navLink = driver.FindElement(By.CssSelector("div.actions-toolbar-trigger:nth-child(5) > button:nth-child(1)"));
            navLink.Click();



            
  
        }

        private object WebDriverWait(IWebDriver driver, int v)
        {
            throw new NotImplementedException();
        }

        [TearDown]
        public void closeBrowser()
        {
            driver.Close();
        }


        //Menu XPath is the XPath of menu for which you have to perform a hover operation
        public void JustMouseHover(IWebDriver _driver, String MenuXPath, int _time)
        {

            WebDriverWait wait = new WebDriverWait(_driver, TimeSpan.FromSeconds(_time));
            var element = wait.Until(ExpectedConditions.ElementIsVisible(By.XPath(MenuXPath)));
            Actions action = new Actions(_driver);
            action.MoveToElement(element).Perform();
            //Waiting for the menu to be displayed    
            System.Threading.Thread.Sleep(4000);


        }
    }
}
