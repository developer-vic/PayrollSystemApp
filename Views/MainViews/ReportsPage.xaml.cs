using Org.Apache.Http.Authentication;
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

    private void OnMonthSelected(object sender, EventArgs e)
    {
        if (BindingContext is ReportsVM viewModel && MonthPicker.SelectedIndex != -1)
        {
            viewModel.SelectedMonth = MonthPicker.Items[MonthPicker.SelectedIndex];
        }
    }

    private void OnYearSelected(object sender, EventArgs e)
    {
        if (BindingContext is ReportsVM viewModel && YearPicker.SelectedIndex != -1)
        {
            viewModel.SelectedYear = int.Parse(YearPicker.Items[YearPicker.SelectedIndex]);
        }
    }

    internal class ReportsVM : BaseViewModel
    {
        private ObservableCollection<UserModel> employeeList = [];
        private PayrollModel? selectedPayroll;
        private string totalPayroll = "Total: 0";
        private string totalSalary = "Total: 0";
        private string totalExtra = "Total: 0";
        private string totalDeduction = "Total: 0";
        private string totalNetSalary = "Total: 0";
        private bool showLoading;
        private string _selectedMonth = DateTime.Now.ToString("MMMM");
        private int _selectedYear = 2024;

        public List<UserModel> _Employees = new List<UserModel>(); //to send to add/edit 
        public ICommand? MyCommand { get; set; }
        public string TotalPayroll { get => totalPayroll; set { SetProperty(ref totalPayroll, value); } }
        public string TotalSalary { get => totalSalary; set { SetProperty(ref totalSalary, value); } }
        public string TotalExtra { get => totalExtra; set { SetProperty(ref totalExtra, value); } }
        public string TotalDeduction { get => totalDeduction; set { SetProperty(ref totalDeduction, value); } }
        public string TotalNetSalary { get => totalNetSalary; set { SetProperty(ref totalNetSalary, value); } }
        public ObservableCollection<UserModel> Reports { get => employeeList; set { SetProperty(ref employeeList, value); } }
        //public PayrollModel? SelectedPayroll { get => selectedPayroll; set { SetProperty(ref selectedPayroll, value); OnPayrollSelected(value); } }
        private void OnPayrollSelected(PayrollModel? value) { if (value != null) VUtils.GetoPage(new PayrollAddEditPage(value, _Employees, true)); }
        public bool ShowLoading { get => showLoading; set { SetProperty(ref showLoading, value); } }

        public ObservableCollection<string> Months { get; set; }
        public ObservableCollection<int> Years { get; set; }
        public string SelectedMonth { get => _selectedMonth; set { SetProperty(ref _selectedMonth, value); } }
        public int SelectedYear { get => _selectedYear; set { SetProperty(ref _selectedYear, value); } }
        List<PayrollModel> AllPayrollList = [];

        public ReportsVM()
        {
            RunCommands(); InitializeData();
            Months = new ObservableCollection<string> { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
            Years = new ObservableCollection<int> { 2024, 2025, 2026, 2027, 2028, 2029, 2030 };  
        }
        private async void InitializeData()
        {
            _Employees = await VUtils.GetEmployeeList();
            AllPayrollList = await VUtils.GetAllPayrollList();
        }

        private void RunCommands()
        {
            MyCommand = new Command<string>((string par) =>
            {
                switch (par)
                {
                    case "generate":
                        GenerateReport();
                        break;
                    case "print":
                        PrintReport();
                        break;
                }
            });
        }

        private void GenerateReport()
        {
            if (string.IsNullOrEmpty(SelectedMonth) || SelectedYear == 0)
            {
                VUtils.ToastText("Pls select payroll month and year");
                return;
            }

            List<UserModel> ReportsLoaded = new List<UserModel>(); 
            var reportRes = AllPayrollList.Where(p => p.Month == SelectedMonth && p.Year == SelectedYear).FirstOrDefault();
            if (reportRes != null)
            {
                ReportsLoaded = reportRes.Employee;
                if (!VUtils.LoggedInUser.isAdmin)
                    ReportsLoaded = ReportsLoaded.Where(p => p.UserId == VUtils.LoggedInUser.UserId).ToList();
            }

            Reports = new ObservableCollection<UserModel>(ReportsLoaded);
            TotalPayroll = "Total Payroll Report: " + ReportsLoaded.Count();
            TotalSalary = "Total Salary: " + ReportsLoaded.Sum(p => p.BasicSalary)?.ToString("N2");
            TotalExtra = "Total Extras: " + ReportsLoaded.Sum(p => p.Extras)?.ToString("N2");
            TotalDeduction = "Total Deduction: " + ReportsLoaded.Sum(p => p.Deductions)?.ToString("N2");
            TotalNetSalary = "Total Net Salary: " + ReportsLoaded.Sum(p => p.NetSalary)?.ToString("N2"); 
        }

        public async void PrintReport()
        {
            try
            {
                string htmlContent = GenerateHtmlContent();
                var filePath = Path.Combine(FileSystem.Current.AppDataDirectory, "payroll_report.html");
                File.WriteAllText(filePath, htmlContent);

                await Launcher.Default.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(filePath)
                }); 
            }
            catch (Exception ex)
            {
                VUtils.ToastText(ex.Message);
            }
        }

        public string GenerateHtmlContent()
        { 
            string htmlContent = $@"
    <html>
    <head>
        <title>Payroll Report</title>
        <style>
            body {{ font-family: Arial, sans-serif; margin: 20px; }}
            h1, h2 {{ text-align: center; color: #37256A; }}
            table {{ width: 100%; border-collapse: collapse; margin: 20px 0; }}
            th, td {{ border: 1px solid #ddd; padding: 8px; text-align: left; }}
            th {{ background-color: #6E4AD3; color: white; }}
            tfoot td {{ font-weight: bold; }}
            .footer {{ text-align: right; margin-top: 20px; }}
        </style>
    </head>
    <body>
        <h1>Payroll Report</h1>
        <h2>{SelectedMonth} {SelectedYear}</h2>
        <table>
            <thead>
                <tr>
                    <th>Full Name</th>
                    <th>Position</th>
                    <th>Basic Salary</th>
                    <th>Extras</th>
                    <th>Deductions</th>
                    <th>Net Salary</th>
                </tr>
            </thead>
            <tbody>";
             
            foreach (var report in Reports)
            {
                htmlContent += $@"
                <tr>
                    <td>{report.FullName}</td>
                    <td>{report.Position}</td>
                    <td>{report.BasicSalary:N2}</td>
                    <td>{report.Extras:N2}</td>
                    <td>{report.Deductions:N2}</td>
                    <td>{report.NetSalary:N2}</td>
                </tr>";
            }

            // Footer Section
            var totalSalary = Reports.Sum(r => r.BasicSalary);
            var totalExtras = Reports.Sum(r => r.Extras);
            var totalDeductions = Reports.Sum(r => r.Deductions);
            var totalNetSalary = Reports.Sum(r => r.NetSalary);

            htmlContent += $@"
            </tbody>
            <tfoot>
                <tr>
                    <td colspan='2'>Totals</td>
                    <td>{totalSalary:N2}</td>
                    <td>{totalExtras:N2}</td>
                    <td>{totalDeductions:N2}</td>
                    <td>{totalNetSalary:N2}</td>
                </tr>
            </tfoot>
        </table>
        <div class='footer'>
            <p>Total Salary: {totalSalary:N2}</p>
            <p>Total Extras: {totalExtras:N2}</p>
            <p>Total Deductions: {totalDeductions:N2}</p>
            <p>Total Net Salary: {totalNetSalary:N2}</p>
        </div>
    <script>
        window.onload = function() {{ window.print(); }};
    </script>
    </body>
    </html>"; 
            return htmlContent;
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