using System.ComponentModel;
using R3;

namespace Rx3;

/// <summary>
/// A bindable reactive property that implements <see cref="INotifyPropertyChanged"/>
/// for XAML binding, wrapping R3's <see cref="R3.BindableReactiveProperty{T}"/>.
/// </summary>
/// <typeparam name="T">The type of the property value.</typeparam>
public sealed class BindableReactiveProperty<T> : INotifyPropertyChanged, IDisposable
{
    private readonly R3.BindableReactiveProperty<T> _inner;

    /// <summary>
    /// Initializes a new instance with the default value of <typeparamref name="T"/>.
    /// </summary>
    public BindableReactiveProperty()
    {
        _inner = new R3.BindableReactiveProperty<T>();
    }

    /// <summary>
    /// Initializes a new instance with the specified <paramref name="initialValue"/>.
    /// </summary>
    /// <param name="initialValue">The initial value.</param>
    public BindableReactiveProperty(T initialValue)
    {
        _inner = new R3.BindableReactiveProperty<T>(initialValue);
    }

    /// <summary>
    /// Gets or sets the current value. Setting the same value does not trigger notifications.
    /// </summary>
    public T Value
    {
        get => _inner.Value;
        set => _inner.Value = value;
    }

    /// <summary>
    /// Gets the inner <see cref="R3.BindableReactiveProperty{T}"/> as an <see cref="Observable{T}"/>.
    /// Subscribe to this to react to value changes.
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
    /// Releases all resources used by the <see cref="BindableReactiveProperty{T}"/>.
    /// </summary>
    public void Dispose()
    {
        _inner.Dispose();
    }
}
