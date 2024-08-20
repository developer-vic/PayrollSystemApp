namespace PayrollSystemApp.Controls
{
    class AllModels
    {
    }
    public class UserModel
    {
        public string UserId { get; set; } = "";
        public string UserType { get; set; } = "";
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Password { get; set; } = "";
        public string Organization { get; set; } = "";
        public bool isAdmin { get=> UserType=="Employer"; } 
        public string Position { get; set; } = "";
    }
    public class PayrollModel
    {
        public string payrollId { get; set; } = "";
        public UserModel Employee { get; set; } = new UserModel();
        public double BasicSalary { get; set; }
        public double Overtime { get; set; }
        public double Bonuses { get; set; }
        public double Deductions { get; set; } 
        public double Extras =>  Overtime + Bonuses;
        public double NetSalary => BasicSalary + Overtime + Bonuses - Deductions;
        public string Organization { get; set; } = "";
    }
     
}
