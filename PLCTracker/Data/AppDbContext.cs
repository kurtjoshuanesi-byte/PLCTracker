using Microsoft.EntityFrameworkCore;
using PLCTracker.Models;
using System.IO;

namespace PLCTracker.Data;

public class AppDbContext : DbContext
{
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        var folder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "PLCTracker");
        Directory.CreateDirectory(folder);
        var dbPath = Path.Combine(folder, "tracker.db");
        options.UseSqlite($"Data Source={dbPath}");
    }
}