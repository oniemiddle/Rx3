using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using R3;

namespace Rx3.Avalonia;

/// <summary>
/// Provides Avalonia-specific binding extension methods for <see cref="Observable{T}"/>.
/// </summary>
public static class AvaloniaBindingExtensions
{
    /// <summary>
    /// Binds an <see cref="Observable{T}"/> to an Avalonia object's property.
    /// The property is updated on the UI thread automatically.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <param name="source">The source observable.</param>
    /// <param name="target">The target Avalonia object (e.g. a Control).</param>
    /// <param name="property">The Avalonia property to bind to.</param>
    /// <returns>A disposable that cancels the binding.</returns>
    public static IDisposable BindTo<T>(this Observable<T> source, AvaloniaObject target, AvaloniaProperty property)
    {
        return source.ObserveOnUIThreadDispatcher().Subscribe(x =>
        {
            if (property.PropertyType.IsInstanceOfType(x))
            {
                target.SetValue(property, x);
            }
        });
    }

    /// <summary>
    /// Binds an <see cref="Observable{T}"/> to an Avalonia object's property
    /// with a selector function for type conversion.
    /// </summary>
    /// <typeparam name="T">The type of the source value.</typeparam>
    /// <typeparam name="TProperty">The type of the target property.</typeparam>
    /// <param name="source">The source observable.</param>
    /// <param name="target">The target Avalonia object.</param>
    /// <param name="property">The Avalonia property to bind to.</param>
    /// <param name="selector">Converts source value to target property type.</param>
    /// <returns>A disposable that cancels the binding.</returns>
    public static IDisposable BindTo<T, TProperty>(this Observable<T> source, AvaloniaObject target,
        AvaloniaProperty<TProperty> property, Func<T, TProperty> selector)
    {
        return source.ObserveOnUIThreadDispatcher().Subscribe(x =>
        {
            target.SetValue(property, selector(x));
        });
    }

    /// <summary>
    /// Subscribes to an <see cref="Observable{T}"/> and sets the <see cref="ContentControl.Content"/> property.
    /// </summary>
    public static IDisposable SubscribeToContent<T>(this Observable<T> source, ContentControl control)
    {
        return source.ObserveOnUIThreadDispatcher().Subscribe(x => control.Content = x);
    }

    /// <summary>
    /// Subscribes to a string observable and sets the <see cref="TextBlock.Text"/> property.
    /// </summary>
    public static IDisposable SubscribeToText(this Observable<string> source, TextBlock textBlock)
    {
        return source.ObserveOnUIThreadDispatcher().Subscribe(x => textBlock.Text = x);
    }

    /// <summary>
    /// Subscribes to a boolean observable and sets the <see cref="InputElement.IsEnabled"/> property.
    /// </summary>
    public static IDisposable SubscribeToIsEnabled(this Observable<bool> source, InputElement control)
    {
        return source.ObserveOnUIThreadDispatcher().Subscribe(x => control.IsEnabled = x);
    }

    /// <summary>
    /// Subscribes to a boolean observable and sets the <see cref="Visual.IsVisible"/> property.
    /// </summary>
    public static IDisposable SubscribeToIsVisible(this Observable<bool> source, Visual control)
    {
        return source.ObserveOnUIThreadDispatcher().Subscribe(x => control.IsVisible = x);
    }

    /// <summary>
    /// Observes the observable on the Avalonia UI thread dispatcher.
    /// Uses <c>ObserveOn(AvaloniaDispatcherTimeProvider.Default)</c> internally.
    /// </summary>
    private static Observable<T> ObserveOnUIThreadDispatcher<T>(this Observable<T> source)
    {
        return source.ObserveOn(AvaloniaDispatcherTimeProvider.Default);
    }
}
