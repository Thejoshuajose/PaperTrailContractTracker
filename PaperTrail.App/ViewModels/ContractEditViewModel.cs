using CommunityToolkit.Mvvm.ComponentModel;
using PaperTrail.Core.Models;

namespace PaperTrail.App.ViewModels;

public partial class ContractEditViewModel : ObservableObject
{
    [ObservableProperty]
    private Contract model = new();
}
