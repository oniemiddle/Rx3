using System.ComponentModel;
using R3;

namespace Rx3;

/// <summary>
/// A read-only bindable reactive property that implements <see cref="INotifyPropertyChanged"/>
/// for XAML binding. Created from an <see cref="Observable{T}"/> source.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
public sealed class ReadOnlyReactiveProperty<T> : INotifyPropertyChanged, IDisposable
{
    private readonly R3.BindableReactiveProperty<T> _inner;
    private readonly IDisposable _subscription;

    /// <summary>
    /// Initializes a new instance from an <see cref="Observable{T}"/> source.
    /// </summary>
    /// <param name="source">The source observable whose latest value is exposed.</param>
    /// <param name="initialValue">The initial value before the source emits.</param>
    public ReadOnlyReactiveProperty(Observable<T> source, T initialValue = default!)
    {
        _inner = new R3.BindableReactiveProperty<T>(initialValue);
        _subscription = source.Subscribe(x => _inner.Value = x);
    }

    /// <summary>
    /// Gets the current value.
    /// </summary>
    public T Value => _inner.Value;

    /// <summary>
    /// Gets an <see cref="Observable{T}"/> that emits the current value on subscribe
    /// and whenever the source value changes.
    /// </summary>
    public Observable<T> AsObservable() => _inner;

    /// <summary>
    /// Occurs when the <see cref="Value"/> changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged
    {
        add => ((INotifyPropertyChanged)_inner).PropertyChanged += value;
        remove => ((INotifyPropertyChanged)_inner).PropertyChanged -= value;
    }

    /// <summary>
    /// Releases all resources used by the <see cref="ReadOnlyReactiveProperty{T}"/>.
    /// </summary>
    public void Dispose()
    {
        _subscription.Dispose();
        _inner.Dispose();
    }
}
