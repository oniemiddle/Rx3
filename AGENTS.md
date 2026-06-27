## Agent skills

### Issue tracker

Issues and PRDs live as GitHub issues on `https://github.com/oniemiddle/Rx3`. See `docs/agents/issue-tracker.md`.

### Triage labels

The five canonical triage roles use their default label names. See `docs/agents/triage-labels.md`.

### Domain docs

Single-context layout — one CONTEXT.md + docs/adr/ at the repo root. See `docs/agents/domain.md`.

### Branching strategy

`main` with `feat/*`, `fix/*`, `release/*` branches, PR-only merges to main, SemVer tagging. See `docs/branching-strategy.md`.

### Release automation

[Release Please](https://github.com/googleapis/release-please) automatically creates release PRs from conventional commits. Merge a release PR to tag, publish, and update CHANGELOG.md.

**Commit convention required:**
- `feat: ...` — minor bump (`0.2.0`)
- `fix: ...` — patch bump (`0.1.1`)
- `chore:` / `docs:` — no bump
- `BREAKING CHANGE:` — major bump (`1.0.0`)

See `.github/workflows/release-please.yml` and `release-please-config.json`.
