using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.Win32;
using System.Windows;
using Microsoft.Win32;
using WpfApplication = System.Windows.Application;
using WpfWindow = System.Windows.Window;

namespace ThommyKalkulator.WPF.Services;

public sealed class AppConfiguration
{
    public string Appearance { get; set; } = AppThemeModes.Dark;
    public int WindowWidth { get; set; } = 1400;
    public int WindowHeight { get; set; } = 900;
}

public static class AppThemeModes
{
    public const string Dark = "dark";
    public const string Light = "light";
    public const string System = "system";

    public static string Normalize(string? value)
    {
        if (string.Equals(value, Light, StringComparison.OrdinalIgnoreCase))
        {
            return Light;
        }

        if (string.Equals(value, System, StringComparison.OrdinalIgnoreCase))
        {
            return System;
        }

        return Dark;
    }
}

public sealed class AppConfigurationStore
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public AppConfigurationStore(string filePath)
    {
        _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));
        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    public AppConfiguration Load()
    {
        if (!File.Exists(_filePath))
        {
            return CreateDefault();
        }

        try
        {
            var json = File.ReadAllText(_filePath);
            var configuration = JsonSerializer.Deserialize<AppConfiguration>(json, _jsonOptions);

            return Normalize(configuration);
        }
        catch
        {
            return CreateDefault();
        }
    }

    public void Save(AppConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var directory = Path.GetDirectoryName(_filePath);
        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(Normalize(configuration), _jsonOptions);
        File.WriteAllText(_filePath, json);
    }

    public static AppConfiguration CreateDefault()
    {
        return new AppConfiguration();
    }

    public static AppConfiguration Normalize(AppConfiguration? configuration)
    {
        return new AppConfiguration
        {
            Appearance = AppThemeModes.Normalize(configuration?.Appearance),
            WindowWidth = NormalizeDimension(configuration?.WindowWidth ?? 1400, 1100),
            WindowHeight = NormalizeDimension(configuration?.WindowHeight ?? 900, 700)
        };
    }

    private static int NormalizeDimension(int value, int minimum)
    {
        return value < minimum ? minimum : value;
    }
}

public static class AppearanceManager
{
    public static string ResolveEffectiveAppearance(string requestedAppearance)
    {
        var normalized = AppThemeModes.Normalize(requestedAppearance);
        if (!string.Equals(normalized, AppThemeModes.System, StringComparison.OrdinalIgnoreCase))
        {
            return normalized;
        }

        try
        {
            using var personalizeKey = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize",
                false);

            var value = personalizeKey?.GetValue("AppsUseLightTheme");
            if (value is int intValue)
            {
                return intValue == 0 ? AppThemeModes.Dark : AppThemeModes.Light;
            }
        }
        catch
        {
            // bewusst ignoriert, Fallback unten
        }

        return AppThemeModes.Dark;
    }

    {
        ArgumentNullException.ThrowIfNull(application);

        var effectiveAppearance = ResolveEffectiveAppearance(requestedAppearance);
        var resourcePath = string.Equals(effectiveAppearance, AppThemeModes.Light, StringComparison.OrdinalIgnoreCase)
            ? "Resources/Themes/LightTheme.xaml"
            : "Resources/Themes/DarkTheme.xaml";

        application.Resources.MergedDictionaries.Clear();
        application.Resources.MergedDictionaries.Add(new ResourceDictionary
        {
            Source = new Uri(resourcePath, UriKind.Relative)
        });
    }

    public static void ApplyWindowSize(WpfWindow window, AppConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(configuration);

        window.Width = Math.Max(window.MinWidth, configuration.WindowWidth);
        window.Height = Math.Max(window.MinHeight, configuration.WindowHeight);
    }
}
