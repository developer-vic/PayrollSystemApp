using PayrollSystemApp.Controls;
using System.Net.Mail;
using System.Windows.Input;
using static Android.Provider.ContactsContract.CommonDataKinds;

namespace PayrollSystemApp.Views.MainViews;

public partial class EmployeeAddEditPage : ContentPage
{
	public EmployeeAddEditPage(UserModel employee, bool justView = false)
	{
		InitializeComponent();
        BindingContext = new EmployeeAddEditVM(employee, justView);
    }

    private class EmployeeAddEditVM : BaseViewModel
    {
        private UserModel employee = new UserModel();
        private bool showLoading;
        private bool passwordInputType = true;
        private string passwordToggleImage = "eye"; 

        public UserModel Employee { get => employee; set { SetProperty(ref employee, value); } } 
        public bool ShowLoading { get => showLoading; set { SetProperty(ref showLoading, value); } }

        public bool PasswordInputType { get => passwordInputType; set { SetProperty(ref passwordInputType, value); } }
        public string PasswordToggleImage { get => passwordToggleImage; set { SetProperty(ref passwordToggleImage, value); } }
        public bool EmailEnabled { get; set; } = true;
        public bool FieldsEnabled { get; set; } = true;
        public string PageTitle { get; set; } = "Employee";
        public ICommand? MyCommand { get; protected set; }

        public EmployeeAddEditVM(UserModel employee, bool justView = false)
        {
            Employee = employee ?? new UserModel();
            if (!string.IsNullOrEmpty(Employee.UserId)) EmailEnabled = false;
            if (justView) { FieldsEnabled = false; PageTitle = "View Employee"; }
            else PageTitle = string.IsNullOrEmpty(Employee?.UserId) ? "Add Employee" : "Edit Employee";

            MyCommand = new Command<string>((string par) =>
            {
                switch (par)
                {
                    case "save":
                        SaveEmployee();
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

        private async void SaveEmployee()
        {
            UserModel savingUser = Employee;
            if (string.IsNullOrEmpty(savingUser.Email) || string.IsNullOrEmpty(savingUser.Password) 
               || string.IsNullOrEmpty(savingUser.FullName) || string.IsNullOrEmpty(savingUser.Phone) 
               || string.IsNullOrEmpty(savingUser.Position)) VUtils.ToastText("Pls fill in all fields");
            else if (!VUtils.IsValidEmailFormat(savingUser.Email)) VUtils.ToastText("Incorrect Email Format");
            else if (savingUser.BasicSalary == 0) VUtils.ToastText("Incorrect Salary Format"); 
            else
            {
                ShowLoading = true; bool isNew = string.IsNullOrEmpty(savingUser.UserId);
                if (isNew)
                {
                    savingUser.UserType = "Employee"; savingUser.UserId = VUtils.GetTransactionRef();
                    savingUser.Organization = VUtils.LoggedInUser.Organization;
                } 
                bool regSuccess = await VUtils.RegisterUser(savingUser, isNew, false);
                if (regSuccess) VUtils.GoBack();
                ShowLoading = false;
            }
        }
    }

}