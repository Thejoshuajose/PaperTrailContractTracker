using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PaperTrail.App.Services;
using PaperTrail.Core.Models;
using PaperTrail.Core.Repositories;
using PaperTrail.Core.Services;
using System.Collections;
using System.Collections.ObjectModel;
using System.Windows;
using System.ComponentModel;

namespace PaperTrail.App.ViewModels;

public partial class ContractEditViewModel : ObservableObject, INotifyDataErrorInfo

{
    private readonly IContractRepository _repository;
    private readonly ImportService _importService;
    private readonly DialogService _dialogService;
    private readonly ILicenseService _licenseService;

    private readonly Dictionary<string, List<string>> _errors = new();

    public bool HasErrors => _errors.Any();
    public IEnumerable<string> ErrorList => _errors.SelectMany(kv => kv.Value);
    public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
    IEnumerable INotifyDataErrorInfo.GetErrors(string? propertyName)
    {
        if (string.IsNullOrEmpty(propertyName))
            return _errors.SelectMany(kv => kv.Value);
        return _errors.TryGetValue(propertyName, out var list) ? list : Enumerable.Empty<string>();
    }

    [ObservableProperty] private Guid id;
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private Party? selectedParty;
    [ObservableProperty] private ObservableCollection<Party> parties = new();
    [ObservableProperty] private ContractStatus selectedStatus = ContractStatus.Active;
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

    public ContractEditViewModel(IContractRepository repository, ImportService importService, DialogService dialogService, ILicenseService licenseService)
    {
        _repository = repository;
        _importService = importService;
        _dialogService = dialogService;
        _licenseService = licenseService;
        isPro = _licenseService.IsPro;

        SaveCommand = new AsyncRelayCommand(SaveAsync, CanSave);
        CancelCommand = new RelayCommand(OnCancel);
        DeleteCommand = new RelayCommand(OnDelete);
        AddTagCommand = new RelayCommand(AddTag);
        RemoveTagCommand = new RelayCommand<string>(RemoveTag);
        AddPartyCommand = new RelayCommand(AddParty);
        AddAttachmentCommand = new AsyncRelayCommand(AddAttachmentAsync);
        RemoveAttachmentCommand = new RelayCommand<Attachment>(RemoveAttachment);
        OpenAttachmentCommand = new RelayCommand<Attachment>(_ => { });
        RevealAttachmentCommand = new RelayCommand<Attachment>(_ => { });
        DragDropAttachmentCommand = new AsyncRelayCommand<IDataObject>(OnDropAsync);
        AddReminderCommand = new RelayCommand(AddReminder);
        RemoveReminderCommand = new RelayCommand<Reminder>(r => { if (r != null) Reminders.Remove(r); });

        tags.CollectionChanged += (_, __) => HasChanges = true;
        attachments.CollectionChanged += (_, __) => HasChanges = true;
        reminders.CollectionChanged += (_, __) => HasChanges = true;
    }

    private bool CanSave() => !HasErrors;

    private void OnPropertyChangedAndValidate(string propertyName)
    {
        HasChanges = true;
        ValidateProperty(propertyName);
        SaveCommand.NotifyCanExecuteChanged();
    }

    partial void OnTitleChanged(string value) => OnPropertyChangedAndValidate(nameof(Title));
    partial void OnSelectedPartyChanged(Party? value) => OnPropertyChangedAndValidate(nameof(SelectedParty));
    partial void OnEffectiveDateChanged(DateOnly? value) { OnPropertyChangedAndValidate(nameof(EffectiveDate)); RecalculateComputedDates(); }
    partial void OnRenewalDateChanged(DateOnly? value) { OnPropertyChangedAndValidate(nameof(RenewalDate)); RecalculateComputedDates(); }
    partial void OnTerminationDateChanged(DateOnly? value) { OnPropertyChangedAndValidate(nameof(TerminationDate)); }
    partial void OnRenewalTermMonthsChanged(int? value) { OnPropertyChangedAndValidate(nameof(RenewalTermMonths)); RecalculateComputedDates(); }
    partial void OnNoticePeriodDaysChanged(int? value) { OnPropertyChangedAndValidate(nameof(NoticePeriodDays)); RecalculateComputedDates(); }
    partial void OnValueAmountChanged(decimal? value) => OnPropertyChangedAndValidate(nameof(ValueAmount));
    partial void OnNotesChanged(string? value) => OnPropertyChangedAndValidate(nameof(Notes));

    public void RecalculateComputedDates()
    {
        DateOnly? next = null;
        if (RenewalDate.HasValue)
        {
            next = RenewalDate;
        }
        else if (EffectiveDate.HasValue && RenewalTermMonths.HasValue)
        {
            try
            {
                next = EffectiveDate.Value.AddMonths(RenewalTermMonths.Value);
            }
            catch { }
        }
        ComputedNextRenewal = next;
        if (next.HasValue && NoticePeriodDays.HasValue)
        {
            try
            {
                ComputedNoticeDate = next.Value.AddDays(-NoticePeriodDays.Value);
            }
            catch { ComputedNoticeDate = null; }
        }
        else
        {
            ComputedNoticeDate = null;
        }
    }

    private void ValidateProperty(string propertyName)
    {
        switch (propertyName)
        {
            case nameof(Title):
                ClearErrors(nameof(Title));
                if (string.IsNullOrWhiteSpace(Title))
                    AddError(nameof(Title), "Title is required");
                else if (Title.Length < 3)
                    AddError(nameof(Title), "Title must be at least 3 characters");
                break;
            case nameof(SelectedParty):
                ClearErrors(nameof(SelectedParty));
                if (SelectedParty == null)
                    AddError(nameof(SelectedParty), "Party required");
                break;
            case nameof(RenewalTermMonths):
                ClearErrors(nameof(RenewalTermMonths));
                if (RenewalTermMonths.HasValue && RenewalTermMonths.Value <= 0)
                    AddError(nameof(RenewalTermMonths), "Renewal term must be > 0");
                break;
            case nameof(ValueAmount):
                ClearErrors(nameof(ValueAmount));
                if (ValueAmount.HasValue && ValueAmount.Value < 0)
                    AddError(nameof(ValueAmount), "Value must be >= 0");
                break;
            case nameof(EffectiveDate):
            case nameof(RenewalDate):
            case nameof(TerminationDate):
                ValidateDates();
                break;
            case nameof(NoticePeriodDays):
                ClearErrors(nameof(NoticePeriodDays));
                if (NoticePeriodDays.HasValue && NoticePeriodDays.Value < 0)
                    AddError(nameof(NoticePeriodDays), "Notice must be >= 0");
                break;
        }
    }

    private void ValidateDates()
    {
        ClearErrors(nameof(EffectiveDate));
        ClearErrors(nameof(RenewalDate));
        ClearErrors(nameof(TerminationDate));
        if (EffectiveDate.HasValue && RenewalDate.HasValue && EffectiveDate > RenewalDate)
            AddError(nameof(RenewalDate), "Renewal must be after effective date");
        if (RenewalDate.HasValue && TerminationDate.HasValue && RenewalDate > TerminationDate)
            AddError(nameof(TerminationDate), "Termination must be after renewal");
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

    public List<string> Validate()
    {
        var props = new[] { nameof(Title), nameof(SelectedParty), nameof(RenewalTermMonths), nameof(ValueAmount), nameof(EffectiveDate), nameof(RenewalDate), nameof(TerminationDate), nameof(NoticePeriodDays) };
        foreach (var p in props)
            ValidateProperty(p);
        return _errors.SelectMany(e => e.Value).ToList();
    }

    private async Task SaveAsync()
    {
        Validate();
        if (HasErrors)
            return;
        var model = ToModel();
        if (model.Id == Guid.Empty)
        {
            model.Id = Guid.NewGuid();
            await _repository.AddAsync(model);
        }
        else
        {
            await _repository.UpdateAsync(model);
        }
        var reminderList = ReminderFactory.Create(model).ToList();
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
        if (!HasChanges || MessageBox.Show("Discard changes?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
        {
            Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
        }
    }

    private void OnDelete()
    {
        if (Id != Guid.Empty)
        {
            // simple delete
            _ = _repository.DeleteAsync(Id);
        }
        Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.DataContext == this)?.Close();
    }

    private void AddTag()
    {
        if (!string.IsNullOrWhiteSpace(NewTagText))
        {
            Tags.Add(NewTagText.Trim());
            NewTagText = string.Empty;
        }
    }

    private void RemoveTag(string? tag)
    {
        if (tag != null)
            Tags.Remove(tag);
    }

    private void AddParty()
    {
        var p = new Party { Id = Guid.NewGuid(), Name = "New Party" };
        Parties.Add(p);
        SelectedParty = p;
    }

    private async Task AddAttachmentAsync()
    {
        var file = _dialogService.OpenFile("All Files|*.*");
        if (file == null)
            return;
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
        var files = (string[])data.GetData(DataFormats.FileDrop);
        foreach (var f in files)
            await ImportFileAsync(f);
    }

    private async Task ImportFileAsync(string path)
    {
        var attachment = await _importService.ImportAsync(Id, path);
        if (Attachments.Any(a => a.Hash == attachment.Hash))
        {
            MessageBox.Show("Duplicate attachment", "Info");
            return;
        }
        Attachments.Add(attachment);
    }

    private void AddReminder()
    {
        Reminders.Add(new Reminder { Id = Guid.NewGuid(), Type = ReminderType.Custom, DueUtc = DateTime.UtcNow });
    }

    public bool ConfirmClose()
    {
        if (!HasChanges)
            return true;
        return MessageBox.Show("Discard changes?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
    }
}
