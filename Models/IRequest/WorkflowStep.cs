using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
namespace App.Models.IRequest
{
    [Table("WorkflowSteps")]
    public class WorkflowStep
    {
        [Key]
        public int StepID { get; set; }

        [Required(ErrorMessage = "Phải có tên bước")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [Column(TypeName = "nvarchar(100)")]
        [Display(Name = "Tên bước")]
        public string StepName { get; set; }
        [Display(Name = "Danh mục quy trình")]
        public int? WorkflowID { get; set; }
        [ForeignKey("WorkflowID")]
        [Display(Name = "Tên quy trình")]
        public Workflow? Workflow { get; set; }
        [Display(Name = "StepOrder")]
        public int StepOrder { get; set; } = 0;

        [Display(Name = "Người phụ trách")]
        public string? AssignedUserId { get; set; }
        [ForeignKey("AssignedUserId")]
        [Display(Name = "Người phụ trách")]
        public AppUser? AssignedUser { get; set; }

        [Display(Name = "Role được phép phê duyệt")]
        public string? RequiredRoleId { get; set; }
        [ForeignKey("RequiredRoleId")]
        [Display(Name = "Role phê duyệt")]
        public Roles? RequiredRole { get; set; }

        [Display(Name = "Thời gian xử lý")]
        public int? TimeLimitHours { get; set; }
        [Display(Name = "Có cần phê duyệt không")]
        public bool ApprovalRequired { get; set; } = false;
        
        [Display(Name = "Trạng thái tiếp theo")]
        public int? StatusID { get; set; }
        [ForeignKey("StatusID")]
        [Display(Name = "Tên trạng thái")]
        public Status? statsus { get; set; }
    }
}