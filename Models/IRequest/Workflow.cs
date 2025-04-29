using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.IRequest
{
    [Table("Workflow")]
    public class Workflow
    {
        [Key]
        public int WorkflowID { get; set; }
        [Column(TypeName = "nvarchar(255)")]
        [Required(ErrorMessage = "Phải có tên quy trình")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [Display(Name = "Tên quy trình")]
        public string WorkflowName { get; set; }
        [Display(Name = "Danh mục ưu tiên")]
        public int? PriorityID { get; set; }
        [ForeignKey("PriorityID")]
        [Display(Name = "Tên danh mục ưu tiên")]
        public Priority? Priority { get; set; }
        [Display(Name = "Mô tả")]
        [DataType(DataType.Text)]
        public string? Description { get; set; }
        [Display(Name = "Có đang hoạt động không")]
        public bool IsActive { get; set; } = true;

        public ICollection<WorkflowStep> Steps { get; set; }

        public ICollection<Request> Requests { get; set; }
        public Workflow()
        {
            Steps = new HashSet<WorkflowStep>();
            Requests = new HashSet<Request>();
        }
    }
}