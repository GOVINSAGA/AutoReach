using System.IO;
using MailKit.Net.Smtp;
using MimeKit;
using AutoReach.Models;

namespace AutoReach.Services;

/// <inheritdoc cref="IEmailService"/>
public class EmailService : IEmailService
{
    /// <inheritdoc/>
    public async Task ProcessEmailsAsync(EmailSettings settings, IProgress<string> progress, CancellationToken ct)
    {
        if (!File.Exists(settings.EmailListPath) || !File.Exists(settings.ResumePath))
        {
            progress.Report("Error: Required files (resume or email list) not found.");
            return;
        }

        var allEmails = (await File.ReadAllLinesAsync(settings.EmailListPath, ct))
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .ToList();

        var remainingEmails  = new List<string>(allEmails);
        var newlySentEmails  = new List<string>();
        int sentCount        = 0;

        using var client = new SmtpClient();
        try
        {
            progress.Report("Connecting to SMTP...");
            await client.ConnectAsync(settings.SmtpHost, settings.SmtpPort, MailKit.Security.SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(settings.Address, settings.AppPassword, ct);

            foreach (var email in allEmails)
            {
                if (ct.IsCancellationRequested || sentCount >= settings.DailyLimit) break;

                try
                {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(settings.SenderName, settings.Address));
                    message.To.Add(new MailboxAddress(string.Empty, email.Trim()));
                    message.Subject = settings.Subject;

                    var builder = new BodyBuilder { TextBody = settings.TemplateBody };
                    builder.Attachments.Add(settings.ResumePath);
                    message.Body = builder.ToMessageBody();

                    await client.SendAsync(message, ct);

                    sentCount++;
                    newlySentEmails.Add(email);
                    remainingEmails.Remove(email);

                    progress.Report($"[{sentCount}] Successfully sent to: {email}");

                    // Anti-spam delay between sends
                    await Task.Delay(3000, ct);
                }
                catch (OperationCanceledException)
                {
                    throw; // Let the outer handler deal with cancellation
                }
                catch (Exception ex)
                {
                    progress.Report($"Failed to send to {email}: {ex.Message}");
                }
            }
        }
        finally
        {
            if (client.IsConnected) await client.DisconnectAsync(true);

            if (newlySentEmails.Count > 0)
            {
                await File.AppendAllLinesAsync(settings.SentListPath, newlySentEmails, ct);
                await File.WriteAllLinesAsync(settings.EmailListPath, remainingEmails, ct);
                progress.Report($"Batch finished. {remainingEmails.Count} email(s) remaining.");
            }
        }
    }
}
