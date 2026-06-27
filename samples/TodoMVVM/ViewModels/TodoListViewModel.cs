using ObservableCollections;
using R3;
using Rx3;

namespace TodoMVVM.ViewModels;

public partial class TodoListViewModel : ReactiveObject
{
    private readonly ObservableList<TodoItemViewModel> _items = new();

    public IReadOnlyList<TodoItemViewModel> Items => _items;

    [Reactive]
    public partial string NewItemText { get; set; }

    [Reactive]
    public partial string FilterText { get; set; }

    public Rx3.BindableReactiveProperty<int> ActiveCount { get; }

    public ReactiveCommand<TodoItemViewModel> RemoveItemCommand { get; }

    private IDisposable? _filterSub;

    public TodoListViewModel()
    {
        var always = new BehaviorSubject<bool>(true);

        // RemoveItem command (parameterized, can't use source generator)
        RemoveItemCommand = always.ToReactiveCommand<TodoItemViewModel>(item =>
        {
            _items.Remove(item);
            item?.Dispose();
            UpdateActiveCount();
        });

        // Seed data
        _items.Add(new TodoItemViewModel("Learn Rx3") { IsCompleted = true });
        _items.Add(new TodoItemViewModel("Build an app"));
        _items.Add(new TodoItemViewModel("Write tests"));

        ActiveCount = new Rx3.BindableReactiveProperty<int>(_items.Count(x => !x.IsCompleted));

        // Wire filter text changes
        _filterSub = WhenValueChanged(() => FilterText)
            .Subscribe(_ => UpdateActiveCount());
    }

    [ReactiveCommand]
    private void AddItem()
    {
        if (string.IsNullOrWhiteSpace(NewItemText)) return;
        _items.Add(new TodoItemViewModel(NewItemText.Trim()));
        NewItemText = "";
        UpdateActiveCount();
    }

    [ReactiveCommand]
    private void ClearCompleted()
    {
        var completed = _items.Where(x => x.IsCompleted).ToArray();
        foreach (var item in completed)
        {
            _items.Remove(item);
            item.Dispose();
        }
        UpdateActiveCount();
    }

    private void UpdateActiveCount()
    {
        var filtered = string.IsNullOrEmpty(FilterText)
            ? _items
            : _items.Where(x => x.Text.Contains(FilterText, StringComparison.OrdinalIgnoreCase));
        ActiveCount.Value = filtered.Count(x => !x.IsCompleted);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _filterSub?.Dispose();
            foreach (var item in _items)
                item.Dispose();
        }
        base.Dispose(disposing);
    }
}
