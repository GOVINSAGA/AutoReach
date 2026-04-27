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

                    var htmlBody = ConvertXamlToHtml(settings.TemplateBody);
                    var builder = new BodyBuilder { HtmlBody = htmlBody };
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

    private string ConvertXamlToHtml(string xaml)
    {
        if (string.IsNullOrWhiteSpace(xaml)) return string.Empty;
        if (!xaml.Contains("<Section"))
            return $"<div style=\"white-space: pre-wrap; font-family: sans-serif;\">{System.Net.WebUtility.HtmlEncode(xaml)}</div>";

        try
        {
            var doc = System.Xml.Linq.XDocument.Parse(xaml);
            var sb = new System.Text.StringBuilder();
            var ns = doc.Root.Name.Namespace;

            sb.Append("<div style=\"font-family: sans-serif;\">");
            foreach (var element in doc.Root.Elements())
            {
                ParseXamlElement(element, sb, ns);
            }
            sb.Append("</div>");
            return sb.ToString();
        }
        catch
        {
            return $"<div style=\"white-space: pre-wrap; font-family: sans-serif;\">{System.Net.WebUtility.HtmlEncode(xaml)}</div>";
        }
    }

    private void ParseXamlElement(System.Xml.Linq.XElement element, System.Text.StringBuilder sb, System.Xml.Linq.XNamespace ns)
    {
        if (element.Name.LocalName == "Paragraph")
        {
            sb.Append("<p style=\"margin: 0; padding: 0;\">");
            foreach (var child in element.Nodes())
            {
                if (child is System.Xml.Linq.XElement childEl) ParseXamlElement(childEl, sb, ns);
                else if (child is System.Xml.Linq.XText text) sb.Append(System.Net.WebUtility.HtmlEncode(text.Value));
            }
            sb.Append("</p>");
        }
        else if (element.Name.LocalName == "List")
        {
            sb.Append("<ul style=\"margin: 0; padding-left: 20px;\">");
            foreach (var item in element.Elements(ns + "ListItem"))
            {
                sb.Append("<li>");
                foreach (var child in item.Elements()) ParseXamlElement(child, sb, ns);
                sb.Append("</li>");
            }
            sb.Append("</ul>");
        }
        else if (element.Name.LocalName == "Run")
        {
            bool isBold = element.Attribute("FontWeight")?.Value == "Bold";
            bool isItalic = element.Attribute("FontStyle")?.Value == "Italic";

            if (isBold) sb.Append("<b>");
            if (isItalic) sb.Append("<i>");

            var text = element.Attribute("Text")?.Value ?? element.Value ?? string.Empty;
            sb.Append(System.Net.WebUtility.HtmlEncode(text).Replace("\n", "<br/>"));

            if (isItalic) sb.Append("</i>");
            if (isBold) sb.Append("</b>");
        }
        else if (element.Name.LocalName == "Span")
        {
            foreach (var child in element.Elements()) ParseXamlElement(child, sb, ns);
        }
        else if (element.Name.LocalName == "LineBreak")
        {
            sb.Append("<br/>");
        }
        else
        {
            foreach (var child in element.Elements()) ParseXamlElement(child, sb, ns);
        }
    }

}
