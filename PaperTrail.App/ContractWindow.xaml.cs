using System.Windows;

namespace PaperTrail.App;

public partial class ContractWindow : Window
{
    public ContractWindow()
    {
        InitializeComponent();
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DataContext is ViewModels.ContractEditViewModel vm)
        {
            e.Cancel = !vm.ConfirmClose();
        }
    }
}
