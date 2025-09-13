using System.Windows;
using System.Windows.Input;
using PaperTrail.App.ViewModels;

namespace PaperTrail.App;

public partial class LandingWindow : Window
{
    public LandingWindow()
    {
        InitializeComponent();
        Loaded += LandingWindow_Loaded;
    }
    private async void LandingWindow_Loaded(object sender, RoutedEventArgs e)
    {
        if (DataContext is LandingViewModel vm)
            await vm.LoadAsync();
    }

    private async void PreviousContracts_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is LandingViewModel vm)
            await vm.OpenContractAsync(vm.SelectedPreviousContract, true);
    }

    private async void ImportedContracts_DoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (DataContext is LandingViewModel vm)
            await vm.OpenContractAsync(vm.SelectedImportedContract, false);
    }
}
