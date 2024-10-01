using Firebase.Database;
using Firebase.Database.Query;
using Newtonsoft.Json;

namespace PayrollSystemApp.Controls
{
    public class FirebaseClass
    {
        private readonly FirebaseClient _firebaseClient;
        private const string MATRIC_NO = "CS-HND-F22-3347";

        public FirebaseClass()
        {
            _firebaseClient = new FirebaseClient("https://fedpoffacs-default-rtdb.firebaseio.com/");
        }

        internal async Task<string> GetResponse(string actionname, string key, string value)
        {
            try
            {
                if (actionname == "/noteTakerGet")
                {
                    if (key == VUtils.USER_DB_CACHE_KEY)
                    {
                        var users = await _firebaseClient.Child(MATRIC_NO)
                            .Child("users").OnceAsync<UserModel>();
                        var listRes = users.Select(item => item.Object).ToList();
                        return JsonConvert.SerializeObject(listRes);
                    }
                    else if (key.Contains(VUtils.PAYROLL_DB_CACHE_KEY))
                    {
                        var allPayroll = await GetAllPayrollList();
                        var meetings = await _firebaseClient.Child(MATRIC_NO).Child("payrolls")
                            .Child(key.Replace(VUtils.PAYROLL_DB_CACHE_KEY, "")).OnceAsync<PayrollModel>();
                        var listRes = meetings.Select(item => item.Object).ToList();
                        return JsonConvert.SerializeObject(listRes);
                    }
                }
                else if (actionname == "/noteTakerSet")
                {
                    if (key == VUtils.USER_DB_CACHE_KEY)
                    {
                        var userListModel = JsonConvert.DeserializeObject<List<UserModel>>(value);
                        if (userListModel != null)
                        {
                            await _firebaseClient.Child(MATRIC_NO).Child("users").DeleteAsync();
                            foreach (var user in userListModel)
                            {
                                await _firebaseClient.Child(MATRIC_NO).Child("users").Child(user.UserId).PutAsync(user);
                            }
                        }
                        return "true";
                    }
                    else if (key.Contains(VUtils.PAYROLL_DB_CACHE_KEY))
                    {
                        if(string.IsNullOrEmpty(value))
                            await _firebaseClient.Child(MATRIC_NO).Child("payrolls").Child(key).DeleteAsync(); 
                        else
                        {
                            var payrollModel = JsonConvert.DeserializeObject<PayrollModel>(value);
                            await _firebaseClient.Child(MATRIC_NO).Child("payrolls")
                                .Child(key.Replace(VUtils.PAYROLL_DB_CACHE_KEY, "")).PutAsync(payrollModel);
                        }
                        return "true";
                    }
                }
            }
            catch (Exception ex)
            {

            }
            return "";
        }

        internal async Task<List<PayrollModel>> GetAllPayrollList()
        {
            try
            {
                var meetings = await _firebaseClient.Child(MATRIC_NO).Child("payrolls").OnceAsync<PayrollModel>();
                var objList = meetings.Select(item => item.Object).ToList();
                return objList;
            }
            catch (Exception)
            {
                return new List<PayrollModel>();
            }
        }
    }
}
