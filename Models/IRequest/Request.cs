using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.IRequest
{
    [Table("Requests")]
    public class Request
    {
        [Key]
        public int RequestID { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        [Required(ErrorMessage = "Phải có Tiêu đề")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [Display(Name = "Tên tiêu đề")]
        public string Title { get; set; }

        [DataType(DataType.Text)]
        [Display(Name = "Nội dung mô tả")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "Phải tạo url")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [RegularExpression(@"^[a-z0-9-]*$", ErrorMessage = "Chỉ dùng các ký tự [a-z0-9-]")]
        [Display(Name = "Link đính kèm")]
        public string AttachmentURL { get; set; }
        [Display(Name = "Có được duyệt không")]
        public bool IsApproved { get; set; } = false;

        // Fkey Status
        [Display(Name = "Danh mục trạng thái")]
        public int? StatusID { get; set; }
        [ForeignKey("StatusID")]
        [Display(Name = "Danh mục trạng thái")]
        public Status? Status { get; set; }

        [Display(Name = "Danh mục ưu tiên")]
        public int? PriorityID { get; set; }
        [ForeignKey("PriorityID")]
        [Display(Name = "Danh mục ưu tiên")]
        public Priority? Priority { get; set; }

        [Display(Name = "Danh mục quy trình")]
        public int? WorkflowID { get; set; }
        [ForeignKey("WorkflowID")]
        [Display(Name = "Danh mục quy trình")]
        public Workflow? Workflow { get; set; }
        [Display(Name = "Người dùng")]
        public string UsersId { get; set; }
        [ForeignKey("UsersId")]
        public AppUser? User { get; set; }

        [Display(Name = "Người được giao")]
        public string AssignedUserId { get; set; }

        [ForeignKey("AssignedUserId")]
        public AppUser? AssignedUser { get; set; }



        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Display(Name = "Ngày cập nhật")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ClosedAt { get; set; }
    }
}