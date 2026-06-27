using R3;
using Shouldly;
using Xunit;

namespace Rx3.Tests;

public class ReactiveObjectTests
{
    // ── SetProperty ──────────────────────────────────────────────────────

    [Fact]
    public void SetProperty_TriggersPropertyChanged()
    {
        // Arrange
        var obj = new TestReactiveObject();
        var changedProperties = new List<string?>();
        obj.PropertyChanged += (_, e) => changedProperties.Add(e.PropertyName);

        // Act
        obj.Name = "Alice";

        // Assert
        changedProperties.ShouldBe(["Name"]);
    }

    [Fact]
    public void SetProperty_DoesNotTriggerOnSameValue()
    {
        // Arrange
        var obj = new TestReactiveObject();
        obj.Name = "Bob";
        var invocationCount = 0;
        obj.PropertyChanged += (_, _) => invocationCount++;

        // Act
        obj.Name = "Bob";

        // Assert
        invocationCount.ShouldBe(0);
    }

    [Fact]
    public void SetProperty_ReturnsTrueOnChange()
    {
        // Verify that PropertyChanged is fired when value differs
        var obj = new TestReactiveObject();
        var fired = false;
        obj.PropertyChanged += (_, _) => fired = true;

        obj.Name = "Charlie";

        fired.ShouldBeTrue();
    }

    [Fact]
    public void SetProperty_ReturnsFalseOnNoChange()
    {
        // Verify that PropertyChanged is NOT fired when value is the same
        var obj = new TestReactiveObject();
        obj.Name = "Dave";
        var fired = false;
        obj.PropertyChanged += (_, _) => fired = true;

        obj.Name = "Dave";

        fired.ShouldBeFalse();
    }

    // ── OnPropertyChanged ────────────────────────────────────────────────

    [Fact]
    public void OnPropertyChanged_RaisesEvent()
    {
        // Arrange
        var obj = new TestReactiveObject();
        string? capturedName = null;
        obj.PropertyChanged += (_, e) => capturedName = e.PropertyName;

        // Act
        obj.RaisePropertyChanged("CustomProp");

        // Assert
        capturedName.ShouldBe("CustomProp");
    }

    // ── Dispose ──────────────────────────────────────────────────────────

    [Fact]
    public void Dispose_ClearsPropertyChangedSubscribers()
    {
        // Arrange
        var obj = new TestReactiveObject();
        var invocationCount = 0;
        obj.PropertyChanged += (_, _) => invocationCount++;

        // Act
        obj.Dispose();
        obj.Name = "Eve";

        // Assert
        invocationCount.ShouldBe(0);
    }

    [Fact]
    public void Dispose_CallsDisposeOnAddedSubscriptions()
    {
        // Arrange
        var obj = new TestReactiveObject();
        var disposable = new TrackedDisposable();
        obj.AddDisposable(disposable);

        // Act
        obj.Dispose();

        // Assert
        disposable.IsDisposed.ShouldBeTrue();
    }

    [Fact]
    public void Dispose_IsIdempotent()
    {
        // Arrange
        var obj = new TestReactiveObject();
        var disposable = new TrackedDisposable();
        obj.AddDisposable(disposable);

        // Act
        obj.Dispose();
        obj.Dispose();

        // Assert
        // Should not throw — idempotent
        disposable.DisposeCount.ShouldBe(1);
    }

    [Fact]
    public void DisposeVirtual_CanBeOverridden()
    {
        // Arrange
        var obj = new DisposeTrackingReactiveObject();

        // Act
        obj.Dispose();

        // Assert
        obj.DisposeManagedCalled.ShouldBeTrue();
    }

    [Fact]
    public void Dispose_WithMultipleSubscriptions_DisposesAll()
    {
        // Arrange
        var obj = new TestReactiveObject();
        var d1 = new TrackedDisposable();
        var d2 = new TrackedDisposable();
        var d3 = new TrackedDisposable();
        obj.AddDisposable(d1);
        obj.AddDisposable(d2);
        obj.AddDisposable(d3);

        // Act
        obj.Dispose();

        // Assert
        d1.IsDisposed.ShouldBeTrue();
        d2.IsDisposed.ShouldBeTrue();
        d3.IsDisposed.ShouldBeTrue();
    }

    // ── WhenValueChanged ─────────────────────────────────────────────────

    [Fact]
    public async Task WhenValueChanged_EmitsOnPropertyChange()
    {
        // Arrange
        var obj = new TestReactiveObject();
        var values = new List<string>();

        // Act
        using var subscription = obj.WhenValueChanged(() => obj.Name)
            .Subscribe(values.Add);

        obj.Name = "Frank";
        obj.Name = "Grace";

        // Assert
        // Prepend emits initial value, then each change
        values.ShouldBe(["(default)", "Frank", "Grace"]);
    }

    [Fact]
    public async Task WhenValueChanged_EmitsCurrentValueOnSubscribe()
    {
        // Arrange
        var obj = new TestReactiveObject();
        obj.Name = "Heidi";

        // Act
        var value = await obj.WhenValueChanged(() => obj.Name).FirstAsync(TestContext.Current.CancellationToken);

        // Assert
        value.ShouldBe("Heidi");
    }

    [Fact]
    public void WhenValueChanged_FiltersByPropertyName()
    {
        // Arrange
        var obj = new TestReactiveObject();
        var nameValues = new List<string>();
        var ageValues = new List<int>();

        // Act
        using var nameSub = obj.WhenValueChanged(() => obj.Name).Subscribe(nameValues.Add);
        using var ageSub = obj.WhenValueChanged(() => obj.Age).Subscribe(ageValues.Add);

        obj.Name = "Ivan";
        obj.Age = 30;
        obj.Name = "Judy";
        obj.Age = 25;

        // Assert
        nameValues.ShouldBe(["(default)", "Ivan", "Judy"]);
        ageValues.ShouldBe([0, 30, 25]);
    }

    [Fact]
    public void WhenValueChanged_WithNonMemberExpression_Throws()
    {
        // Arrange
        var obj = new TestReactiveObject();

        // Act & Assert
        Should.Throw<ArgumentException>(() =>
            obj.WhenValueChanged(() => 42));
    }

    [Fact]
    public void WhenValueChanged_Dispose_StopsEmitting()
    {
        // Arrange
        var obj = new TestReactiveObject();
        var values = new List<string>();
        var subscription = obj.WhenValueChanged(() => obj.Name)
            .Subscribe(values.Add);

        obj.Name = "Karl";
        values.Clear(); // remove prepended + "Karl"

        // Act
        subscription.Dispose();
        obj.Name = "Leo";

        // Assert
        values.ShouldBeEmpty();
    }

    // ── Test helpers ─────────────────────────────────────────────────────

    private class TestReactiveObject : ReactiveObject
    {
        private string _name = "(default)";
        private int _age;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public int Age
        {
            get => _age;
            set => SetProperty(ref _age, value);
        }

        public void RaisePropertyChanged(string name) => OnPropertyChanged(name);
        public void AddDisposable(IDisposable d) => d.AddTo(ref DisposableBag);
    }

    private class TrackedDisposable : IDisposable
    {
        public bool IsDisposed { get; private set; }
        public int DisposeCount { get; private set; }

        public void Dispose()
        {
            IsDisposed = true;
            DisposeCount++;
        }
    }

    private class DisposeTrackingReactiveObject : ReactiveObject
    {
        public bool DisposeManagedCalled { get; private set; }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DisposeManagedCalled = true;
            }

            base.Dispose(disposing);
        }
    }
}
