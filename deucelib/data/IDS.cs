using System.Data.Common;

namespace deuce;

public interface IDB<T>
{
    Task<List<T>> GetList();
}