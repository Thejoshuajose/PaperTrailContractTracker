using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CsvHelper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Toolkit.Uwp.Notifications;
using MongoDB.Bson;
using MongoDB.Driver;
using Ookii.Dialogs.Wpf;
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


namespace PaperTrail.App.ViewModels;

public partial class ContractListViewModel : ObservableObject
{
    private readonly IContractRepository _contracts;
    private readonly ImportService _import;
    private readonly ExportService _export;
    private readonly DialogService _dialog;
    private readonly ILicenseService _license;

    public ObservableCollection<Contract> Items { get; } = new();

    [ObservableProperty]
    private Contract? selectedContract;

    [ObservableProperty]
    private string? searchText;

    public IAsyncRelayCommand NewCommand { get; }
    public IAsyncRelayCommand RefreshCommand { get; }
    public IAsyncRelayCommand ImportCommand { get; }
    public IAsyncRelayCommand ExportCommand { get; }

    public ContractListViewModel(IContractRepository contracts, ImportService import, ExportService export, DialogService dialog, ILicenseService license)
    {
        _contracts = contracts;
        _import = import;
        _export = export;
        _dialog = dialog;
        _license = license;
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
        var repo = App.Services.GetRequiredService<ICustomContractRepository>();
        var selector = new NewContractWindow(repo);
        if (selector.ShowDialog() != true)
            return;

        var title = string.IsNullOrWhiteSpace(selector.SelectedTitle) ? "New Contract" : selector.SelectedTitle;
        var contract = new Contract { Id = Guid.NewGuid(), Title = title };
        await _contracts.AddAsync(contract);
        var vm = new ContractEditViewModel(_contracts, _import, _dialog, _license);
        vm.LoadFromModel(contract);
        var win = new ContractWindow { DataContext = vm };
        win.ShowDialog();
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

        var model = await _contracts.GetByIdAsync(SelectedContract.Id);
        if (model != null)
        {
            var vm = new ContractEditViewModel(_contracts, _import, _dialog, _license);
            vm.LoadFromModel(model);
            var win = new ContractWindow { DataContext = vm };
            win.ShowDialog();
        }
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
