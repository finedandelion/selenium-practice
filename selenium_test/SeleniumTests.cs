using System.Data;
using System.Drawing;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.DevTools.V133.Autofill;
using OpenQA.Selenium.Support.UI;

namespace selenium_test;

public abstract class SeleniumTests
{
    protected ChromeDriver _Driver;
    protected Size _BrowserWindowSize;
    protected TimeSpan _BrowserImplicitWait;

    protected const string _UserLogin = "user1";
    protected const string _UserPassword = "Qq!2333098";
    protected const string _BaseUrl = "https://staff-testing.testkontur.ru/";

    [SetUp]
    public void BaseSetup()
    {  
        ConfigureDriverParams(new Size(1920, 1080));
    }

    [TearDown]
    public void BaseTearDown()
    {
        _Driver!.Quit();
    }

    protected void Authorize()
    {
        _Driver.Navigate().GoToUrl(_BaseUrl);

        var loginField = _Driver.FindElement(By.Id("Username"));
        loginField.SendKeys(_UserLogin);

        var passwordField =  _Driver.FindElement(By.Id("Password"));
        passwordField.SendKeys(_UserPassword);

        _Driver.FindElement(By.Name("button")).Click();

        await(ExpectedConditions.TitleIs("Новости"));
    }

    protected void ConfigureDriverParams(Size browserWindowSize, int secondsToImplicitWait = 5)
    {
        _BrowserWindowSize = browserWindowSize;
        _BrowserImplicitWait = TimeSpan.FromSeconds(secondsToImplicitWait);
    }

    protected void SetUpDriver()
    {
        _Driver = new();
        _Driver.Manage().Timeouts().ImplicitWait = _BrowserImplicitWait;
        _Driver.Manage().Window.Size = _BrowserWindowSize;
    }

    protected void HeadToCommunities()
    {
        _Driver.FindElement(By.CssSelector("[data-tid=\"SidebarMenuButton\"]")).Click();
        _Driver.FindElements(By.CssSelector("[data-tid=\"Community\"]"))
                .First(e => e.Displayed)
                .Click();

        await(ExpectedConditions.TitleIs("Сообщества"));
    }

    protected int GetCommunitiesCount()
    {
        var text = _Driver.FindElement(By.CssSelector("[data-tid=\"CommunitiesCounter\"] span")).Text;
        return Int32.Parse(text.Split(' ').First());
    }

    protected void OpenCommunityCreationModal()
    {
        _Driver.FindElement(By.CssSelector("[data-tid=\"PageHeader\"] button[class=\"sc-juXuNZ sc-ecQkzk WTxfS vPeNx\"]")).Click();
    }

    protected void SelectCommunityMessageField(out IWebElement field)
    {
        field = _Driver.FindElement(By.CssSelector("label[data-tid=\"Message\"] textarea[placeholder=\"Описание сообщества\"]"));
    }

    protected void SelectCommunityNameField(out IWebElement field)
    {
        field = _Driver.FindElement(By.CssSelector("label[data-tid=\"Name\"] textarea[placeholder=\"Название сообщества\"]"));
    }

    protected void await(Func<IWebDriver, bool> condition, int secondsToWait = 3)
    {
        var wait = new WebDriverWait(_Driver, TimeSpan.FromSeconds(secondsToWait));
        wait.Until(condition);
    }
}

[TestFixture]
public class SeleniumTestsPractice : SeleniumTests
{
    [SetUp]
    public void PracticeSetup()
    {
        ConfigureDriverParams(new Size(1280, 720));
        SetUpDriver();
        Authorize();
    }

    [Test]
    public void AuthorizationNavigationTest()
    {
        Assert.That(_Driver.Title, Is.EqualTo("Новости"), message: "Навигация после авторизации не соответствует странице \"Новости\"");
    }

    [Test]
    public void CommunitiesNavigationTest()
    {
        HeadToCommunities();

        Assert.That(_Driver.Title, Is.EqualTo("Сообщества"), message: "Навигация в раздел \"Сообщества\" из сайдбара некорректна");
    }
    
    [TestCase("")]
    [TestCase("Самый обычный текст для проверки")]
    [TestCase("Это не совсем уж и огромный текст, но, на самом деле, его достаточно для проверки, ведь он чуть больше чем 100 символов!")]
    public void CommunityNameFieldInsertionTest(string name)
    {
        HeadToCommunities();
        OpenCommunityCreationModal();
        SelectCommunityNameField(out var nameField);
        nameField.SendKeys(name);

        Assert.That(nameField.Text.Count(), Is.Not.GreaterThan(100));
    }

    [TestCase("CreateCommunityTest", "CreateCommunityTest")]
    public void CreateCommunityTest(string name, string message)
    {
        HeadToCommunities();
        var countPre = GetCommunitiesCount();

        OpenCommunityCreationModal();
        SelectCommunityNameField(out var nameField);
        nameField.SendKeys(name);

        SelectCommunityMessageField(out var messageField);
        messageField.SendKeys(message);

        _Driver.FindElement(By.CssSelector("[data-tid=\"CreateButton\"] button")).Click();

        HeadToCommunities();
        var countAfter = GetCommunitiesCount();

        Assert.That(countPre, Is.Not.EqualTo(countAfter), message: "Количество сообществ не изменилось. Новое сообщество не создано.");
    }
}
