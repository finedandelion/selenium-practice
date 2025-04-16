﻿using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V133.Autofill;
using OpenQA.Selenium.Support.UI;

namespace selenium_test;

public class Tests
{
    private readonly static ChromeDriver _Driver = new();
    private const string _UserLogin = "user1";
    private const string _UserPassword = "Qq!2333098";
    private const string _BaseUrl = "https://staff-testing.testkontur.ru/";

    [SetUp]
    public void BaseSetup()
    {
        ConfigureDriver(_Driver);
        Authorize(_Driver);
    }

    [TearDown]
    public void BaseTearDown()
    {
        _Driver!.Quit();
    }

    [Test]
    public void AuthorizationNavigationTest()
    {
        Assert.That(_Driver.Title, Does.Contain("Новости"), message: "Навигация после авторизации не соответствует странице \"Новости\"");
    }

    [Test]
    public void CommunitiesNavigationTest()
    {
        HeadToCommunities(_Driver);

        var title = _Driver.FindElement(By.CssSelector("[data-tid=\"Title\"]")).Text;
        Assert.That(title, Is.EqualTo("Сообщества"), message: "Навигация в раздел \"Сообщества\" некорректна");
    }

    private void Authorize(IWebDriver driver)
    {
        driver.Navigate().GoToUrl(_BaseUrl);

        var loginField = driver.FindElement(By.Id("Username"));
        loginField.SendKeys(_UserLogin);

        var passwordField =  driver.FindElement(By.Id("Password"));
        passwordField.SendKeys(_UserPassword);

        driver.FindElement(By.Name("button")).Click();

        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
        wait.Until(ExpectedConditions.TitleIs("Новости"));
    }

    private void ConfigureDriver(IWebDriver driver)
    {
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(3);
    }

    private void HeadToCommunities(IWebDriver driver)
    {
        driver.FindElement(By.CssSelector("[data-tid=\"SidebarMenuButton\"]")).Click();
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(3));
       
        var button = driver.FindElements(By.CssSelector("[data-tid=\"Community\"]")).First(e => e.Displayed);
        button.Click();
    }
}
