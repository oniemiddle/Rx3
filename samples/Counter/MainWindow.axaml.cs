using Avalonia.Controls;

namespace Rx3.Samples.Counter;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosed(EventArgs e)
    {
        (DataContext as IDisposable)?.Dispose();
        base.OnClosed(e);
    }
}
