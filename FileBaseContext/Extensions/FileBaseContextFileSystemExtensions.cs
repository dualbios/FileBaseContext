using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace kDg.FileBaseContext.Extensions;

public static class FileBaseContextFileSystemExtensions
{
    public static IServiceCollection AddFileSystem(this IServiceCollection services)
    {
        services.AddSingleton<IFileSystem, FileSystem>();
        return services;
    }

    public static IServiceCollection AddMockFileSystem(this IServiceCollection services, MockFileSystem mockFileSystem)
    {
        services.RemoveAll<IFileSystem>();
        services.AddSingleton<IFileSystem>(mockFileSystem);

        return services;
    }
}