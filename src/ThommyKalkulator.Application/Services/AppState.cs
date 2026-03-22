using ThommyKalkulator.Application.Interfaces;
using ThommyKalkulator.Domain.Models;

namespace ThommyKalkulator.Application.Services;

public sealed class AppState : IAppState
{
    private readonly IDataStore _dataStore;

    public AppData CurrentData { get; private set; } = new();

    public event EventHandler? DataChanged;

    public AppState(IDataStore dataStore)
    {
        _dataStore = dataStore ?? throw new ArgumentNullException(nameof(dataStore));
    }

    public void Load()
    {
        CurrentData = _dataStore.Load();
        OnDataChanged();
    }

    public void Save()
    {
        _dataStore.Save(CurrentData);
        OnDataChanged();
    }

    public void Replace(AppData data)
    {
        CurrentData = data ?? throw new ArgumentNullException(nameof(data));
        OnDataChanged();
    }

    private void OnDataChanged()
    {
        DataChanged?.Invoke(this, EventArgs.Empty);
    }
}
