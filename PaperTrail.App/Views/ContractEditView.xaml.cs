using System.Windows;
using System.Windows.Controls;

namespace PaperTrail.App.Views;

public partial class ContractEditView : UserControl
{
    public ContractEditView()
    {
        // Required to load the associated XAML. Without this constructor,
        // the user control renders blank because InitializeComponent is never called.
        InitializeComponent();
    }

    private void AttachmentDragOver(object sender, DragEventArgs e)
    {
        if (e.Data.GetDataPresent(DataFormats.FileDrop))
            e.Effects = DragDropEffects.Copy;
        else
            e.Effects = DragDropEffects.None;
        e.Handled = true;
    }

    private async void AttachmentDrop(object sender, DragEventArgs e)
    {
        if (DataContext is ViewModels.ContractEditViewModel vm)
        {
            var data = e.Data;
            if (vm.DragDropAttachmentCommand.CanExecute(data))
                await vm.DragDropAttachmentCommand.ExecuteAsync(data);
        }
    }

    private async void PartySearch_LostFocus(object sender, RoutedEventArgs e)
    {
        if (DataContext is ViewModels.ContractEditViewModel vm)
            await vm.EnsurePartySelectedAsync();
    }
}
