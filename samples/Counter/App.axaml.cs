using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace Counter;

public partial class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new global::Counter.MainWindow
            {
                DataContext = new global::Counter.CounterViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
