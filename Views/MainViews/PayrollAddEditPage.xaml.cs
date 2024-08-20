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

    private class PayrollAddEditVM : BaseViewModel
    {
        private PayrollModel payroll = new();
        private UserModel? selectedEmployee;
        private bool showLoading;

        public PayrollModel Payroll { get => payroll; set { SetProperty(ref payroll, value); } }
        public bool ShowLoading { get => showLoading; set { SetProperty(ref showLoading, value); } }
        public ObservableCollection<UserModel> Employees { get; set; } = [];
        public UserModel? SelectedEmployee { get => selectedEmployee; set { SetProperty(ref selectedEmployee, value); } }

        public bool FieldsEnabled { get; set; } = true;
        public string PageTitle { get; set; } = "Payroll";
        public ICommand? MyCommand { get; protected set; }
        public ICommand? PickerCommand { get; protected set; }

        public PayrollAddEditVM(PayrollModel payroll, List<UserModel> _Employees, bool justView = false)
        {
            Payroll = payroll ?? new PayrollModel(); RunCommands();
            Employees = new ObservableCollection<UserModel>(_Employees);
            SelectedEmployee = Employees.FirstOrDefault(e => e.UserId == Payroll?.Employee?.UserId); 

            if (justView) { FieldsEnabled = false; PageTitle = "View Payroll"; } 
            else PageTitle = string.IsNullOrEmpty(Payroll?.payrollId) ? "Add Payroll" : "Edit Payroll"; 
        }

        private void RunCommands()
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
        }

        private async void SavePayroll()
        {
            PayrollModel savingPayroll = Payroll;
            if (SelectedEmployee == null || string.IsNullOrEmpty(SelectedEmployee.UserId)) 
                VUtils.ToastText("Pls select payroll"); 
            else if (savingPayroll.BasicSalary == 0) VUtils.ToastText("Incorrect Salary Format");
            else
            {
                ShowLoading = true;
                if (string.IsNullOrEmpty(savingPayroll.payrollId))
                {
                    savingPayroll.payrollId = VUtils.GetTransactionRef();
                    savingPayroll.Organization = VUtils.LoggedInUser.Organization;
                    Payroll.Employee = SelectedEmployee;
                }
                bool regSuccess = await VUtils.AddUpdatePayroll(savingPayroll);
                if (regSuccess) VUtils.GoBack(); ShowLoading = false;
            }
        }
    }

}