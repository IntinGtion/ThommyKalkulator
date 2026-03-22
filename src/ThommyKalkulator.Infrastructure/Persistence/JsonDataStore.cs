using System.Text.Encodings.Web;
using System.Text.Json;
using ThommyKalkulator.Application.Interfaces;
using ThommyKalkulator.Domain.Models;

namespace ThommyKalkulator.Infrastructure.Persistence;

public sealed class JsonDataStore : IDataStore
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonDataStore(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
            throw new ArgumentException("Der Dateipfad für data.json darf nicht leer sein.", nameof(filePath));

        _filePath = filePath;

        _jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };
    }

    public AppData Load()
    {
        if (!File.Exists(_filePath))
            return CreateDefaultAppData();

        var json = File.ReadAllText(_filePath);

        if (string.IsNullOrWhiteSpace(json))
            return CreateDefaultAppData();

        var appData = JsonSerializer.Deserialize<AppData>(json, _jsonOptions);

        return appData ?? CreateDefaultAppData();
    }

    public void Save(AppData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        var directory = Path.GetDirectoryName(_filePath);

        if (!string.IsNullOrWhiteSpace(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(data, _jsonOptions);
        File.WriteAllText(_filePath, json);
    }

    private static AppData CreateDefaultAppData()
    {
        return new AppData
        {
            GlobalSettings = new GlobalSettings
            {
                Currency = "€",
                ElectricityPricePerKwh = 0m,
                LaborRate = 0m,
                ConstructionLaborRate = 0m,
                DefaultSurchargePercent = 0m,
                LogoPath = string.Empty,
                FloatingLiveDefault = false,
                PdfAutoOpen = false
            },
            Machines = new List<Machine>(),
            MaterialTypes = new List<string>(),
            Materials = new List<Material>(),
            Projects = new List<CalculationProject>()
        };
    }
}