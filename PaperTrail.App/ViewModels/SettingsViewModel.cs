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

    public IRelayCommand<Window> SaveCommand { get; }

    public SettingsViewModel(SettingsService settings)
    {
        _settings = settings;
        CompanyName = settings.CompanyName;
        SaveCommand = new RelayCommand<Window>(Save);
    }

    private void Save(Window? window)
    {
        _settings.CompanyName = CompanyName;
        _settings.Save();
        window?.Close();
    }
}
