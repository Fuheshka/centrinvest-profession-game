# Centr-invest: profession game (case)

Hackathon case from Bank "Centr-invest": a browser game that lets a teenager (under 16) try one profession from the inside. Prize fund 100 000 RUB. Full case text: `docs/case/case-centrinvest.pdf` (plain text next to it). Requirements as a checklist: `docs/CASE.md`.

Chosen: a game about a game designer (`docs/GDD.md`), demo scope in `docs/DEMO.md`. Stack: Unity 6, Universal 2D, Web build.

## Source of truth

- `docs/CASE.md` – hard requirements and judging criteria. Any design or code decision that breaks one of them is wrong, whatever the prompt says.
- `docs/IDEAS.md` – profession and concept candidates, comparison, chosen direction. Once a direction is picked, the GDD goes to `docs/GDD.md` and wins over prompts.
- `docs/STATE.md` – where work stopped and what to continue with. Read it before a task, update it in the same commit after the task.
- `docs/BUGS.md` – open bugs and questions, fixed-bug changelog.
- `docs/ROADMAP.md` – milestones and class contracts; `docs/PROMPTS.md` – task cards; `docs/TEAM.md` – who owns what; `docs/NONCODE.md` – non-code tasks; `docs/AUDIO.md` – sound and music list; `docs/mockup/` – UI mockups (PNG + HTML).

## Hard constraints from the case

- Runs in a browser: open the page, start, play a full session. No sign-up by e-mail, phone or other details; asking for a nickname is allowed.
- One complete slice of gameplay (one level or one story chapter) that shows the idea and core mechanics.
- Clear concept, connected characters and a storyline; gives a real picture of the chosen profession.
- No crashes or blocking bugs in a session.
- Room to extend story and mechanics; mechanics that set it apart from existing games on the topic.
- Stars (bonus): save progress; several difficulty levels.
- Presentation: stack, team and roles, concept and built gameplay, screenshots, references, QR codes to the game and the repo.

## Working rules

- Four developers share the repo; ownership of folders and git rules are in `docs/TEAM.md`. Edit only files your role owns. Every task card gets its own branch off `dev` (`p-N-short-name`) and a PR into `dev` (squash merge). `main` moves only by releases. Check `git status` before and after each task; nothing stays uncommitted across tasks.
- One task = one commit, docs included. Work through tasks without asking, but `git commit` only after the user confirms it in chat (show the message first). Commit messages in English, Conventional Commits.
- Never add Claude attribution (Co-Authored-By etc.) to commits or PRs.
- UI lives in scenes and prefabs (`Assets/_Project/Prefabs/UI`, `Scenes/`), not built by code (`new GameObject`, `AddComponent`). Code only links inspector references and updates data; list rows are `Instantiate`d row prefabs. Artists must be able to edit the look without touching code.
- TDD (red – green – refactor) for game logic (rules, scoring, save, difficulty). Not required for pure visuals or layout.
- Keep the change inside the task's boundary. Anything else noticed goes to `docs/BUGS.md` (Open, dated) instead of being fixed on the side.
- Task prompts (cards in `docs/PROMPTS.md`, hand-offs to other agents) follow the template in `.agent/skills/task-prompt/SKILL.md`.
- Record non-obvious decisions and trade-offs in `docs/DECISIONS.md` (short, dated).
- Dash in text is "–", never "—". No emoji in game text and docs.

## Reporting (for the presentation)

- After every task append one entry to `docs/DEVLOG.md` (template at its top). Status words never stronger than the evidence: **сделано** (code exists, builds, tests pass), **проверено агентом** (seen running: screenshot, probe), **ждёт проверки командой** (default for visuals and feel).
- Any change visible in the game gets a progress screenshot: `python3 tools/progress.py add <feature> "<english caption>" <png>` (writes to `docs/progress/<feature>/`). Stable kebab-case feature names; same camera and screen state for the same feature so the timelapse compares. `python3 tools/progress.py build --all` builds timelapses into `docs/progress/_out/` (gitignored). These screenshots feed the presentation slide on visual style.

## Layout

Unity 6, Universal 2D (URP), target Web (WebGL). Everything of ours lives in `Assets/_Project/`; ownership per folder is in `docs/TEAM.md`.

- `Scripts/Core/` – assembly `Studio.Core` (namespace `Studio.Core`), engine-only logic, no UI references. `Enums.cs`; `Data/` ScriptableObjects (`CardSO`, `ComboSO`, `OptionSO`, `MessageSO`, `QuoteSO`, `ContentDB`); `State/` serializable state (`GameState`, `BoardState`, `DecisionRecord`, `VersionRecord`); `Systems/` static classes (`DecisionResolver`, `BoardRules`, `DayClock`, `ReviewSystem`, `Difficulty`, `SaveSystem`). Contracts: `docs/ROADMAP.md`.
- `Scripts/UI/{Messenger,Menu,Board,Versions,Common}/` – assembly `Studio.UI` (references Core, TextMeshPro, UnityEngine.UI).
- `Tests/EditMode/{Core,Board,Review}/` – assembly `Studio.Tests.EditMode` (Editor only, NUnit). Each role tests its own area.
- `Data/Test/` – test ScriptableObjects; `Data/Level1/` – content of level "First release".
- `Scenes/` – `Menu` (build index 0) and `Game`. `Prefabs/UI/{Common,Messenger,Board,Versions}/` – root prefabs placed in `Game`. `Art/` – sprites and fonts.
- Web build: `Builds/Web/` (gitignored), Brotli + Decompression Fallback + Data Caching. Serve locally: `cd Builds/Web && python3 -m http.server`.
- Run tests: Unity Editor, Window > General > Test Runner > EditMode > Run All. Via REST (Editor open): `POST /skill/test_run_by_name {"testName":"Studio.Tests"}`, then `test_get_result` with the returned `jobId`. Do not use plain `test_run` – it also runs the unity-skills package tests (about 1700) and blocks the Editor for minutes.

## Environment notes

- Unity REST (unity-skills package `com.besty.unity-skills`) runs at `http://localhost:8090/` while the Editor is open; add `?expectProject=centrinvest-profession-game` to every call. Skill docs: `.agent/skills/unity-skills/SKILL.md`. After editing `.cs`, call `asset_refresh` and wait on the returned `waitUrl` for compile errors.
