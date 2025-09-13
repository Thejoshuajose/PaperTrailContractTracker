using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PaperTrail.App.Services;
using System.Windows;

namespace PaperTrail.App.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
    private readonly SettingsService _settings;

    [ObservableProperty]
    private string? companyName;

    [ObservableProperty]
    private string? contactEmail;

    [ObservableProperty]
    private string? contactPhone;

    [ObservableProperty]
    private string? address;

    public IRelayCommand<Window> SaveCommand { get; }

    public SettingsViewModel(SettingsService settings)
    {
        _settings = settings;
        CompanyName = settings.CompanyName;
        ContactEmail = settings.ContactEmail;
        ContactPhone = settings.ContactPhone;
        Address = settings.Address;
        SaveCommand = new RelayCommand<Window>(Save);
    }

    private void Save(Window? window)
    {
        _settings.CompanyName = CompanyName;
        _settings.ContactEmail = ContactEmail;
        _settings.ContactPhone = ContactPhone;
        _settings.Address = Address;
        _settings.Save();
        window?.Close();
    }
}
