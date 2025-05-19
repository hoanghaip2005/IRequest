using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.IRequest
{
    [Table("RequestHistories")]
    public class RequestHistory
    {
        [Key]
        public int HistoryID { get; set; }

        [Required]
        [Display(Name = "Yêu cầu")]
        public int RequestID { get; set; }
        [ForeignKey("RequestID")]
        [Display(Name = "Yêu cầu")]
        public Request? Request { get; set; }

        [Required]
        [Display(Name = "Bước xử lý")]
        public int StepID { get; set; }
        [ForeignKey("StepID")]
        [Display(Name = "Bước xử lý")]
        public WorkflowStep? WorkflowStep { get; set; }

        [Required]
        [Display(Name = "Người xử lý")]
        public string UserID { get; set; }
        [ForeignKey("UserID")]
        [Display(Name = "Người xử lý")]
        public AppUser? User { get; set; }

        [Required]
        [Display(Name = "Thời gian bắt đầu")]
        public DateTime StartTime { get; set; }

        [Display(Name = "Thời gian kết thúc")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Trạng thái")]
        public string Status { get; set; } = "Pending"; // Pending, InProgress, Completed, Rejected

        [Display(Name = "Ghi chú")]
        public string? Note { get; set; }

        [Display(Name = "Thời gian xử lý (giờ)")]
        public double? ProcessingTime
        {
            get
            {
                if (EndTime.HasValue)
                {
                    return (EndTime.Value - StartTime).TotalHours;
                }
                return null;
            }
        }
    }
} 