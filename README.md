# Rx3

> 基于 [R3](https://github.com/Cysharp/R3) 的轻量级 MVVM 框架，面向 Avalonia 应用。  
> ReactiveUI 的现代化替代品，拥抱 C# 13 + R3 设计哲学。

[![GitHub Release](https://img.shields.io/github/v/release/oniemiddle/Rx3)](https://github.com/oniemiddle/Rx3/releases)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4)](https://dotnet.microsoft.com/)
[![R3](https://img.shields.io/badge/R3-1.3.1-blue)](https://github.com/Cysharp/R3)
[![License](https://img.shields.io/github/license/oniemiddle/Rx3)](LICENSE)

---

## Features

- **ReactiveObject** — 响应式基类，`INotifyPropertyChanged` + `DisposableBag` 订阅生命周期管理
- **BindableReactiveProperty\<T\>** — 可绑定响应式属性，直接对接 XAML 绑定
- **ReactiveCommand** — 基于 R3 的 `ICommand` 实现，可观察的执行事件流
- **`[Reactive]` Source Generator** — C# 13 分部属性自动生成属性通知
- **`[ReactiveCommand]` Source Generator** — 方法标记自动生成 `ICommand`
- **Rx3Analyzer** — 编译时诊断，预防订阅泄漏
- **Avalonia 集成** — `UseRx3()` 一行接入 + 绑定扩展

## Installation

```bash
dotnet add package Rx3
dotnet add package Rx3.Avalonia
```

> NuGet 包尚未发布，目前通过 [GitHub Releases](https://github.com/oniemiddle/Rx3/releases) 获取源码。

## Quick Start

### 1. 初始化 Avalonia 应用

```csharp
// App.axaml.cs
public static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .UseRx3();
```

### 2. 定义 ViewModel

```csharp
public partial class LoginViewModel : ReactiveObject
{
    [Reactive]
    public partial string Username { get; set; }

    [Reactive]
    public partial string Password { get; set; }

    public BindableReactiveProperty<bool> IsLoading { get; } = new();

    [ReactiveCommand]
    private async Task LoginAsync()
    {
        IsLoading.Value = true;
        try
        {
            await Task.Delay(1000); // simulate login
            // ...
        }
        finally
        {
            IsLoading.Value = false;
        }
    }
}
```

### 3. 绑定到 View

```xml
<StackPanel>
    <TextBox Text="{Binding Username.Value}" Watermark="Username" />
    <TextBox Text="{Binding Password.Value}" Watermark="Password" />
    <Button Content="Login" Command="{Binding LoginCommand}" />
    <ProgressBar IsVisible="{Binding IsLoading.Value}" />
</StackPanel>
```

### 4. 生命周期管理

```csharp
public partial class MyViewModel : ReactiveObject
{
    private readonly IDisposable _subscription;

    public MyViewModel()
    {
        _subscription = Observable.Interval(TimeSpan.FromSeconds(1))
            .Subscribe(x => /* ... */)
            .AddTo(ref DisposableBag); // auto-cleanup on Dispose
    }

    // Dispose(bool) is auto-analyzed by Rx3Analyzer (RX3_001)
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // DisposableBag cleans up all added subscriptions
        }
        base.Dispose(disposing);
    }
}
```

## API Overview

### ReactiveObject

| Member | Description |
|---|---|
| `SetProperty<T>(ref T, T, [CallerMemberName])` | Set backing field + raise `PropertyChanged` |
| `OnPropertyChanged([CallerMemberName])` | Direct raise for source-generator properties |
| `WhenValueChanged<T>(Expression<Func<T>>)` | Observable stream for any property |
| `DisposableBag` | Subscription container, auto-disposed |
| `Dispose()` / `Dispose(bool)` | Standard dispose pattern |

### BindableReactiveProperty\<T\>

```csharp
var name = new BindableReactiveProperty<string>("initial");
name.Value = "updated";          // triggers PropertyChanged
name.AsObservable().Subscribe(); // observable stream
```

### ReactiveCommand

直接使用 R3 的类型:

```csharp
var canExecute = new BehaviorSubject<bool>(true);
var cmd = canExecute.ToReactiveCommand(_ => DoSomething());
cmd.Execute(null);    // ICommand
cmd.Subscribe(...);   // Observable<Unit>
```

### Source Generators

```csharp
// [Reactive] — partial property → auto-generated backing field + notification
[Reactive]
public partial string Name { get; set; }
// Generates: WhenNameChanged() Observable<string>

// [ReactiveCommand] — method → auto-generated ICommand property
[ReactiveCommand]
private async Task SaveAsync() { /* ... */ }
// Generates: SaveCommand (ReactiveCommand) + WhenSave() Observable<Unit>
```

## Requirements

- .NET 10.0+
- Avalonia 11.2+ (for `Rx3.Avalonia`)
- R3 1.3.1+

## Development

```bash
git clone https://github.com/oniemiddle/Rx3.git
cd Rx3
dotnet build
dotnet run --project test/Rx3.Tests
```

### Commit Convention

This project uses [Conventional Commits](https://www.conventionalcommits.org/):

```
feat: add [Reactive] source generator
fix: correct dispose ordering in ReactiveObject
chore: update dependencies
docs: add binding examples to README
```

Release Please automatically creates release PRs from these commits.

## Project Structure

```
Rx3/
├── src/
│   ├── Rx3/                          # Core library
│   │   ├── ReactiveObject.cs
│   │   ├── BindableReactiveProperty.cs
│   │   └── ...
│   ├── Rx3.Avalonia/                  # Avalonia integration
│   │   ├── AvaloniaProviderInitializer.cs
│   │   └── AvaloniaBindingExtensions.cs
│   └── Rx3.SourceGenerator/           # Source generators + analyzers
│       ├── ReactivePropertyGenerator.cs
│       ├── ReactiveCommandGenerator.cs
│       └── Rx3Analyzer.cs
├── test/
│   └── Rx3.Tests/
├── docs/
│   └── ...
└── README.md
```

## License

MIT
