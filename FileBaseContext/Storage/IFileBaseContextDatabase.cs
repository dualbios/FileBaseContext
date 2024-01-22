using Microsoft.EntityFrameworkCore.Storage;

namespace kDg.FileBaseContext.Storage;

public interface IFileBaseContextDatabase : IDatabase
{
    IFileBaseContextStore Store { get; }
}