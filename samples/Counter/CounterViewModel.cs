using R3;
using Rx3;

namespace Counter;

public partial class CounterViewModel : ReactiveObject
{
    [Reactive]
    public partial int Count { get; set; }

    [Reactive]
    public partial int Step { get; set; }

    public BindableReactiveProperty<bool> CanIncrement { get; }

    public CounterViewModel()
    {
        Step = 1;

        CanIncrement = new BindableReactiveProperty<bool>(true);
        
        
        // CanIncrement follows the count threshold
        // Observable.EveryValueChanged(this, x => x.Count)
        WhenValueChanged(() => Count)
            .Select(c => c < 10)
            .Subscribe(x => CanIncrement.Value = x)
            .AddTo(ref DisposableBag);
    }

    [ReactiveCommand]
    private void Increment() => Count += Step;

    [ReactiveCommand]
    private void Decrement() => Count -= Step;

    [ReactiveCommand]
    private void Reset() => Count = 0;
}
