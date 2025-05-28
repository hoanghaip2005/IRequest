using System.ComponentModel.DataAnnotations;

namespace App.Models.IRequest
{
    public class AdditionalInfoModel
    {
        [Required]
        public int RequestId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập thông tin bổ sung")]
        public string Content { get; set; }

        public string Comment { get; set; }
    }
} 