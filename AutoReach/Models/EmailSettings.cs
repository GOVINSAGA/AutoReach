namespace AutoReach.Models;

/// <summary>
/// Holds all user-configurable settings for an email batch.
/// This model is persisted to JSON between sessions.
/// </summary>
public class EmailSettings
{
    // ── Sender identity ────────────────────────────────────────────────────
    public string SenderName   { get; set; } = string.Empty;
    public string Address      { get; set; } = string.Empty;
    public string AppPassword  { get; set; } = string.Empty;

    // ── SMTP configuration ─────────────────────────────────────────────────
    public string SmtpHost     { get; set; } = "smtp.gmail.com";
    public int    SmtpPort     { get; set; } = 587;

    // ── Email content ──────────────────────────────────────────────────────
    public string Subject      { get; set; } = "Job Application";
    public string TemplateBody { get; set; } = string.Empty;

    // ── Batch settings ─────────────────────────────────────────────────────
    public int    DailyLimit   { get; set; } = 50;

    // ── File paths ─────────────────────────────────────────────────────────
    public string ResumePath    { get; set; } = string.Empty;
    public string EmailListPath { get; set; } = string.Empty;
    public string SentListPath  { get; set; } = string.Empty;
}