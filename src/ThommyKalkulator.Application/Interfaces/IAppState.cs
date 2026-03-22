using ThommyKalkulator.Domain.Models;

namespace ThommyKalkulator.Application.Interfaces;

public interface IAppState
{
    AppData CurrentData { get; }

    event EventHandler? DataChanged;

    void Load();
    void Save();
    void Replace(AppData data);
}