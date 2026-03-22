using ThommyKalkulator.Domain.Models;

namespace ThommyKalkulator.Application.Interfaces;

public interface IDataStore
{
    AppData Load();
    void Save(AppData data);
}