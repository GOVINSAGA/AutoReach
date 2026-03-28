using AutoReach.Models;

namespace AutoReach.Services;

/// <summary>
/// Defines the contract for sending batched emails.
/// </summary>
public interface IEmailService
{
    /// <summary>
    /// Processes the email list defined in <paramref name="settings"/>,
    /// sending one email per address up to the configured daily limit.
    /// </summary>
    Task ProcessEmailsAsync(EmailSettings settings, IProgress<string> progress, CancellationToken ct);
}
