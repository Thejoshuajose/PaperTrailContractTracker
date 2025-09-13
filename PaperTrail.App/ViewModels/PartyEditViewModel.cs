using CommunityToolkit.Mvvm.ComponentModel;
using PaperTrail.Core.Models;

namespace PaperTrail.App.ViewModels;

public partial class PartyEditViewModel : ObservableObject
{
    [ObservableProperty]
    private Party model = new();
}
