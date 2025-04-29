using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.IRequest
{
    [Table("Workflow")]
    public class Priority
    {
        [Key]
        public int PriorityID { get; set; }
        [Column(TypeName = "nvarchar(50)")]
        [Required(ErrorMessage = "Phải nhập {0}")]
        [StringLength(50)]
        [Display(Name = "Tên Phòng Ban")]
        public string PriorityName { get; set; }
        // [Display(Name = "")]
        // public int SortOrder { get; set; } = 0;
        [Display(Name = "Thời gian giải quyết")]
        public int? ResolutionTime { get; set; }
        [Display(Name = "Thời gian phản hồi")]
        public int? ResponseTime { get; set; }
        [Required(ErrorMessage = "Phải nhập {0}")]
        [StringLength(255)]
        [Display(Name = "Mô tả")]
        public string? Description { get; set; }
        [Display(Name = "Có phải trạng thái đang hoạt động không")]
        public bool IsActive { get; set; } = true;

        public ICollection<Workflow> Workflows { get; set; }

        public ICollection<Request> Requests { get; set; }
        public Priority()
        {
            Workflows = new HashSet<Workflow>();
            Requests = new HashSet<Request>();
        }
    }
}