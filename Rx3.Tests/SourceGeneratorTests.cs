using R3;
using Shouldly;
using Xunit;

namespace Rx3.Tests;

/// <summary>
/// Tests for the [Reactive] source generator.
/// The test ViewModel class uses partial property syntax.
/// </summary>
public partial class SourceGeneratorTests
{
    [Fact]
    public void ReactiveAttribute_GeneratesPropertyWithDefault()
    {
        var vm = new TestViewModel();
        vm.Name.ShouldBeNull();
    }

    [Fact]
    public void ReactiveAttribute_SetProperty_RaisesPropertyChanged()
    {
        var vm = new TestViewModel();
        string? captured = null;
        vm.PropertyChanged += (_, e) => captured = e.PropertyName;

        vm.Name = "updated";

        captured.ShouldBe("Name");
        vm.Name.ShouldBe("updated");
    }

    [Fact]
    public void ReactiveAttribute_WhenChanged_EmitsOnChange()
    {
        var vm = new TestViewModel();
        var values = new List<string?>();

        using var sub = vm.WhenNameChanged().Subscribe(values.Add);

        vm.Name = "a";
        vm.Name = "b";

        values.ShouldBe([null, "a", "b"]);
    }

    [Fact]
    public void ReactiveAttribute_SetSameValue_DoesNotNotify()
    {
        var vm = new TestViewModel();
        vm.Name = "fixed";
        var count = 0;
        vm.PropertyChanged += (_, _) => count++;

        vm.Name = "fixed";

        count.ShouldBe(0);
    }

    // ── ReactiveCommand generator ─────────────────────────────────────

    [Fact]
    public void ReactiveCommandAttribute_GeneratesProperty()
    {
        var vm = new CommandViewModel();
        vm.LoginCommand.ShouldNotBeNull();
    }

    [Fact]
    public void ReactiveCommandAttribute_Execute_InvokesMethod()
    {
        var vm = new CommandViewModel();
        vm.LoginCommand.Execute(R3.Unit.Default);
        vm.LoginCalled.ShouldBeTrue();
    }

    [Fact]
    public void ReactiveCommandAttribute_WhenChanged_Emits()
    {
        var vm = new CommandViewModel();
        var count = 0;
        using var sub = vm.WhenLogin().Subscribe(_ => count++);

        vm.LoginCommand.Execute(R3.Unit.Default);

        count.ShouldBe(1);
    }

    // Test ViewModel with [Reactive] partial property
    private partial class TestViewModel : ReactiveObject
    {
        [Reactive]
        public partial string? Name { get; set; }

        public string NonReactive { get; set; } = "";
    }

    // Test ViewModel with [ReactiveCommand]
    private partial class CommandViewModel : ReactiveObject
    {
        public bool LoginCalled;

        [ReactiveCommand]
        private void Login() => LoginCalled = true;
    }
}
