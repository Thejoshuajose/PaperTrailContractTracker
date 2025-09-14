using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using PaperTrail.App.Services;
using PaperTrail.App.ViewModels;
using PaperTrail.App;
using PaperTrail.Core.DTO;
using PaperTrail.Core.Data;
using PaperTrail.Core.Models;
using PaperTrail.Core.Repositories;
using PaperTrail.Core.Services;
using Quartz.Impl;
using Quartz.Spi;
using Quartz;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows;
using System;
using Windows.UI.Notifications;


public partial class LandingViewModel : ObservableObject
{
    private readonly MainViewModel _mainViewModel;
    private readonly SettingsService _settings;
    private readonly IContractRepository _importedRepo;
    private readonly PreviousContractRepository _previousRepo;
    private readonly ImportService _importService;
    private readonly DialogService _dialogService;
    private readonly ILicenseService _licenseService;

    public ObservableCollection<Contract> PreviousContracts { get; } = new();
    public ObservableCollection<Contract> ImportedContracts { get; } = new();

    [ObservableProperty]
    private Contract? selectedPreviousContract;

    [ObservableProperty]
    private Contract? selectedImportedContract;

    public MainViewModel Main => _mainViewModel;

    [ObservableProperty]
    private bool isMainViewVisible;

    public IAsyncRelayCommand OpenMainCommand { get; }
    public IRelayCommand OpenSettingsCommand { get; }
    public IRelayCommand ShowHomeCommand { get; }
    public IAsyncRelayCommand<Contract> DeleteImportedContractCommand { get; }

    public LandingViewModel(MainViewModel mainViewModel,
                            SettingsService settings,
                            IContractRepository importedRepo,
                            PreviousContractRepository previousRepo,
                            ImportService importService,
                            DialogService dialogService,
                            ILicenseService licenseService)
    {
        _mainViewModel = mainViewModel;
        _settings = settings;
        _importedRepo = importedRepo;
        _previousRepo = previousRepo;
        _importService = importService;
        _dialogService = dialogService;
        _licenseService = licenseService;
        OpenMainCommand = new AsyncRelayCommand(OpenMainAsync);
        OpenSettingsCommand = new RelayCommand(OpenSettings);
        ShowHomeCommand = new RelayCommand(ShowHome);
        DeleteImportedContractCommand = new AsyncRelayCommand<Contract>(DeleteImportedContractAsync);
    }

    private async Task OpenMainAsync()
    {
        await _mainViewModel.Contracts.LoadAsync();
        IsMainViewVisible = true;
    }

    private void OpenSettings()
    {
        var vm = new SettingsViewModel(_settings);
        var win = new SettingsWindow { DataContext = vm };
        win.ShowDialog();
    }

    private void ShowHome()
    {
        IsMainViewVisible = false;
    }

    public async Task LoadAsync()
    {
        PreviousContracts.Clear();
        var prev = await _previousRepo.GetAllAsync(new FilterOptions());
        foreach (var c in prev)
            PreviousContracts.Add(c);

        ImportedContracts.Clear();
        var imp = await _importedRepo.GetAllAsync(new FilterOptions());
        foreach (var c in imp)
            ImportedContracts.Add(c);
    }

    public async Task OpenContractAsync(Contract? contract, bool isPrevious)
    {
        if (contract == null)
            return;

        var repo = isPrevious ? (IContractRepository)_previousRepo : _importedRepo;
        var model = await repo.GetByIdAsync(contract.Id);
        if (model == null) return;

        await _previousRepo.AddOrUpdateAsync(model);

        var vm = new ContractEditViewModel(repo, _importService, _dialogService, _licenseService);
        vm.LoadFromModel(model);
        var win = new ContractWindow { DataContext = vm };
        win.ShowDialog();
        await LoadAsync();
    }

    private async Task DeleteImportedContractAsync(Contract? contract)
    {
        if (contract == null)
            return;
        var created = contract.CreatedUtc.ToLocalTime();
        var message = $"Are you sure you want to delete \"{contract.Title}\" created on {created:yyyy-MM-dd}?";
        if (MessageBox.Show(message, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
            return;
        await _importedRepo.DeleteAsync(contract.Id);
        await LoadAsync();
    }
}
