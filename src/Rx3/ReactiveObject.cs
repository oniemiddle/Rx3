using System.ComponentModel;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using R3;

namespace Rx3;

/// <summary>
/// Base class for reactive objects that implement <see cref="INotifyPropertyChanged"/>
/// and provide built-in subscription lifecycle management via <see cref="DisposableBag"/>.
/// </summary>
/// <remarks>
/// Supports two usage modes:
/// <list type="bullet">
///   <item><description><b>Manual mode</b> — call <see cref="SetProperty{T}"/> in property setters.</description></item>
///   <item><description><b>Source Generator mode</b> — mark fields with <c>[Reactive]</c> to auto-generate properties.</description></item>
/// </list>
/// Subscribe to property changes via <see cref="WhenValueChanged{T}"/> to get an <c>Observable&lt;T&gt;</c> stream.
/// All subscriptions added to <see cref="DisposableBag"/> are automatically disposed when this object is disposed.
/// </remarks>
public abstract class ReactiveObject : INotifyPropertyChanged, IDisposable
{
    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Gets the disposable bag for managing subscription lifetimes.
    /// Subscriptions added via <c>AddTo(ref DisposableBag)</c> will be disposed
    /// when this <see cref="ReactiveObject"/> is disposed.
    /// </summary>
    protected DisposableBag DisposableBag;

    private Subject<PropertyChangedEventArgs>? _propertyChangedSubject;

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed. Automatically filled by the compiler.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        var args = new PropertyChangedEventArgs(propertyName);
        PropertyChanged?.Invoke(this, args);
        _propertyChangedSubject?.OnNext(args);
    }

    /// <summary>
    /// Sets the backing field to the new value and raises <see cref="PropertyChanged"/>
    /// if the value has changed (compared using <see cref="EqualityComparer{T}.Default"/>).
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="field">The backing field reference.</param>
    /// <param name="value">The new value.</param>
    /// <param name="propertyName">The name of the property. Automatically filled by the compiler.</param>
    /// <returns><c>true</c> if the value was changed; otherwise <c>false</c>.</returns>
    protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return false;
        }

        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    /// <summary>
    /// Returns an <see cref="Observable{T}"/> that emits the current value immediately on subscribe,
    /// and then emits new values whenever the property specified by <paramref name="expression"/> changes.
    /// </summary>
    /// <typeparam name="T">The type of the property value.</typeparam>
    /// <param name="expression">An expression that accesses the property, e.g. <c>() => Name</c>.</param>
    /// <returns>An observable sequence of property values.</returns>
    /// <exception cref="ArgumentException">Thrown when the expression is not a member expression.</exception>
    public Observable<T> WhenValueChanged<T>(Expression<Func<T>> expression)
    {
        if (expression.Body is not MemberExpression memberExpr)
        {
            // Handle value type boxing: () => (object)SomeValue
            if (expression.Body is UnaryExpression { NodeType: ExpressionType.Convert, Operand: MemberExpression innerMember })
            {
                memberExpr = innerMember;
            }
            else
            {
                throw new ArgumentException(
                    "Expression must be a member expression (e.g. () => PropertyName).", nameof(expression));
            }
        }

        var propertyName = memberExpr.Member.Name;
        var getter = expression.Compile();

        _propertyChangedSubject ??= new Subject<PropertyChangedEventArgs>();

        return _propertyChangedSubject
            .Where(e => e.PropertyName == propertyName)
            .Select(_ => getter())
            .Prepend(getter());
    }

    /// <summary>
    /// Releases all resources used by the <see cref="ReactiveObject"/>.
    /// Disposes the <see cref="DisposableBag"/> and clears the <see cref="PropertyChanged"/> event.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the managed and unmanaged resources used by the <see cref="ReactiveObject"/>.
    /// </summary>
    /// <param name="disposing">
    /// <c>true</c> to release both managed and unmanaged resources;
    /// <c>false</c> to release only unmanaged resources.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            DisposableBag.Dispose();
            _propertyChangedSubject?.Dispose();
            PropertyChanged = null;
        }
    }
}
