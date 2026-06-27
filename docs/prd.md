# PRD: Rx3 — 轻量级 MVVM 框架 for R3 + Avalonia

## Problem Statement

.NET 生态中缺少一个既拥抱 R3（Cysharp 的新一代 Reactive Extensions）设计哲学，又轻量精简的 MVVM 框架。ReactiveUI 虽然功能完整但过于臃肿，依赖传统的 Rx.NET（`System.Reactive`），无法利用 R3 的性能优势、`TimeProvider`/`FrameProvider` 抽象以及 `OnErrorResume` 不中断管道的设计。

开发者在构建 Avalonia 响应式应用时面临两难：要么使用完整但沉重的 ReactiveUI，要么自己手工拼凑 R3 + MVVM 的胶水代码。需要一个专门为 R3 设计、拥抱 Frame-based 操作、同时提供 Source Generator 减少样板代码的轻量级 MVVM 框架。

## Solution

**Rx3** — 一个基于 R3 的轻量级 MVVM 框架，核心设计理念：

- **原生 R3**：底层依赖 R3，而非传统的 `System.Reactive`。原生使用 `TimeProvider`/`FrameProvider`，Frame-based 操作是一等公民而非附加组件。
- **双层 API**：80% 场景用 `[Reactive]` + Source Generator 自动生成样板代码；20% 热路径场景直接裸 R3 Observable + FrameProvider，零通知接口开销。
- **精简内核**：只提供 ViewModel 层的基类和工具，不涉足 DI、View 基类、ViewModel 定位器。用户自由选择基础设施。
- **Avalonia 友好**：提供 `UseR3()` 一行接入、Avalonia 专用的 `TimeProvider`/`FrameProvider`、XAML 绑定扩展。

## User Stories

### ViewModel 开发

1. As a **ViewModel 开发者**, I want **用 `[Reactive]` 标记字段就能自动生成可绑定属性 + INotifyPropertyChanged**， so that **我不用手写重复的属性样板代码**。
2. As a **ViewModel 开发者**, I want **用 `[ReactiveCommand]` 标记方法就能自动生成 ICommand 属性**， so that **我不用手写 Command 样板代码**。
3. As a **ViewModel 开发者**, I want **`ReactiveObject` 内置订阅生命周期管理（DisposableBag）**， so that **我不担心订阅泄漏，`Dispose()` 时自动清理所有订阅**。
4. As a **ViewModel 开发者**, I want **`ReactiveObject` 提供 `SetProperty<T>()` 方法，支持手动编写属性**， so that **在需要手写优化的场景我可以绕过 Source Generator**。
5. As a **ViewModel 开发者**, I want **`WhenValueChanged<T>()` 返回 `Observable<T>`**， so that **我可以将任意属性的变化接入 Rx 管道进行组合操作**。
6. As a **ViewModel 开发者**, I want **`BindableReactiveProperty<T>` 可直接用于 XAML 绑定（`{Binding Prop.Value}`）**， so that **我可以零摩擦地将响应式属性绑定到 View**。
7. As a **ViewModel 开发者**, I want **`ReadOnlyReactiveProperty<T>` 公开只读的可绑定属性**， so that **外部只能观察不能修改，符合封装原则**。
8. As a **ViewModel 开发者**, I want **`ReactiveCommand` 接受 `Observable<bool>` 作为 CanExecute 源**， so that **命令的可用性自动与 View 状态同步**。
9. As a **ViewModel 开发者**, I want **`ReactiveCommand` 暴露 `ThrownExceptions` Observable 流**， so that **我可以集中处理命令执行中的异常而不需要 try-catch 每一个**。
10. As a **ViewModel 开发者**, I want **`ReactiveObject` 的 `Dispose()` 方法可重写**， so that **子类可以添加额外的清理逻辑**。
11. As a **ViewModel 开发者**, I want **`DisposableBag` 是 struct 类型，零分配**， so that **在高频场景下不会产生 GC 压力**。
12. As a **ViewModel 开发者**, I want **`AddTo(ref DisposableBag)` 扩展方法**， so that **我可以将任意 `IDisposable` 订阅快速加入生命周期管理**。

### 帧操作（Frame-based Operations）

13. As a **性能敏感型开发者**, I want **`Observable.EveryUpdate(frameProvider)` 每个 UI 帧推送一次值**， so that **我可以实现高频率轮询或帧同步逻辑**。
14. As a **性能敏感型开发者**, I want **`Observable.IntervalFrame(n, frameProvider)` 每隔 n 帧推送一次值**， so that **我可以降低轮询频率减少开销**。
15. As a **性能敏感型开发者**, I want **`Observable.EveryValueChanged(source, x => x.Property, frameProvider)` 通过帧轮询检测属性变化**， so that **我可以观察不实现 `INotifyPropertyChanged` 的对象的属性变化**。
16. As a **性能敏感型开发者**, I want **`DelayFrame()`、`DebounceFrame()`、`ThrottleFirstFrame()` 等帧级操作符**， so that **我可以用帧计数而非时间来控制操作节奏，避免帧率不稳的问题**。
17. As a **性能敏感型开发者**, I want **`NextFrame()` 跳过当前帧、在下一帧推送**， so that **我可以实现"推迟到下一帧执行"的常见 UI 模式**。
18. As a **性能敏感型开发者**, I want **所有 Frame 操作符都不经过 `INotifyPropertyChanged`**， so that **热路径没有任何通知接口开销**。

### Avalonia 集成

19. As a **Avalonia 应用开发者**, I want **在 `AppBuilder` 中调用 `.UseR3()` 完成全部初始化**， so that **一行代码即可接入 Rx3**。
20. As a **Avalonia 应用开发者**, I want **`.UseR3()` 自动设置 `AvaloniaDispatcherTimeProvider` 和 `AvaloniaDispatcherFrameProvider` 为默认 Provider**， so that **时间/帧操作自动在 UI 线程上执行，无需手动 `ObserveOn`**。
21. As a **Avalonia 应用开发者**, I want **`ObserveOnDispatcher()` 扩展方法**， so that **当需要显式跳转到 UI 线程时有明确的 API 可用**。
22. As a **Avalonia 应用开发者**, I want **`BindTo(target, x => x.Property)` 扩展方法**， so that **我可以将 Observable 流直接绑定到 Avalonia 控件的属性上**。
23. As a **Avalonia 应用开发者**, I want **`AvaloniaRenderingFrameProvider` 绑定到 CompositionTarget.Rendering 事件**， so that **帧同步操作与渲染管线对齐，性能最优**。

### 测试

24. As a **测试开发者**, I want **使用 `FakeTimeProvider` 测试时间相关的 Observable 管道**， so that **测试不依赖真实时间，快速且确定性强**。
25. As a **测试开发者**, I want **使用 `FakeFrameProvider` 测试帧相关的操作符**， so that **我可以精确控制帧的推进，验证帧级行为**。
26. As a **测试开发者**, I want **`ReactiveObject` 的 `DisposableBag` 提供 `IsDisposed` 状态查询**， so that **我可以在测试中验证订阅是否在 Dispose 后被正确清理**。

### 扩展性

27. As a **框架高级用户**, I want **可以自定义 `TimeProvider` 和 `FrameProvider`**， so that **Rx3 可以在非 Avalonia 平台（WPF、WinForms、控制台）上运行**。
28. As a **框架高级用户**, I want **`ObservableSystem` 的可配置默认 Provider 被 Rx3 的初始化过程正确设置**， so that **我的自定义 Provider 能无缝接入整个管道**。

## Implementation Decisions

### 模块划分

项目分为三个核心包和一个初始化扩展：

1. **Rx3 (Core)** — `src/Rx3/`
   - `ReactiveObject` — 抽象基类，实现 `INotifyPropertyChanged`，内置 `DisposableBag`（struct，零分配）
   - `BindableReactiveProperty<T>` — 包装 R3 的 `ReactiveProperty` 并实现 `INotifyPropertyChanged`/`INotifyDataErrorInfo`
   - `ReadOnlyReactiveProperty<T>` — `BindableReactiveProperty` 的只读包装
   - `ReactiveCommand` / `ReactiveCommand<T>` — 基于 `Observable<bool>` CanExecute 源的 `ICommand` 实现
   - `DisposableBag` — 轻量级 struct 订阅容器，支持 `Add/Clear/Dispose`
   - 扩展方法：`AddTo(ref DisposableBag)`、`WhenValueChanged<T>()` 等

2. **Rx3.SourceGenerator (Source Gen)** — `src/Rx3.SourceGenerator/`
   - `ReactivePropertyGenerator` — 增量 Source Generator，识别 `[Reactive]` 标记的字段，生成完整属性 + `INotifyPropertyChanged` + Observable 流
   - `ReactiveCommandGenerator` — 增量 Source Generator，识别 `[ReactiveCommand]` 标记的方法，生成 `ICommand` 属性

3. **Rx3.Avalonia (Avalonia Bindings)** — `Rx3.Avalonia/`
   - `AvaloniaProviderInitializer` — `UseR3()` 扩展方法，设置默认 Provider
   - `AvaloniaDispatcherTimeProvider` — 基于 `DispatcherTimer` 的 TimeProvider
   - `AvaloniaDispatcherFrameProvider` — 基于 `DispatcherTimer` 轮询的 FrameProvider（默认 60fps）
   - `AvaloniaRenderingFrameProvider` — 绑定 `TopLevel.Rendering` 事件的高性能 FrameProvider
   - Avalonia 绑定扩展方法集合

### 架构决策

- **ReactiveObject 的 Dispose 模式**：基类实现 `IDisposable`，内置 `DisposableBag` 字段。`Dispose()` 调用 `DisposableBag.Dispose()` + 清理 `PropertyChanged` 订阅。子类通过重写 `Dispose(bool disposing)` 扩展清理逻辑。不自动绑定 ViewModel 生命周期，View 层不负责 Dispose ViewModel。

- **Source Generator vs 手写**：`[Reactive]` 生成的代码走 `INotifyPropertyChanged` 通知路径（适合 XAML 绑定）。热路径场景直接使用 `Observable.EveryUpdate(frameProvider).Subscribe(...)` 或裸 R3 `Observable` 链，跳过所有通知层。两个路径同属框架一等公民。

- **Frame 操作作为一等抽象**：`FrameProvider` 与 `TimeProvider` 平行的顶级抽象。R3 的 `Interval`/`Delay`/`Debounce` 等操作符都有对应的 `IntervalFrame`/`DelayFrame`/`DebounceFrame` 版本。Rx3 不额外发明新的 Frame API，而是依赖 R3 的原生 Frame 操作符。

- **依赖关系**：
  - `Rx3` → `R3` (only)
  - `Rx3.SourceGenerator` → `Microsoft.CodeAnalysis.CSharp` (仅构建时)
  - `Rx3.Avalonia` → `Rx3` + `R3Extensions.Avalonia`

- **初始化流程**（Avalonia）：
  ```csharp
  // 用户 App.axaml.cs
  public static AppBuilder BuildAvaliaApp()
      => AppBuilder.Configure<App>()
          .UsePlatformDetect()
          .UseR3(); // 设置 AvaloniaDispatcherTimeProvider + AvaloniaDispatcherFrameProvider
  ```
  `UseR3()` 内部调用 `AvaloniaProviderInitializer.SetDefaultObservableSystem()`，设置 `ObservableSystem.DefaultTimeProvider` 和 `DefaultFrameProvider`，同时注册 `UnhandledExceptionHandler`。

## Testing Decisions

### 测试哲学

- **只测外部行为，不测实现细节**。测试应该通过公开 API 验证行为契约，而不是断言内部状态或私有方法的调用。
- **使用 `FakeTimeProvider` / `FakeFrameProvider` 替代真实 Provider**，确保测试快速、确定、不依赖真实时间。
- **Source Generator 使用快照测试**（`Verify` / `ApprovalTests`），验证生成的代码符合预期形状。

### 测试模块

| 模块 | 测试类型 | 测试内容 |
|---|---|---|
| **ReactiveObject** | 单元测试 | `SetProperty` 触发 `PropertyChanged`；`WhenValueChanged` 正确推送变化；`Dispose` 清理所有订阅 |
| **BindableReactiveProperty** | 单元测试 | 值变化时触发 `PropertyChanged`；去重逻辑（相同值不通知）；初始值正确；`Value` setter 工作正常 |
| **ReactiveCommand** | 单元测试 | CanExecute 根据 Observable 动态变化；Execute 触发订阅的回调；`ThrownExceptions` 捕获异常；`IsDisposed` 状态 |
| **DisposableBag** | 单元测试 | 多个订阅管理；`Dispose` 清理全部；重复 Dispose 安全；struct 复制语义不泄漏 |
| **Source Generators** | 快照测试 | `[Reactive]` 生成正确的属性代码；`[ReactiveCommand]` 生成正确的命令代码；同时使用两个特性 |
| **AvaloniaProviderInitializer** | 集成测试 | `UseR3()` 正确设置默认 Provider；`UnhandledExceptionHandler` 注册成功 |

### 测试工具

- 测试框架：xUnit
- 模拟框架：NSubstitute（如需要）
- 时间模拟：R3 自带的 `FakeTimeProvider` / `FakeFrameProvider`
- Source Generator 测试：`Microsoft.CodeAnalysis.CSharp.SourceGenerators.Testing` + `Verify.SourceGenerators`
- 断言：Shouldly / FluentAssertions

## Out of Scope

- **DI / IoC 容器集成** — 用户自行选择 DI 方案，框架不提供也不依赖任何容器。
- **View 基类** — 不提供 `ReactiveWindow`、`ReactiveUserControl` 等 View 层基类。
- **ViewModel 定位器 / 导航** — 不提供 ViewModel 定位或导航框架。
- **Avalonia 之外的 UI 框架支持** — V0.1 和 V0.2 阶段仅支持 Avalonia。接口设计中预留扩展点但不提供实现。
- **ObservableCollection / 集合绑定** — 集合相关的变更观察推荐使用 `ObservableCollections.R3`，框架本身不额外封装。
- **消息总线 / Event Aggregator** — 推荐使用 `MessagePipe`，框架不内置。
- **编译时验证 / 代码分析器** — 不对 `[Reactive]` 标记的错误用法（如非 `partial class`、非字段目标等）提供 Roslyn Analyzer 诊断，V0.1 编译器错误即可。
- **UI 测试 / 自动化测试工具** — 不提供 View 层的自动化测试工具。
- **性能分析工具** — 不提供 `ObservableTracker` 之类的运行时性能监控。

## Further Notes

### MVP 分阶段计划

**V0.1 — 核心骨架**
- `ReactiveObject` 基类（内置 DisposableBag + INotifyPropertyChanged）
- `BindableReactiveProperty<T>` / `ReadOnlyReactiveProperty<T>`
- `ReactiveCommand` / `ReactiveCommand<T>`（基于 Observable CanExecute + ICommand）
- Source Generator: `[Reactive]` 属性生成
- Source Generator: `[ReactiveCommand]` 命令生成
- `AvaloniaProviderInitializer` + `UseR3()`
- 基础 Avalonia 绑定扩展（`BindTo`, `ObserveOnDispatcher`）
- 单元测试覆盖上述全部
- 发布 NuGet 包

**V0.2 — 补充特性**
- Avalonia 绑定扩展完善（`SubscribeToText`, `SubscribeToIsVisible` 等）
- `AvaloniaRenderingFrameProvider` 支持
- `EveryValueChanged` 帧轮询支持
- 更多测试覆盖（集成测试 + 快照测试）
- 示例项目（Counter, TodoMVVM, 性能对比基准）

### 与 R3 的关系

Rx3 不修改或 fork R3。它是在 R3 之上的一层 MVVM 约定层。所有 Frame-based 操作直接使用 R3 原生 API（`Observable.IntervalFrame`、`EveryUpdate`、`DelayFrame` 等），Rx3 不重新实现 Frame 操作符。

### 命名约定

- 项目名：Rx3（读作 "Rx-Three"）
- 命名空间：`Rx3`、`Rx3.Avalonia`
- NuGet 包名：`Rx3`、`Rx3.Avalonia`、`Rx3.SourceGenerator`
- Source Generator 特性名：`[Reactive]`、`[ReactiveCommand]`
