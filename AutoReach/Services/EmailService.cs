using System.IO;
using MailKit.Net.Smtp;
using MimeKit;
using AutoReach.Models;

namespace AutoReach.Services;

public class EmailService
{
    public async Task ProcessEmailsAsync(EmailSettings settings, IProgress<string> progress, CancellationToken ct)
    {
        var baseDir = AppDomain.CurrentDomain.BaseDirectory;
        var resumePath = Path.Combine(baseDir, settings.ResumePath);
        var emailListPath = Path.Combine(baseDir, settings.EmailListPath);
        var sentListPath = Path.Combine(baseDir, settings.SentListPath);

        if (!File.Exists(emailListPath) || !File.Exists(resumePath))
        {
            progress.Report("Error: Required files (resume or email list) are missing in Assets.");
            return;
        }

        var allEmails = (await File.ReadAllLinesAsync(emailListPath))
            .Where(e => !string.IsNullOrWhiteSpace(e)).ToList();

        var remainingEmails = new List<string>(allEmails);
        var newlySentEmails = new List<string>();
        int sentCount = 0;

        using var client = new SmtpClient();
        try
        {
            progress.Report("Connecting to SMTP...");
            await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(settings.Address, settings.AppPassword, ct);

            foreach (var email in allEmails)
            {
                if (ct.IsCancellationRequested || sentCount >= settings.DailyLimit) break;

                try
                {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress("Govind Sagar", settings.Address));
                    message.To.Add(new MailboxAddress("", email.Trim()));
                    message.Subject = settings.Subject;

                    var builder = new BodyBuilder { TextBody = settings.TemplateBody };
                    builder.Attachments.Add(resumePath);
                    message.Body = builder.ToMessageBody();

                    await client.SendAsync(message, ct);

                    sentCount++;
                    newlySentEmails.Add(email);
                    remainingEmails.Remove(email);

                    progress.Report($"[{sentCount}] Successfully sent to: {email}");

                    // 3-second anti-spam delay
                    await Task.Delay(3000, ct);
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

            if (newlySentEmails.Any())
            {
                await File.AppendAllLinesAsync(sentListPath, newlySentEmails);
                await File.WriteAllLinesAsync(emailListPath, remainingEmails);
                progress.Report($"Batch Finished. {remainingEmails.Count} emails left.");
            }
        }
    }
}
