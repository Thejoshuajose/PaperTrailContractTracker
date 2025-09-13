using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PaperTrail.Core.DTO;
using PaperTrail.Core.Models;
using PaperTrail.Core.Repositories;
using PaperTrail.Core.Services;
using PaperTrail.App.Services;
using System.Collections.ObjectModel;


namespace PaperTrail.App.ViewModels;

public partial class ContractListViewModel : ObservableObject
{
    private readonly IContractRepository _contracts;
    private readonly ImportService _import;
    private readonly ExportService _export;
    private readonly ILicenseService _license;

    public ObservableCollection<Contract> Items { get; } = new();

    [ObservableProperty]
    private Contract? selectedContract;

    [ObservableProperty]
    private string? search;

    public IRelayCommand RefreshCommand { get; }
    public IRelayCommand ImportCommand { get; }
    public IAsyncRelayCommand ExportCommand { get; }

    public ContractListViewModel(IContractRepository contracts, ImportService import, ExportService export, ILicenseService license)
    {
        _contracts = contracts;
        _import = import;
        _export = export;
        _license = license;
        RefreshCommand = new RelayCommand(async () => await LoadAsync());
        ImportCommand = new RelayCommand(async () => await ImportAsync());
        ExportCommand = new AsyncRelayCommand(ExportAsync);
    }

    public async Task LoadAsync()
    {
        Items.Clear();
        var list = await _contracts.GetAllAsync(new FilterOptions { Search = Search });
        foreach (var c in list)
            Items.Add(c);
    }

    private async Task ImportAsync()
    {
        // Placeholder for dialog integration
    }

    private async Task ExportAsync()
    {
        if (!_license.IsPro) return;
        var data = await _export.ExportAsync(new FilterOptions { Search = Search });
        // Save file via dialog service outside
    }
}
