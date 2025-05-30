// // Description :    Definition of JsonExtensions.cs class
// //
// // Copyright © 2025 - 2025, Alcon. All rights reserved.

using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace kDg.FileBaseContext.Extensions;

public static class JsonExtensions {
    /// <summary>
    /// Extension methods for configuring <see cref="JsonSerializerOptions"/> in an <see cref="IServiceCollection"/>.
    /// It finds the existing <see cref="JsonSerializerOptions"/> instance or creates a new one if it doesn't exist.
    /// </summary>
    public static IServiceCollection ConfigureJsonSerializerOptions(this IServiceCollection services,
        Action<JsonSerializerOptions> configure) {
        var descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(JsonSerializerOptions));

        if (descriptor == null) {
            services.AddSingleton(new JsonSerializerOptions());
            descriptor = services.FirstOrDefault(s => s.ServiceType == typeof(JsonSerializerOptions));
        }

        configure(descriptor!.ImplementationInstance as JsonSerializerOptions);

        return services;
    }
}