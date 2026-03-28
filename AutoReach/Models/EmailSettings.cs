namespace AutoReach.Models;

public class EmailSettings
{
    public string Address { get; set; } = string.Empty;
    public string AppPassword { get; set; } = string.Empty;
    public string Subject { get; set; } = "Job Application";
    public string TemplateBody { get; set; } = string.Empty;
    public int DailyLimit { get; set; } = 50;

    public string ResumePath { get; set; } = "Assets/govind_cv.pdf";
    public string EmailListPath { get; set; } = "Assets/emails.txt";
    public string SentListPath { get; set; } = "Assets/emailSentList.txt";
}