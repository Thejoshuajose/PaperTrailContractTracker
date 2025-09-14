using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using PaperTrail.Core.Models;
using System.Linq;
using System.Windows;

namespace PaperTrail.App.ViewModels;

public partial class PartyEditViewModel : ObservableObject
{
    [ObservableProperty]
    private Party model = new();

    public IRelayCommand SaveCommand { get; }
    public IRelayCommand CancelCommand { get; }

    public PartyEditViewModel()
    {
        SaveCommand = new RelayCommand(OnSave);
        CancelCommand = new RelayCommand(OnCancel);
    }

    private void OnSave()
    {
        CloseWindow(true);
    }

    private void OnCancel()
    {
        CloseWindow(false);
    }

    private void CloseWindow(bool result)
    {
        var win = Application.Current.Windows.OfType<Window>()
                     .SingleOrDefault(w => ReferenceEquals(w.DataContext, this));
        if (win != null)
            win.DialogResult = result;
    }
}
