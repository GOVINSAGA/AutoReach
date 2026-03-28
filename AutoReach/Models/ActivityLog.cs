namespace AutoReach.Models;

/// <summary>
/// Represents a single email-send activity record shown in the activity grid.
/// </summary>
public class ActivityLog
{
    public string Recipient { get; set; } = string.Empty;
    public string Status    { get; set; } = string.Empty;
    public string TimeSent  { get; set; } = string.Empty;
}
