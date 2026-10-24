# PROMPTS: карточки задач

Каждая карточка – готовый промпт по шаблону `.agent/skills/task-prompt/SKILL.md` (переведены: P-1; остальные – в старом коротком формате, переводятся перед стартом) для отдельного чата с агентом (Claude Code в этой репе). Скопировать блок целиком. Под заголовком строка статуса, обновлять вместе с галочкой в `ROADMAP.md`.

Кто что делает и правила git – `TEAM.md`. Макеты экранов – `docs/mockup/*.png`. Промпты подходят для любой нейронки: агент в репе читает файлы сам, в браузерный чат – приложить файлы (см. `TEAM.md`, «Нейронки»).

Общее для всех карточек (уже внутри каждого промпта): прочитать `CLAUDE.md`, `docs/TEAM.md`, `docs/STATE.md`, `docs/ROADMAP.md` (раздел «Контракты»), `docs/DEMO.md`; своя ветка от `dev` и PR; коммит только после подтверждения автора; запись в `docs/DEVLOG.md` и обновление `docs/STATE.md` в том же коммите.

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

**Тип.** код, TDD · **Статус.** не сделано · **Кто.** Р1 · Ядро · **Модель.** Sonnet, effort high · **Зависит от.** P-2, P-3

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

**Тип.** код, TDD · **Статус.** не сделано · **Кто.** Р2 · Правила · **Модель.** Opus, effort high (ядро уникальности, нужна аккуратность) · **Зависит от.** P-6

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

**Тип.** UI · **Статус.** не сделано · **Кто.** Р4 · Доска и версии · **Модель.** Sonnet, effort medium · **Зависит от.** P-7 (можно заглушками)

```
Задача P-8 из docs/PROMPTS.md: вкладка «Версии» и финальный экран.

Прочитай CLAUDE.md, docs/TEAM.md (свои папки – только их и правишь), docs/STATE.md, docs/ROADMAP.md, docs/GDD.md (этап 3, финал). Макеты: docs/mockup/3-versions.png и docs/mockup/4-ending.png. Ветка от dev: p-N-<коротко>, в конце PR в dev.

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

## P-9 Меню, сохранение, сложность

**Тип.** код + UI · **Статус.** не сделано · **Кто.** Р1 · Ядро (логика) + Р3 · Чат (меню) · **Модель.** Sonnet, effort medium · **Зависит от.** P-6

```
Задача P-9 из docs/PROMPTS.md: меню, автосохранение, уровни сложности (задачи со звёздочкой кейса).

Прочитай CLAUDE.md, docs/TEAM.md (свои папки – только их и правишь), docs/STATE.md, docs/ROADMAP.md («Контракты»), docs/GDD.md («Сложность», «Сохранение»), docs/CASE.md. Макет: docs/mockup/0-main.png. Ветка от dev: p-N-<коротко>, в конце PR в dev. TDD для Difficulty и SaveSystem.

Сделай:
1. Difficulty.For(level) -> DifficultySettings по таблице GDD: Intern {budgetMul 1.3, daysBonus 3, aiErrorChance 0.05, showThreads true, reviewTolerance 10}, Designer {1.0, 0, 0.2, false, 5}, Lead {0.8, -2, 0.4, false, 0}. GameState.New применяет budgetMul и daysBonus. Подставь showThreads в вызовы BoardRules.Thread и reviewTolerance в ReviewSystem.Review.
2. SaveSystem: JsonUtility -> PlayerPrefs ключ "save_v1" (в WebGL это IndexedDB, переживает перезагрузку). Save после каждого EndDay, после Review и после Commit доски. Load возвращает null при битом JSON (лог, без исключения). HasSave, Delete. Поле saveVersion = 1 в GameState.
3. Сцена Menu: заголовок, поле никнейма (до 16 символов, пустое – «Стажёр»), 3 кнопки сложности с подсказками, «Выйти на работу» (новая игра, старое сохранение удаляется после подтверждения, если есть), «Продолжить · день N» (только если HasSave).
4. Никнейм подставляется в тексты сообщений вместо {name}.
5. Тесты: значения Difficulty; GameState.New с множителями; Save/Load round-trip; битый JSON -> null.

Не трогай: BoardPanel, ChatView, Versions UI.

Готово, когда: тесты зелёные; в Play: начать игру, пройти 2 дня, перезапустить Play, «Продолжить» возвращает на тот же день с теми же решениями.

В конце: DEVLOG, STATE, статус P-9, CASE.md – отметить сохранение и сложность, коммит «feat: menu, autosave and difficulty levels» – жди подтверждения.
```

---

## P-10 Контент уровня «Первый релиз»

**Тип.** контент · **Статус.** не сделано · **Кто.** Р2 · Правила + геймдизайнер команды · **Модель.** Opus, effort medium (тексты и баланс связей) · **Зависит от.** серый проход (P-1…P-9)

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
3. UI: правая панель мессенджера оживает – «Спросить Нейрона» по текущему сообщению (ответ в чате Нейрона и в панели, с припиской «Нейрон иногда ошибается»), «Запустить плейтест».
4. Тесты: при aiErrorChance 0 совет совпадает с лучшим вариантом; при 1 – никогда не совпадает (если вариантов > 1); Playtest пороги. Random передаётся параметром – тесты детерминированные.

Не трогай: ReviewSystem, BoardRules.

Готово, когда: тесты зелёные, в Play Нейрон советует, на Lead заметно ошибается.

В конце: DEVLOG, STATE, статус P-12, коммит «feat: ai advisor and playtest analytics» – жди подтверждения.
```
