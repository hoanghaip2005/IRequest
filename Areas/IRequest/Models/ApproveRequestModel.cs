using System.ComponentModel.DataAnnotations;

namespace App.Models.IRequest
{
    public class ApproveRequestModel
    {
        [Required]
        public int RequestId { get; set; }
        
        public string? Comment { get; set; }

        public string? CustomerResponse { get; set; }

        public string? Resolution { get; set; }

        public string? LinkedIssues { get; set; }

        public string? Issue { get; set; }
    }
} 