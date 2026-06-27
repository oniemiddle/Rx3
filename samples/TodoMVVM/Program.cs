using Avalonia;
using Rx3.Avalonia;

namespace Rx3.Samples.TodoMVVM;

public static class Program
{
    public static void Main(string[] args)
        => BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .UseRx3();
}
