namespace PLCTracker.Models;

public class AuditLog
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public string Action { get; set; } = "";
    public string Details { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.Now;
}