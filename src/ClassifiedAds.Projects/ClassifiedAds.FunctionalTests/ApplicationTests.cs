using ClassifiedAds.FunctionalTests.Navigation;
using System.Threading;
using Xunit;

namespace ClassifiedAds.FunctionalTests
{
    public class ApplicationTests : TestBase
    {
        [Fact]
        public void HomePage()
        {
            var navigator = new Navigator(Driver);

            var loginPage = navigator.LoginPage();
            loginPage.UserName = Configuration.Login_UserName;
            loginPage.Password = Configuration.Login_Password;
            loginPage.RememberMyLogin = true;
            Thread.Sleep(2000);

            var consentPage = loginPage.Login();
            Thread.Sleep(2000);
            consentPage.RememberMyDecision = false;
            Thread.Sleep(2000);
            consentPage.Confirm();
            Thread.Sleep(2000);

            navigator.HomePage();
            Thread.Sleep(2000);

            navigator.PrivacyPage();
            Thread.Sleep(2000);

            navigator.AuthorizedActionPage();
            Thread.Sleep(2000);

            navigator.Logout();
            Thread.Sleep(2000);
        }

    }
}
