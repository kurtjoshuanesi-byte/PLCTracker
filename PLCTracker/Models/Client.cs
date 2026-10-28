namespace PLCTracker.Models;

public enum ClientStatus
{
    Pending,
    Scheduled,
    Onboarded,
    Dropped
}

public class Client
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Phone { get; set; } = "";
    public string Email { get; set; } = "";
    public string Notes { get; set; } = "";
    public ClientStatus Status { get; set; } = ClientStatus.Pending;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public List<TaskItem> Tasks { get; set; } = new();
}