using PayrollSystemApp.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input; 


namespace PayrollSystemApp.Views.MainViews;

public partial class ReportsPage : ContentPage
{ 
    ReportsVM vM;
    public ReportsPage()
    {
        InitializeComponent();
        vM = new ReportsVM();
        BindingContext = vM;
    } 

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        colReports.SelectedItem = (sender as Grid)?.BindingContext;
        colReports.SelectedItem = null;
    } 

    internal class ReportsVM : BaseViewModel
    { 
        private ObservableCollection<PayrollModel> employeeList = [];
        private PayrollModel? selectedPayroll;
        private string totalPayroll = "Total: 0";
        private string totalSalary = "Total: 0";
        private string totalExtra = "Total: 0";
        private string totalDeduction = "Total: 0";
        private string totalNetSalary = "Total: 0";
        private bool showLoading; 

        public List<UserModel> _Employees = new List<UserModel>(); //to send to add/edit 
        public ICommand? MyCommand { get; set; }
        public string TotalPayroll { get => totalPayroll; set { SetProperty(ref totalPayroll, value); } }
        public string TotalSalary { get => totalSalary; set { SetProperty(ref totalSalary, value); } }
        public string TotalExtra { get => totalExtra; set { SetProperty(ref totalExtra, value); } }
        public string TotalDeduction { get => totalDeduction; set { SetProperty(ref totalDeduction, value); } }
        public string TotalNetSalary { get => totalNetSalary; set { SetProperty(ref totalNetSalary, value); } }
        public ObservableCollection<PayrollModel> Reports { get => employeeList; set { SetProperty(ref employeeList, value); } }
        public PayrollModel? SelectedPayroll { get => selectedPayroll; set { SetProperty(ref selectedPayroll, value); OnPayrollSelected(value); } }
        private void OnPayrollSelected(PayrollModel? value) { if (value != null) VUtils.GetoPage(new PayrollAddEditPage(value, _Employees, true)); }
        public bool ShowLoading { get => showLoading; set { SetProperty(ref showLoading, value); if (value) InitializeData(); } }
       
        public ReportsVM()
        {
            RunCommands(); ShowLoading = true;
        }
        private async void InitializeData()
        {
            _Employees = await VUtils.GetEmployeeList();
            GenerateReport();
        }

        private void RunCommands()
        {
            MyCommand = new Command<string>((string par) =>
            {
                switch (par)
                {
                    case "reload":
                        InitializeData();
                        break;
                    case "download": 
                        DownloadReport();
                        break;
                }
            });
        } 
        private async void GenerateReport()
        {
            var ReportsLoaded = await VUtils.GetPayrollList();
            if(!VUtils.LoggedInUser.isAdmin)
                ReportsLoaded=ReportsLoaded.Where(p=>p.Employee.UserId==VUtils.LoggedInUser.UserId).ToList();
            Reports = new ObservableCollection<PayrollModel>(ReportsLoaded);

            TotalPayroll = "Total Payroll Report: " + ReportsLoaded.Count();
            TotalSalary = "Total Salary: " + ReportsLoaded.Sum(p => p.BasicSalary).ToString("N2");
            TotalExtra = "Total Extras: " + ReportsLoaded.Sum(p => p.Extras).ToString("N2");
            TotalDeduction = "Total Deduction: " + ReportsLoaded.Sum(p => p.Deductions).ToString("N2");
            TotalNetSalary = "Total Net Salary: " + ReportsLoaded.Sum(p => p.NetSalary).ToString("N2");

            ShowLoading = false;
        }

        public async void DownloadReport()
        {
            try
            {
                var filePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PayrollReport_All.pdf");
                 
                //GeneratePdf(filePath);
                 
                //await ShareFile(filePath);
            }
            catch (Exception ex)
            {
                VUtils.ToastText(ex.Message);
            }
        } 

        private async Task ShareFile(string filePath)
        {
            if (File.Exists(filePath))
            {
                await Share.Default.RequestAsync(new ShareFileRequest
                {
                    Title = "Share Payroll Report",
                    File = new ShareFile(filePath)
                });
            } 
        }

    } 
}