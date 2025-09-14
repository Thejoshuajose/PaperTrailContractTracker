using System.Collections.Generic;
using System.Windows;
using PaperTrail.Core.Models;

namespace PaperTrail.App.Views;

public partial class PartySelectionWindow : Window
{
    public Party? SelectedParty => PartyList.SelectedItem as Party;

    public PartySelectionWindow(IEnumerable<Party> parties)
    {
        InitializeComponent();
        PartyList.ItemsSource = parties;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = true;
    }
}
