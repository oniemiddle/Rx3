using Avalonia;

namespace Rx3.Avalonia;

/// <summary>
/// Provides initialization for Rx3 in Avalonia applications.
/// Call <see cref="UseRx3"/> on the <see cref="AppBuilder"/> in your application entry point.
/// </summary>
public static class AvaloniaProviderInitializer
{
    /// <summary>
    /// Configures Rx3 with Avalonia's dispatcher time and frame providers.
    /// This must be called in the application entry point:
    /// <code>
    /// public static AppBuilder BuildAvaloniaApp()
    ///     => AppBuilder.Configure&lt;App&gt;()
    ///         .UsePlatformDetect()
    ///         .UseRx3();
    /// </code>
    /// </summary>
    /// <param name="builder">The Avalonia application builder.</param>
    /// <returns>The same application builder for chaining.</returns>
    public static AppBuilder UseRx3(this AppBuilder builder)
    {
        R3.AvaloniaProviderInitializer.SetDefaultObservableSystem(
            ex => System.Diagnostics.Trace.WriteLine($"Rx3 UnhandledException: {ex}"));

        return builder;
    }
}
