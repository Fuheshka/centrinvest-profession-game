# Баги и вопросы

Формат: `- **ID** (дата) – что, где, как воспроизвести.` Исправленное переносится вниз с коммитом.

## Открыто

- **B-001** (2026-10-24) – Unity при открытии правит файлы вне задач: `ProjectSettings/ProjectSettings.asset` (companyName, bundleVersion, WebGL texture format), `Assets/Settings/UniversalRP.asset`, `UniversalRenderPipelineGlobalSettings.asset`, `ProjectSettings/SceneTemplateSettings.json`. В P-1 не коммитятся; решить, какие из них осознанные.
- **B-002** (2026-10-24) – Консоль Unity: «TextMesh Pro Essential Resources are missing» (до импорта `Assets/TextMesh Pro`) и предупреждение, что `Studio.UI` не компилируется без скриптов (уйдёт с первым UI-скриптом).
- **B-003** (2026-10-24) – `test_run` без фильтра в unity-skills гоняет около 1700 тестов пакета и блокирует Editor на минуты; запускать только `test_run_by_name` с `Studio.Tests`.
- **B-004** (2026-10-24) – Доска (P-5): рука не прокручивается, больше 8 карточек уйдут под кнопку «Исследовать рынок»; «Спросить команду» неактивна, пока нет механики (после P-6/P-12); на тач-экране не проверено; `BoardRules.Remove` в UI не вызывается – карточку на доске можно только заменить.

## Исправлено
