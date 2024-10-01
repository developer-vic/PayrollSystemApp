using PayrollSystemApp.Controls; 
using System.Windows.Input;

namespace PayrollSystemApp.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
        BindingContext = new RegisterVM();
    }
    class RegisterVM : BaseViewModel
    {
        private bool showLoading;
        private bool passwordInputType = true;
        private string passwordToggleImage = "eye";   
        public string SelectedAccType = "Employer";
         
        public string? UserFullName { get; set; }
        public string? Organization { get; set; }
        public string? EmailAddress { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Password { get; set; }
        public bool PasswordInputType { get => passwordInputType; set { SetProperty(ref passwordInputType, value); } }
        public string PasswordToggleImage { get => passwordToggleImage; set { SetProperty(ref passwordToggleImage, value); } }
       public bool ShowLoading { get => showLoading; set { SetProperty(ref showLoading, value); } }

        public ICommand? MyCommand { get; protected set; }
        public ICommand? PickerCommand { get; protected set; }

        public RegisterVM()
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
                        ValidateContiueClicked();
                        break;
                    case "sign_in":
                        VUtils.GetoPage(new LoginPage(), true);
                        break;
                    case "toggle_pw":
                        ShowHidePassword();
                        break;
                }
            });
            PickerCommand = new Command<Picker>((Picker picker) =>
            {
                picker.Focus();
            });
        }

        private void ShowHidePassword()
        {
            PasswordInputType = !PasswordInputType;
            PasswordToggleImage = PasswordInputType ? "eye" : "eyeclosed";
        }


        private async void ValidateContiueClicked()
        {
            if (string.IsNullOrEmpty(EmailAddress) || string.IsNullOrEmpty(Password) || string.IsNullOrEmpty(UserFullName)
                 || string.IsNullOrEmpty(SelectedAccType) || string.IsNullOrEmpty(PhoneNumber) || string.IsNullOrEmpty(Organization))
                VUtils.ToastText("Pls fill in all fields");
            else if (!VUtils.IsValidEmailFormat(EmailAddress))
                VUtils.ToastText("Incorrect Email Format");
            else
            {
                ShowLoading = true;
                UserModel newUser = new UserModel()
                {
                    Email = EmailAddress,
                    Password = Password,
                    FullName = UserFullName,
                    Phone = PhoneNumber,
                    Organization = Organization,
                    UserType = SelectedAccType,
                    UserId = VUtils.GetTransactionRef()
                };
                bool regSuccess = await VUtils.RegisterUser(newUser, true, true);
                if (regSuccess) VUtils.GetoPage(new MainViews.DashboardPage(), true);
                ShowLoading = false;
            }
        }

    }
}