# Skill Registry — TallerMecanico_Arqui_Services

**Project**: TallerMecanico_Arqui_Services
**Generated**: 2026-05-07
**Mode**: engram (SDD initialized)

---

## User Skills (Global)

Located in `~/.config/opencode/skills/`:

| Skill | Trigger | Description |
|-------|--------|------------|
| sdd-init | "sdd init", "iniciar sdd" | Initialize SDD context |
| sdd-propose | "sdd-new", "propose" | Create change proposal |
| sdd-spec | "sdd-spec" | Write specifications |
| sdd-design | "sdd-design" | Technical design |
| sdd-tasks | "sdd-tasks" | Task breakdown |
| sdd-apply | "sdd-apply" | Implement change |
| sdd-verify | "sdd-verify" | Verify implementation |
| sdd-archive | "sdd-archive" | Archive completed change |
| sdd-explore | "sdd-explore" | Explore/investigate |
| sdd-onboard | "sdd-onboard" | SDD workflow walkthrough |
| branch-pr | "create pr", "open pr" | PR creation workflow |
| chained-pr | >400 lines, "chained pr" | Split large PRs |
| issue-creation | "create issue", "report bug" | Issue creation |
| judgment-day | "judgment day", "review adversarial" | Dual blind review |
| comment-writer | PR comments, feedback | Warm human comments |
| cognitive-doc-design | docs, READMEs, guides | Cognitive load reduction |
| work-unit-commits | commits, "work unit" | Deliverable commits |
| go-testing | "go test", "testing patterns" | Go testing patterns |
| skill-creator | "create skill", "new skill" | Create AI skills |

---

## Project Conventions

### Architecture
- **Pattern**: Layered Architecture (Core → Services → API)
- **DI**: .NET native dependency injection
- **Service Pattern**: Interfaces (IClienteService, IVehiculoService, etc.)
- **Result Pattern**: Custom Result<T> type for service responses

### Testing
- **Framework**: xUnit 2.5.3
- **Mocking**: Moq 4.20.72
- **Coverage**: coverlet.collector 6.0.0
- **Target**: net10.0 (tests), net8.0 (services)

### Active Code Surfaces
- `Frontend/` — Razor Pages
- `OrdenTrabajoService/` — PostgreSQL + SQL
- `UsersService/` — User management
- `tests/` — xUnit unit tests

### Not Active (compiled artifacts only)
- `src/TallerMecanico.*/` — Legacy layered architecture

---

## Notes

- No .editorconfig, linter, or formatter configured
- Multi-service repository (separate project directories)
- Legacy src/ projects are compiled artifacts, not actively edited
- PostgreSQL local via Docker (see README.md)