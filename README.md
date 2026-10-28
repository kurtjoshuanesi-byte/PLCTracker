# 📋 PLC Intake Tracker

A native Windows desktop application built specifically to assist mental health intake coordinators in managing client follow-ups, callbacks, and onboarding schedules efficiently. 

Developed as a personal tool to streamline daily workflows, ensure no client is left behind, and keep sensitive data securely stored locally.

---

## ✨ Features

*   **👥 Client Management**: Add, edit, and track client details (Name, Phone, Email, Status, Notes).
*   **📋 Smart Task Tracking**: Quick-add tasks for Callbacks (2h), Follow-ups (3d), and Pre-Onboarding (1d before).
*   **⏰ Alarm & Notification System**: 
    *   Custom pop-up alarm window 10 minutes before a task is due.
    *   Built-in sound alerts to ensure tasks are never missed.
*   **📌 System Tray Integration**: 
    *   Minimizing or closing the app sends it to the system tray.
    *   Right-click the tray icon to Open or completely Exit the app.
*   **🌅 Daily Agenda**: A "Good Morning" pop-up summary showing overdue and today's tasks upon opening the app.
*   **📞 Quick Contact**: Direct "Dial" and "Email" buttons that open your default calling/mailing apps instantly.
*   **💾 Backup & Restore**: Easily backup your database to a safe location or restore from a previous backup.
*   **📁 Export to CSV**: Export all client tasks to an Excel-ready CSV file for reporting.
*   **🚀 Auto-Start**: Option to launch the app automatically when Windows boots up.
*   **🌙 Premium Dark Mode UI**: Sleek, modern, dark-themed interface with orange accents.

---

## 🛠️ Tech Stack

*   **Language**: C# (.NET 10)
*   **UI Framework**: WPF (Windows Presentation Foundation)
*   **Database**: SQLite (via Entity Framework Core)
*   **Architecture**: Desktop Native (Offline-first)

---

## 🚀 How to Run (For Developers)

1. Clone this repository:
   ```bash
   git clone https://github.com/kurtjoshuansel-byte/PLCTracker.git
Open the PLCTracker.sln file in Visual Studio 2026 (or 2022 with .NET 10 SDK).

Restore NuGet Packages (Visual Studio should do this automatically).

Press F5 to build and run.

📂 Where is the Data Stored?
Because this app handles sensitive mental health information, all data is stored 100% locally on your machine. Nothing is sent to the cloud.

Database location: C:\Users\<YourUsername>\AppData\Local\PLCTracker\tracker.db

Tip: Use the 💾 Backup DB button in the app regularly to save copies of your data.

👨‍💻 Author
Kurt Joshua

GitHub: @kurtjoshuansel-byte

Built with ❤️ for the hardworking Intake Coordinators at Progressive Life Counseling.
