using System;
using System.Collections.Generic;

public enum LaptopRequestStatus
{
    Created,
    Received,
    ManagerApproved,
    ITApproved,
    InventoryChecked,
    Configured,
    HandedOver,
    Completed,
    Rejected,
    WaitingForInventory
}

public class LaptopRequest
{
    public int Id { get; set; }
    public string EmployeeId { get; set; }
    public string EmployeeName { get; set; }
    public string Department { get; set; }
    public string Reason { get; set; }
    public string DeviceType { get; set; }
    public string DeviceSpecs { get; set; }
    public int StatusID { get; set; } // Mapping to Status table
    public int PriorityID { get; set; } // Mapping to Priority table
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string ManagerId { get; set; }
    public string ManagerName { get; set; }
    public DateTime? ManagerApprovedAt { get; set; }
    public string ManagerNote { get; set; }
    public string ITManagerId { get; set; }
    public string ITManagerName { get; set; }
    public DateTime? ITApprovedAt { get; set; }
    public string ITNote { get; set; }
    public string InventoryStaffId { get; set; }
    public DateTime? InventoryCheckedAt { get; set; }
    public string InventoryNote { get; set; }
    public string ConfigStaffId { get; set; }
    public DateTime? ConfiguredAt { get; set; }
    public string ConfigNote { get; set; }
    public string HandoverStaffId { get; set; }
    public DateTime? HandedOverAt { get; set; }
    public string HandoverNote { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string EmployeeConfirmNote { get; set; }
    public string RejectorId { get; set; }
    public string RejectorName { get; set; }
    public DateTime? RejectedAt { get; set; }
    public string RejectionReason { get; set; }
}

public interface ILaptopRequestService
{
    int CreateRequest(string employeeId, string employeeName, string department, string reason, string deviceType, string deviceSpecs, int priorityId);
    void ReceiveRequest(int requestId, string receiver);
    void ApproveByManager(int requestId, string managerId, string managerName, bool approve, string note = null);
    void ApproveByIT(int requestId, string itManagerId, string itManagerName, bool approve, string note = null);
    void CheckInventory(int requestId, string inventoryStaffId, bool available, string note = null);
    void ConfigureDevice(int requestId, string itStaffId, string configNote);
    void HandOver(int requestId, string itStaffId, string handoverNote);
    void CompleteRequest(int requestId, string employeeConfirmNote);
    void RejectRequest(int requestId, string rejectorId, string rejectorName, string reason);
}

public class LaptopRequestService : ILaptopRequestService
{
    // Giả lập DbContext
    private readonly List<LaptopRequest> _db = new List<LaptopRequest>();
    public int CreateRequest(string employeeId, string employeeName, string department, string reason, string deviceType, string deviceSpecs, int priorityId)
    {
        var req = new LaptopRequest
        {
            Id = _db.Count + 1,
            EmployeeId = employeeId,
            EmployeeName = employeeName,
            Department = department,
            Reason = reason,
            DeviceType = deviceType,
            DeviceSpecs = deviceSpecs,
            StatusID = 1, // 1 = New
            PriorityID = priorityId,
            CreatedAt = DateTime.Now,
            UpdatedAt = DateTime.Now
        };
        _db.Add(req);
        // Gửi thông báo tới bộ phận tiếp nhận
        return req.Id;
    }

    public void ReceiveRequest(int requestId, string receiver)
    {
        var req = _db.Find(r => r.Id == requestId);
        if (req.StatusID != 1) // 1 = New
            throw new InvalidOperationException("Sai trạng thái");
        req.StatusID = 2; // 2 = Open
        req.UpdatedAt = DateTime.Now;
    }

    public void ApproveByManager(int requestId, string managerId, string managerName, bool approve, string note = null)
    {
        var req = _db.Find(r => r.Id == requestId);
        if (req.StatusID != 2) // 2 = Open
            throw new InvalidOperationException("Sai trạng thái");
        
        req.UpdatedAt = DateTime.Now;
        if (approve)
        {
            req.StatusID = 6; // 6 = Approved
            req.ManagerId = managerId;
            req.ManagerName = managerName;
            req.ManagerApprovedAt = DateTime.Now;
            req.ManagerNote = note;
        }
        else
        {
            req.StatusID = 7; // 7 = Rejected
            req.RejectorId = managerId;
            req.RejectorName = managerName;
            req.RejectedAt = DateTime.Now;
            req.RejectionReason = note;
        }
    }

    public void ApproveByIT(int requestId, string itManagerId, string itManagerName, bool approve, string note = null)
    {
        var req = _db.Find(r => r.Id == requestId);
        if (req.StatusID != 6) // 6 = Approved (Manager)
            throw new InvalidOperationException("Sai trạng thái");
        
        req.UpdatedAt = DateTime.Now;
        if (approve)
        {
            req.StatusID = 3; // 3 = In Progress (IT xử lý)
            req.ITManagerId = itManagerId;
            req.ITManagerName = itManagerName;
            req.ITApprovedAt = DateTime.Now;
            req.ITNote = note;
        }
        else
        {
            req.StatusID = 7; // 7 = Rejected
            req.RejectorId = itManagerId;
            req.RejectorName = itManagerName;
            req.RejectedAt = DateTime.Now;
            req.RejectionReason = note;
        }
    }

    public void CheckInventory(int requestId, string inventoryStaffId, bool available, string note = null)
    {
        var req = _db.Find(r => r.Id == requestId);
        if (req.StatusID != 3) // 3 = In Progress
            throw new InvalidOperationException("Sai trạng thái");
        
        req.UpdatedAt = DateTime.Now;
        req.InventoryStaffId = inventoryStaffId;
        req.InventoryCheckedAt = DateTime.Now;
        req.InventoryNote = note;
        
        if (available)
        {
            req.StatusID = 5; // 5 = Pending Review (chờ cấu hình)
        }
        else
        {
            req.StatusID = 4; // 4 = On Hold (chờ nhập hàng)
        }
    }

    public void ConfigureDevice(int requestId, string itStaffId, string configNote)
    {
        var req = _db.Find(r => r.Id == requestId);
        if (req.StatusID != 5) // 5 = Pending Review (chờ cấu hình)
            throw new InvalidOperationException("Sai trạng thái");
        
        req.UpdatedAt = DateTime.Now;
        req.StatusID = 8; // 8 = Resolved (đã cấu hình xong)
        req.ConfigStaffId = itStaffId;
        req.ConfiguredAt = DateTime.Now;
        req.ConfigNote = configNote;
    }

    public void HandOver(int requestId, string itStaffId, string handoverNote)
    {
        var req = _db.Find(r => r.Id == requestId);
        if (req.StatusID != 8) // 8 = Resolved (đã cấu hình xong)
            throw new InvalidOperationException("Sai trạng thái");
        
        req.UpdatedAt = DateTime.Now;
        req.StatusID = 9; // 9 = Closed (bàn giao xong)
        req.HandoverStaffId = itStaffId;
        req.HandedOverAt = DateTime.Now;
        req.HandoverNote = handoverNote;
    }

    public void CompleteRequest(int requestId, string employeeConfirmNote)
    {
        var req = _db.Find(r => r.Id == requestId);
        if (req.StatusID != 9) // 9 = Closed
            throw new InvalidOperationException("Sai trạng thái");
        
        req.UpdatedAt = DateTime.Now;
        req.CompletedAt = DateTime.Now;
        req.EmployeeConfirmNote = employeeConfirmNote;
    }

    public void RejectRequest(int requestId, string rejectorId, string rejectorName, string reason)
    {
        var req = _db.Find(r => r.Id == requestId);
        req.UpdatedAt = DateTime.Now;
        req.StatusID = 7; // 7 = Rejected
        req.RejectorId = rejectorId;
        req.RejectorName = rejectorName;
        req.RejectedAt = DateTime.Now;
        req.RejectionReason = reason;
    }
} 