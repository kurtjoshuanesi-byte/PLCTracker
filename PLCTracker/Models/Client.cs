namespace PLCTracker.Models;

public enum ClientStatus
{
    NewReferral,
    Screening,
    Scheduled,
    Completed,
    Missed,
    Closed
}

public class Client
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Notes { get; set; } = "";
    public ClientStatus Status { get; set; } = ClientStatus.NewReferral;
    public string StatusReason { get; set; } = ""; // BAG-O: Reason for Missed/Closed
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<TaskItem> Tasks { get; set; } = [];

    // Display helper para sa DataGrid
    public string StatusLabel => Status switch
    {
        ClientStatus.NewReferral => "🆕 New Referral",
        ClientStatus.Screening => "🔍 Screening",
        ClientStatus.Scheduled => "📅 Scheduled",
        ClientStatus.Completed => "✅ Completed",
        ClientStatus.Missed => "❌ Missed",
        ClientStatus.Closed => "🔒 Closed",
        _ => Status.ToString()
    };
}