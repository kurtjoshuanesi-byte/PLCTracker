# 📋 PLC Intake Tracker

A native Windows desktop application built specifically to assist healthcare intake coordinators in managing client follow-ups, callbacks, and onboarding schedules efficiently. 

Designed with a modern, dark-mode Windows 11 aesthetic (powered by WPF-UI), this app ensures no client is left behind while maintaining strict local data privacy and a defensible audit trail.

---

## ✨ Key Features

*   **🔒 PIN Login Security**: Protects sensitive client data with a 4-digit PIN upon startup (Default PIN: `1234`).
*   **📊 Analytics Dashboard**: Real-time stat cards showing Active Clients, Scheduled Tasks, Overdue Tasks, and Tasks Completed This Week.
*   **🔄 Intake Workflow Pipeline**: Track clients through their lifecycle: *New Referral, Screening, Scheduled, Completed, Missed, and Closed*. Includes reasons and timestamps for status changes.
*   **📋 Audit Trail (Soft Delete)**: Tracks every action (calls, edits, completed tasks, status changes) in a history timeline. Deleting a client or task archives it instead of permanently removing it, preserving audit compliance.
*   **🖨️ Print Daily Call Sheet**: Generates a print-friendly summary of all overdue and today's tasks directly to your printer or PDF.
*   **📁 Export & Import CSV**: Bulk import client lists from Excel/CSV files, and export task data for external reporting.
*   **🔊 Custom Alarm Sound**: Set your own `.wav` or `.mp3` file to play when a task alarm goes off.
*   **🎨 Modern Windows 11 UI**: Clean, dark-mode dashboard with a sleek sidebar navigation and Mica backdrop effects.
*   **⏰ Custom Alarm System**: A dedicated pop-up alarm window 10 minutes before a task is due, complete with sound and snooze options.
*   **📌 System Tray Integration**: Minimizing or closing the app sends it seamlessly to the system tray. Right-click the tray icon to Open or Exit.
*   **🌅 Daily Agenda**: A "Good Morning" pop-up summary showing overdue and today's tasks upon opening the app.
*   **📞 Quick Contact**: Direct "Dial" and "Email" buttons that open your default calling/mailing apps instantly.
*   **💾 Backup & Restore**: Easily backup your SQLite database to a safe location or restore from a previous backup.

---

## 🛠️ Tech Stack

*   **Language**: C# (.NET 10)
*   **UI Framework**: WPF (Windows Presentation Foundation) with [WPF-UI](https://github.com/lepoco/wpfui)
*   **Database**: SQLite (via Entity Framework Core)
*   **Architecture**: Native Desktop, Offline-first, Privacy-focused

---

## 📥 How to Install & Run (For Users)

1. Go to the **[Releases](../../releases/latest)** tab on the right side of this page.
2. Download the latest **`.zip`** file.
3. Right-click the downloaded `.zip` file and select **"Extract All..."**.
4. Open the extracted folder and double-click **`PLCTracker.exe`**.
5. **Important:** If Windows shows a "Windows protected your PC" warning, click **"More info"** and then **"Run anyway"**. *(This is normal for custom-built apps without a paid digital signature).*

---

## 🔒 Data Privacy & Location

Because this app handles sensitive mental health information, **all data is stored 100% locally** on your machine. Nothing is sent to the cloud.
*   **Database location:** `C:\Users\<YourUsername>\AppData\Local\PLCTracker\tracker.db`
*   **Audit Logs:** All actions are timestamped and linked to the specific client ID. 
*   *Tip: Use the **💾 Backup DB** button in the app regularly to save copies of your data to a safer location.*

---

## 👨‍💻 How to Build from Source (For Developers)

1. Clone this repository:
   ```bash
   git clone https://github.com/kurtjoshuansel-byte/PLCTracker.git
   ```
2. Open the PLCTracker.sln file in Visual Studio 2026 (or 2022 with .NET 10 SDK).

3. Restore NuGet Packages (Visual Studio should do this automatically).

4. Press F5 to build and run. (Default PIN is 1234)

👤 Author
Kurt Joshua

GitHub: **[@kurtjoshuansel-byte](https://github.com/kurtjoshuanesi-byte)**

Built with ❤️
   
