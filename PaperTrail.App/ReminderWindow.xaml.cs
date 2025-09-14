using System;
using System.Windows;

namespace PaperTrail.App;

public partial class ReminderWindow : Window
{
    public DateTime SelectedDate { get; private set; }
    public string? Note { get; private set; }
    public bool AddToCalendar { get; private set; }

    public ReminderWindow()
    {
        InitializeComponent();
        datePicker.SelectedDate = DateTime.Today;
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        SelectedDate = datePicker.SelectedDate ?? DateTime.Today;
        Note = noteText.Text;
        AddToCalendar = calendarCheck.IsChecked == true;
        DialogResult = true;
    }
}
