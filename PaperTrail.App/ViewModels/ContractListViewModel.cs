using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PaperTrail.Core.DTO;
using PaperTrail.Core.Models;
using PaperTrail.Core.Repositories;
using PaperTrail.App.Services;
using System.Collections.ObjectModel;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using PaperTrail.App;

namespace PaperTrail.App.ViewModels;

public partial class ContractListViewModel : ObservableObject
{
    private readonly IContractRepository _contracts;
    private readonly ImportService _import;
    private readonly ExportService _export;
    private readonly DialogService _dialog;

    public ObservableCollection<Contract> Items { get; } = new();

    [ObservableProperty]
    private Contract? selectedContract;

    [ObservableProperty]
    private string? searchText;

    public IAsyncRelayCommand NewCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand ImportCommand { get; }
    public IAsyncRelayCommand ExportCommand { get; }

    public ContractListViewModel(IContractRepository contracts, ImportService import, ExportService export, DialogService dialog)
    {
        _contracts = contracts;
        _import = import;
        _export = export;
        _dialog = dialog;
        RefreshCommand = new AsyncRelayCommand(LoadAsync);
        ImportCommand = new AsyncRelayCommand(ImportAsync);
        ExportCommand = new AsyncRelayCommand(ExportAsync);
        NewCommand = new AsyncRelayCommand(NewAsync);
    }

    public async Task LoadAsync()
    {
        Items.Clear();
        var list = await _contracts.GetAllAsync(new FilterOptions { SearchText = SearchText });
        foreach (var c in list)
            Items.Add(c);
    }

    private async Task NewAsync()
    {
        var contract = new Contract { Id = Guid.NewGuid(), Title = "New Contract" };
        await _contracts.AddAsync(contract);
        var vm = new ContractEditViewModel { Model = contract };
        var win = new ContractWindow { DataContext = vm };
        win.ShowDialog();
        await _contracts.UpdateAsync(contract);
        await LoadAsync();
        SelectedContract = Items.FirstOrDefault(c => c.Id == contract.Id);
    }

    private async Task ImportAsync()
    {
        if (SelectedContract == null)
        {
            MessageBox.Show("Please select a contract first.", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var file = _dialog.OpenFile("PDF Files|*.pdf");
        if (file == null) return;
        await _import.ImportAsync(SelectedContract.Id, file);
        MessageBox.Show("Import completed.", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private async Task ExportAsync()
    {
        var data = await _export.ExportAsync(new FilterOptions { SearchText = SearchText });
        if (data == null || data.Length == 0)
        {
            MessageBox.Show("No data to export.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var file = _dialog.SaveFile("CSV Files|*.csv", ".csv");
        if (file == null) return;
        File.WriteAllBytes(file, data);
        MessageBox.Show("Export completed.", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
