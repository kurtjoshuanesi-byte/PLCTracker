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
    public bool IsDeleted { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Giusab ngadto sa [] para mawala ang IDE0028 warning
    public List<TaskItem> Tasks { get; set; } = [];
}