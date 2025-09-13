using System;
using System.IO;
using System.Text.Json;

namespace PaperTrail.App.Services;

public class SettingsService
{
    private readonly string _filePath;
    private SettingsData _data;

    private class SettingsData
    {
        public string? CompanyName { get; set; }
        public string? ContactEmail { get; set; }
        public string? ContactPhone { get; set; }
        public string? Address { get; set; }
    }

    public SettingsService()
    {
        var dir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "PaperTrailContractTracker");
        Directory.CreateDirectory(dir);
        _filePath = Path.Combine(dir, "settings.json");
        if (File.Exists(_filePath))
        {
            var json = File.ReadAllText(_filePath);
            _data = JsonSerializer.Deserialize<SettingsData>(json) ?? new SettingsData();
        }
        else
        {
            _data = new SettingsData();
        }
    }

    public string? CompanyName
    {
        get => _data.CompanyName;
        set => _data.CompanyName = value;
    }

    public string? ContactEmail
    {
        get => _data.ContactEmail;
        set => _data.ContactEmail = value;
    }

    public string? ContactPhone
    {
        get => _data.ContactPhone;
        set => _data.ContactPhone = value;
    }

    public string? Address
    {
        get => _data.Address;
        set => _data.Address = value;
    }

    public void Save()
    {
        var json = JsonSerializer.Serialize(_data);
        File.WriteAllText(_filePath, json);
    }
}
