using System.ComponentModel;
using R3;
using Shouldly;
using Xunit;

namespace Rx3.Tests;

public class ReactivePropertyTests
{
    // ── BindableReactiveProperty: construction ─────────────────────────────

    [Fact]
    public void BindableProperty_StartsWithDefaultValue()
    {
        var prop = new BindableReactiveProperty<int>();
        prop.Value.ShouldBe(0);
    }

    [Fact]
    public void BindableProperty_WithInitialValue()
    {
        var prop = new BindableReactiveProperty<string>("hello");
        prop.Value.ShouldBe("hello");
    }

    // ── BindableReactiveProperty: value and notifications ──────────────────

    [Fact]
    public void BindableProperty_SetValue_UpdatesProperty()
    {
        var prop = new BindableReactiveProperty<int>(10);
        prop.Value = 42;
        prop.Value.ShouldBe(42);
    }

    [Fact]
    public void BindableProperty_SetValue_RaisesPropertyChanged()
    {
        var prop = new BindableReactiveProperty<string>("old");
        string? capturedName = null;
        ((INotifyPropertyChanged)prop).PropertyChanged += (_, e) => capturedName = e.PropertyName;

        prop.Value = "new";

        capturedName.ShouldBe("Value");
    }

    [Fact]
    public void BindableProperty_SetSameValue_DoesNotRaisePropertyChanged()
    {
        var prop = new BindableReactiveProperty<string>("same");
        var count = 0;
        ((INotifyPropertyChanged)prop).PropertyChanged += (_, _) => count++;

        prop.Value = "same";

        count.ShouldBe(0);
    }

    // ── BindableReactiveProperty: Observable ───────────────────────────────

    [Fact]
    public void BindableProperty_AsObservable_EmitsOnChange()
    {
        var prop = new BindableReactiveProperty<string>("a");
        var values = new List<string>();

        using var sub = prop.AsObservable().Subscribe(values.Add);

        prop.Value = "b";
        prop.Value = "c";

        values.ShouldBe(["a", "b", "c"]);
    }

    [Fact]
    public void BindableProperty_AsObservable_DoesNotEmitSameValue()
    {
        var prop = new BindableReactiveProperty<string>("x");
        var values = new List<string>();

        using var sub = prop.AsObservable().Subscribe(values.Add);
        values.Clear();

        prop.Value = "x";

        values.ShouldBeEmpty();
    }

    [Fact]
    public void BindableProperty_Dispose_StopsEmitting()
    {
        var prop = new BindableReactiveProperty<string>("a");
        var values = new List<string>();
        var sub = prop.AsObservable().Subscribe(values.Add);
        values.Clear(); // a

        sub.Dispose();
        prop.Value = "b";

        values.ShouldBeEmpty();
    }

    // ── ReadOnlyReactiveProperty ───────────────────────────────────────────

    [Fact]
    public void ReadOnlyProperty_FromObservable_ReflectsValue()
    {
        var source = new Subject<string>();
        var ro = new ReadOnlyReactiveProperty<string>(source, "init");

        ro.Value.ShouldBe("init");

        source.OnNext("updated");
        ro.Value.ShouldBe("updated");
    }

    [Fact]
    public void ReadOnlyProperty_HasNoSetter()
    {
        var source = new Subject<int>();
        var ro = new ReadOnlyReactiveProperty<int>(source, 0);

        // Verify it's read-only via reflection
        var prop = ro.GetType().GetProperty("Value");
        prop!.CanWrite.ShouldBeFalse();
        prop.CanRead.ShouldBeTrue();
    }

    [Fact]
    public void ReadOnlyProperty_EmitsOnSourceChange()
    {
        var source = new Subject<string>();
        var ro = new ReadOnlyReactiveProperty<string>(source, "start");
        var values = new List<string>();

        using var sub = ro.AsObservable().Subscribe(values.Add);

        source.OnNext("first");
        source.OnNext("second");

        values.ShouldBe(["start", "first", "second"]);
    }

    // ── Extension methods ──────────────────────────────────────────────────

    [Fact]
    public void ToBindablePropertyExtension_CreatesProperty()
    {
        var source = new Subject<int>();
        var prop = source.ToBindableProperty(0);

        prop.Value.ShouldBe(0);

        source.OnNext(99);
        prop.Value.ShouldBe(99);
    }

    [Fact]
    public void ToReadOnlyPropertyExtension_CreatesProperty()
    {
        var source = Observable.Return(42);
        var ro = source.ToReadOnlyProperty(0);

        ro.Value.ShouldBe(42);
    }

    [Fact]
    public void BindToExtension_InvokesSetter()
    {
        var source = new Subject<string>();
        var received = new List<string>();

        using var binding = source.BindTo(received.Add);

        source.OnNext("a");
        source.OnNext("b");

        received.ShouldBe(["a", "b"]);
    }

    // ── DisposableBag integration ──────────────────────────────────────────

    [Fact]
    public void BindableProperty_AddToDisposableBag_DisposesWithBag()
    {
        var bag = new DisposableBag();
        var prop = new BindableReactiveProperty<string>("live");
        prop.AddTo(ref bag);

        prop.Value.ShouldBe("live"); // still alive before

        bag.Dispose();

        Should.Throw<ObjectDisposedException>(() => prop.Value = "dead");
    }
}
