using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace App.Models.IRequest
{
    [Table("RequestStepHistory")]
    public class RequestStepHistory
    {
        [Key]
        public int Id { get; set; }
        public int RequestID { get; set; }
        public int StepOrder { get; set; }
        public string ActionByUserId { get; set; }
        public DateTime ActionTime { get; set; }
        public string Note { get; set; }
        public int StatusID { get; set; }
    }
}
