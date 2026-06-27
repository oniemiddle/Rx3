namespace Rx3;

/// <summary>
/// Marks a method to generate a reactive command property.
/// The containing class must be <c>partial</c> and extend <see cref="ReactiveObject"/>.
/// </summary>
/// <remarks>
/// Usage:
/// <code>
/// [ReactiveCommand]
/// private async Task LoginAsync() { ... }
/// // Generates:
/// //   public ReactiveCommand LoginCommand { get; }
/// </code>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public sealed class ReactiveCommandAttribute : Attribute;
