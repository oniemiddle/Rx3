namespace Rx3;

/// <summary>
/// Marks a partial property to auto-generate its implementation with
/// <see cref="System.ComponentModel.INotifyPropertyChanged"/> notification.
/// The containing class must be <c>partial</c> and extend <see cref="ReactiveObject"/>.
/// </summary>
/// <remarks>
/// Usage (C# 13+ partial property):
/// <code>
/// [Reactive]
/// public partial string Name { get; }
/// // Generator produces: backing field + getter + setter with SetProperty()
/// // Also generates:     public Observable&lt;string&gt; WhenNameChanged() => ...;
/// </code>
///
/// Legacy field usage (still supported):
/// <code>
/// [Reactive] private string _name;
/// // Generator produces the same output
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public sealed class ReactiveAttribute : Attribute;
