using Newtonsoft.Json;
using PayrollSystemApp.Controls;

namespace PayrollSystemApp
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            MainPage = new Views.WelcomePage();

            //string admin = "{\"UserId\":\"20240820043551095\",\"UserType\":\"Employer\",\"FullName\":\"Vic\",\"Email\":\"babsgodwin @gmail.com\",\"Phone\":\"0809656\",\"Password\":\"vic\",\"Organization\":\"Gwin\",\"isAdmin\":false}";
            //string user = "{\"UserId\":\"20240820065221004\",\"UserType\":\"Employee\",\"FullName\":\"v9wc\",\"Email\":\"godwincomputerplanet@gmail.com\",\"Phone\":\"6688\",\"Password\":\"vic\",\"Organization\":\"Gwin\",\"isAdmin\":false,\"Position\":\"vu\"}";
            //VUtils.LoggedInUser = JsonConvert.DeserializeObject<UserModel>(user); MainPage = new NavigationPage(new Views.MainViews.DashboardPage());
        }
    }
}
