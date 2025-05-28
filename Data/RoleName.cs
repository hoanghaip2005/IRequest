namespace App.Data
{
    public class RoleName 
    {
        public const string Administrator = "Administrator";
        public const string Editor = "Editor";
        public const string Member = "Member";
        public const string Approver = "Approver";
        public const string Manager = "Manager";
        
        // Roles for laptop request workflow
        public const string ITStaff = "ITStaff";           // Nhân viên IT
        public const string ITManager = "ITManager";       // Trưởng phòng IT
        public const string DepartmentHead = "DepartmentHead"; // Trưởng phòng ban
        public const string FinanceStaff = "FinanceStaff"; // Nhân viên tài chính
        public const string FinanceManager = "FinanceManager"; // Trưởng phòng tài chính
        public const string HRStaff = "HRStaff";          // Nhân viên nhân sự
        public const string HRManager = "HRManager";      // Trưởng phòng nhân sự
        public const string ProcurementStaff = "ProcurementStaff"; // Nhân viên mua sắm
        public const string ProcurementManager = "ProcurementManager"; // Trưởng phòng mua sắm
    }
}