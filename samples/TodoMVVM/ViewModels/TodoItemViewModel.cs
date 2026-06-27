using R3;

namespace Rx3.Samples.TodoMVVM;

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
