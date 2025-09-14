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
}
