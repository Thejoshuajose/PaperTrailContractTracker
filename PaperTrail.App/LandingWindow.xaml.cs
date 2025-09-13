using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using PaperTrail.App.ViewModels;

namespace PaperTrail.App;

public partial class LandingWindow : Window
{
    public LandingWindow()
    {
        InitializeComponent();
        DataContext = ((App)Application.Current).Services.GetRequiredService<LandingViewModel>();
    }
}
