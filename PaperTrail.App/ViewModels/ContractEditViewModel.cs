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
using PaperTrail.App.Views;
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

public partial class ContractEditViewModel : ObservableObject, INotifyDataErrorInfo
{
    private readonly IContractRepository _repository;
    private readonly ImportService _importService;
    private readonly DialogService _dialogService;
    private readonly ILicenseService _licenseService;
    private readonly IPartyRepository _partyRepository;
    private readonly CalendarService _calendarService;

    private readonly Dictionary<string, List<string>> _errors = new();
    private DateTime _createdUtc;

    public bool HasErrors => _errors.Any();
    public IEnumerable<string> ErrorList => _errors.SelectMany(kv => kv.Value);
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    IEnumerable INotifyDataErrorInfo.GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return _errors.SelectMany(kv => kv.Value);
        return _errors.TryGetValue(propertyName!, out var list) ? list : Enumerable.Empty<string>();
    }

    // ------------ Public data for XAML controls ------------

    // Enum list for Status ComboBox (avoids XAML enum reflection)
    public IEnumerable<ContractStatus> ContractStatusValues { get; } =
        Enum.GetValues(typeof(ContractStatus)).Cast<ContractStatus>();

    // Core fields
    [ObservableProperty] private Guid id;
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private Party? selectedParty;
    [ObservableProperty] private ObservableCollection<Party> parties = new();
    [ObservableProperty] private string partySearchText = string.Empty;
    [ObservableProperty] private ContractStatus selectedStatus = ContractStatus.Active;

    // Store your true date types in DateOnly? for the domain model
    [ObservableProperty] private DateOnly? effectiveDate;
    [ObservableProperty] private DateOnly? renewalDate;
    [ObservableProperty] private DateOnly? terminationDate;

    [ObservableProperty] private int? renewalTermMonths;
    [ObservableProperty] private int? noticePeriodDays;
    [ObservableProperty] private decimal? valueAmount;

    [ObservableProperty] private ObservableCollection<string> tags = new();
    [ObservableProperty] private string newTagText = string.Empty;

    [ObservableProperty] private ObservableCollection<Attachment> attachments = new();
    [ObservableProperty] private ObservableCollection<Reminder> reminders = new();
    [ObservableProperty] private string? notes;

    [ObservableProperty] private DateOnly? computedNextRenewal;
    [ObservableProperty] private DateOnly? computedNoticeDate;

    [ObservableProperty] private bool hasChanges;
    [ObservableProperty] private bool isPro;
    [ObservableProperty] private bool isReadOnly;

    private string? _computedNextRenewalText;
    public string? ComputedNextRenewalText
    {
        get => _computedNextRenewalText;
        private set => SetProperty(ref _computedNextRenewalText, value);
    }

    private string? _computedNoticeDateText;
    public string? ComputedNoticeDateText
    {
        get => _computedNoticeDateText;
        private set => SetProperty(ref _computedNoticeDateText, value);
    }

    // DatePicker-friendly proxies (bind DatePicker.SelectedDate to these)
    public DateTime? EffectiveDateDateTime
    {
        get => EffectiveDate.HasValue ? new DateTime(EffectiveDate.Value.Year, EffectiveDate.Value.Month, EffectiveDate.Value.Day) : (DateTime?)null;
        set
        {
            var newDo = value.HasValue ? new DateOnly(value.Value.Year, value.Value.Month, value.Value.Day) : (DateOnly?)null;
            if (EffectiveDate != newDo)
                EffectiveDate = newDo; // triggers OnEffectiveDateChanged
        }
    }

    public DateTime? RenewalDateDateTime
    {
        get => RenewalDate.HasValue ? new DateTime(RenewalDate.Value.Year, RenewalDate.Value.Month, RenewalDate.Value.Day) : (DateTime?)null;
        set
        {
            var newDo = value.HasValue ? new DateOnly(value.Value.Year, value.Value.Month, value.Value.Day) : (DateOnly?)null;
            if (RenewalDate != newDo)
                RenewalDate = newDo; // triggers OnRenewalDateChanged
        }
    }

    public DateTime? TerminationDateDateTime
    {
        get => TerminationDate.HasValue ? new DateTime(TerminationDate.Value.Year, TerminationDate.Value.Month, TerminationDate.Value.Day) : (DateTime?)null;
        set
        {
            var newDo = value.HasValue ? new DateOnly(value.Value.Year, value.Value.Month, value.Value.Day) : (DateOnly?)null;
            if (TerminationDate != newDo)
                TerminationDate = newDo; // triggers OnTerminationDateChanged
        }
    }

    // ------------ Commands ------------

    public IAsyncRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }
    public IRelayCommand DeleteCommand { get; }
    public IRelayCommand AddTagCommand { get; }
    public IRelayCommand<string> RemoveTagCommand { get; }
    public IRelayCommand AddPartyCommand { get; }
    public IAsyncRelayCommand AddAttachmentCommand { get; }
    public IRelayCommand<Attachment> RemoveAttachmentCommand { get; }
    public IRelayCommand<Attachment> OpenAttachmentCommand { get; }
    public IRelayCommand<Attachment> RevealAttachmentCommand { get; }
    public IAsyncRelayCommand<IDataObject> DragDropAttachmentCommand { get; }
    public IRelayCommand AddReminderCommand { get; }
    public IRelayCommand<Reminder> RemoveReminderCommand { get; }

    // ------------ ctor ------------

    public ContractEditViewModel(
        IContractRepository repository,
        ImportService importService,
        DialogService dialogService,
        ILicenseService licenseService,
        IPartyRepository partyRepository,
        CalendarService calendarService)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _importService = importService ?? throw new ArgumentNullException(nameof(importService));
        _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
        _licenseService = licenseService ?? throw new ArgumentNullException(nameof(licenseService));
        _partyRepository = partyRepository ?? throw new ArgumentNullException(nameof(partyRepository));
        _calendarService = calendarService ?? throw new ArgumentNullException(nameof(calendarService));

        isPro = _licenseService.IsPro;

        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
        CancelCommand = new RelayCommand(OnCancel);
        DeleteCommand = new RelayCommand(OnDelete);
        AddTagCommand = new RelayCommand(AddTag);
        RemoveTagCommand = new RelayCommand<string>(RemoveTag);
        AddPartyCommand = new RelayCommand(AddParty);
        AddAttachmentCommand = new AsyncRelayCommand(AddAttachmentAsync);
        RemoveAttachmentCommand = new RelayCommand<Attachment>(RemoveAttachment);
        OpenAttachmentCommand = new RelayCommand<Attachment>(OpenAttachment);
        RevealAttachmentCommand = new RelayCommand<Attachment>(RevealAttachmentInExplorer);
        DragDropAttachmentCommand = new AsyncRelayCommand<IDataObject>(OnDropAsync);
        AddReminderCommand = new RelayCommand(AddReminder);
        RemoveReminderCommand = new RelayCommand<Reminder>(r => { if (r != null) Reminders.Remove(r); });

        tags.CollectionChanged += (_, __) => HasChanges = true;
        attachments.CollectionChanged += (_, __) => HasChanges = true;
        reminders.CollectionChanged += (_, __) => HasChanges = true;

        _ = RefreshPartiesAsync();
    }

    // ------------ Loading ------------

    public async Task LoadFromModelAsync(Contract model)
    {
        if (model is null) throw new ArgumentNullException(nameof(model));

        Id = model.Id;
        Title = model.Title ?? string.Empty;

        await RefreshPartiesAsync(model.Counterparty);
        if (SelectedParty == null && Parties.Count > 0)
            SelectedParty = Parties[0];

        SelectedStatus = model.Status;

        EffectiveDate = model.EffectiveDate;
        RenewalDate = model.RenewalDate;
        TerminationDate = model.TerminationDate;

        RenewalTermMonths = model.RenewalTermMonths;
        NoticePeriodDays = model.NoticePeriodDays;
        ValueAmount = model.ValueAmount;

        Tags.Clear();
        var tagString = model.Tags ?? string.Empty;
        foreach (var t in tagString.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            Tags.Add(t);

        Attachments.Clear();
        if (model.Attachments != null)
            foreach (var a in model.Attachments)
                Attachments.Add(a);

        Reminders.Clear();
        if (model.Reminders != null)
            foreach (var r in model.Reminders)
                Reminders.Add(r);

        Notes = model.Notes;
        _createdUtc = model.CreatedUtc;

        RecalculateComputedDates();
        HasChanges = false;
        ClearAllErrors();
        SaveCommand.NotifyCanExecuteChanged();
    }

    // ------------ Validation and change hooks ------------

    private bool CanSave() => !HasErrors && !IsReadOnly;

    private void OnPropertyChangedAndValidate(string propertyName)
    {
        HasChanges = true;
        ValidateProperty(propertyName);
        SaveCommand.NotifyCanExecuteChanged();
        UpdateComputedText();
    }

    partial void OnTitleChanged(string value) => OnPropertyChangedAndValidate(nameof(Title));
    partial void OnSelectedPartyChanged(Party? value) => OnPropertyChangedAndValidate(nameof(SelectedParty));
    partial void OnPartySearchTextChanged(string value) => _ = RefreshPartiesAsync();
    partial void OnSelectedStatusChanged(ContractStatus value) => OnPropertyChangedAndValidate(nameof(SelectedStatus));

    partial void OnEffectiveDateChanged(DateOnly? value)
    {
        OnPropertyChangedAndValidate(nameof(EffectiveDate));
        RecalculateComputedDates();
    }

    partial void OnRenewalDateChanged(DateOnly? value)
    {
        OnPropertyChangedAndValidate(nameof(RenewalDate));
        RecalculateComputedDates();
    }

    partial void OnTerminationDateChanged(DateOnly? value)
    {
        OnPropertyChangedAndValidate(nameof(TerminationDate));
    }

    partial void OnRenewalTermMonthsChanged(int? value)
    {
        OnPropertyChangedAndValidate(nameof(RenewalTermMonths));
        RecalculateComputedDates();
    }

    partial void OnNoticePeriodDaysChanged(int? value)
    {
        OnPropertyChangedAndValidate(nameof(NoticePeriodDays));
        RecalculateComputedDates();
    }

    partial void OnValueAmountChanged(decimal? value) => OnPropertyChangedAndValidate(nameof(ValueAmount));
    partial void OnNotesChanged(string? value) => OnPropertyChangedAndValidate(nameof(Notes));

    public void RecalculateComputedDates()
    {
        DateOnly? next = null;
        if (RenewalDate.HasValue)
        {
            next = RenewalDate;
        }
        else if (EffectiveDate.HasValue && RenewalTermMonths.HasValue && RenewalTermMonths.Value > 0)
        {
            try { next = EffectiveDate.Value.AddMonths(RenewalTermMonths.Value); } catch { /* overflow safety */ }
        }

        ComputedNextRenewal = next;

        if (next.HasValue && NoticePeriodDays.HasValue && NoticePeriodDays.Value >= 0)
        {
            try { ComputedNoticeDate = next.Value.AddDays(-NoticePeriodDays.Value); }
            catch { ComputedNoticeDate = null; }
        }
        else
        {
            ComputedNoticeDate = null;
        }

        UpdateComputedText();
    }

    private void UpdateComputedText()
    {
        ComputedNextRenewalText = ComputedNextRenewal?.ToString("yyyy-MM-dd");
        ComputedNoticeDateText = ComputedNoticeDate?.ToString("yyyy-MM-dd");
    }

    private void ValidateProperty(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(Title):
                ClearErrors(nameof(Title));
                if (string.IsNullOrWhiteSpace(Title))
                    AddError(nameof(Title), "Title is required.");
                else if (Title.Length < 3)
                    AddError(nameof(Title), "Title must be at least 3 characters.");
                break;

            case nameof(SelectedParty):
                ClearErrors(nameof(SelectedParty));
                if (SelectedParty == null)
                    AddError(nameof(SelectedParty), "Party is required.");
                break;

            case nameof(RenewalTermMonths):
                ClearErrors(nameof(RenewalTermMonths));
                if (RenewalTermMonths.HasValue && RenewalTermMonths.Value <= 0)
                    AddError(nameof(RenewalTermMonths), "Renewal term must be > 0.");
                break;

            case nameof(ValueAmount):
                ClearErrors(nameof(ValueAmount));
                if (ValueAmount.HasValue && ValueAmount.Value < 0)
                    AddError(nameof(ValueAmount), "Value must be ≥ 0.");
                break;

            case nameof(EffectiveDate):
            case nameof(RenewalDate):
            case nameof(TerminationDate):
                ValidateDates();
                break;

            case nameof(NoticePeriodDays):
                ClearErrors(nameof(NoticePeriodDays));
                if (NoticePeriodDays.HasValue && NoticePeriodDays.Value < 0)
                    AddError(nameof(NoticePeriodDays), "Notice must be ≥ 0.");
                break;
        }
    }

    private void ValidateDates()
    {
        ClearErrors(nameof(EffectiveDate));
        ClearErrors(nameof(RenewalDate));
        ClearErrors(nameof(TerminationDate));

        if (EffectiveDate.HasValue && RenewalDate.HasValue && EffectiveDate > RenewalDate)
            AddError(nameof(RenewalDate), "Renewal must be on or after the effective date.");

        if (RenewalDate.HasValue && TerminationDate.HasValue && RenewalDate > TerminationDate)
            AddError(nameof(TerminationDate), "Termination must be on or after the renewal date.");
    }

    private void AddError(string prop, string error)
    {
        if (!_errors.TryGetValue(prop, out var list))
        {
            list = new List<string>();
            _errors[prop] = list;
        }
        if (!list.Contains(error))
        {
            list.Add(error);
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop));
            OnPropertyChanged(nameof(ErrorList));
        }
    }

    private void ClearErrors(string prop)
    {
        if (_errors.Remove(prop))
        {
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(prop));
            OnPropertyChanged(nameof(ErrorList));
        }
    }

    private void ClearAllErrors()
    {
        var keys = _errors.Keys.ToList();
        _errors.Clear();
        foreach (var k in keys)
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(k));
        OnPropertyChanged(nameof(ErrorList));
    }

    public List<string> Validate()
    {
        var props = new[]
        {
            nameof(Title), nameof(SelectedParty), nameof(RenewalTermMonths),
            nameof(ValueAmount), nameof(EffectiveDate), nameof(RenewalDate),
            nameof(TerminationDate), nameof(NoticePeriodDays)
        };
        foreach (var p in props)
            ValidateProperty(p);
        return _errors.SelectMany(e => e.Value).ToList();
    }

    // ------------ Save / Delete / Cancel ------------

    private async Task SaveAsync()
    {
        Validate();
        if (HasErrors) return;

        var model = ToModel();

        if (model.Id == Guid.Empty)
        {
            model.Id = Guid.NewGuid();
            model.CreatedUtc = DateTime.UtcNow;
            model.UpdatedUtc = model.CreatedUtc;
            await _repository.AddAsync(model);
        }
        else
        {
            model.CreatedUtc = _createdUtc;
            model.UpdatedUtc = DateTime.UtcNow;
            await _repository.UpdateAsync(model);
        }

        // Regenerate reminders from current details
        var reminderList = ReminderFactory.Create(model).ToList();
        reminderList.AddRange(Reminders.Where(r => r.Type == ReminderType.Custom));
        await _repository.AddRemindersAsync(model.Id, reminderList);

        Reminders.Clear();
        foreach (var r in reminderList)
            Reminders.Add(r);

        HasChanges = false;
    }

    private Contract ToModel()
    {
        return new Contract
        {
            Id = Id,
            Title = Title,
            Counterparty = SelectedParty,
            CounterpartyId = SelectedParty?.Id,
            Status = SelectedStatus,
            EffectiveDate = EffectiveDate,
            RenewalDate = RenewalDate,
            TerminationDate = TerminationDate,
            RenewalTermMonths = RenewalTermMonths,
            NoticePeriodDays = NoticePeriodDays,
            ValueAmount = ValueAmount,
            Tags = string.Join(',', Tags),
            Notes = Notes,
            Attachments = Attachments.ToList(),
            Reminders = Reminders.ToList()
        };
    }

    private void OnCancel()
    {
        if (!HasChanges || MessageBox.Show("Discard changes?", "Confirm",
                MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            CloseWindowForThisVM();
        }
    }

    private async void OnDelete()
    {
        if (Id != Guid.Empty)
        {
            if (MessageBox.Show("Delete this contract?", "Confirm",
                    MessageBoxButton.YesNo, MessageBoxImage.Warning) != MessageBoxResult.Yes)
                return;

            try { await _repository.DeleteAsync(Id); }
            catch (Exception ex)
            {
                MessageBox.Show($"Delete failed: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        CloseWindowForThisVM();
    }

    // ------------ Tags ------------

    private void AddTag()
    {
        if (!string.IsNullOrWhiteSpace(NewTagText))
        {
            var tag = NewTagText.Trim();
            if (!Tags.Contains(tag, StringComparer.OrdinalIgnoreCase))
                Tags.Add(tag);
            NewTagText = string.Empty;
        }
    }

    private void RemoveTag(string? tag)
    {
        if (tag != null)
            Tags.Remove(tag);
    }

    // ------------ Party ------------

    private async Task RefreshPartiesAsync(Party? select = null)
    {
        var list = await _partyRepository.GetAllAsync();
        if (select != null && list.All(p => p.Id != select.Id))
            list.Add(select);
        if (!string.IsNullOrWhiteSpace(PartySearchText))
            list = list.Where(p => p.Name.Contains(PartySearchText, StringComparison.OrdinalIgnoreCase)).ToList();
        Parties.Clear();
        foreach (var p in list)
            Parties.Add(p);
        if (select != null)
            SelectedParty = Parties.FirstOrDefault(p => p.Id == select.Id);
    }

    private async void AddParty()
    {
        var vm = new PartyEditViewModel();
        var win = new PartyEditWindow { DataContext = vm };
        if (win.ShowDialog() == true)
        {
            var party = vm.Model;
            if (party.Id == Guid.Empty)
                party.Id = Guid.NewGuid();
            await _partyRepository.AddAsync(party);
            await RefreshPartiesAsync(party);
        }
    }

    // ------------ Attachments ------------

    private async Task AddAttachmentAsync()
    {
        var file = _dialogService.OpenFile("All Files|*.*");
        if (file == null) return;
        await ImportFileAsync(file);
    }

    private void RemoveAttachment(Attachment? att)
    {
        if (att != null)
            Attachments.Remove(att);
    }

    private async Task OnDropAsync(IDataObject? data)
    {
        if (data == null || !data.GetDataPresent(DataFormats.FileDrop))
            return;

        if (Id == Guid.Empty)
        {
            MessageBox.Show("Please save the contract before adding attachments.", "Info",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var files = (string[])data.GetData(DataFormats.FileDrop);
        foreach (var f in files)
            await ImportFileAsync(f);
    }

    private async Task ImportFileAsync(string path)
    {
        try
        {
            var attachment = await _importService.ImportAsync(Id, path);
            if (Attachments.Any(a => !string.IsNullOrEmpty(a.Hash) && a.Hash == attachment.Hash))
            {
                MessageBox.Show("Duplicate attachment skipped.", "Info",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            Attachments.Add(attachment);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to import file:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void OpenAttachment(Attachment? att)
    {
        if (att == null) return;
        if (string.IsNullOrWhiteSpace(att.FilePath) || !File.Exists(att.FilePath))
        {
            MessageBox.Show("File not found on disk.", "Open Attachment",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = att.FilePath,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to open file:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void RevealAttachmentInExplorer(Attachment? att)
    {
        if (att == null) return;
        if (string.IsNullOrWhiteSpace(att.FilePath) || !File.Exists(att.FilePath))
        {
            MessageBox.Show("File not found on disk.", "Reveal in Explorer",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        try
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "explorer.exe",
                Arguments = $"/select,\"{att.FilePath}\"",
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Unable to reveal file:\n{ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // ------------ Reminders ------------

    private void AddReminder()
    {
        var win = new ReminderWindow();
        if (win.ShowDialog() == true)
        {
            var reminder = new Reminder
            {
                Id = Guid.NewGuid(),
                Type = ReminderType.Custom,
                DueUtc = win.SelectedDate.ToUniversalTime(),
                CreatedUtc = DateTime.UtcNow,
                Note = win.Note
            };
            Reminders.Add(reminder);
            if (win.AddToCalendar)
                _calendarService.AddToCalendar(reminder, Title);
        }
    }

    // ------------ Window helpers ------------

    public bool ConfirmClose()
    {
        if (!HasChanges) return true;
        return MessageBox.Show("Discard changes?", "Confirm",
                   MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }

    private void CloseWindowForThisVM()
    {
        var win = Application.Current.Windows.OfType<Window>()
                     .SingleOrDefault(w => ReferenceEquals(w.DataContext, this));
        win?.Close();
    }
}
