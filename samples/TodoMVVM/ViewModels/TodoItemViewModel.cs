using Rx3;

namespace TodoMVVM.ViewModels;

public partial class TodoItemViewModel : ReactiveObject
{
    [Reactive]
    public partial string Text { get; set; }

    [Reactive]
    public partial bool IsCompleted { get; set; }

    public TodoItemViewModel(string text)
    {
        Text = text;
    }
}
