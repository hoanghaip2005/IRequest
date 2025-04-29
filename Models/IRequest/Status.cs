using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.IRequest
{
    public class Status
    {
        [Key]
        public int StatusID { get; set; }
        [Column(TypeName = "nvarchar(100)")]
        [Required(ErrorMessage = "Phải có tên trạng thái")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "{0} dài {1} đến {2}")]
        [Display(Name = "Tên trạng thái")]
        public string StatusName { get; set; }
        [DataType(DataType.Text)]
        [Display(Name = "Mô tả")]
        public string Description { get; set; }
        [Display(Name = "Có phải trạng thái cuối không")]
        public bool IsFinal { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Request> Requests { get; set; }

        public ICollection<WorkflowStep> WorkflowSteps { get; set; }

        public Status()
        {
            Requests = new HashSet<Request>();
            WorkflowSteps = new HashSet<WorkflowStep>();
        }
    }
}