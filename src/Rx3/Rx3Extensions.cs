using R3;

namespace Rx3;

/// <summary>
/// Extension methods for creating and binding reactive properties and commands.
/// </summary>
public static class Rx3Extensions
{
    extension<T>(Observable<T> source)
    {
        /// <summary>
        /// Creates a <see cref="BindableReactiveProperty{T}"/> from an <see cref="Observable{T}"/> source.
        /// The property will reflect the latest value from the source observable.
        /// </summary>
        public BindableReactiveProperty<T> ToBindableProperty(T initialValue = default!)
        {
            var result = new BindableReactiveProperty<T>(initialValue);
            source.Subscribe(x => result.Value = x);
            return result;
        }

        /// <summary>
        /// Creates a <see cref="ReadOnlyReactiveProperty{T}"/> from an <see cref="Observable{T}"/> source.
        /// </summary>
        public ReadOnlyReactiveProperty<T> ToReadOnlyProperty(T initialValue = default!)
        {
            return new ReadOnlyReactiveProperty<T>(source, initialValue);
        }

        /// <summary>
        /// Subscribes to the observable and invokes the <paramref name="setter"/> for each emitted value.
        /// </summary>
        public IDisposable BindTo(Action<T> setter)
        {
            return source.Subscribe(setter);
        }
    }

    /// <summary>
    /// Adds the <see cref="BindableReactiveProperty{T}"/> to a <see cref="DisposableBag"/>
    /// and returns the same property for chaining.
    /// </summary>
    public static BindableReactiveProperty<T> AddTo<T>(this BindableReactiveProperty<T> property, ref DisposableBag bag)
    {
        ((IDisposable)property).AddTo(ref bag);
        return property;
    }

    /// <summary>
    /// Adds the <see cref="ReadOnlyReactiveProperty{T}"/> to a <see cref="DisposableBag"/>
    /// and returns the same property for chaining.
    /// </summary>
    public static ReadOnlyReactiveProperty<T> AddTo<T>(this ReadOnlyReactiveProperty<T> property, ref DisposableBag bag)
    {
        ((IDisposable)property).AddTo(ref bag);
        return property;
    }
}
