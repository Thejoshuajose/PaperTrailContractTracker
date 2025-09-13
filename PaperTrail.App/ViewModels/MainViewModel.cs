using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PaperTrail.App.Services;
using PaperTrail.App;

namespace PaperTrail.App.ViewModels;

public class MainViewModel : ObservableObject
{
    private readonly SettingsService _settings;

    public ContractListViewModel Contracts { get; }
    public IRelayCommand OpenSettingsCommand { get; }

    public MainViewModel(ContractListViewModel contracts, SettingsService settings)
    {
        Contracts = contracts;
        _settings = settings;
        OpenSettingsCommand = new RelayCommand(OpenSettings);
    }

    private void OpenSettings()
    {
        var vm = new SettingsViewModel(_settings);
        var window = new SettingsWindow { DataContext = vm };
        window.ShowDialog();
    }
}
