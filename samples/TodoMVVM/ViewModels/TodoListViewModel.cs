using Avalonia.Collections;
using R3;
using Rx3;

namespace TodoMVVM.ViewModels;

public partial class TodoListViewModel : ReactiveObject
{
    public AvaloniaList<TodoItemViewModel> Items { get; } = [];

    [Reactive] public partial string NewItemText { get; set; } = string.Empty;

    [Reactive] public partial string FilterText { get; set; } = string.Empty;

    [Reactive]
    public partial int ActiveCount { get; set; }

    public ReactiveCommand<TodoItemViewModel> RemoveItemCommand { get; }

    private readonly IDisposable? _filterSub;

    public TodoListViewModel()
    {  
        var always = new BehaviorSubject<bool>(true);

        // RemoveItem command (parameterized, can't use source generator)
        RemoveItemCommand = always.ToReactiveCommand<TodoItemViewModel>(item =>
        {
            Items.Remove(item);
            item.Dispose();
            UpdateActiveCount();
        }).AddTo(ref DisposableBag);

        // Seed data
        Items.Add(new TodoItemViewModel("Learn Rx3") { IsCompleted = true });
        Items.Add(new TodoItemViewModel("Build an app"));
        Items.Add(new TodoItemViewModel("Write tests"));

        ActiveCount = Items.Count(x => !x.IsCompleted);

        // Wire filter text changes
        _filterSub = WhenValueChanged(() => FilterText)
            .Subscribe(_ => UpdateActiveCount());
    }

    [ReactiveCommand]
    private void AddItem()
    {
        if (string.IsNullOrWhiteSpace(NewItemText)) return;
        Items.Add(new TodoItemViewModel(NewItemText.Trim()));
        NewItemText = "";
        UpdateActiveCount();
    }

    [ReactiveCommand]
    private void ClearCompleted()
    {
        var completed = Items.Where(x => x.IsCompleted).ToArray();
        foreach (var item in completed)
        {
            Items.Remove(item);
            item.Dispose();
        }
        UpdateActiveCount();
    }

    private void UpdateActiveCount()
    {
        var filtered = string.IsNullOrEmpty(FilterText)
            ? Items
            : Items.Where(x => x.Text.Contains(FilterText, StringComparison.OrdinalIgnoreCase));
        ActiveCount = filtered.Count(x => !x.IsCompleted);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _filterSub?.Dispose();
            foreach (var item in Items)
                item.Dispose();
        }
        base.Dispose(disposing);
    }
}
