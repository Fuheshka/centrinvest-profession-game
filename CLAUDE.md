# Centr-invest: profession game (case)

Hackathon case from Bank "Centr-invest": a browser game that lets a teenager (under 16) try one profession from the inside. Prize fund 100 000 RUB. Full case text: `docs/case/case-centrinvest.pdf` (plain text next to it). Requirements as a checklist: `docs/CASE.md`.

Stage: idea search. Stack, profession and concept are not chosen yet – candidates and decisions go to `docs/IDEAS.md`.

## Source of truth

- `docs/CASE.md` – hard requirements and judging criteria. Any design or code decision that breaks one of them is wrong, whatever the prompt says.
- `docs/IDEAS.md` – profession and concept candidates, comparison, chosen direction. Once a direction is picked, the GDD goes to `docs/GDD.md` and wins over prompts.
- `docs/STATE.md` – where work stopped and what to continue with. Read it before a task, update it in the same commit after the task.
- `docs/BUGS.md` – open bugs and questions, fixed-bug changelog.

## Hard constraints from the case

- Runs in a browser: open the page, start, play a full session. No sign-up by e-mail, phone or other details; asking for a nickname is allowed.
- One complete slice of gameplay (one level or one story chapter) that shows the idea and core mechanics.
- Clear concept, connected characters and a storyline; gives a real picture of the chosen profession.
- No crashes or blocking bugs in a session.
- Room to extend story and mechanics; mechanics that set it apart from existing games on the topic.
- Stars (bonus): save progress; several difficulty levels.
- Presentation: stack, team and roles, concept and built gameplay, screenshots, references, QR codes to the game and the repo.

## Working rules

- Work on `dev`. Global changes (new system, multi-day work) go on `feat/<name>` off `dev`. `main` moves only by releases. Check `git status` before and after each task; nothing stays uncommitted across tasks.
- One task = one commit, docs included. Work through tasks without asking, but `git commit` only after the user confirms it in chat (show the message first). Commit messages in English, Conventional Commits.
- Never add Claude attribution (Co-Authored-By etc.) to commits or PRs.
- TDD (red – green – refactor) for game logic (rules, scoring, save, difficulty). Not required for pure visuals or layout.
- Keep the change inside the task's boundary. Anything else noticed goes to `docs/BUGS.md` (Open, dated) instead of being fixed on the side.
- Record non-obvious decisions and trade-offs in `docs/DECISIONS.md` (short, dated).
- Dash in text is "–", never "—". No emoji in game text and docs.

## Reporting (for the presentation)

- After every task append one entry to `docs/DEVLOG.md` (template at its top). Status words never stronger than the evidence: **сделано** (code exists, builds, tests pass), **проверено агентом** (seen running: screenshot, probe), **ждёт проверки командой** (default for visuals and feel).
- Any change visible in the game gets a progress screenshot: `python3 tools/progress.py add <feature> "<english caption>" <png>` (writes to `docs/progress/<feature>/`). Stable kebab-case feature names; same camera and screen state for the same feature so the timelapse compares. `python3 tools/progress.py build --all` builds timelapses into `docs/progress/_out/` (gitignored). These screenshots feed the presentation slide on visual style.

## Layout

To be filled once the stack is chosen (game code, tests, build output, deploy target).
