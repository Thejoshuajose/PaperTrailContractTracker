using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PaperTrail.Core.DTO;
using PaperTrail.Core.Models;
using PaperTrail.Core.Repositories;
using PaperTrail.Core.Services;
using PaperTrail.App.Services;
using System.Collections.ObjectModel;
using System;
using System.IO;

namespace PaperTrail.App.ViewModels;

public partial class ContractListViewModel : ObservableObject
{
    private readonly IContractRepository _contracts;
    private readonly ImportService _import;
    private readonly ExportService _export;
    private readonly ILicenseService _license;
    private readonly DialogService _dialog;

    public ObservableCollection<Contract> Items { get; } = new();

    [ObservableProperty]
    private Contract? selectedContract;

    [ObservableProperty]
    private string? search;

    public IRelayCommand NewCommand { get; }
    public IRelayCommand RefreshCommand { get; }
    public IRelayCommand ImportCommand { get; }
    public IAsyncRelayCommand ExportCommand { get; }

    public ContractListViewModel(IContractRepository contracts, ImportService import, ExportService export, ILicenseService license, DialogService dialog)
    {
        _contracts = contracts;
        _import = import;
        _export = export;
        _license = license;
        _dialog = dialog;
        RefreshCommand = new RelayCommand(async () => await LoadAsync());
        ImportCommand = new RelayCommand(async () => await ImportAsync());
        ExportCommand = new AsyncRelayCommand(ExportAsync);
        NewCommand = new RelayCommand(async () => await NewAsync());
    }

    public async Task LoadAsync()
    {
        Items.Clear();
        var list = await _contracts.GetAllAsync(new FilterOptions { Search = Search });
        foreach (var c in list)
            Items.Add(c);
    }

    private async Task NewAsync()
    {
        var contract = new Contract { Id = Guid.NewGuid(), Title = "New Contract" };
        await _contracts.AddAsync(contract);
        await LoadAsync();
    }

    private async Task ImportAsync()
    {
        if (SelectedContract == null) return;
        var file = _dialog.OpenFile("PDF Files|*.pdf");
        if (file == null) return;
        await _import.ImportAsync(SelectedContract.Id, file);
    }

    private async Task ExportAsync()
    {
        if (!_license.IsPro) return;
        var data = await _export.ExportAsync(new FilterOptions { Search = Search });
        if (data == null) return;
        var file = _dialog.SaveFile("CSV Files|*.csv", ".csv");
        if (file == null) return;
        File.WriteAllBytes(file, data);
    }
}
