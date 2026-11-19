# Instructions for AI assistants

UporediPlate is an ASP.NET Core MVC app (.NET 11, Windows only) that imports salary data from RFZO and
from healthcare institutions (ZU), compares them by JMBG, and produces reports (screen, Excel, PDF).
Code, comments and UI text are in Serbian.

## Git workflow - mandatory

`main` is protected. Every change goes through a branch and a pull request.

1. **Never commit or push to `main` directly**, and never force-push it - not even for a one-line fix.
2. Start every change from an up-to-date `main` on a new branch named `<author>/<topic>`,
   e.g. `rade/izvoz-csv`. One topic per branch.
3. Write commit messages that say what changed (e.g. `Add CSV export for overtime report`), never `.`.
4. Push the branch and open a PR into `main`: `gh pr create --base main`.
5. Merge only through the PR on GitHub. Prefer **Squash and merge**, then delete the branch.
6. **Ask the user before** pushing, opening a PR, merging, deleting a branch, or rewriting any history.

Stacked PRs (B builds on A): open B with `--base <A's branch>`, merge bottom-up. After A is squash-merged,
retarget B to `main` and run `git rebase --onto origin/main <A's branch> <B's branch>` before merging B.
Do not delete A's branch on GitHub while B still targets it - GitHub closes B instead of retargeting it.

If your local `main` has diverged from `origin/main` or contains commits that are not on GitHub, stop and
ask - do not merge or push it. (The history was rewritten once to remove a leaked secret; old clones must
be re-cloned, never pushed.)

## Do not undo these decisions

- **No secrets in the repo.** The connection string lives in User Secrets; `appsettings.json` keeps a placeholder.
- **`PDFsharp-MigraDoc` is pinned to `[6.2.4]`** - the last MIT version. 6.5.0+ is AGPL and refuses to
  render without a license. Do not upgrade or unpin it.
- **`TreatWarningsAsErrors` is on.** Do not add `NoWarn` for `NU1901`-`NU1904` (vulnerable packages) -
  fix or replace the package instead.
- **Windows only** (`net11.0-windows`, startup check in `Program.cs`, PDF fonts come from Windows).
- `bin/`, `obj/` and IDE folders are git-ignored - never commit them.

## Structure

- `Data/` - `AppDbContext`; `Migrations/` stays in the project root (EF default).
- `Uvoz/` - Excel import of RFZO and ZU files, import timestamp key (`VremenskiKljuc`).
- `Izvestaji/` - one file per report (row model, texts, sorting, screen/Excel/PDF columns, query).
  `Izvestaji/Okvir/` - shared rendering for all reports. A new report = a new file here, one line in
  `RegistracijaServisa.cs`, and a checkbox in `Views/Izvestaj/Index.cshtml`.