using PayrollSystemApp.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PayrollSystemApp.Views.MainViews;

public partial class PayrollListPage : ContentPage
{
    PayrollListVM vM;
    public PayrollListPage()
    {
        InitializeComponent();
        vM = new PayrollListVM();
        BindingContext = vM;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        vM.ShowLoading = true;
    }

    private void TapGestureRecognizer_Tapped(object sender, EventArgs e)
    { 
        PayrollModel? SelectedItem = (sender as ImageButton)?.BindingContext as PayrollModel;
        VUtils.GetoPage(new PayrollAddEditPage(SelectedItem ?? new PayrollModel(), vM._Employees, true));
    }
    private void EditImageButton_Clicked(object sender, EventArgs e)
    {
        PayrollModel? SelectedItem = (sender as ImageButton)?.BindingContext as PayrollModel;
        VUtils.GetoPage(new PayrollAddEditPage(SelectedItem ?? new PayrollModel(),  vM._Employees, false));
    }
    private void DeleteImageButton_Clicked(object sender, EventArgs e)
    {
        PayrollModel? SelectedItem = (sender as ImageButton)?.BindingContext as PayrollModel;
        vM.DeletePayroll(SelectedItem);
    }

    internal class PayrollListVM : BaseViewModel
    {
        private List<PayrollModel> FullPayrollList = [];
        private ObservableCollection<PayrollModel> employeeList = [];
        private PayrollModel? selectedPayroll;
        private string totalPayroll = "Total: 0";
        private bool showLoading;
        private string searchTitle = "";

        public List<UserModel> _Employees = new List<UserModel>(); //to send to add/edit 
        public ICommand? MyCommand { get; set; } 
        public string TotalPayroll { get => totalPayroll; set { SetProperty(ref totalPayroll, value); } }
        public ObservableCollection<PayrollModel> PayrollList { get => employeeList; set { SetProperty(ref employeeList, value); } }
        public PayrollModel? SelectedPayroll { get => selectedPayroll; set { SetProperty(ref selectedPayroll, value); OnPayrollSelected(value); } }
        private void OnPayrollSelected(PayrollModel? value) { if (value != null) VUtils.GetoPage(new PayrollAddEditPage(value, _Employees, true)); }
        public bool ShowLoading { get => showLoading; set { SetProperty(ref showLoading, value); if (value) InitializeData(); } }
        public string SearchTitle { get => searchTitle; set { SetProperty(ref searchTitle, value); OnSearchTitle(value); } }

        private void OnSearchTitle(string value)
        {
            if (!string.IsNullOrEmpty(value))
                PayrollList = new ObservableCollection<PayrollModel>(FullPayrollList.Where(p => p.MonthYear.ToLower().Contains(searchTitle.ToLower())));
            else PayrollList = new ObservableCollection<PayrollModel>(FullPayrollList);
        }

        public PayrollListVM()
        {
            RunCommands();
        } 
        private async void InitializeData()
        {
            _Employees = await VUtils.GetEmployeeList();
            FullPayrollList = await VUtils.GetAllPayrollList();
            PayrollList = new ObservableCollection<PayrollModel>(FullPayrollList);
            TotalPayroll = "Total: " + FullPayrollList.Count(); ShowLoading = false;
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
                    case "add":
                        if (_Employees.Count == 0)
                            VUtils.ToastText("Pls Add Employee First");
                        else
                            VUtils.GetoPage(new PayrollAddEditPage(new PayrollModel(), _Employees));
                        break;
                }
            });
        }

        internal async void DeletePayroll(PayrollModel? delPayroll)
        {
            if (delPayroll == null || Application.Current?.MainPage == null) return;
            bool canDel = await Application.Current.MainPage.DisplayAlert("Delete Confirmation", "Are you sure you want to delete?", "YES, DELETE", "NO, CLOSE");
            if (canDel)
            {
                await VUtils.DeletePayroll(delPayroll); ShowLoading = true;
            }
        }
    }

}