# 📋 PLC Intake Tracker

A native Windows desktop application built specifically to assist mental health intake coordinators in managing client follow-ups, callbacks, and onboarding schedules efficiently. 

Designed with a modern, dark-mode Windows 11 aesthetic, this app ensures no client is left behind while keeping sensitive data securely stored locally.

---

## ✨ Features

*   **🎨 Modern Windows 11 UI**: Clean, dark-mode dashboard with a sleek sidebar navigation (powered by WPF-UI).
*   **👥 Client Management**: Add, edit, and track client details (Name, Phone, Email, Status, Notes).
*   **📋 Smart Task Tracking**: Quick-add tasks for Callbacks (2h), Follow-ups (3d), and Pre-Onboarding (1d before).
*   **⏰ Custom Alarm System**: A dedicated pop-up alarm window 10 minutes before a task is due, complete with sound and snooze options.
*   **📌 System Tray Integration**: Minimizing or closing the app sends it seamlessly to the system tray. Right-click the tray icon to Open or Exit.
*   **🌅 Daily Agenda**: A "Good Morning" pop-up summary showing overdue and today's tasks upon opening the app.
*   **📞 Quick Contact**: Direct "Dial" and "Email" buttons that open your default calling/mailing apps instantly.
*   **📥 CSV Import**: Import client lists directly from Excel/CSV files to save time.
*   **💾 Backup & Restore**: Easily backup your SQLite database to a safe location or restore from a previous backup.
*   **🚀 Auto-Start**: Option to launch the app automatically when Windows boots up.

---

## 🛠️ Tech Stack

*   **Language**: C# (.NET 10)
*   **UI Framework**: WPF (Windows Presentation Foundation) with [WPF-UI](https://github.com/lepoco/wpfui)
*   **Database**: SQLite (via Entity Framework Core)
*   **Architecture**: Native Desktop, Offline-first

---

## 📥 How to Install (For Users)

1. Go to the **[Releases](../../releases)** tab on the right side of this page.
2. Download the latest **`PLCTracker_v1.0.0.zip`** file.
3. Right-click the downloaded `.zip` file and select **"Extract All..."**.
4. Open the extracted folder and double-click **`PLCTracker.exe`**.
5. **Important:** If Windows shows a "Windows protected your PC" warning, click **"More info"** and then **"Run anyway"**. *(This is normal for custom-built apps without a paid digital signature).*

---

## 🔒 Data Privacy & Location

Because this app handles sensitive mental health information, **all data is stored 100% locally** on your machine. Nothing is sent to the cloud.
*   **Database location:** `C:\Users\<YourUsername>\AppData\Local\PLCTracker\tracker.db`
*   *Tip: Use the **💾 Backup DB** button in the app regularly to save copies of your data to a safer location (e.g., Google Drive folder).*

---

## 👨‍💻 How to Build from Source (For Developers)

1. Clone this repository:
   ```bash
   git clone https://github.com/kurtjoshuansel-byte/PLCTracker.git

   Open the PLCTracker.sln file in Visual Studio 2026 (or 2022 with .NET 10 SDK).

2. Restore NuGet Packages (Visual Studio should do this automatically).

2. Press F5 to build and run.

👤 Author - Kurt Joshua

GitHub: @kurtjoshuansel-byte

Built with ❤️
