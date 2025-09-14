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
    private readonly IPartyRepository _partyRepository;
    private readonly CalendarService _calendarService;

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
    public IAsyncRelayCommand<Contract> DuplicateImportedContractCommand { get; }
    public IAsyncRelayCommand OpenClosedContractsCommand { get; }
    public IAsyncRelayCommand<Contract> ClosePreviousContractCommand { get; }
    public IAsyncRelayCommand<Contract> CloseImportedContractCommand { get; }

    public LandingViewModel(MainViewModel mainViewModel,
                            SettingsService settings,
                            IContractRepository importedRepo,
                            PreviousContractRepository previousRepo,
                            ImportService importService,
                            DialogService dialogService,
                            ILicenseService licenseService,
                            IPartyRepository partyRepository,
                            CalendarService calendarService)
    {
        _mainViewModel = mainViewModel;
        _settings = settings;
        _importedRepo = importedRepo;
        _previousRepo = previousRepo;
        _importService = importService;
        _dialogService = dialogService;
        _licenseService = licenseService;
        _partyRepository = partyRepository;
        _calendarService = calendarService;
        OpenMainCommand = new AsyncRelayCommand(OpenMainAsync);
        OpenSettingsCommand = new RelayCommand(OpenSettings);
        ShowHomeCommand = new RelayCommand(ShowHome);
        DeleteImportedContractCommand = new AsyncRelayCommand<Contract>(DeleteImportedContractAsync);
        DuplicateImportedContractCommand = new AsyncRelayCommand<Contract>(DuplicateImportedContractAsync);
        OpenClosedContractsCommand = new AsyncRelayCommand(OpenClosedContractsAsync);
        ClosePreviousContractCommand = new AsyncRelayCommand<Contract>(c => CloseContractAsync(c, true));
        CloseImportedContractCommand = new AsyncRelayCommand<Contract>(c => CloseContractAsync(c, false));
    }

    private async Task OpenMainAsync()
    {
        _mainViewModel.Contracts.Statuses = Enum.GetValues<ContractStatus>()
            .Where(s => s != ContractStatus.Archived)
            .ToArray();
        await _mainViewModel.Contracts.LoadAsync();
        IsMainViewVisible = true;
    }

    private async Task OpenClosedContractsAsync()
    {
        _mainViewModel.Contracts.Statuses = new[] { ContractStatus.Archived };
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
        _ = LoadAsync();
    }

    public async Task LoadAsync()
    {
        PreviousContracts.Clear();
        var filter = new FilterOptions
        {
            Statuses = Enum.GetValues<ContractStatus>()
                .Where(s => s != ContractStatus.Archived)
                .ToArray()
        };
        var prev = await _previousRepo.GetAllAsync(filter);
        foreach (var c in prev)
            PreviousContracts.Add(c);

        ImportedContracts.Clear();
        var imp = await _importedRepo.GetAllAsync(filter);
        foreach (var c in imp)
            ImportedContracts.Add(c);
    }

    private async Task CloseContractAsync(Contract? contract, bool isPrevious)
    {
        if (contract == null)
            return;
        contract.Status = ContractStatus.Archived;
        contract.UpdatedUtc = DateTime.UtcNow;
        var repo = isPrevious ? (IContractRepository)_previousRepo : _importedRepo;
        await repo.UpdateAsync(contract);
        await LoadAsync();
        await _mainViewModel.Contracts.LoadAsync();
    }

    public async Task OpenContractAsync(Contract? contract, bool isPrevious)
    {
        if (contract == null)
            return;

        var repo = isPrevious ? (IContractRepository)_previousRepo : _importedRepo;
        var model = await repo.GetByIdAsync(contract.Id);
        if (model == null) return;

        await _previousRepo.AddOrUpdateAsync(model);

        var vm = new ContractEditViewModel(repo, _importService, _dialogService, _licenseService, _partyRepository, _calendarService);
        vm.IsReadOnly = model.Status == ContractStatus.Archived;
        await vm.LoadFromModelAsync(model);
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

    private async Task DuplicateImportedContractAsync(Contract? contract)
    {
        if (contract == null)
            return;

        var copy = new Contract
        {
            Id = Guid.NewGuid(),
            Title = contract.Title,
            CounterpartyId = contract.CounterpartyId,
            Counterparty = contract.Counterparty,
            Status = contract.Status,
            EffectiveDate = contract.EffectiveDate,
            RenewalDate = contract.RenewalDate,
            TerminationDate = contract.TerminationDate,
            RenewalTermMonths = contract.RenewalTermMonths,
            NoticePeriodDays = contract.NoticePeriodDays,
            Tags = contract.Tags,
            ValueAmount = contract.ValueAmount,
            Notes = contract.Notes,
            CreatedUtc = DateTime.UtcNow,
            UpdatedUtc = DateTime.UtcNow,
            Attachments = new List<Attachment>(),
            Reminders = new List<Reminder>()
        };

        await _importedRepo.AddAsync(copy);

        if (contract.Attachments != null)
        {
            foreach (var att in contract.Attachments)
            {
                var newAtt = new Attachment
                {
                    Id = Guid.NewGuid(),
                    ContractId = copy.Id,
                    Contract = copy,
                    FileName = att.FileName,
                    FilePath = att.FilePath,
                    Hash = att.Hash,
                    CreatedUtc = att.CreatedUtc,
                    MissingFile = att.MissingFile
                };
                await _importedRepo.AddAttachmentAsync(copy.Id, newAtt);
                copy.Attachments.Add(newAtt);
            }
        }

        var vm = new ContractEditViewModel(_importedRepo, _importService, _dialogService, _licenseService, _partyRepository, _calendarService);
        await vm.LoadFromModelAsync(copy);
        var win = new ContractWindow { DataContext = vm };
        win.ShowDialog();
        await LoadAsync();
    }
}
