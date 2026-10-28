namespace PLCTracker.Models;

public enum TaskType
{
    Callback,
    FollowUp,
    PreOnboarding,
    Custom
}

public class TaskItem
{
    public int Id { get; set; }
    public int ClientId { get; set; }
    public Client? Client { get; set; }
    public TaskType Type { get; set; }
    public DateTime DueDate { get; set; }
    public string Notes { get; set; } = "";
    public bool IsCompleted { get; set; }
    public bool IsNotified { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    public string TypeLabel => Type switch
    {
        TaskType.Callback => "📞 Callback",
        TaskType.FollowUp => "🔁 Follow-up",
        TaskType.PreOnboarding => "📋 Pre-Onboarding",
        _ => "📌 Custom"
    };

    public string StatusLabel =>
        IsCompleted ? "✅ Done" :
        DueDate < DateTime.Now ? "🔴 OVERDUE" :
        DueDate.Date == DateTime.Today ? "🟡 Today" :
        "🟢 Upcoming";

    public string TimeLeft
    {
        get
        {
            if (IsCompleted) return "";
            var diff = DueDate - DateTime.Now;
            if (diff.TotalMinutes < 0) return $"Overdue {Math.Abs(diff.TotalMinutes):F0}m";
            if (diff.TotalHours < 1) return $"in {diff.TotalMinutes:F0}m";
            if (diff.TotalDays < 1) return $"in {diff.TotalHours:F1}h";
            return $"in {diff.TotalDays:F1}d";
        }
    }
}