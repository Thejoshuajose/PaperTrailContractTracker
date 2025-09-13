using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Windows;
using System.Threading.Tasks;
using PaperTrail.App.Services;
using PaperTrail.App;

namespace PaperTrail.App.ViewModels;

public class LandingViewModel : ObservableObject
{
    private readonly MainViewModel _mainViewModel;
    private readonly SettingsService _settings;

    public IAsyncRelayCommand<Window> OpenMainCommand { get; }
    public IRelayCommand OpenSettingsCommand { get; }

    public LandingViewModel(MainViewModel mainViewModel, SettingsService settings)
    {
        _mainViewModel = mainViewModel;
        _settings = settings;
        OpenMainCommand = new AsyncRelayCommand<Window>(OpenMainAsync);
        OpenSettingsCommand = new RelayCommand(OpenSettings);
    }

    private async Task OpenMainAsync(Window? window)
    {
        await _mainViewModel.Contracts.LoadAsync();
        var main = new MainWindow { DataContext = _mainViewModel };
        main.Show();
        window?.Close();

        if (string.IsNullOrWhiteSpace(_settings.CompanyName))
        {
            var settingsVm = new SettingsViewModel(_settings);
            var settingsWindow = new SettingsWindow { DataContext = settingsVm, Owner = main };
            settingsWindow.ShowDialog();
        }
    }

    private void OpenSettings()
    {
        var vm = new SettingsViewModel(_settings);
        var win = new SettingsWindow { DataContext = vm };
        win.ShowDialog();
    }
}
