# Branching Strategy

## Overview

```
main
  └── feat/       ← feature branches
  └── fix/        ← bug fix branches
  └── release/    ← release preparation branches
```

## Branch Roles

| Branch | Purpose | Merge Rule |
|---|---|---|
| `main` | Stable, always releasable. Each merge triggers NuGet publish. | PR only |
| `feat/<module>` | Feature development. Branch from `main`, PR back to `main`. | PR → main |
| `fix/<desc>` | Bug fixes. Branch from `main`, PR back to `main`. | PR → main |
| `release/v*` | Release preparation (version bump, final testing). | PR → main + tag |

## Workflow

1. Create feature branch from `main`: `git checkout -b feat/reactive-object`
2. Develop and commit on the feature branch
3. Open a PR to merge back into `main`
4. After PR approval/merge, delete the feature branch
5. Tag releases on `main`: `git tag v0.1.0`

## V0.1 Feature Branch Plan

| Order | Branch | Content | Depends On |
|---|---|---|---|
| 1 | `feat/reactive-object` | ReactiveObject base class + DisposableBag | — |
| 2 | `feat/reactive-property` | BindableReactiveProperty + ReadOnlyReactiveProperty | 1 |
| 3 | `feat/reactive-command` | ReactiveCommand | 1 |
| 4 | `feat/avalonia-init` | AvaloniaProviderInitializer + UseR3() + binding extensions | 2, 3 |
| 5 | `feat/source-generator-reactive` | `[Reactive]` source generator | 1 |
| 6 | `feat/source-generator-command` | `[ReactiveCommand]` source generator | 5 |
| 7 | `release/v0.1.0` | V0.1 release | 1–6 |

## Versioning (SemVer)

### Pre-1.0 (`0.x.y`)

| Version | Content |
|---|---|
| `0.1.0` | MVP core skeleton |
| `0.1.1`, `0.1.2`, … | Patch fixes |
| `0.2.0` | Supplementary features |

### Stable (`1.0.0+`)

Standard SemVer: `MAJOR.MINOR.PATCH`
