using CommunityToolkit.Mvvm.ComponentModel;

namespace PaperTrail.App.ViewModels;

public class MainViewModel : ObservableObject
{
    public ContractListViewModel Contracts { get; }

    public MainViewModel(ContractListViewModel contracts)
    {
        Contracts = contracts;
    }
}
