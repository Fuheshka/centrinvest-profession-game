# PROMPTS: карточки задач

**Актуально (2026-10-25):** MVP уровня 0 «Обучение» – требования [MVP_SPEC.md](MVP_SPEC.md), правила [MVP_RULES.md](MVP_RULES.md), карточки [P-18…P-29](#mvp-tutorial). Начинать с P-19. После P-29 предложен отдельный post-MVP трек визуальной полировки P-30…P-32. Прежние P-18…P-26 (план `GAMEPLAY_REWORK.md`) заменены и лежат в архиве в конце файла. Старые карточки P-1…P-16 – история прототипа; P-12, P-13, P-15, P-16 отложены.

Каждая карточка – готовый промпт (шаблон `.agent/skills/task-prompt/SKILL.md` в репозитории отсутствует; рабочая структура – P-1 и карточки MVP) для отдельного чата с агентом (Codex или другой агент в этой репе). Скопировать блок целиком. Под заголовком строка статуса, обновлять вместе с галочкой в `ROADMAP.md`.

Кто что делает и правила git – `TEAM.md`. Макеты экранов – `docs/mockup/*.png`. Промпты подходят для любой нейронки: агент в репе читает файлы сам, в браузерный чат – приложить файлы (см. `TEAM.md`, «Нейронки»).

**Правило UI для всех карточек.** Интерфейс собирается в сцене и префабах (Assets/_Project/Prefabs/UI, сцены Scenes/), а не создаётся кодом через new GameObject / AddComponent. Код только связывает: публичные ссылки на Button, TMP_Text, Transform настраиваются в инспекторе, скрипт слушает события и обновляет текст. Исключения (динамические списки) – Instantiate префаба-строки, а не сборка из примитивов. Так художник правит вид без кода. Сцену и префабы собирай через unity-skills REST, ссылки между объектами ставь component_set_property.

Общее для всех карточек (уже внутри каждого промпта): прочитать `CLAUDE.md`, `docs/TEAM.md`, `docs/STATE.md`, `docs/ROADMAP.md` (раздел «Контракты»), `docs/MVP_RULES.md`; своя ветка от `dev` и PR; коммит только после подтверждения автора; запись в `docs/DEVLOG.md` и обновление `docs/STATE.md` в том же коммите.

---

## P-1 Каркас проекта

**Тип.** код · **Статус.** сделано (ждёт мержа) · **Кто.** Р1 · Ядро · **Модель.** лёгкая (Sonnet), усилие среднее · **Зависит от.** –

```
Проект «Билд к пятнице» (Unity 6, Universal 2D, Web). Роль: Р1 Ядро – правишь только свои папки из docs/TEAM.md (на этой карточке – весь каркас, остальные ещё не начали). Ветка: p-1-skeleton от свежего dev. Задача: P-1 – каркас проекта, сборки, все контракты заглушками, сцены, Web-профиль.

МОДЕЛЬ И РЕЖИМ
Лёгкая модель (Sonnet / GPT-5 mini / Gemini Flash), усилие среднее. /unlazy: в начале GATES.md по воротам ниже (GATES.md в .gitignore) – пунктов 7, файлов много. Решений с компромиссами нет: всё задано контрактами; если контракт не компилируется как написан – остановись и скажи.
Субагенты – только для механики (прогон тестов, логи). Других не запускай.

ЧТО ПРОЧИТАТЬ ПЕРВЫМ (только это)
- CLAUDE.md, docs/TEAM.md («Роли и владение», «Unity: как не ломать сцены»), docs/STATE.md, docs/ROADMAP.md («Контракты» – сигнатуры обязательны дословно).
- Дизайн: docs/DEMO.md («Системы в Unity»).
- Код: в Assets/ пока только шаблон Universal 2D, своего кода нет.
- Вкусы автора: тире «–», без эмодзи; сцена и префаб прежде кода; никакой логики сверх заглушек.

ЧТО СДЕЛАТЬ
1. Папки по владению: Assets/_Project/{Scripts/Core/State, Scripts/Core/Data, Scripts/Core/Systems, Scripts/UI/{Messenger,Menu,Board,Versions,Common}, Data/{Test,Level1}, Prefabs/UI/{Common,Messenger,Board,Versions}, Scenes, Art, Audio/{Music,SFX}, Tests/EditMode/{Core,Board,Review}}. Пустые папки – с .gitkeep, чтобы ушли в git.
2. Сборки (asmdef):
   - Studio.Core (Scripts/Core) – ссылается только на UnityEngine.
   - Studio.UI (Scripts/UI) – Studio.Core, Unity.TextMeshPro, UnityEngine.UI, Unity.InputSystem.
   - Studio.Tests.EditMode (Tests/EditMode) – Studio.Core, NUnit (Test Assemblies), только Editor.
3. Все контракты из ROADMAP.md заглушками, namespace Studio.Core:
   - Scripts/Core/Enums.cs – enum'ы и struct Cost.
   - Scripts/Core/Data – OptionSO, MessageSO, CardSO, ComboSO, QuoteSO, ContentDB: поля полностью, CreateAssetMenu "Studio/<Имя>".
   - Scripts/Core/State – GameState, BoardState (+SlotEntry, Link), DecisionRecord, VersionRecord (+QuoteBlame): поля полностью, [Serializable], только List и массивы (JsonUtility).
   - Scripts/Core/Systems – DecisionResolver, BoardRules, DayClock, ReviewSystem, Difficulty (+DifficultySettings), SaveSystem: статические классы, файл на класс, сигнатуры дословно, тело throw new NotImplementedException("P-N") с номером карточки, которая реализует (Decision – P-2, Board – P-3, DayClock – P-6, Review – P-7, Difficulty и Save – P-9).
4. Тест Tests/EditMode/Core/SmokeTests: Axis имеет 4 значения; GameState и VersionRecord переживают JsonUtility.ToJson/FromJson (пустые). TDD здесь формальный: тест сначала красный (нет типов), потом зелёный.
5. Сцены Scenes/Menu и Scenes/Game, обе в Build Profiles (Menu первая). В каждой: Canvas (Screen Space Overlay), CanvasScaler Scale With Screen Size 1920×1080, match 0.5; EventSystem с Input System UI Input Module. В Game под Canvas – пустые префабы TopBar (Prefabs/UI/Common), MessengerPanel (Prefabs/UI/Messenger), BoardPanel (Prefabs/UI/Board), VersionsPanel и EndingPanel (Prefabs/UI/Versions), каждый растянут на весь экран, Board/Versions/Ending выключены. Сцену собирай через unity-skills REST, не одноразовыми скриптами.
6. Build Profile Web: Compression Brotli, Decompression Fallback вкл., Data Caching вкл. Собрать в Builds/Web, открыть локально: python3 -m http.server 8000 --directory Builds/Web. Вес сборки – в DEVLOG.
7. Раздел Layout в CLAUDE.md: где код, данные, префабы, сцены, тесты; как гонять тесты; как собрать Web.

НЕ ТРОГАТЬ
docs/GDD.md, docs/DEMO.md, docs/ROADMAP.md (кроме галочки M0), .claude/, .agent/, .github/. Никакой логики в системах – только заглушки. Найденное попутно – в docs/BUGS.md (Открыто, с датой).

ТЕХНИКА (грабли)
- Unity Editor открыт с этим проектом. REST unity-skills: http://localhost:8090/?expectProject=centrinvest-profession-game; инструкции – .agent/skills/unity-skills/SKILL.md. Без REST – сцену и префабы делает человек в редакторе по пункту 5, агент пишет только .cs и asmdef.
- После правки .cs и asmdef: asset_refresh, дождаться компиляции по waitUrl, ошибок консоли 0.
- Тесты – Test Runner, EditMode, сборка Studio.Tests.EditMode; число зелёных записать.
- ProjectSettings/ и Packages/ на этой карточке твои: коммитить только осознанные изменения (Build Profile, Input System), шум от открытия редактора – тоже можно, это Р1.
- Если Unity молчит – модальный диалог в редакторе: сказать человеку, не ретраить.

ОТЧЁТНОСТЬ
- docs/DEVLOG.md: запись «P-1 Каркас», статус «сделано» (код компилируется, тесты зелёные), вес Web-сборки.
- docs/STATE.md: P-1 сделан, дальше P-2…P-5 параллельно по ролям. docs/PROMPTS.md: статус P-1. docs/ROADMAP.md: галочка M0.
- Коммит один: «chore: project skeleton, assemblies, contract stubs and web build profile». Сначала покажи сообщение, коммить после «да». Файлы добавляй по именам или папкам, не git add -A.
- Перед стартом: git status, git pull origin dev. В конце: git push -u origin p-1-skeleton, PR в dev (тело: что сделано, как проверить, что не проверено).

ВОРОТА (по числам)
G1 SmokeTests падал до появления типов (показать вывод). G2 Web-сборка открывается в браузере, видно пустую сцену Menu без ошибок в консоли браузера. G3 EditMode-тесты зелёные (число), ошибок консоли редактора 0, все 6 систем бросают NotImplementedException с номером карточки. G4 DEVLOG, STATE, PROMPTS, ROADMAP, Layout в CLAUDE.md обновлены. G5 git status чист.
```

---

## P-2 Состояние игры и решения

**Тип.** код, TDD · **Статус.** сделано (ждёт мержа) · **Кто.** Р1 · Ядро · **Модель.** Sonnet, effort high · **Зависит от.** P-1

```
Задача P-2 из docs/PROMPTS.md: GameState, данные решений и DecisionResolver.

Прочитай CLAUDE.md, docs/TEAM.md (свои папки – только их и правишь), docs/STATE.md, docs/ROADMAP.md («Контракты» – сигнатуры обязательны), docs/DEMO.md («Модель чисел»). Ветка от dev: p-N-<коротко>, в конце PR в dev. TDD: сначала красные тесты, потом код.

Сделай в Assets/_Project/Scripts/Core:
1. ScriptableObject-классы OptionSO, MessageSO, CardSO, ComboSO, QuoteSO, ContentDB – поля строго по «Контрактам», CreateAssetMenu "Studio/<Имя>". CardSO и ComboSO нужны здесь только как данные; правила доски – задача P-3.
2. [Serializable] классы GameState, BoardState (+ SlotEntry, Link), DecisionRecord, VersionRecord (+ QuoteBlame). Только List и массивы, без Dictionary – всё должно сериализоваться JsonUtility. GameState.New(ContentDB, string nickname, DifficultyLevel) создаёт стартовое состояние: day = 1, deadline и budget из ContentDB, axes = {30,30,30,30}, goals = {50,50,50,50}, stage = Idea, бриф в inbox.
3. static class DecisionResolver:
   - CanAfford(state, option): хватает бюджета и дней (day + cost.days <= deadline).
   - Apply(state, message, option, db): списывает бюджет, сдвигает day на cost.days, прибавляет option.deltas к axes, затем Цельность по тегам: +5 за каждый тег варианта, совпавший с тегом любой карточки на доске; –8 за каждую пару (тег варианта, тег карточки), которая есть в db.tagConflicts (порядок в паре не важен). Оси зажать в 0..100. Помечает message как answered, убирает из inbox, пишет DecisionRecord (deltas – итоговые, с учётом тегов) и возвращает его. Если не CanAfford – InvalidOperationException.
4. Тесты Studio.Tests.EditMode/DecisionResolverTests: списание цены; запрет без денег; запрет после дедлайна; совпадение тегов; конфликт тегов в обоих порядках пары; зажим 0..100; запись в журнал; JsonUtility round-trip GameState. Данные в тестах – ScriptableObject.CreateInstance.

Не трогай: UI, сцены, BoardRules (P-3), DayClock (P-6), ReviewSystem (P-7).

Готово, когда: все тесты зелёные, сигнатуры совпадают с «Контрактами» (если пришлось поменять – сначала правка в ROADMAP.md и строка в DECISIONS.md).

В конце: DEVLOG, STATE, статус P-2, сообщение коммита «feat(core): game state, content data and decision resolver» и жди подтверждения.
```

---

## P-3 Правила доски

**Тип.** код, TDD · **Статус.** сделано (ждёт мержа) · **Кто.** Р2 · Правила · **Модель.** Sonnet, effort high · **Зависит от.** P-2

```
Задача P-3 из docs/PROMPTS.md: BoardRules – нитки, комбо и цели осей.

Прочитай CLAUDE.md, docs/TEAM.md (свои папки – только их и правишь), docs/STATE.md, docs/ROADMAP.md («Контракты»), docs/GDD.md (Этап 1), docs/DEMO.md (уровень, этап 1). Ветка от dev: p-N-<коротко>, в конце PR в dev. TDD.

Сделай static class BoardRules в Scripts/Core:
1. Place(state, CardSO card): кладёт карточку в её слот (заменяет прежнюю, связи старой карточки удаляет). Remove(state, Slot).
2. Connect(state, a, b) / Disconnect: связь только между двумя карточками на доске, без дублей и без связи с собой.
3. Thread(a, b, db, board) -> ThreadKind:
   - есть ComboSO на эту пару (в любом порядке) и обе карточки revealed или сложность показывает нитки -> kind комбо (Match или Conflict);
   - есть комбо, но не раскрыто -> Unknown;
   - комбо нет -> Neutral.
   Видимость: передай флаг showThreads параметром (bool), не читай Difficulty напрямую.
4. Reveal(state, cardId): «Исследовать рынок» раскрывает карточку (только запись в revealed; цена – на стороне UI через DecisionResolver не нужна, просто вычти budget в отдельном методе Research(state, cardId, Cost)).
5. Goals(board, db) -> int[4]: старт {50,50,50,50} + goalDeltas всех карточек на доске + deltas всех связанных комбо типа Match (за Conflict: –10 к Цельности цели нет, вместо этого конфликт снижает axes.Coherence на 10 при фиксации доски). Зажать 0..100.
6. Commit(state, db): фиксирует доску в конце этапа 1 – пишет goals, применяет эффекты комбо к axes (Match: +deltas, Conflict: Coherence –10), переводит stage в Production.
7. Тесты BoardRulesTests: замена карточки в слоте и удаление её связей; запрет дублей связей; Thread для Match/Conflict/Unknown/Neutral и обоих порядков пары; Goals с комбо; Commit меняет stage и оси.

Не трогай: UI, DecisionResolver (кроме вызова), DayClock, ReviewSystem.

Готово, когда: тесты зелёные, контракт в ROADMAP.md совпадает с кодом.

В конце: DEVLOG, STATE, статус P-3, коммит «feat(core): board rules, threads, combos and goals» – жди подтверждения.
```

---

## P-4 UI мессенджера (серый)

**Тип.** UI · **Статус.** сделано (ждёт PR и проверки командой) · **Кто.** Р3 · Чат · **Модель.** Sonnet, effort medium · **Зависит от.** P-1 (P-2 можно заглушками)

```
Задача P-4 из docs/PROMPTS.md: серый интерфейс мессенджера в сцене Game.

Прочитай CLAUDE.md, docs/TEAM.md (свои папки – только их и правишь), docs/STATE.md, docs/ROADMAP.md («Контракты»), docs/GDD.md («Оболочка: мессенджер», этап 2). Макет: docs/mockup/2-messenger.png (HTML-исходник рядом, открывается в браузере). Ветка от dev: p-N-<коротко>, в конце PR в dev.

Всё собирается в сцене и префабах (Assets/_Project/Prefabs/UI), код только связывает. uGUI + TextMeshPro, серые плашки вместо арта.

Сделай:
1. TopBar.prefab: название игры, 3 этапа (активный подсвечен), «До дедлайна: N дней · день X из Y», полоска дедлайна, бюджет, кнопка «Закончить день». Компонент TopBarView.Refresh(GameState).
2. ChatList: список чатов по Sender (имя, последняя строка, время, счётчик непрочитанных). Вкладки Чаты / Доска / Версии переключают правую часть (Доска и Версии пока пустые панели).
3. ChatView: лента сообщений выбранного чата (пузыри), вложенные картинки сеткой (для 3 вариантов героя – выбираемые карточки с тегами), снизу панель вариантов ответа: текст + цена («1 дн. · 20 000 ₽»). Недоступный вариант (не хватает бюджета) – серый, с подсказкой почему.
4. Правая панель-заглушка под ИИ-помощника и плейтест (только разметка, кнопки неактивны).
5. MessengerController (Studio.UI): держит GameState и ContentDB, по клику на вариант зовёт DecisionResolver.Apply и обновляет все View. Если P-2 ещё не в dev – временная заглушка с теми же сигнатурами в Scripts/UI/Stubs, удалить после слияния.
6. Тестовый ContentDB в Data/Test с 3 сообщениями от разных отправителей, одно с 3 картинками-заглушками.

Не трогай: Scripts/Core (кроме чтения), сцену Menu, docs/GDD.md.

Готово, когда: в Play из сцены Game видно бриф, можно выбрать чат, ответить, бюджет и дни в TopBar меняются, ответ пропадает из входящих. Скриншот: python3 tools/progress.py add messenger "grey messenger v1" <png>.

В конце: DEVLOG (статус «проверено агентом» только при скриншоте из Play), STATE, статус P-4, коммит «feat(ui): grey messenger, chat list and top bar» – жди подтверждения.
```

---

## P-5 UI доски (серый)

**Тип.** UI · **Статус.** сделано (ждёт мержа) · **Кто.** Р4 · Доска и версии · **Модель.** Sonnet, effort high · **Зависит от.** P-3 (можно заглушками), P-4

```
Задача P-5 из docs/PROMPTS.md: пиновая доска – вкладка «Доска» в сцене Game.

Прочитай CLAUDE.md, docs/TEAM.md (свои папки – только их и правишь), docs/STATE.md, docs/ROADMAP.md («Контракты», «Риски» про нитки), docs/GDD.md (Этап 1). Макет: docs/mockup/1-board.png. Ветка от dev: p-N-<коротко>, в конце PR в dev.

Сделай (префабы в Prefabs/UI/Board, код в Scripts/UI/Board):
1. BoardPanel: область доски с 6 слотами (Slot enum), подписи слотов, пустой слот – пунктирная рамка «перетащи карточку».
2. CardView.prefab: тип слота, название, заметка (популярность, если revealed). Перетаскивание мышью и пальцем (IBeginDrag/IDrag/IEndDrag) из руки в слот своего типа; в чужой слот – возврат на место. Повторная кладка заменяет карточку (BoardRules.Place).
3. Нитки: клик по булавке карточки A, затем по B – BoardRules.Connect. Нитка – растянутый и повёрнутый Image между центрами булавок (без LineRenderer). Вид по ThreadKind: Match – сплошная зелёная, Conflict – двойная красная, Unknown – пунктир жёлтый, Neutral – тонкая серая. Цвет всегда дублируется формой. Клик по нитке – Disconnect.
4. Плашка комбо над ниткой: «Комбо: <title>» или «Конфликт: <title>», только для раскрытых.
5. Правая панель: бриф, рука карточек (сетка), кнопки «Исследовать рынок · цена» (выбор карточки, затем BoardRules.Research) и «Спросить команду» (пока неактивна), цели по осям – полоски из BoardRules.Goals, легенда ниток.
6. Кнопка «Отправить питч» в TopBar на этапе Idea: BoardRules.Commit и переход на вкладку Чаты.

Не трогай: Scripts/Core (только вызовы), MessengerController кроме подписки на смену вкладки.

Готово, когда: в Play можно разложить 6 карточек, связать, увидеть все 4 вида нитки на тестовых комбо, цели меняются, питч переводит в этап Production. Скриншот: tools/progress.py add board "grey board v1".

В конце: DEVLOG, STATE, статус P-5, коммит «feat(ui): pin board with drag, threads and goals» – жди подтверждения.
```

---

## P-6 Цикл дня и этапы

**Тип.** код, TDD · **Статус.** сделано (ждёт мержа) · **Кто.** Р1 · Ядро · **Модель.** Sonnet, effort high · **Зависит от.** P-2, P-3

```
Задача P-6 из docs/PROMPTS.md: DayClock – конец дня, отложенные сообщения, переходы этапов.

Прочитай CLAUDE.md, docs/TEAM.md (свои папки – только их и правишь), docs/STATE.md, docs/ROADMAP.md («Контракты»), docs/GDD.md (этап 2), docs/DEMO.md (уровень: дни этапов). Ветка от dev: p-N-<коротко>, в конце PR в dev. TDD.

Сделай:
1. В GameState: List<Pending {messageId, day}> pending – отложенные ответы.
2. DecisionResolver.Apply: если у варианта есть followUp – добавить Pending на day + followUpDelayDays (правка в P-2 коде, тест на это).
3. static class DayClock:
   - EndDay(state, db) -> List<MessageSO>: day += 1; в inbox добавляются сообщения из db.messages с message.day == state.day и message.stage == state.stage, и все pending с наступившим днём. Неотвеченные сообщения прошлых дней остаются в inbox. Возвращает новые.
   - Переходы: Idea -> Production только через BoardRules.Commit; Production -> Release, когда day >= первый день сообщений этапа Release в db (или по db.releaseStartDay – добавь поле в ContentDB); Release -> Done, когда версия одобрена (P-7) или day > deadline.
   - IsOverDeadline(state).
4. Тесты DayClockTests: доставка сообщений дня; pending приходит в нужный день; неотвеченное не теряется; переход Production -> Release; дедлайн -> Done.
5. Подключи кнопку «Закончить день» в MessengerController к DayClock.EndDay (только вызов и Refresh, UI-логику не переписывай).

Не трогай: BoardRules кроме вызова, ReviewSystem, префабы UI.

Готово, когда: тесты зелёные; в Play на тестовом контенте за 3 «Закончить день» приходят сообщения следующих дней и follow-up ответ.

В конце: DEVLOG, STATE, статус P-6, коммит «feat(core): day clock, follow-ups and stage transitions» – жди подтверждения.
```

---

## P-7 Ревью версий и правка решений

**Тип.** код, TDD · **Статус.** сделано (ждёт мержа) · **Кто.** Р2 · Правила · **Модель.** Opus, effort high (ядро уникальности, нужна аккуратность) · **Зависит от.** P-6

```
Задача P-7 из docs/PROMPTS.md: ReviewSystem – оценка версии, цитаты, поиск виновного решения, переоткрытие.

Прочитай CLAUDE.md, docs/TEAM.md (свои папки – только их и правишь), docs/STATE.md, docs/ROADMAP.md («Контракты»), docs/GDD.md (этап 3), docs/DEMO.md («Модель чисел»). Ветка от dev: p-N-<коротко>, в конце PR в dev. TDD.

Сделай static class ReviewSystem:
1. Review(state, db) -> VersionRecord: number = versions.Count + 1 (v0.N); снимок axes; одобрено, если для всех осей axes[i] >= goals[i] - tolerance (tolerance из DifficultySettings, пока параметр). Цитаты: для каждой оси ниже цели – 1–2 QuoteSO этой оси (детерминированно: по порядку в db, без повторов между версиями, пока есть новые), плюс одна цитата-похвала оси выше цели (QuoteSO с полем praise = true – добавь поле). Первая версия на сложности Designer и Lead одобряется только если все оси выше цели (а не с допуском) – так v0.1 почти всегда «доработать». Цена сборки: 1 день.
2. Blame(state, version, quoteId) -> messageId: решение из журнала с самым большим минусом по оси цитаты (при равенстве – более позднее). Решения этапа Idea – это фиксация доски: messageId = "board".
3. CheckBlame(state, version, quoteId, guessedMessageId) -> bool: угадал – запись в version.blamed; не угадал – day += 1 (штраф), возвращает false.
4. ReopenCost(record): цена исходного варианта × 2 для Idea, × 1 для Production; минимум 1 день.
5. Reopen(state, record, newOption, db): откатывает record.deltas из axes, списывает ReopenCost, применяет newOption через DecisionResolver (новая запись в журнале, старая помечается replaced = true – добавь поле). Для "board" – вернуть stage в Idea с текущей доской и вкладкой доски; после повторного Commit – обратно в Release.
6. Тесты ReviewSystemTests: одобрение с допуском; v0.1 не одобряется на Designer; цитаты только по слабым осям + одна похвала; Blame выбирает худшее решение; штраф за неверную догадку; Reopen откатывает и применяет новое; цена ×2 для доски.

Не трогай: UI, BoardRules кроме вызова Commit.

Готово, когда: тесты зелёные; контракт в ROADMAP.md обновлён под новые поля (praise, replaced, ReopenCost).

В конце: DEVLOG, STATE, статус P-7, коммит «feat(core): version review, blame and reopen» – жди подтверждения.
```

---

## P-8 UI версий и финал (серый)

**Тип.** UI · **Статус.** сделано (серый, ждёт проверки командой) · **Кто.** Р4 · Доска и версии · **Модель.** Sonnet, effort medium · **Зависит от.** P-7 (можно заглушками)

```
Задача P-8 из docs/PROMPTS.md: вкладка «Версии» и финальный экран.

Прочитай CLAUDE.md, docs/TEAM.md (свои папки – только их и правишь), docs/STATE.md, docs/ROADMAP.md, docs/GDD.md (этап 3, финал). Макеты: docs/mockup/3-versions.png и docs/mockup/4-ending.png. Ветка от dev: p-N-<коротко>, в конце PR в dev.

ВАЖНО: Интерфейс собирается в сцене и префабах (Assets/_Project/Prefabs/UI, сцены Scenes/), а не создаётся кодом через new GameObject / AddComponent. Код только связывает: публичные ссылки на Button, TMP_Text, Transform настраиваются в инспекторе, скрипт слушает события и обновляет текст. Исключения (динамические списки) – Instantiate префаба-строки, а не сборка из примитивов. Так художник правит вид без кода. Сцену и префабы собирай через unity-skills REST, ссылки между объектами ставь component_set_property.

Сделай (Prefabs/UI/Versions, Scripts/UI/Versions):
1. Слева история версий: v0.N, день, статус (доработать / одобрено / черновик), что изменилось (решения с replaced после прошлой версии).
2. Центр: фидбек начальника, 4 карточки осей «значение / цель» (цвет + текст «ниже цели» для доступности), список цитат со статусом: не разобрана / выбрана / найдена причина / похвала.
3. Справа журнал решений (без replaced): день, этап, текст выбранного варианта. Выбор цитаты, затем решения – ReviewSystem.CheckBlame; ошибка – тост «Не то. –1 день». Найдено – блок «Переоткрыть решение · цена», по кнопке – окно выбора нового варианта из того же MessageSO (без текущего) – ReviewSystem.Reopen. Для "board" – переход на вкладку Доска.
4. Кнопка в TopBar на этапе Release: «Собрать v0.N и отправить» – ReviewSystem.Review, ответ начальника приходит сообщением в его чат.
5. Финальный экран (панель в сцене Game или отдельная сцена End): название игры игрока (из доски: жанр + фишка), оценка 0–100 (среднее осей), одна из 4 концовок (<40 провал, <60 середнячок, <80 хит, иначе игра года), 4 отзыва, разбор: 2 лучших и 2 худших решения из журнала с осью, текст о профессии, кнопки «Новый проект» (в меню) и «Узнать о профессии больше» (ссылка – заглушка).

Не трогай: Scripts/Core, BoardPanel, ChatView кроме подписки на события.

Готово, когда: в Play на тестовом контенте можно отправить v0.1, получить «доработать», найти причину цитаты, переоткрыть решение, отправить v0.2 и увидеть финал. Скриншоты: tools/progress.py add versions "grey versions v1" и add ending "grey ending v1".

В конце: DEVLOG, STATE, статус P-8, коммит «feat(ui): versions tab, feedback blame flow and ending» – жди подтверждения.
```

---

## P-8b Версии и финал в префабах

**Тип.** UI, переделка · **Статус.** сделано (ждёт PR и проверки командой) · **Кто.** Р4 · Доска и версии · **Модель.** Sonnet, effort medium · **Зависит от.** P-8

```
Задача P-8b из docs/PROMPTS.md: перенести вкладку «Версии» и финальный экран из кода в префабы.

Прочитай CLAUDE.md, docs/TEAM.md (свои папки – только их и правишь), docs/STATE.md, docs/DECISIONS.md («UI версий и финала»). Макеты: docs/mockup/3-versions.png и docs/mockup/4-ending.png. Ветка от dev: p-N-<коротко>, в конце PR в dev.

Сейчас VersionsPanelView и EndingPanelView строят интерфейс кодом (Scripts/UI/Versions/UiKit.cs). Нужно, чтобы тот же интерфейс лежал в префабах и сцене.

Сделай:
1. Prefabs/UI/Versions: VersionsPanel (три колонки: история версий, фидбек с 4 карточками осей и цитатами, журнал решений), строки-префабы VersionRow, AxisCard, QuoteRow, JournalRow, тост, окно переоткрытия с строкой-вариантом, кнопка «Собрать vN и отправить», EndingPanel (название игры, оценка, концовка, 4 отзыва, разбор, кнопки). Внешний вид – по макету, цвета и шрифты как в docs/TEAM.md.
2. VersionsPanelView и EndingPanelView: убрать создание из кода (BuildSkeleton, Column, UiKit.Label и т.п.), оставить ссылки из инспектора и обновление данных. Списки строк – Instantiate префаба-строки, как в ChatListView. Ending.cs и логику не менять.
3. Баг с оверлеем из P-9: Content скрывается по умолчанию в самом префабе, не по флагу shown.
4. Удалить UiKit.cs, если им больше никто не пользуется (меню P-9 уже на префабах).

Не трогай: Scripts/Core, BoardPanel, Messenger.

Готово, когда: сцена Game открывается в редакторе, панели Версии и Финал видны в Scene view без Play; тесты EndingTests зелёные; в Play полный путь P-8 проходит как раньше (v0.1 доработать, найти причину, переоткрыть, v0.2, финал). Скриншоты: progress.py add versions / ending с теми же камерами.

В конце: DEVLOG, STATE, статус P-8b, DECISIONS, коммит «refactor(ui): versions and ending as prefabs» – жди подтверждения.
```

---

## P-9 Меню, сохранение, сложность

**Тип.** код + UI · **Статус.** сделано (ждёт мержа) · **Кто.** Р1 · Ядро (логика) + Р3 · Чат (меню) · **Модель.** Sonnet, effort medium · **Зависит от.** P-6

```
Задача P-9 из docs/PROMPTS.md: меню, автосохранение, уровни сложности (задачи со звёздочкой кейса).

Прочитай CLAUDE.md, docs/TEAM.md (свои папки – только их и правишь), docs/STATE.md, docs/ROADMAP.md («Контракты»), docs/GDD.md («Сложность», «Сохранение»), docs/CASE.md. Макет: docs/mockup/0-main.png. Ветка от dev: p-N-<коротко>, в конце PR в dev. TDD для Difficulty и SaveSystem.

Сделай:
1. Difficulty.For(level) -> DifficultySettings по таблице GDD: Intern {budgetMul 1.3, daysBonus 3, aiErrorChance 0.05, showThreads true, reviewTolerance 10}, Designer {1.0, 0, 0.2, false, 5}, Lead {0.8, -2, 0.4, false, 0}. GameState.New применяет budgetMul и daysBonus. Подставь showThreads в вызовы BoardRules.Thread и reviewTolerance в ReviewSystem.Review.
2. SaveSystem: JsonUtility -> PlayerPrefs ключ "save_v1" (в WebGL это IndexedDB, переживает перезагрузку). Save после каждого EndDay, после Review и после Commit доски. Load возвращает null при битом JSON (лог, без исключения). HasSave, Delete. Поле saveVersion = 1 в GameState.
3. Сцена Menu собрана в сцене и префабах, не кодом. Корневой префаб Prefabs/UI/Menu/MenuScreen.prefab (по макету docs/mockup/0-main.png, слева колонка, справа плашка под арт): заголовок, подзаголовок, поле никнейма TMP_InputField (до 16 символов, пустое – «Стажёр», плейсхолдер «Стажёр»), 3 карточки сложности (префаб LevelCard: название, подсказка, рамка выбранной), кнопка «Выйти на работу», кнопка «Продолжить · день N» (видна только если HasSave), окно подтверждения «Начать заново? Прошлое сохранение будет удалено» с двумя кнопками. MenuController (Scripts/UI/Menu) только держит ссылки из инспектора: слушает кнопки, читает SaveSystem, пишет в Session, грузит сцену Game. Новая игра: старое сохранение удаляется после подтверждения, если оно есть. Общие кнопки и плашки – в Prefabs/UI/Common.
4. Никнейм подставляется в тексты сообщений вместо {name}.
5. Тесты: значения Difficulty; GameState.New с множителями; Save/Load round-trip; битый JSON -> null.

Не трогай: BoardPanel, ChatView, Versions UI. Не создавай элементы меню кодом (UiKit, new GameObject).

Готово, когда: тесты зелёные; сцена Menu открывается в редакторе и видна без Play; в Play: начать игру, пройти 2 дня, перезапустить Play, «Продолжить» возвращает на тот же день с теми же решениями.

В конце: DEVLOG, STATE, статус P-9, CASE.md – отметить сохранение и сложность, коммит «feat: menu, autosave and difficulty levels» – жди подтверждения.
```

---

## P-10 Контент уровня «Первый релиз»

**Тип.** контент · **Статус.** сделано (ждёт PR и проверки командой) · **Кто.** Р2 · Правила + геймдизайнер команды · **Модель.** Opus, effort medium (тексты и баланс связей) · **Зависит от.** серый проход (P-1…P-9)

```
Задача P-10 из docs/PROMPTS.md: весь контент уровня «Первый релиз» в ScriptableObject.

Прочитай CLAUDE.md, docs/TEAM.md (свои папки – только их и правишь), docs/STATE.md, docs/GDD.md, docs/DEMO.md («Уровень»), docs/ROADMAP.md («Контракты»). Ветка от dev: p-N-<коротко>, в конце PR в dev.

Сделай в Assets/_Project/Data/Level1 (ContentDB_Level1 + ассеты):
1. ~20 CardSO по 6 слотам из DEMO.md, у каждой 2–3 тега, popularity, productionCost (дорогие: Кооператив, Петля времени), goalDeltas («Дети 6+» – Понятность +20 и т.п.).
2. ~10 ComboSO: 6 Match с именами («День сурка», «Вверх ногами», «Котокафе»...), 4 Conflict. tagConflicts: 8–10 пар (мрачный/мультяшный, сложный/казуальный, дорогой/дешёвый...).
3. Бриф и ~25 MessageSO на дни 4–12 по DEMO.md: обязательно 3 варианта героя от Леры (теги мультяшный/мрачный/пиксельный), фича программиста за 3 дня, урезание бюджета от продюсера, 3 идеи Гоши (одна – скрытое комбо через теги), баг накануне билда. 2–3 варианта ответа, ответ до 5 слов, сообщение до 2 строк. 4–6 follow-up ответов.
4. ~15 QuoteSO: по 3 на ось + 3 похвалы. Цитаты не называют причину прямо.
5. Сообщения начальника для этапа 3: ответ на каждую версию (доработать / одобрено) и предупреждение «первая версия почти всегда уходит на доработку».
6. Таблица docs/balance/level1.md: все варианты с ценой и дельтами, сумма лучшего и худшего пути по осям. Проверь, что лучший путь укладывается в бюджет и дни на Designer, а наивный путь (всегда первый вариант) даёт хотя бы 2 оси ниже цели.

Тексты: дефис «–», без эмодзи, для подростка, с юмором. Персонажи: Борис Петрович (начальник), Лера (художница), Костя (программист), Марина (продюсер), Гоша (племянник начальника), Нейрон (ИИ). Обращение к игроку – {name}.

Не трогай: код, префабы.

Готово, когда: полное прохождение на Designer от брифа до финала проходит без пустых дней и тупиков (проверь в Play), таблица баланса есть.

В конце: DEVLOG, STATE, статус P-10, коммит «content: level 1 cards, combos, messages and quotes» – жди подтверждения.
```

---

## P-11 WebGL-сборка и выкладка

**Тип.** сборка · **Статус.** не сделано · **Кто.** Р1 · Ядро · **Модель.** Sonnet, effort medium · **Зависит от.** P-10

```
Задача P-11 из docs/PROMPTS.md: рабочая Web-сборка в открытом доступе.

Прочитай CLAUDE.md, docs/TEAM.md (свои папки – только их и правишь), docs/STATE.md, docs/CASE.md. Ветка от dev: p-N-<коротко>, в конце PR в dev.

Сделай:
1. Web-сборка Release (Brotli, Decompression Fallback), вес записать в DEVLOG; цель – до 30 МБ. Если больше – найди крупнейшие ассеты (Editor.log Build Report) и сожми текстуры.
2. Выкладка на GitHub Pages: workflow .github/workflows/pages.yml, который публикует папку docs-build/ или ветку gh-pages (выбери проще; сборку Unity в CI не делаем – публикуем собранную локально). Альтернатива – itch.io, если Pages не отдаёт .br с нужными заголовками: тогда Decompression Fallback решает.
3. Ссылки на игру и репозиторий в README.md, два QR-кода PNG в docs/presentation/ (python qrcode или любой офлайн-генератор).
4. Смоук-тест: открыть ссылку в Chrome и Safari, на телефоне – пройти бриф и 1 день. Записать результат.

Не трогай: код игры.

Готово, когда: игра по ссылке открывается без регистрации и проходится, QR-коды ведут куда надо.

В конце: DEVLOG, STATE, CASE.md (браузерная версия), коммит «build: web release and github pages deploy» – жди подтверждения.
```

---

## P-12 ИИ-помощник и аналитика (приоритет B)

**Тип.** код + UI · **Статус.** не сделано · **Кто.** Р2 · Правила (логика) + Р3 · Чат (панель) · **Модель.** Sonnet, effort medium · **Зависит от.** P-6, P-9

```
Задача P-12 из docs/PROMPTS.md: ИИ-помощник «Нейрон» и кнопка «Плейтест».

Прочитай CLAUDE.md, docs/TEAM.md (свои папки – только их и правишь), docs/STATE.md, docs/GDD.md (ИИ-помощник, аналитика), docs/ROADMAP.md. Макет: docs/mockup/2-messenger.png, правая панель. Ветка от dev: p-N-<коротко>, в конце PR в dev. TDD для логики.

Сделай:
1. static class Advisor (Scripts/Core): Advise(state, message, db, settings, System.Random rng) -> {optionId, deltas int[4], confidence}. Считает настоящий эффект каждого варианта (как DecisionResolver, но без применения) и советует лучший по сумме «недобора до цели». С вероятностью aiErrorChance советует случайный другой вариант с той же уверенной формулировкой. Цена запроса 15 000 (поле в ContentDB).
2. Playtest(state) -> грубые оценки осей: «низко» (< цель – 15), «норм», «высоко» (> цель + 10). Цена: 1 день, 40 000.
3. UI (префаб правой панели в Prefabs/UI/Messenger, не кодом): правая панель мессенджера оживает – «Спросить Нейрона» по текущему сообщению (ответ в чате Нейрона и в панели, с припиской «Нейрон иногда ошибается»), «Запустить плейтест».
4. Тесты: при aiErrorChance 0 совет совпадает с лучшим вариантом; при 1 – никогда не совпадает (если вариантов > 1); Playtest пороги. Random передаётся параметром – тесты детерминированные.

Не трогай: ReviewSystem, BoardRules.

Готово, когда: тесты зелёные, в Play Нейрон советует, на Lead заметно ошибается.

В конце: DEVLOG, STATE, статус P-12, коммит «feat: ai advisor and playtest analytics» – жди подтверждения.
```

---

## P-13 Экраны и навигация

**Тип.** UI, переделка · **Статус.** не сделано · **Кто.** Р1 · Ядро (сцена `Game`, `ScreenRouter`) + Р3 · Чат (панель навигации в `Prefabs/UI/Common`) · **Модель.** Sonnet, effort high · **Зависит от.** P-8b

```
Задача P-13 из docs/PROMPTS.md: разнести игру на отдельные экраны с понятным переключением.

Прочитай CLAUDE.md, docs/TEAM.md (свои папки – только их и правишь), docs/STATE.md, docs/GDD.md («Оболочка: мессенджер»), docs/DEMO.md. Макеты: docs/mockup/2-messenger.png, 1-board.png, 3-versions.png, 4-ending.png. Ветка от dev: p-N-<коротко>, в конце PR в dev.

Проблема. Сейчас переключатель «Чаты / Доска / Версии» – три мелкие кнопки внутри левой колонки мессенджера, выглядят как часть списка чатов, активная вкладка почти не видна. Переключение живёт в MessengerController (ShowTab, поля chatsTab/boardTab/versionsTab), хотя он должен отвечать только за чаты. В сцене Game панели лежат стопкой друг на друге, в Scene view их не разобрать и не поправить.

Сделай:
1. Экран = корневой префаб-панель на весь Canvas под TopBar: ChatsScreen (список чатов + лента + правая панель), BoardScreen (BoardPanel), VersionsScreen (VersionsPanel), EndingScreen (EndingPanel, без вкладки, показывается по концовке). Левая колонка мессенджера остаётся только списком чатов, переключателя там нет.
2. NavBar.prefab (Prefabs/UI/Common): отдельная заметная панель экранов под TopBar или сверху слева – кнопки «Чаты», «Доска», «Версии» с иконкой, активная подсвечена цветом и подчёркнута (цвет + форма, не один цвет), на «Чаты» бейдж с числом непрочитанных, на «Версии» бейдж при новом фидбеке. Недоступный экран (Версии до этапа Release, Доска после питча – по GDD) серый, с подсказкой «Откроется на этапе …». Горячие клавиши 1, 2, 3.
3. ScreenRouter (Studio.UI, Scripts/UI/Common): enum Screen { Chats, Board, Versions, Ending }, Show(Screen), событие Changed, Current, CanShow(Screen, GameState). Один владелец включения/выключения экранов; остальные зовут router.Show (питч на доске -> Chats, сборка версии -> Versions, концовка -> Ending). Убрать из MessengerController ShowTab и поля вкладок; автосохранение перенести на Changed.
4. Сцена Game: каждый экран – отдельный корневой объект с префабом, по умолчанию все выключены, включается один; в Scene view экран открывается по одному (тумблером объекта), без наложения. Порядок и имена объектов – по списку выше.
5. При смене экрана: фокус не остаётся на старом (EventSystem.SetSelectedGameObject(null)), прокрутка списка чатов сохраняется, перетаскиваемая карточка отменяется.

Не трогай: Scripts/Core, логику BoardRules и ReviewSystem, сцену Menu, внутренности BoardPanel и VersionsPanel.

Готово, когда: в Play экран переключается кнопками NavBar и клавишами 1-3, активный виден сразу, недоступный объяснён; на этапе Idea после питча игра сама открывает Чаты; сборка версии открывает Версии; в Scene view каждый экран можно включить отдельно и править. Тесты на CanShow (EditMode, по этапам) зелёные. Скриншоты: progress.py add navigation "nav bar v1" с теми же камерами, по одному на экран.

В конце: DEVLOG, STATE, статус P-13, DECISIONS (кто владеет переключением), коммит «refactor(ui): screens and navigation bar» – жди подтверждения.
```


## P-15 Главное меню как в PC-игре

**Тип.** UI, переделка · **Статус.** не сделано · **Кто.** Р3 · Чат (сцена `Menu`, `Prefabs/UI/Menu/`, `Scripts/UI/Menu/`) · **Модель.** Sonnet, effort high · **Зависит от.** P-14

```
Задача P-15 из docs/PROMPTS.md: главное меню должно выглядеть как меню игры, а не как сайт.

Прочитай CLAUDE.md, docs/TEAM.md, docs/STATE.md, docs/DECISIONS.md («Новый UI»), docs/mockup/ и каркасы автора (меню: арт слева, шапка, кнопки). Ветка от dev: p-15-main-menu.

Проблема. Сейчас меню – шапка с текстом и три плитки, как лендинг: нет логотипа, нет фона-сцены, нет привычного списка пунктов.

Сделай:
1. Фон-сцена на весь экран (вечерний офис студии, пока заглушка со слоями для параллакса), логотип игры вместо текстового заголовка.
2. Вертикальный список пунктов по привычке PC-игр: «Продолжить» (если есть сохранение), «Новая игра», «Настройки», «Выйти» (в WebGL скрыть). Выбор мышью, клавиатурой (стрелки, Enter, Esc) и тачем; фокус виден.
3. «Новая игра» открывает выбор уровня (обучение, Стажёр, Дизайнер, Ведущий), потом окно ника. Уровни остаются теми же, что в `Difficulty`.
4. Короткая анимация появления пунктов и звук наведения/клика (заглушки по `docs/AUDIO.md`).
5. Вид и положение – только в префабах, код держит ссылки.

Не трогай: Scripts/Core, сцену Game, внутренности Board и Versions.

Готово, когда: меню управляется мышью и клавиатурой, выглядит как меню игры, все старые сценарии (новая игра, продолжить, перезапись сохранения) работают; тесты зелёные; скриншот: progress.py add menu-v3.

В конце: DEVLOG, STATE, статус P-15, DECISIONS, коммит «feat(ui): game-style main menu» – жди подтверждения.
```

## P-16 Пауза и настройки

**Тип.** UI + логика · **Статус.** не сделано · **Кто.** Р3 · Чат (окна и кнопка паузы) + Р1 · Ядро (хранение настроек) · **Модель.** Sonnet, effort high · **Зависит от.** P-15

```
Задача P-16 из docs/PROMPTS.md: пауза в игре и экран настроек.

Прочитай CLAUDE.md, docs/TEAM.md, docs/STATE.md, docs/AUDIO.md. Ветка от dev: p-16-pause-settings.

Сделай:
1. Окно паузы (префаб `PauseMenu`): Esc или кнопка в верхней панели открывает; пункты «Продолжить», «Настройки», «В главное меню» (с сохранением и подтверждением). Игра внутри дня не должна идти, пока пауза открыта (когда появится таймер дня – ставить его на паузу).
2. Окно настроек (префаб `SettingsPanel`, одно для меню и паузы): громкость музыки и звука (ползунки, mute), размер текста (обычный/крупный), полноэкранный режим там, где он есть. Значения живут в `Settings` (Studio.Core, чистые данные + `SettingsStore` на `PlayerPrefs`, ключ `settings_v1`), с тестами на сохранение и битый JSON.
3. Настройки применяются сразу и переживают перезапуск. Аудио-шины подключить, когда будет звук; пока ползунки пишут значение.
4. Окна закрываются Esc, фокус и клавиатура работают, фон под окном затемнён.

Не трогай: BoardRules, ReviewSystem, содержимое экранов Доска/Чаты/Версии.

Готово, когда: пауза и настройки открываются из меню и из игры, значения сохраняются после перезапуска; тесты на `SettingsStore` зелёные; скриншоты: progress.py add pause-settings.

В конце: DEVLOG, STATE, статус P-16, DECISIONS, коммит «feat: pause menu and settings» – жди подтверждения.
```

<a id="gameplay-rework"></a>

<a id="mvp-tutorial"></a>
## MVP: уровень 0 «Обучение» (P-18…P-29)

Актуальные карточки. Требования – `docs/MVP_SPEC.md`, правила и числа – `docs/MVP_RULES.md`, контракты – `docs/ROADMAP.md` («Контракты MVP»), владение – `docs/TEAM.md`. Прежние P-18…P-26 заменены 2026-10-24 и лежат в архиве в конце файла; не исполнять их.

Структура карточки: вход → результат → действия → границы → проверка → отчёт. Шаблон `.agent/skills/task-prompt/SKILL.md`, на который ссылается `CLAUDE.md`, в репозитории отсутствует; рабочая структура – эта и P-1.

Общие правила для P-19…P-29 (повторены внутри каждого промпта ссылкой):

- Модель и уровень размышлений выбрать в клиенте до отправки; название в тексте ничего не переключает. Рекомендации – в заголовке карточки.
- Начать с `git status`. Чужие незакоммиченные изменения не сбрасывать, не прятать, не добавлять в свой коммит. Ветка `p-N-short-name` от актуального `dev`, PR в `dev`. Если нужная карточка-зависимость ещё не в `dev`, сказать об этом и не выдавать старый `dev` за актуальную игру.
- Правила игры – только в Core (`Studio.Core`). UI читает `LevelState` и вызывает операции Core, свою логику правил не заводит. UI собирается в сценах и префабах, код связывает ссылки; строки списков – `Instantiate` префабов.
- Игровая логика – TDD: тест сначала красный, потом зелёный. Тесты – `test_run_by_name` с `Studio.Tests`, не весь пакет.
- Числа – из `LevelTuning` и данных уровня, не константы в коде. Не возвращать сложности, исследования рынка, «Спросить команду», четыре оси.
- Владение – `TEAM.md`. Чужой файл не правим: нужное изменение – конкретный handoff владельцу в отчёте (что, где, критерий). Сообщения коллегам агент не отправляет.
- После задачи: `DEVLOG.md`, `STATE.md`, статус карточки; `DECISIONS.md` при новом решении; `progress.py` для видимых изменений. Статусы не сильнее доказательства.
- Один коммит с документами. Показать сообщение (английский, Conventional Commits) и состав, коммитить после подтверждения автора. Без AI attribution.

## P-18 Правила MVP, контракты и карточки

**Тип.** дизайн + документы · **Статус.** сделано (документы, ждёт подтверждения автора) · **Кто.** Р1 + документы · **Модель.** gpt-6-astra · **Размышления.** xhigh · **Зависит от.** –

Результат: `docs/MVP_RULES.md` (противоречия, правила и числа с пометкой допущений, состояния и переходы, три бумажных маршрута, вопросы автору), контракты MVP в `ROADMAP.md`, карта владения в `TEAM.md`, карточки P-19…P-29, согласованные GDD, DEMO, GAMEPLAY_REWORK. Исходный промпт задачи передан автором в чате; прежний текст P-18 – в архиве.

## P-19 Типы и состояние MVP, сохранение v2

**Тип.** логика, TDD · **Статус.** не начато · **Кто.** Р1 · **Модель.** gpt-6.1-sol · **Размышления.** high · **Зависит от.** P-18

```text
Выполни только P-19 из docs/PROMPTS.md (раздел «MVP: уровень 0»). Роль Р1 Ядро.
Ветка p-19-mvp-state от актуального dev.

ВХОД
CLAUDE.md, docs/TEAM.md, docs/STATE.md, docs/MVP_SPEC.md, docs/MVP_RULES.md
(разделы 3 и 4), docs/ROADMAP.md («Контракты MVP» – сигнатуры обязательны
дословно; «Существующий API прототипа» – не трогать), общие правила раздела MVP.
Код: Scripts/Core/Enums.cs, State/GameState.cs, Systems/SaveSystem.cs,
Tests/EditMode/Core/SaveSystemTests.cs – как образец стиля.

РЕЗУЛЬТАТ
Компилируются все типы MVP из ROADMAP: enum'ы, LevelSO с вложенными типами и
LevelTuning, LevelState и вложенные состояния, NightReport. Работают
LevelState.New и SaveSystem.SaveLevel/LoadLevel/HasLevelSave/DeleteLevel.
Остальные системы MVP (ConceptRules, Staffing, WorkSim, Review) – статические
классы с сигнатурами из ROADMAP и телом
throw new NotImplementedException("P-N") с номером карточки, которая их
реализует (ConceptRules – P-20, WorkSim – P-21, Staffing и Review – P-22).
UI-карточки после этого могут писаться против API.

ДЕЙСТВИЯ
1. Enums.cs: добавить BlockType, Discipline, NightMode, LevelStage,
   FailReason, LinkColor, WorkStatus, WaitReason. Старые enum'ы не менять.
2. Scripts/Core/Data/LevelSO.cs: [CreateAssetMenu "Studio/Level"], поля и
   вложенные [Serializable] классы Requirement, ParamDef, PairRule, Effect,
   TaskTemplate, CandidateDef, TextDef, LevelTuning со значениями по умолчанию
   из MVP_RULES (daySeconds = 300 и т.д.). Методы-помощники только для чтения:
   Param(id), Candidate(id), Text(key).
3. Scripts/Core/State/: LevelState.cs (+ ConceptState, Block, ConceptLink,
   EmployeeState, TaskState, FileState, BuildRecord, BuiltFile, ChatEntry,
   NightReport) – по файлу на корневой тип или по смыслу, только поля,
   List и массивы. Имя ConceptLink, а не Link: Link уже есть в BoardState.
4. LevelState.New(level, nickname): saveVersion 2, levelId, nickname
   (пустой -> "Стажёр", обрезка до 16), day 1, dayElapsed 0, stage Concept,
   budget = startBudget, deadline = deadlineDays, пустые списки, одна запись
   чата {chatId "boss", day 1, kind "text", text = briefText с подстановкой
   {name}, {days}, {budget}}.
5. SaveSystem: SaveLevel/LoadLevel/HasLevelSave/DeleteLevel, ключ save_v2,
   JsonUtility. LoadLevel возвращает null и LogWarning при битом JSON или
   saveVersion != 2. save_v1 и старые Save/Load/HasSave не трогать
   (их отключит P-28).
6. Заглушки систем MVP из ROADMAP.
7. Тестовые данные: Data/Test/Core/TestLevel.asset (2 параметра на каждый
   тип, 1 правило пары, 4 шаблона задач, 2 кандидата) – только для тестов.

ТЕСТЫ (Tests/EditMode/Core/LevelStateTests.cs, сначала красные)
- New: day 1, stage Concept, budget = startBudget, deadline = deadlineDays,
  одна запись чата руководителя, {name} подставлен, длинный ник обрезан до 16,
  пустой -> "Стажёр".
- JSON round-trip LevelState с блоками, линией, сотрудником, задачей
  с прогрессом, файлом, сборкой и записью чата: все поля равны,
  float dayElapsed сохраняется.
- SaveLevel -> LoadLevel даёт равное состояние; HasLevelSave true;
  DeleteLevel -> HasLevelSave false.
- LoadLevel: битый JSON -> null; saveVersion 1 -> null; save_v1 не читается
  и не удаляется.
- LevelTuning по умолчанию совпадает с MVP_RULES (daySeconds 300,
  tierEnergy [10,35,60], passScore уровня задаётся в данных).
- Заглушки бросают NotImplementedException с номером карточки.
В тестах сохранения не стирать реальный слот игрока: использовать отдельный
ключ через внутренний параметр или сохранять и восстанавливать save_v2
вокруг теста.

ГРАНИЦЫ
Только файлы Р1 из TEAM.md и свои тесты. Не трогать BoardRules, ReviewSystem,
UI, сцены, контент Level1, существующий API прототипа. Логика концепта,
работы и оценки – не здесь. Ничего не удалять из прототипа.

ПРОВЕРКА (критерий завершения)
G1 Новые тесты падали до кода (вывод в отчёте). G2 asset_refresh без ошибок
компиляции. G3 test_run_by_name "Studio.Tests": все зелёные, число записано,
старые тесты не сломаны. G4 Сигнатуры совпадают с ROADMAP дословно
(расхождение – правка ROADMAP + DECISIONS, а не тихо в коде). G5 git status:
в коммите только файлы задачи.

ОТЧЁТ
DEVLOG (статус «сделано»), STATE (P-19 сделан, дальше P-20, P-21, P-24
параллельно), статус P-19, DECISIONS при отступлении от контракта.
Сообщение коммита после подтверждения: feat(core): add mvp level state and save v2
```

## P-20 Концепт: блоки, линии, бафы, задачи

**Тип.** правила, TDD · **Статус.** не начато · **Кто.** Р2 · **Модель.** gpt-6.1-sol · **Размышления.** high · **Зависит от.** P-19

```text
Выполни только P-20. Общие правила – раздел «MVP: уровень 0» docs/PROMPTS.md.
Роль Р2. Ветка p-20-concept-rules.

ВХОД
CLAUDE, TEAM, STATE, MVP_RULES (3.8, 3.9, 3.10 «Генерация задач», 4.3),
ROADMAP «Контракты MVP», типы P-19, Data/Test/Core/TestLevel.

РЕЗУЛЬТАТ
ConceptRules реализован: Place, SetParam, Move, Remove, Connect, Disconnect,
PairEffects, LinkColor, Totals, CanApprove, Approve.

ДЕЙСТВИЯ
1. Один блок на тип; блок без параметра не входит в Totals; дубли линий
   в любом порядке отклоняются; удаление блока удаляет его линии.
2. LinkColor: Green только если плюсов строго больше минусов; 0:0 – Red.
3. Totals: эффекты всех пар блоков с параметрами, независимо от линий,
   зажатые ±conceptClamp.
4. CanApprove: шесть блоков с параметрами, связная сеть линий; reason –
   человеческим языком («Не выбран параметр: Стиль», «Блок Звук... не связан»).
5. Approve: approved, stage Production, dayElapsed 0, задачи по
   TaskTemplate с названием из параметров и уровнем (из блока или fixedTier),
   effort = effortPerTier × tier, запись руководителя в чат. Повтор – false.
   После утверждения все операции концепта – false/null.

ТЕСТЫ
Дубль типа; параметр чужого типа; линия к себе и дубль; удаление блока;
зелёная/красная/равенство; пара без правила; Totals без линий равен Totals
с линиями; зажим ±3; неполный и несвязный концепт; Approve создаёт 4 задачи
с уровнями из MVP_RULES (Реализм -> Арт ур. 3, Крафт -> ур. 3);
повторный Approve; правка после утверждения.

ГРАНИЦЫ
ConceptRules и тесты Р2, тестовые данные Р2. Не трогать State/WorkSim (Р1),
UI. Не хватает поля – handoff Р1.

ПРОВЕРКА
Тесты красные, затем зелёные; компиляция; маршруты А/Б/В из MVP_RULES 5:
Totals концептов равны +3/+3/+1, +1/-1/0, -1/0/+1 (М/Д/З) на тестовых
правилах этих пар.

ОТЧЁТ
DEVLOG, STATE, статус, DECISIONS. Коммит после подтверждения:
feat(core): concept board rules and task generation
```

## P-21 Время, работа, энергия, ночь, оплата

**Тип.** логика, TDD · **Статус.** не начато · **Кто.** Р1 · **Модель.** gpt-6.1-sol · **Размышления.** xhigh · **Зависит от.** P-19

```text
Выполни только P-21. Общие правила – раздел «MVP: уровень 0». Роль Р1.
Ветка p-21-work-sim.

ВХОД
CLAUDE, TEAM, STATE, MVP_RULES (3.1–3.7, 3.10 «Работа», «Файл», «Исход»,
4.2, 4.4–4.7, 5.4), ROADMAP «Контракты MVP», типы P-19.

РЕЗУЛЬТАТ
WorkSim реализован: Assign, SetPriority, Status, Tick, EndDay. День идёт
только через Tick, конец дня – одна операция с защитой от повтора.

ДЕЙСТВИЯ
1. Tick(dt): только этап Production; dt зажат maxTickSeconds; dayElapsed
   растёт; для каждого сотрудника – выбор задачи по приоритету и порогу,
   оплата дня при первом старте (paidDay), расход/восстановление энергии,
   прогресс speed очков/с, qualitySum с усталостью; завершение задачи –
   файл (или ревизия +1 и качество 50/50 для исправления) и запись в чат.
   При dayElapsed >= daySeconds вызвать EndDay и вернуть отчёт.
   Внутренний шаг – не больше 0,25 с, чтобы пороги энергии считались точно
   (достаточно разбить dt на подшаги по событиям или по 0,05 с).
2. EndDay(expectedDay): порядок MVP_RULES 3.2 – ночь, отдых, дедлайн
   (Failed(Deadline), score = последняя сборка или 0), утро. Review не
   вызывать: тупик бюджета проверяет GameLoop (P-28). Повтор с тем же
   днём – null.
3. Ночь: только nightShift и NightMode Yes/Owl, порог задачи, оплата
   nightPayShare (paidNightDay), Yes – расход ×1,5 и качество –10.
   Утром nightShift = false.
4. Status: вычисляет WorkStatus и WaitReason (NoEnergy, NoBudget,
   NoAssignee, NotFirst).

ТЕСТЫ
Расход 0,4/с и 250 с до нуля; простой +0,1/с; порог 60 не даёт начать,
начатую задачу ведёт до нуля; усталость ниже 25; прогресс и качество на
границе дня сохраняются; ставка один раз за день при двух стартах;
нет денег – нет старта и бюджет не уходит в минус; таймер 300 с и кнопка
в одном кадре – один переход и одни расходы; EndDay чужого дня – null;
ночь Yes/Owl/No; отдых +50 только неработавшим; дедлайн -> Failed(Deadline)
без сборки; Tick с dt = 5 с продвигает время только на 0,25 с; загрузка
посреди дня не платит повторно. Числа сверить с маршрутом А MVP_RULES 5.1
(Костя: Уровни 200/300 к концу дня 1, качество 63,75 после дня 2).

ГРАНИЦЫ
WorkSim и тесты Р1. Не трогать ConceptRules, Staffing, Review (Р2), UI.
Сотрудников и задачи в тестах создавать прямо в состоянии.

ПРОВЕРКА
Красные -> зелёные, компиляция, все тесты Studio.Tests зелёные.

ОТЧЁТ
DEVLOG, STATE, статус, DECISIONS (время, ночь, оплата). Коммит:
feat(core): workday timer, energy, night shifts and payroll
```

## P-22 Найм, сборка, оценка, исправление

**Тип.** правила, TDD · **Статус.** не начато · **Кто.** Р2 · **Модель.** gpt-6.1-sol · **Размышления.** xhigh · **Зависит от.** P-20, P-21

```text
Выполни только P-22. Общие правила – раздел «MVP: уровень 0». Роль Р2.
Ветка p-22-hire-review.

ВХОД
CLAUDE, TEAM, STATE, MVP_RULES (3.6, 3.7, 3.10, 4.4, 4.8, 5), ROADMAP
«Контракты MVP», готовые ConceptRules и WorkSim.

РЕЗУЛЬТАТ
Staffing (DailyRate, CanHire, Hire, SetNightShift) и Review (CanBuild,
Build, Score, CanFix, SendToFix, CheckBudgetDeadlock) реализованы.

ДЕЙСТВИЯ
1. Ставка = ratePerPoint × (speed + quality) + надбавка ночи. Найм только в
   Production, лимит teamLimit, без повтора; приветствие в чат сотрудника.
2. CanBuild: все задачи концепта done, нет открытых исправлений, есть
   изменённый файл с прошлой сборки (первая – всегда). reason по-человечески.
3. Score/Build по формуле MVP_RULES 3.10 (взвешенное качество, итог
   концепта, требования брифа, округление половина вверх). Build пишет
   снимок, запись отзыва в чат руководителя; 70+ -> Passed и score.
4. SendToFix: условия 3.7, задача "fix-<fileId>-<revision>" первой в списке.
5. CheckBudgetDeadlock: правило 3.10 «Тупик по бюджету».

ТЕСТЫ
Ставки шести кандидатов из MVP_RULES 5; лимит и повтор найма; ночь у No;
сборка без изменений запрещена; оценки маршрутов: А = 88, Б = 69, после
исправления Лерой 74, исправление Викой снова 69; требования -10 каждое;
исправление без нанятого сотрудника дисциплины или без денег – false;
второе исправление того же файла – false; тупик: нет денег ни на кого –
true, все задачи сделаны и сборка доступна – false.

ГРАНИЦЫ
Staffing, Review и тесты Р2. WorkSim, State – Р1 (handoff при нужде). UI нет.

ПРОВЕРКА И ОТЧЁТ
Красные -> зелёные, все Studio.Tests зелёные. DEVLOG, STATE, статус,
DECISIONS. Коммит: feat(core): hiring, build scoring and file fixes
```

## P-23 Контент уровня 0 и проверка маршрутов

**Тип.** контент + баланс · **Статус.** не начато · **Кто.** Р2 · **Модель.** gpt-6.1-sol · **Размышления.** high · **Зависит от.** P-22

```text
Выполни только P-23. Общие правила – раздел «MVP: уровень 0». Роль Р2.
Ветка p-23-level0-content.

ВХОД
CLAUDE, TEAM, STATE, MVP_SPEC, MVP_RULES (весь), готовые системы P-20…P-22.

РЕЗУЛЬТАТ
Data/Level0/Level0.asset (LevelSO): ТЗ руководителя, 6 типов по 3 параметра,
таблица сочетаний (включая все пары маршрутов MVP_RULES 5, равенство
Платформер + Телефон), 4 шаблона задач, 6 кандидатов, реплики (приветствия,
сдача файла, утро, последний день, отзывы, итоги). Без цен в ответах.

ДЕЙСТВИЯ
1. Ассет и тексты (тире «–», без эмодзи, до 2 строк на реплику).
2. Tests/EditMode/Level0/Level0RouteTests.cs: три маршрута MVP_RULES 5
   через настоящие ConceptRules, Staffing, WorkSim, Review с dt = 0,25 и
   нажатием конца дня там, где его жмёт игрок. Сверить: А – пройден 88,
   Б – 69, исправление -> 74 (в день 3), В – Failed(Deadline) в конце дня 3,
   Механика 93 %, Уровни 44 %.
3. docs/balance/level0.md: таблицы маршрутов из тестов (время, бюджет,
   энергия, файлы, оценка). Расхождение с MVP_RULES – поправить числа в
   данных или описать в DECISIONS и MVP_RULES, не подгонять формулы.
4. Проверка данных тестом: все id уникальны и разрешаются, у каждого
   шаблона задачи есть параметр уровня, каждому критерию есть кандидат.

ГРАНИЦЫ
Data/Level0, docs/balance, тесты Р2. Data/Level1 не удалять. Код систем не
менять ради баланса – только handoff.

ПРОВЕРКА И ОТЧЁТ
Тесты маршрутов зелёные; длительность маршрутов (игровые секунды) записана.
DEVLOG, STATE, статус. Коммит: content: tutorial level 0 data and route tests
```

## P-24 Меню, верхняя панель, чаты

**Тип.** UI · **Статус.** не начато · **Кто.** Р3 · **Модель.** gpt-6.1-sol · **Размышления.** high · **Зависит от.** P-19 (контент – P-23, можно начать на TestLevel)

```text
Выполни только P-24. Общие правила – раздел «MVP: уровень 0». Роль Р3.
Ветка p-24-menu-topbar-chat.

ВХОД
CLAUDE, TEAM, STATE, MVP_SPEC (2, 4 «Чат»), MVP_RULES (3.1, 3.11),
ROADMAP «Контракты MVP», префабы MenuScreen, TopBar, MessengerPanel.

РЕЗУЛЬТАТ
1. Меню: логотип, название, выбор уровня (одна кнопка «0 · Обучение»),
   окно никнейма с читаемым заголовком, текстом ввода и placeholder на
   светлом поле; «Продолжить · день N» только при save_v2; новая игра при
   сохранении – подтверждение. Выбор сложности удалён из префаба и кода.
   Session передаёт levelId и ник.
2. TopBar: день, оставшееся время дня (мм:сс), бюджет, кнопка «Закончить
   день» (подтверждение; в фазе Concept неактивна с подсказкой), кнопка
   паузы, вкладки четырёх окон: Чат, Дэшборд, Хедхантер, Таск-лист
   (Хедхантер и Таск-лист неактивны до утверждения концепта, с причиной).
   Окно ночного отчёта (Prefabs/UI/Common/NightReport).
3. Чаты строятся из LevelState.chat: на старте только руководитель;
   чат сотрудника – после найма; запись "file" – строка-файл с названием,
   ревизией, качеством и кнопкой «На исправление» (активна при негативном
   отзыве, вызывает Review.CanFix/SendToFix; выбор исполнителя – список
   нанятых сотрудников дисциплины). Цен на кнопках ответов нет.
Время, пауза и сохранение – не здесь: кнопки отдают события, которые
подключит GameLoop (P-28). До P-28 панели получают LevelState из
LevelState.New(TestLevel) в простом тестовом загрузчике своей папки.

ГРАНИЦЫ
Scripts/UI/Messenger, Menu, Prefabs/UI/Messenger, Common, Menu, сцена Menu.
Не трогать Board, Tasks, Versions, сцену Game, Core.

ПРОВЕРКА
Play: меню -> ник -> игра; читаемость окна ника (скриншот); чат только
руководителя; после ручного добавления сотрудника в тестовом состоянии
появляется его чат и файл. 1920×1080 и 1366×768. progress.py menu-mvp,
chat-mvp. Ограничения проверки перечислить.

ОТЧЁТ
DEVLOG, STATE, статус. Коммит: feat(ui): mvp menu, workday top bar and chats
```

## P-25 Хедхантер

**Тип.** UI · **Статус.** не начато · **Кто.** Р3 · **Модель.** gpt-6.1-sol · **Размышления.** medium · **Зависит от.** P-22, P-24

```text
Выполни только P-25. Общие правила – раздел «MVP: уровень 0». Роль Р3.
Ветка p-25-hire-window.

РЕЗУЛЬТАТ
Prefabs/UI/Hire/HirePanel + Scripts/UI/Hire/: список кандидатов из LevelSO
(портрет, имя, дисциплина, скорость, качество, ночь, ставка в день),
кнопка «Нанять» с причиной недоступности из Staffing.CanHire, нанятые
отмечены, у нанятых – энергия и переключатель «Работать ночью»
(Staffing.SetNightShift, неактивен у NightMode.No). Строка кандидата –
префаб CandidateRow.

ГРАНИЦЫ
Scripts/UI/Hire, Prefabs/UI/Hire. Корневой префаб в сцену ставит Р1 (P-28):
handoff с именем префаба.

ПРОВЕРКА
Play на TestLevel/Level0: найм открывает чат (через P-24), лимит команды
показывает причину, ночь у «нет» неактивна. Скриншот progress.py hire.

ОТЧЁТ
DEVLOG, STATE, статус. Коммит: feat(ui): headhunter window
```

## P-26 Дэшборд: свободный холст

**Тип.** UI · **Статус.** не начато · **Кто.** Р4 · **Модель.** gpt-6.1-sol · **Размышления.** high · **Зависит от.** P-20

```text
Выполни только P-26. Общие правила – раздел «MVP: уровень 0». Роль Р4.
Ветка p-26-dashboard-canvas.

ВХОД
CLAUDE, TEAM, STATE, MVP_SPEC 4 «Дэшборд», MVP_RULES 3.8, 3.9, 4.3,
ConceptRules (P-20), текущий BoardPanel.

РЕЗУЛЬТАТ
Новый BoardPanel: слева панель шести типов блоков (тип неактивен после
размещения); перетаскивание или клик кладёт блок на холст (ConceptRules.Place);
клик по блоку – всплывающее меню параметров (SetParam); перемещение (Move),
удаление (Remove); связь – ручка на блоке или режим «Связать» (Connect),
клик по линии – подпись «+N / –M» с текстами эффектов и удаление.
Линия зелёная/красная по ConceptRules.LinkColor, смысл дублирован подписью.
Итог концепта по трём критериям (Totals) и кнопка «Утвердить концепт» с
причиной из CanApprove. Бриф, цели, рука карточек, «Исследовать рынок»,
«Спросить команду», шесть слотов и легенда четырёх статусов удалены.
После утверждения холст только для чтения.

ГРАНИЦЫ
Scripts/UI/Board, Prefabs/UI/Board. Блок и линия – префабы; линия –
повёрнутый Image (как прежде). Не трогать Core, TopBar, Game.

ПРОВЕРКА
Play обычными кликами: шесть блоков, параметры, пять линий, зелёная,
красная, равенство (Платформер + Телефон), неполный концепт с причиной,
утверждение. 1920×1080 и 1366×768. progress.py dashboard.

ОТЧЁТ
DEVLOG, STATE, статус. Коммит: feat(ui): free-canvas concept dashboard
```

## P-27 Таск-лист, сборка, отзыв, итог

**Тип.** UI · **Статус.** не начато · **Кто.** Р4 · **Модель.** gpt-6.1-sol · **Размышления.** high · **Зависит от.** P-22, P-26

```text
Выполни только P-27. Общие правила – раздел «MVP: уровень 0». Роль Р4.
Ветка p-27-tasks-review.

РЕЗУЛЬТАТ
1. Prefabs/UI/Tasks/TaskPanel + Scripts/UI/Tasks/: строки задач (префаб
   TaskRow): тип, название, уровень, прогресс %, исполнитель (выпадающий
   список нанятых нужной дисциплины, WorkSim.Assign), статус и причина
   ожидания (WorkSim.Status: «Устал: нужно 60, есть 53», «Нет денег на
   день»), стрелки приоритета (SetPriority). Кнопка «Собрать игру» с
   причиной из Review.CanBuild.
2. Экран отзыва (Prefabs/UI/Versions, модальный): звук, дизайн, механики,
   оценка, требования брифа, слабейший критерий с файлами (качество,
   автор) и минусами концепта; при оценке ниже 70 – «На исправление» у
   файла (Review.CanFix/SendToFix, выбор исполнителя). История сборок.
3. Итог: пройден / не пройден, оценка уровня = оценка игры, причины
   (незавершённые задачи с %, невыполненные требования), «Заново» и «В меню».
Старые вкладка «Версии», журнал, поиск виновника, концовки по осям –
отключить в своих префабах.

ГРАНИЦЫ
Scripts/UI/Tasks, Versions, их префабы. Core и чужие префабы – handoff.

ПРОВЕРКА
Play на Level0: назначение, приоритет, статусы ожидания, сборка, негативный
отзыв, исправление, повторная сборка, итог. progress.py tasks, review.

ОТЧЁТ
DEVLOG, STATE, статус. Коммит: feat(ui): task list, build review and result
```

## P-28 Интеграция и отключение прототипа

**Тип.** интеграция · **Статус.** сделано (2026-10-25): EditMode 153/153, Web-билд проверен в P-29 · **Кто.** Р1 · **Модель.** gpt-6.1-sol · **Размышления.** high · **Зависит от.** P-23…P-27

```text
Выполни только P-28. Общие правила – раздел «MVP: уровень 0». Роль Р1.
Ветка p-28-mvp-integration.

РЕЗУЛЬТАТ
1. Scripts/UI/Common/GameLoop.cs: создаёт или загружает LevelState по
   Session, вызывает WorkSim.Tick(Time.unscaledDeltaTime) только без паузы
   и модальных окон (MVP_RULES 3.1), после каждого конца дня –
   Review.CheckBudgetDeadlock, показывает ночной отчёт, автосохранение
   каждые autosaveSeconds и после операций, «Продолжить» открывает на паузе.
2. Сцена Game: корневые префабы TopBar, MessengerPanel, BoardPanel,
   HirePanel, TaskPanel, окна отзыва и итога; ссылки на Level0.
3. Отключение прототипа: UI больше не ссылается на GameState, BoardRules,
   DecisionResolver, DayClock, ReviewSystem, Difficulty, ContentDB_Level1.
   Удалить устаревшие системы и их тесты отдельным списком в DECISIONS
   (или оставить с [Obsolete], если удаление ломает чужие префабы – handoff).
4. Web-сборка, размер, проверка открытия.

ПРОВЕРКА
Компиляция, все тесты зелёные, Play: новая игра до первого файла;
Web-билд открывается. DEVLOG, STATE, статус, DECISIONS.
Коммит: feat: wire mvp tutorial loop and retire prototype systems
```

## P-29 Проверка в браузере

**Тип.** QA · **Статус.** проверено агентом (2026-10-25): `docs/playtests/mvp-level0.md`, блокеров нет · **Кто.** Р1 + QA · **Модель.** gpt-6.1-sol · **Размышления.** high · **Зависит от.** P-28

```text
Выполни только P-29. Общие правила – раздел «MVP: уровень 0». Роль Р1, QA
без правок чужих файлов.

РЕЗУЛЬТАТ
docs/playtests/mvp-level0.md: три маршрута MVP_RULES 5 пройдены в Web-билде
обычным вводом (время, дни, бюджет, оценка – факт против бумаги);
краевые случаи 5.4: таймер + кнопка, усталость, незавершённая задача,
нет денег, F5 посреди дня (без повторной оплаты), пересборка без изменений.
Приёмка MVP_SPEC 7 по пунктам: да / нет / не проверено. Каждому провалу –
воспроизведение, ожидание, факт, владелец, скриншот; открытые – в BUGS.
Длительность полной сессии – измеренная.

ГРАНИЦЫ
Исправлять только файлы Р1; остальное – handoff. Не подменять состояние
консолью для доказательства.

ОТЧЁТ
DEVLOG, STATE, BUGS, CASE, статус. Коммит при изменениях:
test: verify tutorial level in web build
```

---

## Post-MVP: визуальный juice (P-30…P-32)

Эти карточки – отдельный трек полировки после P-29. Они предполагают, что
MVP уже интегрирован и проверен в браузере. Игровые правила, расчёты и
содержание остаются прежними; работа добавляет понятный визуальный отклик,
ритм и атмосферу студии. Порядок: P-30 задаёт общий язык движения; затем
P-31 оживляет доску и работу; P-32 завершает цикл сборкой, отзывом и релизом.

Общие референсы: [макеты проекта](mockup/), [A Short Hike](https://ashorthike.com/)
для цельности пиксельного мира, [Good Pizza, Great Pizza – press kit](https://goodpizzagreatpizza.com/press/)
для уютного делового цикла и персонажей, [Papers, Please – press kit](https://www.papersplea.se/presskit/)
для предметного пиксельного интерфейса и отчёта о дне. Использовать как
принципы, не копировать графику или экраны.

## P-30 Язык движения: студия, навигация и общие реакции

**Тип.** визуальная полировка · **Статус.** частично сделано (2026-10-25): кнопки, смена окон, верхняя панель, найм; меню/чат/снимки не доделаны · **Кто.** Р3 · UI · **Модель.** gpt-6.1-sol · **Размышления.** high · **Зависит от.** P-29

```text
Выполни только P-30 из docs/PROMPTS.md. Общие правила – раздел «MVP: уровень 0».
Роль Р3, UI меню, чата, хедхантера и общих компонентов.

ВХОД
Прочитай CLAUDE.md, docs/TEAM.md, docs/STATE.md, docs/MVP_SPEC.md,
docs/MVP_RULES.md, docs/ROADMAP.md и docs/AUDIO.md. Осмотри текущие сцены,
префабы и отчёт P-29. Если P-29 ещё не принят или MVP в браузере не проходит,
не переносить состояние через отладочные инструменты и не менять его логику:
подготовить план/ограничения в отчёте и остановить игровые правки.

РЕЗУЛЬТАТ
Интерфейс «РЕЛИЗ!» реагирует на действия одним согласованным ритмом.
Оформление остаётся пиксельным и тёплым: тёмная студия, олива, кремовый,
жёлтый, существующие портреты и рамки. Фон и важные надписи остаются
читаемыми; движение подсказывает действие и результат, не задерживая игрока.

СДЕЛАЙ
1. Создай docs/UI_MOTION.md как краткий общий лист: виды переходов, состояния
   кнопок, принцип «действие → короткий отклик → устойчивое состояние»,
   рекомендуемые диапазоны времени и правила пропуска/доступности. За основу
   возьми 80–160 мс для микровзаимодействий, 160–260 мс для появления карточки
   или панели, 450–800 мс для важного раскрытия. Это стартовые значения,
   настрой на ощущение в Play. Не вводи постоянное движение каждого элемента.
2. Общие кнопки: различимые hover, keyboard focus, pressed, disabled и
   selected; лёгкий подъём/контур при наведении, короткое сжатие при нажатии,
   быстрое возвращение. Состояния должны быть понятны и без цвета. Touch не
   должен зависеть от hover. Не менять назначение и доступность кнопок.
3. Меню и переход между основными экранами: короткое появление/уход без
   продолжительных заставок. Один мягкий акцент при старте новой смены.
4. TopBar: при реальном изменении дня, денег или энергии подсветить только
   соответствующее поле и плавно обновить число. Не имитировать начисление,
   которое не произошло в состоянии игры.
5. Messenger: новое сообщение появляется вместе с портретом и компактным
   акцентом непрочитанного; выбор ответа получает краткое подтверждение.
   Не печатать текст медленно посимвольно и не подменять реальные сообщения.
6. Hire: при успешном найме карточка явно переходит в состояние «В команде»,
   портрет появляется в составе, а стоимость подсвечивается только после
   реального списания. Отказ и недоступность не маскировать анимацией успеха.
7. Используй только имеющиеся зависимости, предпочтительно установленный
   PrimeTween после чтения соответствующей skill-инструкции. Не добавляй
   пакеты и не меняй ProjectSettings. Все элементы и анимационные состояния
   остаются редактируемыми в префабах; не создавать UI через код.
8. Не добавлять звуки в этой задаче. Обозначить подходящие точки синхронизации
   с SFX в отчёте, если текущая спецификация docs/AUDIO.md это поддерживает.

ГРАНИЦЫ
Scripts/UI/Menu, Messenger, Hire; Prefabs/UI/Menu, Messenger, Hire, Common.
TopBar – только принадлежащий Р3 префаб/скрипт. Не менять GameLoop, сцены,
Core, Board, Tasks, Versions и чужие префабы. Изменение общей схемы переходов
или API – handoff владельцу.

ПРОВЕРКА
Play и обычный ввод: hover/click/focus/disabled, старт, переходы, новое
сообщение, выбор ответа, доступный и недоступный кандидат. Проверить 1920×1080
и 1366×768; WebGL проверить на pointer и touch-эмуляции, если доступны.
Сделать одинаковые кадры до/после для меню, чата и найма; добавить их через
tools/progress.py со стабильным feature slug ui-juice-common. Зафиксировать
непроверенные платформы и отсутствие тестов (логика не меняется).

ОТЧЁТ
DEVLOG, STATE, статус P-30. Коммит только после подтверждения автора:
feat(ui): add motion feedback to studio and common screens
```

## P-31 Осязаемая доска и рабочий день

**Тип.** визуальная полировка · **Статус.** частично сделано (2026-10-25): код есть, эффект глазами не проверен · **Кто.** Р4 · Доска и задачи · **Модель.** gpt-6.1-sol · **Размышления.** high · **Зависит от.** P-30

```text
Выполни только P-31 из docs/PROMPTS.md. Общие правила – раздел «MVP: уровень 0».
Роль Р4, доска и таск-лист.

ВХОД
Прочитай CLAUDE.md, docs/TEAM.md, docs/STATE.md, docs/MVP_SPEC.md,
docs/MVP_RULES.md, docs/ROADMAP.md, docs/UI_MOTION.md и отчёты P-29/P-30.
Следуй общему языку движения. Проверь, что P-29 принят и текущие правила
работы, концепта, прогресса и файлов доступны из Core без новых полей.

РЕЗУЛЬТАТ
Концепт выглядит собранным руками, а работа команды – видимым прогрессом.
Игрок всегда понимает, что выбралось, куда встало, что изменилось и почему.

СДЕЛАЙ
1. Доска: карточка при захвате слегка поднимается, получает аккуратную тень
   и масштаб; при помещении мягко фиксируется в слот. Не мешать точному
   перетаскиванию мышью или touch и не менять snap/drag правила.
2. Связь между блоками рисуется от первого порта ко второму; финальная точка
   коротко акцентируется. Существующая цветовая логика остаётся неизменной.
   Для неверной/невозможной связи дать короткий понятный отказ (лёгкий сдвиг
   плюс значок/подпись), не использовать сильную тряску всего экрана.
3. Изменения концепции должны иметь видимый, но спокойный отклик. Не
   анимировать неизменившиеся блоки при каждом фоновом Refresh.
4. Таск-лист: прогресс плавно следует фактическому значению. При переходе
   задачи в готово – однократная отметка, а не постоянный цикл. Состояния
   ожидания, усталости и нехватки бюджета показывать текстом и пиктограммой.
5. Готовый файл появляется как предметный артефакт (карточка/иконка файла),
   кратко перемещается в лоток сборки или подсвечивает место назначения.
   Уведомление привязать к настоящему переходу файла в состояние готовности;
   после повторного рендера оно не должно проигрываться заново.
6. Анимации не меняют данные, длительность рабочего тика, стоимость,
   очередность и правила прикрепления файлов. Никакого фальшивого прогресса.
   Если нужна информация, которой нет в UI-модели, оформить handoff Р1/Р2.
7. Используй префабы и их Inspector-ссылки. Не создавать GameObject,
   компоненты, частицы или линии в рантайме; не заменять проектные спрайты
   случайными примитивами. Не добавлять пакеты и не менять Core.

ГРАНИЦЫ
Scripts/UI/Board, Scripts/UI/Tasks и Prefabs/UI/Board, Prefabs/UI/Tasks.
Не менять Core, GameLoop, TopBar, Hire, Messenger, Versions и чужие префабы.

ПРОВЕРКА
Play: разместить/переместить/убрать блок, провести и удалить связь, увидеть
реальную смену прогресса, готовую задачу, файл в лотке, ожидание и усталость.
Проверить мышь и touch, 1920×1080 и 1366×768, затем WebGL, если доступен.
Сравнить кадры доски и таск-листа в одинаковых состояниях через
tools/progress.py, feature slug ui-juice-workloop. Указать недоступные проверки.

ОТЧЁТ
DEVLOG, STATE, статус P-31. Коммит только после подтверждения автора:
feat(ui): make concept and production feedback tactile
```

## P-32 Сборка, отзыв и релиз как кульминация

**Тип.** визуальная полировка · **Статус.** частично сделано (2026-10-25): фаза сборки и раскрытие отзыва, без финальной вспышки и снимков · **Кто.** Р4 · Версии и итог · **Модель.** gpt-6.1-sol · **Размышления.** high · **Зависит от.** P-31

```text
Выполни только P-32 из docs/PROMPTS.md. Общие правила – раздел «MVP: уровень 0».
Роль Р4, сборка, отзыв и итог.

ВХОД
Прочитай CLAUDE.md, docs/TEAM.md, docs/STATE.md, docs/MVP_SPEC.md,
docs/MVP_RULES.md, docs/ROADMAP.md, docs/UI_MOTION.md и отчёты P-29…P-31.
Изучи текущие состояния успешной сборки, отзыва на исправление, повторной
сборки и провала. Сохраняй реальные числа, обложки и причины из состояния.

РЕЗУЛЬТАТ
Сборка ощущается важным действием, отзыв – понятной реакцией заказчика,
а итог – экраном о конкретной игре, которую собрал игрок.

СДЕЛАЙ
1. Нажатие «Собрать игру» даёт короткую фазу «сборка» с понятным движением
   индикатора/печатной полосы и переходом в фактический результат. Нельзя
   показывать фиктивный процент загрузки или создавать ожидание дольше,
   чем нужно для прочтения результата.
2. Отзыв раскрывается в ясном порядке: общий итог, три фактические оценки,
   выполненные требования, слабый критерий и связанный файл/следующее
   действие. Полосы заполняются по реальным числам; не менять score и verdict.
3. Успех и исправление различимы не только цветом: разные текст/пиктограмма/
   композиция; доступные действия появляются сразу. Ошибка сборки, отказ,
   принятый риск и успешный релиз сохраняют разные смыслы и не смешиваются.
4. Связанный с отзывом файл или причина получает аккуратную визуальную связь
   с нужным местом на доске/в задачах, только если маршрут уже поддержан UI.
   Не придумывать переход или данные. При недостающем API – handoff.
5. Финал: сначала появляется обложка созданной игры и название, затем
   результат, выполненные требования, пара ключевых решений и расходы.
   Короткая пиксельная вспышка/звёзды допустимы только для настоящего успеха;
   не закрывают текст и могут быть пропущены. При провале показать ясный
   следующий шаг, без победной конфетти-анимации.
6. Все переходы можно завершить повторным кликом/клавишей или дождаться
   автоматически; повторное открытие панели не переигрывает кульминацию.
   Поддержать настройки уменьшения движения, если они существуют; не
   создавать новый глобальный раздел настроек в этой задаче.
7. Только редактируемые префабы и сериализованные ссылки; никаких новых
   правил оценки/сборки, Core-полей, скрытых сцен или UI, собираемого кодом.
   Не добавлять пакеты.

ГРАНИЦЫ
Scripts/UI/Versions, Scripts/UI/Tasks только если это часть области Р4,
Prefabs/UI/Versions и Prefabs/UI/Tasks. Не менять Core, GameLoop, Menu,
Messenger, Common и чужие префабы.

ПРОВЕРКА
Обычным игровым вводом пройти один успешный релиз, один отзыв «на исправление»
и повторную сборку. Проверить показ настоящих оценок, файла, требований,
обложки, финального статуса, пропуск анимации, 1920×1080 и 1366×768.
После – WebGL, если доступен. Сделать сопоставимые снимки сборки, отзыва,
исправления и финала через tools/progress.py, feature slug ui-juice-release.
Не подменять сохранённое состояние консолью ради приёмки; недоступные пути
пометить непроверенными.

ОТЧЁТ
DEVLOG, STATE, статус P-32. Коммит только после подтверждения автора:
feat(ui): stage build review and release feedback
```

---

## Архив: прежние P-18…P-26 (заменены 2026-10-24)

Заменены карточками раздела «MVP: уровень 0» по `MVP_SPEC.md`. Не исполнять. Причины: план без таймера, без найма и энергии, с четырьмя осями, исследованием, сложностями и причинным поиском по решениям чата (`MVP_RULES.md`, раздел 2). Текст сохранён для истории.

### Общие правила P-18…P-26 (стар.)

Эти карточки исправляют цепочку «задумка → решение → результат → отзыв → доработка». План – `docs/GAMEPLAY_REWORK.md`. Не запускать всё одним промптом. У каждой карточки свой результат и проверка; следующую начинать после готовности зависимости.

- Выбрать модель и effort в клиенте перед отправкой промпта. Написанное внутри промпта название не переключает модель автоматически. Модели Codex: `gpt-6-astra` для P-18, `gpt-6.1-sol` для реализации и проверки; `xhigh` – очень высокий, `high` – высокий, `medium` – средний уровень размышлений. Использовать точные настройки из карточки. Выбор сделать в настройках модели и размышлений чата Codex.
- В начале читать CLAUDE, TEAM, STATE, нужную карточку и общие правила этого раздела. После P-18 актуальный GDD и контракты ROADMAP должны совпадать с принятым планом. Если не совпадают, сначала зафиксировать конкретное расхождение, не реализовывать взаимоисключающие требования.
- Стартовать с `git status`. Текущие незакоммиченные изменения сохранять. Не делать reset/clean/stash, массовое добавление или откат настроек. Перед кодом проверить, что принятые зависимости включены в базу. Ветка `p-N-short-name` от актуального dev, PR в dev; при неподготовленной базе выполнить доступную проверку и сообщить, каких изменений не хватает. Не выдавать старый dev за актуальную игру.
- Владение соблюдать по TEAM и уточнённой в P-18 карте. Вызвать API соседней роли можно, редактировать её файл нельзя. Межролевую проблему записать как конкретный handoff с воспроизведением и критерием, без самостоятельной отправки сообщений коллегам.
- Для игровой логики TDD; устаревшие тесты принудительного отклонения хорошей версии и штрафа за угадывание заменить тестами новых утверждённых правил, а не сохранять ошибочное поведение ради прежнего числа тестов.
- UI – сцены/префабы и сериализованные ссылки. Для Unity сначала прочитать существующий `unity-skills/SKILL.md` (актуально `.agents/skills/unity-skills/SKILL.md`, в других установках `.agent/skills/unity-skills/SKILL.md`). REST `localhost:8090` с `expectProject=centrinvest-profession-game`; соблюдать режим сервера. После .cs – refresh и ожидание компиляции. Тесты только `Studio.Tests` через `test_run_by_name`, не весь пакет.
- Перед Play-проверкой сохранить пользовательский прогресс безопасным способом; запуск новой игры не должен молча стирать его. Не выдавать вызов обработчика или установку состояния за прохождение интерфейса. Если обычный ввод недоступен, отметить ограничение.
- После задачи: DEVLOG, STATE, статус карточки; DECISIONS при изменении правила; progress.py для видимых изменений. Различать «тесты прошли», «проверено агентом» и «проверено игроками». Указывать непроверенное. Не объявлять карточку завершённой по наличию файлов.
- Один согласованный коммит с документами. Показать точный состав своих изменений и английское Conventional Commits сообщение, дождаться подтверждения автора перед коммитом. Без AI attribution. Не публиковать билд автоматически в рамках карточек геймплея.

### P-18 (стар., заменена) Правила одной игры и контракт переделки

**Тип.** дизайн + документация · **Статус.** заменена 2026-10-24 · **Кто.** Р1, координатор документов · **Модель.** gpt-6-astra · **Размышления.** high · **Зависит от.** текущего состояния проекта

```text
Выполни только P-18 из docs/PROMPTS.md. Прочитай общие правила P-18…P-26.
Модель для этого чата: gpt-6-astra, effort high.

РЕЗУЛЬТАТ
Единый, проверенный на бумаге контракт игры, по которому следующие карточки
могут писать код без самостоятельного изобретения правил. Основное направление
уже предложено в docs/GAMEPLAY_REWORK.md: работай от него.

ВХОД
CLAUDE.md, TEAM.md, STATE.md, CASE.md, GAMEPLAY_REWORK.md, GDD.md, DEMO.md,
ROADMAP.md, DECISIONS.md, BUGS.md. Из кода проверь только спорные правила:
BoardRules, DecisionResolver, DayClock, ReviewSystem, Ending и типы данных.
Проверь git status и различия текущей работы с dev/origin/dev.

СДЕЛАЙ
1. Составь короткую карту уже сделанных, незакоммиченных и ещё планируемых
   изменений. Укажи фактическую базу для P-19. Существующие изменения не присваивай.
2. Зафиксируй один бриф, две допустимые концепции, 6–8 значимых решений,
   обязательные требования и необязательные улучшения. Для каждого выбора:
   что игрок знает заранее, стоимость, результат, последующее наблюдение.
3. На бумаге просчитай три маршрута: концепция A с исправлением, концепция B
   с другим разумным выбором, ошибочный маршрут с нехваткой ресурсов.
   Запиши таблицы в docs/GAMEPLAY_SCENARIOS.md: шаг, день, бюджет, результат,
   проблемы, доступный следующий шаг. Отдельно проверь правильное обучение
   с самого начала: ложного отзыва и обязательного отказа v0.1 быть не должно.
4. Согласуй точные правила из GAMEPLAY_REWORK: обязательные слоты, смысл
   ниток, цена концепции и её изменения, время, доступность сборки,
   причины отзывов, разбор без штрафа, исправление/принятие риска и финал.
   Числа пока стартовые; не заявляй баланс по одному идеальному маршруту.
5. Обнови GDD и DEMO, в ROADMAP зафиксируй минимальные контракты:
   требования брифа, факты проекта/снимок версии, проблема с источником,
   проверка доступного действия, единая операция времени, условия контента.
   Назначь владельцев новых файлов в TEAM. Не вводи универсальный движок
   квестов, графовый редактор, найм или симуляцию занятости.
6. Сопоставь контракты с P-19…P-26; уточни карточки, если без этого они
   противоречат друг другу. Исправь устаревший ближайший шаг STATE.
   Проверь отсутствующий task-prompt шаблон: восстанови только из найденного
   источника либо укажи существующую P-1 как рабочую структуру.

ГРАНИЦЫ
Только документы и правила работы. Не менять Assets, сцены, пакеты, настройки,
сохранения, ветки и чужие незакоммиченные файлы. Не начинать P-19.

ПРИЁМКА
Все три маршрута заканчиваются объяснимо; минимум два успешных используют
разные решения; ни один обязательный отзыв не появляется без причины;
каждая карточка имеет владельца и достаточный входной контракт.
Читатель понимает цель, риск и результат без знания скрытых осей.

ОТЧЁТ
Обнови DEVLOG, STATE и статус P-18. Покажи ключевые правила и расчёты,
оставшиеся вопросы отдельно. Сообщение коммита:
docs: define coherent gameplay loop and rework contracts
Коммит только после подтверждения автора.
```

### P-19 (стар., заменена) Единое состояние, время и сохранение

**Тип.** логика, TDD · **Статус.** заменена 2026-10-24 · **Кто.** Р1 · **Модель.** gpt-6.1-sol · **Размышления.** xhigh · **Зависит от.** P-18 и актуальной принятой базы

```text
Выполни только P-19 из docs/PROMPTS.md. Прочитай общие правила P-18…P-26.
Модель: gpt-6.1-sol, effort xhigh. Роль Р1.
Вход: CLAUDE, TEAM, STATE, обновлённые GDD/DEMO/ROADMAP,
GAMEPLAY_REWORK и docs/GAMEPLAY_SCENARIOS.md.

РЕЗУЛЬТАТ
Все последующие правила опираются на одно состояние проекта, один календарь
и сохраняемые причины доступности действий.

СДЕЛАЙ
1. Реализуй типы и сериализацию по контрактам P-18: требования брифа,
   факты проекта, устойчивые id решений, снимки версий, причины проблем,
   прогресс введения. Публичные типы и файлы – только по карте владения.
2. Сделай единую операцию продвижения времени. Переведи Apply/EndDay своей
   роли на неё. Последовательно обработай каждый пересечённый день,
   наступившие сообщения и follow-up без пропусков и дублей. Сначала расход
   и результат действия по контракту, затем события в определённом порядке.
3. Вычисляй этап и доступные действия по состоянию проекта. У действия есть
   причина блокировки и следующий доступный шаг. День 13 сам по себе больше
   не доказывает готовность проекта. Дедлайн всегда ведёт к определённому исходу.
4. Добавь минимальные условия сообщений по выбранной концепции/решению
   из P-18. Не создавай произвольный язык условий.
5. Сохраняй новые данные и восстановление текущего шага. Реализуй выбранную
   в P-18 миграцию старого save_v1 либо явный несовместимый формат без молчаливого
   принятия потерянных полей. Существующий слот не стирай при проверке.

ТЕСТЫ
Действие за несколько дней доставляет все наступившие события один раз;
граница дедлайна; отказ из-за бюджета не меняет состояние; повтор команды
не удваивает решение; обе ветки контента; round-trip в Idea, Production,
Release и после доработки; битый/старый JSON обрабатывается по контракту.

ГРАНИЦЫ
Только файлы Р1 и его тесты. Не менять BoardRules, ReviewSystem, UI или
контент уровня. Владельцам этих систем передай точные сигнатуры, которые
они подключат в P-20/P-21/P-24. До этого не объявляй весь UI новым.

ПРИЁМКА И ОТЧЁТ
Компиляция без ошибок, новые тесты сначала красные затем зелёные;
существующие сценарии совместимости проверены. Заполни DEVLOG/STATE/статус,
DECISIONS для времени и сохранения. Не начинай следующую карточку.
Коммит после подтверждения: feat(core): unify project state and timeline
```

### P-20 (стар., заменена) Доска как осмысленная концепция

**Тип.** правила, TDD · **Статус.** заменена 2026-10-24 · **Кто.** Р2 · **Модель.** gpt-6.1-sol · **Размышления.** xhigh · **Зависит от.** P-19

```text
Выполни только P-20 из docs/PROMPTS.md. Прочитай общие правила P-18…P-26.
Модель: gpt-6.1-sol, effort xhigh. Роль Р2.
Вход: CLAUDE, TEAM, STATE, контракты ROADMAP, GDD, GAMEPLAY_REWORK,
бумажные маршруты P-18 и API состояния/времени P-19.

РЕЗУЛЬТАТ
Концепция определяет создаваемую игру, стоимость и реальные риски.

СДЕЛАЙ
1. Проверяй обязательные элементы и требования брифа. Пустая/неполная
   концепция не проходит. Отличай блокер от осознанно принимаемого риска.
2. Вычисляй значимые совместимости по установленным карточкам независимо
   от links. Нитка – представление связи; её удаление не убирает конфликт.
   Смена карточки пересчитывает все затронутые сочетания.
3. Раздели требования и фактический результат. Хорошее сочетание не должно
   одновременно повышать результат и ровно настолько же ужесточать порог.
   Верни UI описание эффекта и его причины, а не только цвет/число.
4. Реализуй смету и фиксацию концепции по P-18, включая productionCost.
   Проверка стоимости не изменяет состояние; подтверждение списывает её
   ровно один раз. Повторный питч использует правило доработки, а не повторную
   полную оплату без объяснения. Не допусти фарма бонусов переподачей.
5. Подключи исследование конкретного риска с одной объявленной ценой.
   Уже известная информация не продаётся снова. Время – только API P-19.

ТЕСТЫ
Пустая/частичная/полная доска; конфликт без нитки и после её удаления;
две жизнеспособные концепции; бонус и требование независимы; цена карточек;
нехватка денег без частичного списания; повторная фиксация; замена карточки;
доработка и возврат в Release без лишнего EndDay и накопления бонусов.

ГРАНИЦЫ
BoardRules и тесты Р2, минимальные тестовые данные. Не менять Core-файлы Р1,
UI, сцены и весь уровень. Если не хватает контракта – точный handoff Р1.

ПРИЁМКА И ОТЧЁТ
Бумажная концепция и реальный расчёт совпадают. Готовы данные для UI P-23:
текущая цель, недостающее, смета, риски, объяснение каждой связи.
Тесты/компиляция, DEVLOG, STATE, статус, DECISIONS.
Коммит после подтверждения: feat(board): make concept choices consequential
```

### P-21 (стар., заменена) Отзывы о реальных проблемах

**Тип.** правила, TDD · **Статус.** заменена 2026-10-24 · **Кто.** Р2 · **Модель.** gpt-6.1-sol · **Размышления.** xhigh · **Зависит от.** P-20

```text
Выполни только P-21 из docs/PROMPTS.md. Прочитай общие правила P-18…P-26.
Модель: gpt-6.1-sol, effort xhigh. Роль Р2.
Вход: CLAUDE, TEAM, STATE, принятые правила GDD/ROADMAP/GAMEPLAY_REWORK,
бумажный пример обучения и контракты P-19/P-20.

РЕЗУЛЬТАТ
Отзыв объясняется конкретным состоянием продукта. Доработка устраняет
причину, а принятие риска честно отражается в результате.

СДЕЛАЙ
1. У проблемы устойчивый id, условие возникновения, источник решения/
   элемента концепции, доказательство, обязательность и допустимые исправления.
   Отзывы вычисляй по снимку версии; прошлые версии после правки не меняются.
2. Удали принудительный отказ v0.1 и добавление критики на выполненные
   требования. Если нет причины, соответствующей критики не должно быть.
3. Замени поиск минимальной дельты оси как источника любой цитаты на
   причинную связь из контента. Несколько допустимых причин укажи явно.
   Просмотр/проверка гипотезы бесплатны; результат объясняет связь.
4. Раздели состояния «причина найдена», «изменение внесено», «проверено новой
   версией» и «риск принят», если это допускает контракт. Одно нажатие на
   правильную строку не чинит продукт и не меняет его качество.
5. Доработка учитывает цену и использует единое время. Пересборка проверяет
   реальный результат, сохраняет оставшиеся проблемы. Обязательный блокер
   не закрывается кнопкой принятия риска. Готовая хорошая v0.1 допустима.
6. Верни данным финала статус требований, историю исправлений и принятые
   риски. Логику отображения Ending оставь Р4 в P-25.

ТЕСТЫ
Без обучения возникает только соответствующая проблема; с обучением её нет;
после исправления исчезает она, а другая остаётся; история неизменна;
разбор без изменения не чинит; одинаковая сборка не улучшает продукт;
недостаток средств; последняя допустимая сборка; старое решение не становится
причиной новой версии после замены; несколько причин; принятый риск/блокер.

ГРАНИЦЫ
ReviewSystem и тесты Р2, тестовые данные. Не менять UI и чужие состояния;
при необходимости уточнения контракта передать Р1 конкретную потребность.

ПРИЁМКА И ОТЧЁТ
Первый пример GAMEPLAY_REWORK доказан тестами на реальных системах.
Старые тесты искусственной критики заменены проверками новых правил.
DEVLOG, STATE, статус, DECISIONS. Следующую карточку не выполнять.
Коммит после подтверждения: feat(review): tie feedback to actual project issues
```

### P-22 (стар., заменена) Короткая глава с двумя разумными путями

**Тип.** контент + баланс · **Статус.** заменена 2026-10-24 · **Кто.** Р2 · **Модель.** gpt-6.1-sol · **Размышления.** high · **Зависит от.** P-21

```text
Выполни только P-22 из docs/PROMPTS.md. Прочитай общие правила P-18…P-26.
Модель: gpt-6.1-sol, effort high. Роль Р2, контент уровня.
Вход: CLAUDE, TEAM, STATE, GDD, DEMO, GAMEPLAY_REWORK, контракты,
бумажные маршруты и готовые правила P-19…P-21.

РЕЗУЛЬТАТ
Один компактный уровень, в котором все решения касаются одной создаваемой
игры. Две концепции дают различимые задачи и последствия.

СДЕЛАЙ
1. Оформи принятый бриф, вход в доску, реакцию на питч и первый вопрос команды.
   Игрок всегда получает следующую конкретную задачу.
2. Выбери 6–8 значимых решений. У каждого: связь с брифом/концепцией, варианты
   с реальным компромиссом, видимая цена/риск, результат сотрудника,
   изменение фактов проекта, возможное наблюдение плейтестера, исправление.
   Пустые подтверждения и юмор не считать отдельными значимыми решениями.
3. Для двух важных вопросов поменяй разумный выбор в зависимости от концепции.
   Не оставляй доминирующий дешёвый ответ без недостатков в каждом вопросе.
   Не показывай универсальную подсказку «это правильный ответ».
4. Сделай цепочку обучения первой доказательной. Для неё нужны три исхода:
   обучение сразу; экономия и последующее исправление; осознанный выпуск
   с допустимым недостатком. Тексты отзывов точно соответствуют фактам.
5. Все используемые картинки/схемы и факты результата имеют конкретные
   ссылки либо честные заглушки в списке арта. Обещанный героем файл/результат
   не исчезает в пустом сообщении. Сократи активный набор, архив не удаляй.
6. Пересчитай три маршрута реальными системами, без ручной подстановки
   финальных осей. Зафиксируй бюджет, дни, требования и проблемы на каждом
   шаге в docs/balance. Исходные 500000/18 дней – только отправная точка,
   сохраняй их лишь если расчёт подтверждает выбранный объём.

ПРОВЕРКА
Автоматическая проверка id/ссылок, достижимости обязательных сообщений,
совпадения цены с текстом, доступности исправлений; воспроизводимые проходы
двух концепций и неудачного маршрута через системы. Отдельно сохранение
после решения и после получения отзыва. Сбой чужой системы – handoff владельцу.

ГРАНИЦЫ
Data/Level1, docs/balance и тесты Р2. Не менять алгоритмы для маскировки
плохого баланса, чужой UI, сцены или пакеты.

ОТЧЁТ
Покажи одну полную причинную цепочку и чем два проекта реально различаются.
Не называй симуляцию прохождением интерфейса. DEVLOG, STATE, статус карточки.
Коммит после подтверждения: content: build a focused causal first chapter
```

### P-23 (стар., заменена) Доска: цель, выбор, смета и питч

**Тип.** UI · **Статус.** заменена 2026-10-24 · **Кто.** Р4 · **Модель.** gpt-6.1-sol · **Размышления.** high · **Зависит от.** P-22

```text
Выполни только P-23 из docs/PROMPTS.md. Прочитай общие правила P-18…P-26.
Модель: gpt-6.1-sol, effort high. Роль Р4, доска.
Вход: CLAUDE, TEAM, STATE, GDD, GAMEPLAY_REWORK, API P-20,
готовый контент P-22 и нынешний BoardPanel. Используй текущую палитру.

РЕЗУЛЬТАТ
На доске понятно, какую игру собираем, что осталось выбрать, какие риски
есть и что произойдёт после отправки.

СДЕЛАЙ
1. Покажи бриф и одну текущую цель; фиксированные условия не изображай
   свободными слотами. Обязательные выборы, дополнительные и справка различимы.
2. Для карточки показывай действие будущего игрока, пользу, риск и цену.
   Клик работает наравне с drag, выбранную карточку можно заменить/убрать.
3. Отобрази краткую концепцию и её изменения после каждого выбора.
   Качество и требования подписаны раздельно. Объяснение связи доступно
   по клику и не зависит только от красного/зелёного цвета.
4. Нитки визуализируют расчёт P-20; пользователь не может скрыть реальный
   конфликт их удалением. Одна понятная кнопка исследования показывает
   конкретный вопрос, цену и полученное знание.
5. Питч показывает смету и риски до подтверждения, объясняет блокировку.
   Повторный питч зовёт путь доработки и возвращает в релиз без лишнего дня.
6. Исправь прокрутку всей области руки, маску и доступность последнего ряда.
   Убери конкурирующие кнопки/заглушки, которые обещают отсутствующее действие.

ГРАНИЦЫ
Scripts/UI/Board, Prefabs/UI/Board и тесты своей области. Не собирать UI
из new GameObject/AddComponent в рантайме, не править правила и сцену Game.
Изменения верхней панели передать Р3 как точные требования для P-24.

ПРИЁМКА
Обычными кликами: неполная концепция → объяснение → полная → смета → питч;
две концепции, замена, исследование, конфликт без ручной нитки, повторный
питч. Проверить 1920×1080 и 1366×768. Записать что проверено вводом, что нет.
Снимки progress.py, DEVLOG, STATE, статус. UI не считать принятым за автора.
Коммит после подтверждения: feat(ui): guide concept choices and pitching
```

### P-24 (стар., заменена) Бриф, рабочие чаты и следующий шаг

**Тип.** UI · **Статус.** заменена 2026-10-24 · **Кто.** Р3 · **Модель.** gpt-6.1-sol · **Размышления.** high · **Зависит от.** P-23

```text
Выполни только P-24 из docs/PROMPTS.md. Прочитай общие правила P-18…P-26.
Модель: gpt-6.1-sol, effort high. Роль Р3.
Вход: CLAUDE, TEAM, STATE, GDD, GAMEPLAY_REWORK, состояние/доступные действия
P-19, контент P-22 и требования верхней панели из P-23.

РЕЗУЛЬТАТ
Игрок понимает поручение, актуальную задачу, цену ответа, результат и
следующий шаг; интерфейс не заставляет искать нужный чат наугад.

СДЕЛАЙ
1. Новая игра открывается с конкретного брифа и действия «Собрать концепцию».
   Короткие шаги введения используют реальный контент и сохраняются.
2. В верхней панели верни название текущего этапа и ближайшую цель.
   Навигация показывает доступность и причину блокировки из Core.
   После питча – реакция и первый вопрос; после результата – следующий шаг.
3. Различай непрочитанное сообщение и задачу, требующую решения.
   Информационное сообщение не перекрывает следующие варианты ответа.
   Пустое состояние объясняет ожидаемое событие и когда оно произойдёт.
4. В ответах видны стоимость и компромисс человеческим языком. Подсказка
   связывает риск с концепцией, но не помечает один ответ верным на все случаи.
   После ответа показывается квитанция и доступен полученный результат.
5. Настрой прокрутку к новому вопросу после явного перехода/ответа. Чтение
   старой истории не сбрасывается при фоновом обновлении. Панель ответов
   не скрывает вопрос, при её отсутствии освобождается место под историю.
6. Кнопка продвижения времени объясняет ближайшее событие и цену ожидания.
   Если есть неотвеченное обязательное решение, показывай понятное правило
   из Core. Не вводи собственные правила этапов в MonoBehaviour.
7. «Продолжить» восстанавливает нужный шаг и сообщения начальника о версиях
   из сохраняемых данных, не из потерянного runtime-клона ScriptableObject.

ГРАНИЦЫ
UI/Messenger, UI/Menu, их префабы и Prefabs/UI/Common по TEAM. Не менять
Board/Versions, правила Core и Game. Не делать большой ScreenRouter-рефакторинг
ради этой карточки; текущая навигация может использовать проверенный API Core.

ПРИЁМКА
Новая игра до первого ответа без объяснений автора; сообщение без вариантов
перед задачей; длинный чат; пропуск нескольких дней; недоступное действие;
продолжение после отзыва. Реальные клики, два размера экрана, ноль ошибок
консоли. DEVLOG/STATE/статус, progress.py. Ограничения проверок перечислить.
Коммит после подтверждения: feat(ui): make objectives and chat outcomes clear
```

### P-25 (стар., заменена) Продукт, версии, доработка и честный финал

**Тип.** UI + логика финала · **Статус.** заменена 2026-10-24 · **Кто.** Р4 · **Модель.** gpt-6.1-sol · **Размышления.** high · **Зависит от.** P-24

```text
Выполни только P-25 из docs/PROMPTS.md. Прочитай общие правила P-18…P-26.
Модель: gpt-6.1-sol, effort high. Роль Р4, версии и финал.
Вход: CLAUDE, TEAM, STATE, GDD, GAMEPLAY_REWORK, снимки/проблемы P-19/P-21,
контент P-22 и готовые переходы P-24.

РЕЗУЛЬТАТ
Виден продукт, который игрок создал, наблюдение плейтестера и эффект
доработки. Финал объясняет выполнение задания, а не только среднее число.

СДЕЛАЙ
1. В редактируемых префабах сделай блок «Наша игра»: концепция, герой,
   управление, обучение, главная фича. Поля берутся из реального состояния.
   Минимальная схема первого экрана показывает наличие/отсутствие подсказки.
   Подпиши её как схему, не выдавай за исполняемый вложенный билд.
2. Версия показывает снимок этих фактов, требования, оставшиеся проблемы
   и изменения относительно предыдущей версии. Историческая версия неизменна.
3. У отзыва видны наблюдение, переход к связанному решению, объяснение связи,
   стоимость и ожидаемый результат доработки. Раздели найденную причину,
   выполненную правку и подтверждение следующей проверкой.
4. Исправление и допустимое принятие риска – разные действия. Нельзя принять
   блокер. У недоступной доработки есть причина, у сборки – понятный результат.
   Успех и ошибка визуально различимы, но различие не только в цвете.
5. Переоткрытие доски и повторная отправка сохраняют контекст отзыва.
   Сборка не улучшает неизменный продукт. Интерфейс не рисует несуществующую
   v0.1 до первой сборки.
6. Финал: что выпущено, выполненные требования, 2–3 конкретных решения и их
   последствия, исправления, принятые риски, расходы. Verdict/Score согласуй
   с контрактом: провал обязательного требования не называется безусловным хитом.
   Вывод о профессии опирается на действия игрока. Для математики Ending – TDD.
7. Приведи версии и финал к текущей палитре, без редизайна остальных экранов.

ГРАНИЦЫ
UI/Versions, Prefabs/UI/Versions и тесты своей области. Правила проблем,
состояние Core, чужие префабы и Game не менять. Недостающие данные – handoff.

ПРИЁМКА
От новой игры обычными кнопками: отказ от обучения → результат без подсказки
→ соответствующий отзыв → найденная причина → оплаченная правка → подсказка
в новом снимке → исчезновение именно этой проблемы → объяснимый финал.
Ещё два прохода: обучение сразу (без ложного отзыва), допустимый риск принят.
Отдельно сохранить/продолжить внутри доработки. Не устанавливать stage/axes
через консоль для доказательства прохождения. DEVLOG/STATE/статус, progress.py.
Коммит после подтверждения: feat(ui): show project changes and meaningful feedback
```

### P-26 (стар., заменена) Проверка смысла, полного пути и браузерной сборки

**Тип.** интеграция + QA · **Статус.** заменена 2026-10-24 · **Кто.** Р1, QA · **Модель.** gpt-6.1-sol · **Размышления.** high · **Зависит от.** P-25

```text
Выполни только P-26 из docs/PROMPTS.md. Прочитай общие правила P-18…P-26.
Модель: gpt-6.1-sol, effort high. Роль Р1 для интеграции/сборки,
остальные области проверяешь без самостоятельных правок.
Вход: CLAUDE, TEAM, STATE, CASE, GDD, GAMEPLAY_REWORK, отчёты P-18…P-25,
таблица баланса и BUGS.

РЕЗУЛЬТАТ
Воспроизводимое доказательство, что цикл работает в браузере, и отдельный
протокол проверки понятности живыми игроками.

СДЕЛАЙ
1. Проверь интеграцию корневых префабов в Game и актуальность ContentDB.
   Исправляй только файлы Р1. Собери свежий локальный Web-билд и запиши
   commit плюс наличие/состав незакоммиченного diff, версию Unity и размер.
2. Пройди без подмены состояния: обе концепции, обучение с самого начала,
   ошибка и исправление, принятый риск, нехватка денег, последний день,
   повторный питч. В браузере проверяй обычным вводом, не вызовом методов.
3. Проверь F5/продолжение на трёх этапах и внутри доработки, историю версий,
   ответы начальника и отсутствие повторного списания. Затем короткий прогон
   всех сложностей, двух размеров экрана и доступного touch-устройства.
   Недоступные браузер/устройство обозначь как непроверенные.
4. Для каждого провала запиши воспроизведение, ожидание, факт, владельца
   и скриншот. Раздели непонятное правило, ввод/верстку, баланс и технический баг.
   Не исправляй всё одной большой задачей и не маскируй провал ослаблением теста.
5. Создай docs/playtests/gameplay-rework.md: инструкция трём новым игрокам,
   пустые поля наблюдений, вопросы после первых пяти минут и полного пути.
   Вопросы: какое поручение; зачем последний выбор; чем заплатил; почему
   появился отзыв; что нужно сделать дальше; чем отличается новая версия.
   Автор не объясняет управление во время наблюдения. Не выдумывать участников.
6. Подведи итог по требованиям CASE. Публикацию передай P-11, когда нет
   блокеров. Зелёные тесты отдельно от понятности, оценки людей отдельно от
   предположения агента. Если людей пока нет, оставь явный статус ожидания
   этой проверки, завершив всю доступную техническую часть.

ПРИЁМКА
Есть свежий билд и журнал полных сценариев; все блокеры имеют результат
исправления или явно оставлены открытыми. Не утверждай «игра понятна» без
наблюдений игроков. После исправлений повторять затронутые сценарии.

ОТЧЁТ
DEVLOG, STATE, BUGS, CASE и статус P-26 с точной степенью готовности.
При наличии изменений коммит после подтверждения:
test: verify coherent gameplay in web build
```
