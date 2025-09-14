using System.Windows.Controls;
using System.Windows.Input;
using PaperTrail.App.ViewModels;

namespace PaperTrail.App.Views;

public partial class ContractListView : UserControl
{
    public ContractListView()
    {
        InitializeComponent();
    }

    private async void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is ContractListViewModel vm)
        {
            await vm.OpenContractAsync(vm.SelectedContract);
        }
    }
}
