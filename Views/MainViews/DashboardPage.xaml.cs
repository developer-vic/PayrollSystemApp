using PayrollSystemApp.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PayrollSystemApp.Views.MainViews;

public partial class DashboardPage : ContentPage
{
    DashboardVM vM;
    public DashboardPage()
    {
        InitializeComponent();
        vM = new DashboardVM();
        BindingContext = vM;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        vM.ShowLoading = true;
    }
      
    internal class DashboardVM : BaseViewModel
    { 
        private int employeesTotal;
        private int payrollsTotal; 
        private bool showLoading;
        List<PayrollModel> _PayrollList = new List<PayrollModel>();
        List<UserModel> _UserList = new List<UserModel>();
         
        public ICommand? MyCommand { get; set; }
        public string? UserFullName { get; set; } = VUtils.LoggedInUser?.FullName;
        public string? UserEmail { get; set; } = VUtils.LoggedInUser?.Email;
        public string? UserType { get; set; } = VUtils.LoggedInUser?.UserType;
        public bool IsWriter { get; set; } = VUtils.LoggedInUser?.UserType == "Employer";
        public bool IsNotWriter { get; set; } = VUtils.LoggedInUser?.UserType != "Employer";
        public int EmployeesTotal { get => employeesTotal; set { SetProperty(ref employeesTotal, value); } }
        public int PayrollsTotal { get => payrollsTotal; set { SetProperty(ref payrollsTotal, value); } }  
        public bool ShowLoading { get => showLoading; set { SetProperty(ref showLoading, value); if (value) InitializeData(); } } 
         
        public DashboardVM()
        {
            RunCommands();
        }

        private async void InitializeData()
        {  
            _PayrollList = await VUtils.GetPayrollList();  
            PayrollsTotal = _PayrollList.Count();
            _UserList = await VUtils.GetEmployeeList();
            EmployeesTotal = _UserList.Count();
            ShowLoading = false;
        }

        private void RunCommands()
        {
            MyCommand = new Command<string>((string par) =>
            {
                switch (par)
                {
                    case "logout":
                        TryLogOut();
                        break;
                    case "reload":
                        InitializeData();
                        break;
                    case "employee":
                        VUtils.GetoPage(new EmployeeListPage()); 
                        break;
                    case "employed":
                        VUtils.GetoPage(new EmployeeAddEditPage(VUtils.LoggedInUser, true));
                        break;
                    case "payroll":
                        VUtils.GetoPage(new PayrollListPage());
                        break;
                    case "my_payroll":
                        var myPayroll = _PayrollList.Where(p => p.Employee.UserId == VUtils.LoggedInUser.UserId).FirstOrDefault();
                        if (myPayroll != null) VUtils.GetoPage(new PayrollAddEditPage(myPayroll, _UserList, true));
                        else VUtils.ToastText("No Payroll Available for You");
                        break;
                    case "reports":
                        VUtils.GetoPage(new ReportsPage()); 
                        break;
                    case "gwin":
                        Launcher.TryOpenAsync("https://programmergwin.com");
                        break;
                }
            });
        }

        private async void TryLogOut()
        {
            if (Application.Current?.MainPage == null) return;
            bool canLogout = await Application.Current.MainPage.DisplayAlert("Log Out Confirmation", "Are you sure you want to log out?", "YES, LOG OUT", "NO, CLOSE");
            if (canLogout) VUtils.LogOut();
        }
         
    }

}