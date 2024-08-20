using PayrollSystemApp.Controls;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace PayrollSystemApp.Views.MainViews;

public partial class EmployeeListPage : ContentPage
{
    EmployeeListVM vM;
    public EmployeeListPage()
    {
        InitializeComponent();
        vM = new EmployeeListVM();
        BindingContext = vM;
    }
    protected override void OnAppearing()
    {
        base.OnAppearing();
        vM.ShowLoading = true;
    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {
        colEmployeeList.SelectedItem = (sender as HorizontalStackLayout)?.BindingContext;
        colEmployeeList.SelectedItem = null;
    }
    private void EditImageButton_Clicked(object sender, EventArgs e)
    {
        UserModel? SelectedItem = (sender as ImageButton)?.BindingContext as UserModel;
        VUtils.GetoPage(new EmployeeAddEditPage(SelectedItem ?? new UserModel()));
    }
    private void DeleteImageButton_Clicked(object sender, EventArgs e)
    {
        UserModel? SelectedItem = (sender as ImageButton)?.BindingContext as UserModel;
        vM.DeleteEmployee(SelectedItem);
    }

    internal class EmployeeListVM : BaseViewModel
    {
        private List<UserModel> FullEmployeeList = new List<UserModel>();
        private ObservableCollection<UserModel> employeeList = new ObservableCollection<UserModel>();
        private UserModel? selectedEmployee;
        private string totalEmployee = "Total: 0"; 
        private bool showLoading;
        private string searchTitle = "";

        public ICommand? MyCommand { get; set; }  
        public string TotalEmployee { get => totalEmployee; set { SetProperty(ref totalEmployee, value); } }
        public ObservableCollection<UserModel> EmployeeList { get => employeeList; set { SetProperty(ref employeeList, value); } }
        public UserModel? SelectedEmployee { get => selectedEmployee; set { SetProperty(ref selectedEmployee, value); OnEmployeeSelected(value); } }
        private void OnEmployeeSelected(UserModel? value) { if (value != null) VUtils.GetoPage(new EmployeeAddEditPage(value, true)); }
        public bool ShowLoading { get => showLoading; set { SetProperty(ref showLoading, value); if (value) InitializeData(); } }
        public string SearchTitle { get => searchTitle; set { SetProperty(ref searchTitle, value); OnSearchTitle(value); } }

        private void OnSearchTitle(string value)
        {
            if (!string.IsNullOrEmpty(value))
                EmployeeList = new ObservableCollection<UserModel>(FullEmployeeList.Where(p => p.FullName.ToLower().Contains(searchTitle.ToLower())));
            else EmployeeList = new ObservableCollection<UserModel>(FullEmployeeList);
        }

        public EmployeeListVM()
        {
            RunCommands();
        }

        private async void InitializeData()
        { 
            FullEmployeeList = await VUtils.GetEmployeeList();
            EmployeeList = new ObservableCollection<UserModel>(FullEmployeeList);
            TotalEmployee = "Total: " + FullEmployeeList.Count(); ShowLoading = false;
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
                        VUtils.GetoPage(new EmployeeAddEditPage(new UserModel()));
                        break;  
                }
            });
        }
         
        internal async void DeleteEmployee(UserModel? delEmployee)
        {
            if (delEmployee == null || Application.Current?.MainPage == null) return;
            bool canDel = await Application.Current.MainPage.DisplayAlert("Delete Confirmation", "Are you sure you want to delete?", "YES, DELETE", "NO, CLOSE");
            if (canDel)
            {
                await VUtils.DeleteEmployee(delEmployee);
                ShowLoading = true;
            }
        }
    }

}