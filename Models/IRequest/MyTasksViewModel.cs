using System.Collections.Generic;

namespace App.Models.IRequest
{
    public class MyTasksViewModel
    {
        public List<Request> RequestsToProcess { get; set; } // Các yêu cầu cần xử lý
        public List<Request> MySubmittedRequests { get; set; } // Các yêu cầu mình đã gửi và đang ở bước 1
        public List<Request> RequestsWaitingForOthers { get; set; } // Các yêu cầu đang chờ người khác xử lý
        public List<Request> DoneRequests { get; set; } // Các yêu cầu đã hoàn thành, phê duyệt hoặc từ chối
        public List<Request> AssignedRequests { get; set; } // Các yêu cầu được gán cho user
    }
}