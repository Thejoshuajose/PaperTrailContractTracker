using Ookii.Dialogs.Wpf;

namespace PaperTrail.App.Services;

public class DialogService
{
    public string? OpenFile(string filter)
    {
        var dialog = new VistaOpenFileDialog { Filter = filter };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }

    public string? SaveFile(string filter, string defaultExt)
    {
        var dialog = new VistaSaveFileDialog { Filter = filter, DefaultExt = defaultExt };
        return dialog.ShowDialog() == true ? dialog.FileName : null;
    }
}
