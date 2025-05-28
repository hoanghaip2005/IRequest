using System.ComponentModel.DataAnnotations;

namespace App.Models.IRequest
{
    public class ProcessStepRequest
    {
        [Required]
        public int RequestId { get; set; }

        [Required]
        public int StepId { get; set; }

        [Required]
        public string Action { get; set; }

        public string Note { get; set; }
    }
} 