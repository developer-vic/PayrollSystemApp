using PayrollSystemApp.Controls;
using PayrollSystemApp.Views;
using Newtonsoft.Json;
using System.Net.Security;
using System.Text.RegularExpressions;

namespace PayrollSystemApp
{
    public class VUtils
    {
        internal static void ToastText(string text)
        {
            ShowMessage(text);
            //await Toast.Make(text, ToastDuration.Short).Show();
        }

        public static string GetTransactionRef()
        {
            return DateTime.Now.ToString("yyyyMMddhhmmssfff");
        }

        private static Regex _regex = new Regex(
 @"^([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})(\]?)$",
 RegexOptions.CultureInvariant | RegexOptions.Singleline);

        internal static bool IsValidEmailFormat(string emailInput)
        {
            return _regex.IsMatch(emailInput);
        }

        internal async static void CopyTextToClipBoard(string payroll_decription)
        {
            try
            {
                await Clipboard.SetTextAsync(payroll_decription);
                ToastText("copied");
            }
            catch (Exception ex)
            {
                ToastText(ex.Message);
            }
        }

        internal static void GetoPage(Page contentPage, bool isMainPage = false)
        {
            if (Application.Current != null)
            {
                if (isMainPage)
                    Application.Current.MainPage = new NavigationPage(contentPage);
                else if (Application.Current.MainPage != null)
                    Application.Current.MainPage.Navigation.PushAsync(contentPage);
            }
        }
        internal static void GoBack()
        {
            if (Application.Current?.MainPage != null)
                Application.Current.MainPage.Navigation.PopAsync();
        }
        internal static void LogOut()
        { 
            if (Application.Current?.MainPage != null)
                Application.Current.MainPage = new NavigationPage(new WelcomePage());
            LoggedInUser = new UserModel();
        }
        private static void ShowMessage(string message)
        {
            if (Application.Current?.MainPage != null)
                Application.Current.MainPage.DisplayAlert("Alert", message, "OK");
        }

        internal static async Task<string> GetApiAudio(string meetingContent)
        {
            try
            {
                var client = new HttpClient(); var request = new HttpRequestMessage(HttpMethod.Post, "https://play.ht/api/transcribe");
                var content = new StringContent("{\"userId\":\"public-access\",\"platform\":\"landing_demo\",\"ssml\":\"<speak><p>" + meetingContent + "</p></speak>\",\"voice\":\"en-NG-EzinneNeural\",\"narrationStyle\":\"Neural\",\"method\":\"file\"}", null, "application/json");
                request.Content = content; var response = await client.SendAsync(request);
                response.EnsureSuccessStatusCode(); string resMraw = await response.Content.ReadAsStringAsync();
                FileModel? fileModel = JsonConvert.DeserializeObject<FileModel>(resMraw);
                return fileModel?.file ?? "";
            }
            catch (Exception)
            {
            }
            return "";
        }

        //DATABASE 
        internal static UserModel LoggedInUser = new UserModel();
        internal static readonly string USER_DB_CACHE_KEY = "PayrollSavedUserList";
        internal static readonly string PAYROLL_DB_CACHE_KEY = "PayrollSavedPayrollList"; 
        private static HttpClient GetVHttpClient()
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (sender, certificate, chain, sslPolicyErrors) =>
            {
                if (sslPolicyErrors == SslPolicyErrors.None)
                {
                    return true;
                }
                var certificateValidator = new SelfSignedCertificateValidator();
                bool isValid = false;
                if (certificate != null)
                    isValid = certificateValidator.ValidateCertificate(certificate.GetRawCertData(), "https://programmergwin.com");
                return isValid;
            };
            var mclient = new HttpClient(httpClientHandler);
            return mclient;
        }
        private async static Task<string> PostRequest(string actionname, string key, string value)
        {
            try
            {
                if (Connectivity.NetworkAccess != NetworkAccess.Internet) return "";

                return await new FirebaseClass().GetResponse(actionname, key, value);

                //using (var mclient = GetVHttpClient())
                //{
                //    string url = "https://programmergwin.com" + actionname;
                //    if (!string.IsNullOrEmpty(key)) url += "?key=" + key;
                //    if (!string.IsNullOrEmpty(value)) url += "&value=" + value;

                //    mclient.Timeout = TimeSpan.FromMinutes(1);
                //    using (var request = new HttpRequestMessage(new HttpMethod("POST"), url))
                //    {
                //        var response = await mclient.SendAsync(request);
                //        if (response.IsSuccessStatusCode)
                //            return await response.Content.ReadAsStringAsync();
                //        else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                //            return "";
                //    }
                //}
            }
            catch (Exception) { }
            return "";
        }
        internal static async Task<List<string>> GetAllOrganizations()
        {
            List<string> AllOrgs = new List<string>();
            try
            {
                List<UserModel>? UserList = new List<UserModel>();
                string savedUserList = await PostRequest("/noteTakerGet", USER_DB_CACHE_KEY, "");
                if (!string.IsNullOrEmpty(savedUserList))
                    UserList = JsonConvert.DeserializeObject<List<UserModel>>(savedUserList);
                if (UserList != null && UserList.Count != 0)
                    AllOrgs = UserList.DistinctBy(p => p.Organization).Select(s => s.Organization).ToList();
            }
            catch (Exception)
            {
            }
            return AllOrgs;
        }
        internal static async Task<bool> RegisterUser(UserModel newUser, bool isNew = true, bool isRegPage=true)
        {
            try
            {
                List<UserModel>? UserList = new List<UserModel>();
                string savedUserList = await PostRequest("/noteTakerGet", USER_DB_CACHE_KEY, "");
                if (!string.IsNullOrEmpty(savedUserList))
                    UserList = JsonConvert.DeserializeObject<List<UserModel>>(savedUserList);
                if (UserList == null) UserList = new List<UserModel>();
                var exitUserEmail = UserList.Where(p => p.Email == newUser.Email).FirstOrDefault();
                if (exitUserEmail != null)
                {
                    if (!isNew) UserList.Remove(exitUserEmail);
                    else { ShowMessage("Email Already Exist"); return false; } 
                } 
                UserList.Add(newUser); string newMrawList = JsonConvert.SerializeObject(UserList);
                await PostRequest("/noteTakerSet", USER_DB_CACHE_KEY, newMrawList);
                if (isRegPage)
                {
                    ShowMessage("Registration Successful");
                    LoggedInUser = newUser;
                }
                else ShowMessage("Record Updated Successful");
                return true;
            }
            catch (Exception)
            {
                ShowMessage("Registration Failed"); return false;
            }
        }
        internal static async Task<bool> LoginUser(string email, string password)
        {
            try
            {
                List<UserModel>? UserList = new List<UserModel>(); LoggedInUser = new UserModel();
                string savedUserList = await PostRequest("/noteTakerGet", USER_DB_CACHE_KEY, "");
                if (!string.IsNullOrEmpty(savedUserList))
                    UserList = JsonConvert.DeserializeObject<List<UserModel>>(savedUserList);
                if (UserList != null && UserList.Count != 0)
                    LoggedInUser = UserList.Where(p => p.Email == email && p.Password == password).FirstOrDefault() ?? new UserModel();
                //if (LoggedInUser != null)
                //{
                //    ShowMessage("Login Successful"); return true;
                //} 
                string mraw = JsonConvert.SerializeObject(LoggedInUser);
                return !string.IsNullOrEmpty(LoggedInUser.UserId);
            }
            catch (Exception)
            {
            }
            ShowMessage("Incorrect Email or Password"); return false;
        }
        static List<UserModel>? EmployeeList = new List<UserModel>();
        internal static async Task<List<UserModel>> GetEmployeeList()
        {
            try
            {  
                string savedUserList = await PostRequest("/noteTakerGet", USER_DB_CACHE_KEY, "");
                if (!string.IsNullOrEmpty(savedUserList))
                    EmployeeList = JsonConvert.DeserializeObject<List<UserModel>>(savedUserList);
                if (EmployeeList == null) EmployeeList = new List<UserModel>();
                EmployeeList = EmployeeList.Where(p => p.Organization == LoggedInUser?.Organization).ToList();
                if(!LoggedInUser.isAdmin) EmployeeList = EmployeeList.Where(p => !p.isAdmin).ToList();
                else EmployeeList = EmployeeList.Where(p => p.UserId != LoggedInUser?.UserId).ToList();
                return EmployeeList;
            }
            catch (Exception)
            {
                return [];
            }
        }
        internal static async Task<bool> DeleteEmployee(UserModel delPayroll)
        {
            try
            {
                if (EmployeeList == null) return false;
                var existPayroll = EmployeeList.Where(p => p.UserId == delPayroll.UserId).FirstOrDefault();
                if (existPayroll != null) EmployeeList.Remove(existPayroll);
                else { ShowMessage("User Not Found"); return false; }

                string mraw = JsonConvert.SerializeObject(EmployeeList);
                await PostRequest("/noteTakerSet", USER_DB_CACHE_KEY, mraw);
                ShowMessage("User Deleted Successfully"); return true;
            }
            catch (Exception)
            {
                ShowMessage("Error Deleting User"); return false;
            }
        }
        
         
        //internal static async Task<PayrollModel?> GetPayrollList(string selectedMonth, int selectedYear)
        //{
        //    try
        //    {
        //        string savedPayrollList = await PostRequest("/noteTakerGet", PAYROLL_DB_CACHE_KEY + "-" + selectedMonth + "-" + selectedYear, "");
        //        return JsonConvert.DeserializeObject<PayrollModel?>(savedPayrollList);  
        //    }
        //    catch (Exception)
        //    {
        //        return null;
        //    }
        //}
        internal static async Task<bool> AddUpdatePayroll(PayrollModel newPayroll)
        {
            try
            { 
                string mraw = JsonConvert.SerializeObject(newPayroll);
                await PostRequest("/noteTakerSet", PAYROLL_DB_CACHE_KEY + "-" + newPayroll.Month + "-" + newPayroll.Year, mraw);
                ShowMessage("Payroll Updated Successfully"); return true;
            }
            catch (Exception)
            {
                ShowMessage("Error Updating Payroll"); return false;
            }
        }
        internal static async Task<bool> DeletePayroll(PayrollModel delPayroll)
        {
            try
            {  
                await PostRequest("/noteTakerSet", PAYROLL_DB_CACHE_KEY + "-" + delPayroll.Month + "-" + delPayroll.Year, "");
                ShowMessage("Payroll Deleted Successfully"); return true;
            }
            catch (Exception)
            {
                ShowMessage("Error Deleting Payroll"); return false;
            }
        } 
        internal static async Task<List<PayrollModel>> GetAllPayrollList()
        {
           List<PayrollModel> AllPayrolls= await  new FirebaseClass().GetAllPayrollList();
            if (!LoggedInUser.isAdmin)
                return AllPayrolls.Select(p => new PayrollModel() 
                {  
                    Employee = p.Employee.Where(p => p.UserId == LoggedInUser.UserId).ToList(),
                    Month = p.Month, Year =p.Year, Organization = p.Organization, payrollId = p.payrollId
                }).ToList();
            return AllPayrolls.Where(p => p.Organization == LoggedInUser.Organization).ToList();  
        }
    }
    class FileModel
    {
        public string file { get; set; } = "";
    }
    public class SelfSignedCertificateValidator : ICertificateValidator
    {
        public bool ValidateCertificate(byte[] certificateData, string host)
        {
            // Add logic here to validate the certificate.
            // For development, we'll accept any certificate, but in production,
            // you should implement proper validation logic.

            // For example, you can check the certificate's thumbprint or issuer.
            // Here, we're accepting any certificate.
            return true;
        }
    }

    public interface ICertificateValidator
    {
        bool ValidateCertificate(byte[] certificateData, string host);
    }

}
