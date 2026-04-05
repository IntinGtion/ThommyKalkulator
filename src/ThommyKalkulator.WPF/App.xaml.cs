using System.IO;
using System.Windows;
using ThommyKalkulator.Application.Interfaces;
using ThommyKalkulator.Application.Services;
using ThommyKalkulator.Infrastructure.Persistence;
using ThommyKalkulator.WPF.Services;
using ThommyKalkulator.WPF.Views;

namespace ThommyKalkulator.WPF;

public partial class App : System.Windows.Application
{
    public static IAppState AppState { get; private set; } = null!;

    public static AppConfigurationStore ConfigurationStore { get; private set; } = null!;

    public static AppConfiguration CurrentConfiguration { get; private set; } = null!;

    public static event EventHandler<ProjectEditRequestEventArgs>? ProjectEditRequested;

    public static event EventHandler<TabSelectionRequestEventArgs>? TabSelectionRequested;

    public static event EventHandler<AppConfigurationChangedEventArgs>? AppConfigurationChanged;

    protected override void OnStartup(StartupEventArgs e)
    {
        var appFolder = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "ThommyKalkulator");

        var dataFilePath = Path.Combine(appFolder, "data.json");
        var configFilePath = Path.Combine(appFolder, "config.json");

        var dataStore = new JsonDataStore(dataFilePath);
        AppState = new AppState(dataStore);
        AppState.Load();

        ConfigurationStore = new AppConfigurationStore(configFilePath);
        CurrentConfiguration = ConfigurationStore.Load();
        AppearanceManager.ApplyApplicationAppearance(this, CurrentConfiguration.Appearance);

        base.OnStartup(e);

        var mainWindow = new MainWindow();
        MainWindow = mainWindow;
        mainWindow.Show();
    }

    public static void UpdateConfiguration(AppConfiguration configuration)
    {
        CurrentConfiguration = AppConfigurationStore.Normalize(configuration);
        ConfigurationStore.Save(CurrentConfiguration);
        AppearanceManager.ApplyApplicationAppearance(Current, CurrentConfiguration.Appearance);
        AppConfigurationChanged?.Invoke(null, new AppConfigurationChangedEventArgs(CurrentConfiguration));
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

public sealed class AppConfigurationChangedEventArgs : EventArgs
{
    public AppConfigurationChangedEventArgs(AppConfiguration configuration)
    {
        Configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public AppConfiguration Configuration { get; }
}
