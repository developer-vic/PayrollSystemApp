using System.Windows.Input;

namespace PayrollSystemApp.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        BindingContext = new LoginVM();
    }
    class LoginVM : BaseViewModel
    {
        private string emailAddress = "";
        private string password = "";
        private bool passwordInputType = true;
        private string passwordToggleImage = "eye";
        private bool showLoading;

        public string EmailAddress { get => emailAddress; set { SetProperty(ref emailAddress, value); } }
        public string Password { get => password; set { SetProperty(ref password, value); } }

        public string PasswordToggleImage { get => passwordToggleImage; set { SetProperty(ref passwordToggleImage, value); } }
        public bool PasswordInputType { get => passwordInputType; set { SetProperty(ref passwordInputType, value); } }
        public bool ShowLoading { get => showLoading; set { SetProperty(ref showLoading, value); } }

        public ICommand? MyCommand { get; protected set; }

        public LoginVM()
        {
            InitializeCommand();
        }

        private void InitializeCommand()
        {
            MyCommand = new Command<string>((string par) =>
            {
                switch (par)
                {
                    case "sign_up":
                        VUtils.GetoPage(new RegisterPage(), true);
                        break;
                    case "sign_in":
                        ValidateContiueClicked();
                        break;
                    case "toggle_pw":
                        ShowHidePassword();
                        break;
                }
            });
        }

        private void ShowHidePassword()
        {
            PasswordInputType = !PasswordInputType;
            PasswordToggleImage = PasswordInputType ? "eye" : "eyeclosed";
        }


        private async void ValidateContiueClicked()
        {
            if (string.IsNullOrEmpty(EmailAddress) || string.IsNullOrEmpty(Password))
                VUtils.ToastText("Pls fill in all fields");
            else if (!VUtils.IsValidEmailFormat(EmailAddress))
                VUtils.ToastText("Incorrect Email Format");
            else
            {
                ShowLoading = true;
                bool regSuccess = await VUtils.LoginUser(EmailAddress, Password);
                if (regSuccess) VUtils.GetoPage(new MainViews.DashboardPage(), true);
                ShowLoading = false;
            }
        }
    }
}