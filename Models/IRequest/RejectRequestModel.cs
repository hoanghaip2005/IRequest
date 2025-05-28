using System.ComponentModel.DataAnnotations;

namespace App.Models.IRequest
{
    public class RejectRequestModel
    {
        [Required]
        public int RequestId { get; set; }

        [Required]
        public string Action { get; set; }  // "complete" hoặc "requestInfo"

        public string CompleteAction { get; set; }  // "reject" hoặc "close" (chỉ khi Action là "complete")

        public string AdditionalInfo { get; set; }  // Thông tin cần bổ sung (chỉ khi Action là "requestInfo")

        public string Comment { get; set; }  // Nhận xét nội bộ

        public string CustomerResponse { get; set; }  // Phản hồi cho khách hàng

        [Required]
        public string Reason { get; set; }  // Lý do từ chối
    }
} 