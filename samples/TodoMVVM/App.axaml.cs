using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace Rx3.Samples.TodoMVVM;

public partial class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new global::TodoMVVM.ViewModels.TodoListViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
