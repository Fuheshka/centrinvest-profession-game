# STATE: где остановились

Читать перед задачей, обновлять в конце каждой задачи в том же коммите.

- **Обновлено:** 2026-10-24.
- **Этап:** реализация демо, веха M0 (`ROADMAP.md`).
- **Сделано:** `GDD.md`, `DEMO.md`, план `ROADMAP.md` с контрактами, промпты P-1…P-12 в `PROMPTS.md`, UI-макет 5 экранов в `docs/mockup/` (PNG + HTML), онлайн-версия в Claude Design у автора; роли 4 разработчиков и правила git в `TEAM.md`.
- **Каркас (P-1):** готов в ветке `p-1-skeleton`: папки `Assets/_Project/`, asmdef `Studio.Core` / `Studio.UI` / `Studio.Tests.EditMode`, все контракты заглушками (`NotImplementedException("P-N")`), сцены `Menu` и `Game`, Web-профиль, пустая сборка 10 МБ. Layout – в `CLAUDE.md`.
- **P-2:** готов в ветке `p-2-decisions` (поверх `p-1-skeleton`): `GameState.New`, `DecisionResolver` (CanAfford, Apply), 10 тестов; всего 14/14 зелёные. Дальше от P-2 зависят P-3, P-6, P-7.
- **P-3:** готов в ветке `p-3-board-rules` (поверх `p-2-decisions`): `BoardRules` (Place, Remove, Connect, Disconnect, Thread, Reveal, Research, Goals, Commit), 30 тестов; всего 44/44 зелёные. Контракт в `ROADMAP.md` обновлён (`Thread` получил `showThreads`), решение в `DECISIONS.md`. Дальше от P-3 зависят P-5 (UI доски), P-6 (переход Idea -> Production через `Commit`).
- **Дальше:** мерж P-1 в `dev`, затем P-2…P-5 параллельно по ролям из `TEAM.md`. Без кода – Н-1…Н-9 из `NONCODE.md`, сразу: тексты, звук, арт, название, правда о профессии.
- **Открытые вопросы к автору:** дата дедлайна хакатона (в ROADMAP – [ДАТА]). Состав: 2 разработчика, 2 художника (джуны).
