using System.IO;
using System.Windows;
using ThommyKalkulator.Application.Interfaces;
using ThommyKalkulator.Application.Services;
using ThommyKalkulator.Infrastructure.Persistence;

namespace ThommyKalkulator.WPF;

public partial class App : System.Windows.Application
{
    public static IAppState AppState { get; private set; } = null!;

    public static event EventHandler<ProjectEditRequestEventArgs>? ProjectEditRequested;

    public static event EventHandler<TabSelectionRequestEventArgs>? TabSelectionRequested;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        var appFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ThommyKalkulator");

        var dataFilePath = Path.Combine(appFolder, "data.json");

        var dataStore = new JsonDataStore(dataFilePath);
        AppState = new AppState(dataStore);
        AppState.Load();
    }

    public static void RequestProjectEdit(int projectIndex, int targetTabIndex)
    {
        ProjectEditRequested?.Invoke(null, new ProjectEditRequestEventArgs(projectIndex));
        TabSelectionRequested?.Invoke(null, new TabSelectionRequestEventArgs(targetTabIndex));
    }
}

public sealed class ProjectEditRequestEventArgs : EventArgs
{
    public ProjectEditRequestEventArgs(int projectIndex)
    {
        ProjectIndex = projectIndex;
    }

    public int ProjectIndex { get; }
}

public sealed class TabSelectionRequestEventArgs : EventArgs
{
    public TabSelectionRequestEventArgs(int tabIndex)
    {
        TabIndex = tabIndex;
    }

    public int TabIndex { get; }
}
