using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using TodoMVVM.Views;

namespace TodoMVVM;

public partial class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new ViewModels.TodoListViewModel(),
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
