# Git Branching

## Rules

- **`main` is protected** — no direct pushes, all changes go through pull requests
- Every PR requires approval before merging

## Branch Naming

Use a prefix that describes the type of work:

| Prefix | Use |
|--------|-----|
| `feature/` | New functionality |
| `fix/` | Bug fixes |
| `docs/` | Documentation changes |
| `refactor/` | Code restructuring without behavior change |
| `chore/` | Build config, CI, dependencies |

Examples: `feature/cutting-board-block`, `fix/ink-color-mapping`, `docs/crafting-guide`

## Workflow

1. Create a branch from `main`
2. Make changes and commit
3. Push branch and open a PR via `gh pr create`
4. Get approval, then merge
5. Delete the branch after merge
