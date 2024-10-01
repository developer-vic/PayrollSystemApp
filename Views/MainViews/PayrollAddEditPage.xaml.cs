using PayrollSystemApp.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PayrollSystemApp.Views.MainViews;

public partial class PayrollAddEditPage : ContentPage
{
	public PayrollAddEditPage(PayrollModel payroll, List<UserModel> _Employees, bool justView = false)
    {
		InitializeComponent();
        BindingContext = new PayrollAddEditVM(payroll, _Employees, justView);
    }
    private void OnMonthSelected(object sender, EventArgs e)
    {
        if (BindingContext is PayrollAddEditVM viewModel && MonthPicker.SelectedIndex != -1)
        {
            viewModel.SelectedMonth = MonthPicker.Items[MonthPicker.SelectedIndex];
        }
    }

    private void OnYearSelected(object sender, EventArgs e)
    {
        if (BindingContext is PayrollAddEditVM viewModel && YearPicker.SelectedIndex != -1)
        { 
            viewModel.SelectedYear = int.Parse(YearPicker.Items[YearPicker.SelectedIndex]);
        }
    }
    private class PayrollAddEditVM : BaseViewModel
    {
        private PayrollModel payroll = new(); 
        private bool showLoading;
        private string _selectedMonth = "";
        private int _selectedYear = 0;
        private bool fieldsEnabled = true;

        public PayrollModel Payroll { get => payroll; set { SetProperty(ref payroll, value); } }
        public bool ShowLoading { get => showLoading; set { SetProperty(ref showLoading, value); } }
        public ObservableCollection<UserModel> Employees { get; set; } = []; 

        public bool FieldsEnabled { get => fieldsEnabled; set { SetProperty(ref fieldsEnabled, value); } }
        public string PageTitle { get; set; } = "Payroll";
        public ICommand? MyCommand { get; protected set; }
        public ICommand? PickerCommand { get; protected set; }

        public ObservableCollection<string> Months { get; set; }
        public ObservableCollection<int> Years { get; set; }
        public string SelectedMonth { get => _selectedMonth; set { SetProperty(ref _selectedMonth, value); FilterPayrolls(); } }
        public int SelectedYear { get => _selectedYear; set { SetProperty(ref _selectedYear, value); FilterPayrolls(); } }  
        public ObservableCollection<UserModel> FilteredPayrolls { get; set; }
        List<PayrollModel> AllPayrollList = [];

        public PayrollAddEditVM(PayrollModel payroll, List<UserModel> _Employees, bool justView = false)
        {
            FilteredPayrolls = new ObservableCollection<UserModel>();
            Payroll = payroll ?? new PayrollModel();
            Employees = new ObservableCollection<UserModel>(_Employees); 

            if (justView)
            {
                FieldsEnabled = false; PageTitle = "View Payroll";
                SelectedMonth = Payroll.Month; SelectedYear = Payroll.Year; 
            } 
            else PageTitle = string.IsNullOrEmpty(Payroll?.payrollId) ? "Add Payroll" : "Edit Payroll";
             
            Months = new ObservableCollection<string> { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
            Years = new ObservableCollection<int> { 2024, 2025, 2026, 2027, 2028, 2029, 2030 }; 

            RunCommands();//have to be end to trigger after monthyear default selection 
        }

        private void FilterPayrolls()
        {
            FilteredPayrolls.Clear();
            if (!string.IsNullOrEmpty(SelectedMonth) && SelectedYear > 0)
            {
                var filtered = AllPayrollList.Where(p=>p.Month == SelectedMonth && p.Year == SelectedYear).FirstOrDefault(); 
                if (filtered != null)
                {  
                    FieldsEnabled = false; 
                    foreach (var item in filtered.Employee) 
                        FilteredPayrolls.Add(item); 
                } else FieldsEnabled = true;
            } 
        }
         
        private async void RunCommands()
        {
            MyCommand = new Command<string>((string par) =>
            {
                switch (par)
                {
                    case "save":
                        SavePayroll();
                        break;
                }
            });
            PickerCommand = new Command<Picker>((Picker picker) =>
            {
                picker.Focus();
            });
            AllPayrollList = await VUtils.GetAllPayrollList();
            if (!string.IsNullOrEmpty(SelectedMonth) && SelectedYear > 0)
                FilterPayrolls();
        }

        private async void SavePayroll()
        {
            if (string.IsNullOrEmpty(SelectedMonth) || SelectedYear == 0)
                VUtils.ToastText("Pls select payroll month and year"); 
            else
            {
                ShowLoading = true; PayrollModel savingPayroll = Payroll;
                if (string.IsNullOrEmpty(savingPayroll.payrollId))
                {
                    savingPayroll.payrollId = VUtils.GetTransactionRef();
                    savingPayroll.Organization = VUtils.LoggedInUser.Organization;
                    savingPayroll.Employee = await VUtils.GetEmployeeList();
                    savingPayroll.Month = SelectedMonth; savingPayroll.Year = SelectedYear;
                }
                bool regSuccess = await VUtils.AddUpdatePayroll(savingPayroll);
                if (regSuccess) VUtils.GoBack(); ShowLoading = false;
            }
        }
    }

}