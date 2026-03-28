# 🚀 AutoReach: Professional Email Outreach Automator

AutoReach is a high-performance Windows desktop application built with **.NET 10** and **WPF**. It empowers developers and job seekers to automate their email outreach (like sending resumes to HR) with a clean UI, real-time logging, and built-in anti-spam safeguards.



## ✨ Features

- **Modern WPF Dashboard:** A dark-themed terminal interface to monitor your outreach in real-time.
- **Dynamic Configuration:** No more editing JSON files. Input your credentials, subject, and email body directly in the app.
- **File Persistence:** Your settings and paths are saved locally, so you don't have to re-type them every time you open the app.
- **Asynchronous Engine:** Built on `MailKit` and `async/await` to ensure the UI stays responsive during heavy batches.
- **Anti-Spam Logic:** Automatic 3-second delays between emails and customizable daily limits to protect your Gmail account reputation.
- **Outreach Tracking:** Automatically moves contacted emails to a "sent" list to prevent duplicate outreach.

## 🛠️ Tech Stack

- **Framework:** .NET 10.0 (Windows Desktop)
- **Language:** C#
- **Libraries:** - `MailKit` & `MimeKit` for SMTP operations
  - `Microsoft.Extensions.Configuration` for settings management
  - `System.Text.Json` for data persistence

## 🚀 Getting Started

### Prerequisites
- Windows 10/11
- [.NET 10 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)

### Setup for Developers
1. Clone the repository:
   ```bash
   git clone [https://github.com/yourusername/AutoReach.git](https://github.com/yourusername/AutoReach.git)
