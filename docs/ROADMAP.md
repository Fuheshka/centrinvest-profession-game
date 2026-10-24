# ROADMAP: план реализации

**Актуальный MVP (2026-10-24):** требования автора – [MVP_SPEC.md](MVP_SPEC.md), правила и числа – [MVP_RULES.md](MVP_RULES.md), карточки P-19…P-29 – [PROMPTS.md](PROMPTS.md#mvp-tutorial). Ниже: порядок работ MVP, **предлагаемые** контракты MVP (ещё не реализованы) и **существующий** API прототипа (устаревает в P-28). Вехи M0–M5 – история.

Основа прототипа: `GDD.md`, `DEMO.md`, макет UI – `docs/mockup/`, кто что делает – `TEAM.md`.

Дедлайн хакатона: [ДАТА]. Оценки – в рабочих днях одного человека.

## Запланировано: стикеры с креплениями и изогнутые нитки (2026-10-24)

Запрос автора: варианты жёлтого стикера с точками соединения, как в UI-макетах; нитки должны изгибаться в зависимости от расположения стикеров. Владелец реализации – Р4 (дэшборд). Статус: **запланировано**, поведение кривых в этой задаче не реализуется.

- Использовать отдельные спрайты стикеров с двумя боковыми, четырьмя краевыми либо четырьмя попарными боковыми креплениями: `Assets/_Project/Art/UI/GeneratedKit20261024/Stickers/`. Точки подключения вынести в дочерние `RectTransform` префаба, чтобы концы нитки попадали в центр видимого крепления.
- Нитку рисовать динамической плавной кривой, например кубической кривой Безье, а не одной прямой или растянутой заранее нарисованной дугой. Направление выхода определяется стороной крепления, изгиб и длина управляющих отрезков – взаимным положением и расстоянием между стикерами; допускаются естественная дуга и мягкий S-образный изгиб.
- При перетаскивании стикера, изменении масштаба или прокрутке доски пересчитывать кривую в координатах холста. Концы остаются на креплениях; смена подходящей стороны не должна вызывать дрожание около границы выбора. На коротких связях ограничивать изгиб, чтобы не возникали петли и изломы.
- Нитки размещать за карточками, сохраняя существующие цвета и правила связей. У кривой должна быть удобная область наведения/выбора по её фактической форме; удаление связи и подсветка продолжают работать.
- Проверка: карточки по горизонтали, вертикали, диагонали, близко друг к другу и по разные стороны; несколько связей одного стикера; перенос карточек друг через друга; масштабирование и прокрутка; два размера окна. Не должно быть отрыва от креплений, скачков и выхода нитки поверх текста карточки.

Спрайты и точные промпты – отдельный результат этой задачи; импорт и настройка префабов остаются этапом реализации. Не переносить в план удалённые механики из старых макетов.

## Порядок работ MVP (P-18, 2026-10-24)

| Карточка | Результат | Роль | Зависит от | Дни |
|---|---|---|---|---|
| P-18 | Правила, контракты, карточки (документы) | Р1 + документы | – | 0.5 |
| P-19 | Типы данных и состояние MVP, сохранение v2, заглушки систем | Р1 | P-18 | 1 |
| P-20 | Концепт: блоки, линии, бафы, утверждение, генерация задач | Р2 | P-19 | 1 |
| P-21 | Время, работа, энергия, ночь, оплата, файлы, исход по дедлайну | Р1 | P-19 | 1.5 |
| P-22 | Найм, сборка, оценка, отзыв, исправление, тупик бюджета | Р2 | P-20, P-21 | 1 |
| P-23 | Контент уровня 0 и проверка трёх маршрутов тестами | Р2 | P-22 | 1 |
| P-24 | Меню, верхняя панель с таймером, чаты из ленты состояния | Р3 | P-19 (данные – P-23) | 1.5 |
| P-25 | Окно хедхантера | Р3 | P-22, P-24 | 0.5 |
| P-26 | Дэшборд: свободный холст | Р4 | P-20 | 1.5 |
| P-27 | Таск-лист, сборка, отзыв, итог | Р4 | P-22, P-26 | 1.5 |
| P-28 | Интеграция сцены, игровой цикл, отключение прототипа, Web-сборка | Р1 | P-23…P-27 | 1 |
| P-29 | Проверка трёх маршрутов в браузере, сохранение, краевые случаи | Р1 + QA | P-28 | 1 |

Граф без циклов: P-19 → {P-20, P-21, P-24}; P-20 → {P-22, P-26}; P-21 → P-22; P-22 → {P-23, P-25, P-27}; P-24 → P-25; P-26 → P-27; P-23…P-27 → P-28 → P-29. UI-карточки (P-24…P-27) начинаются на заглушках P-19 и тестовых данных, как в M1.

Прежний план переделки (`GAMEPLAY_REWORK.md`, старые P-18…P-26) заменён; P-12, P-13, P-15, P-16 отложены. P-11 публикует результат после P-29.

## Принципы

- Сначала серый проход от брифа до финала, потом арт и контент. Пока серого прохода нет, арт рисуется в стол по макету, но не встраивается.
- Логика – чистый C# в сборке `Studio.Core` с EditMode-тестами. UI только читает `GameState` и зовёт методы систем. Так два разработчика работают параллельно по контрактам ниже, не мешая друг другу.
- Контент – ScriptableObject в `Assets/_Project/Data/`. Тексты правятся без кода.
- Одна карточка = одна ветка и один PR в `dev` (squash). Правила – `TEAM.md`.

## Контракты MVP (предлагаемые, реализуются с P-19)

**Не реализовано.** Сигнатуры ниже – проект P-18. P-19 создаёт типы и заглушки (`NotImplementedException("P-N")`), остальные карточки наполняют методы. Пространство имён `Studio.Core`, сборка `Studio.Core`. Правила – только в Core; UI читает `LevelState` и вызывает операции. Меняет контракты Р1, по просьбе владельца системы, со строкой в `DECISIONS.md`. Числа – в `LevelTuning` (значения по умолчанию – `MVP_RULES.md`, раздел 3).

Идентификаторы: строки из данных (`ParamDef.id`, `CandidateDef.id`, `TaskTemplate.id`); блоки концепта – `"b1"`, `"b2"`… по счётчику `ConceptState.nextId`; задача исправления – `"fix-<fileId>-<revision>"`; файл – id задачи концепта. Записи чата – по порядку в списке.

```csharp
// Enums.cs (добавить; старые enum'ы не трогать до P-28)
public enum BlockType { Genre, Audience, Setting, Style, Mechanics, Platform }
public enum Discipline { Design, Mechanics, Sound }        // дисциплина сотрудника, задачи и критерий оценки; индекс в int[3]
public enum NightMode { No, Yes, Owl }
public enum LevelStage { Concept, Production, Passed, Failed }
public enum FailReason { None, Deadline, NoBudget }
public enum LinkColor { Green, Red }
public enum WorkStatus { Open, Queued, Working, Waiting, Done }   // вычисляется, не хранится
public enum WaitReason { None, NoEnergy, NoBudget, NoAssignee, NotFirst }

// Данные (ScriptableObject + вложенные [Serializable]), Scripts/Core/Data/
LevelSO      : id, title, briefText ({name}, {days}, {budget}), deadlineDays, startBudget, passScore,
               requirements (List<Requirement {BlockType type; string paramId}>),
               parameters (List<ParamDef {id, type, title, int tier}>),        // tier: для Style/Mechanics уровень задачи, иначе 0
               pairRules (List<PairRule {string a, b; List<Effect {Discipline d; int sign}>}>),  // sign +1 / -1; порядок a,b не важен
               tasks (List<TaskTemplate {id, Discipline d, titleFormat, BlockType tierFrom, bool tierFromBlock, int fixedTier}>),
               candidates (List<CandidateDef {id, name, Discipline d, int speed, int quality, NightMode night, Sprite portrait}>),
               texts (List<TextDef {key, text}>),                            // реплики руководителя и сотрудников по ключам
               tuning (LevelTuning)
LevelTuning  : daySeconds=300, nightSeconds=150, maxTickSeconds=0.25, autosaveSeconds=10,
               drainPerSecond=0.4, idleRegenPerSecond=0.1, nightRest=50, nightDrainMul=1.5, nightQualityPenalty=10, nightPayShare=0.5,
               tierEnergy=[10,35,60], effortPerTier=150, qualityBase=40, qualityPerPoint=15, fatigueBelow=25, fatiguePenalty=15,
               ratePerPoint=10000, nightBonusYes=5000, nightBonusOwl=10000, teamLimit=4,
               conceptClamp=3, conceptStep=4, requirementPenalty=10, fixEffortShare=0.5

// Состояние (JsonUtility, только поля, List и массивы), Scripts/Core/State/
LevelState    : saveVersion=2, levelId, nickname, day, float dayElapsed, LevelStage stage, int budget, int deadline,
                ConceptState concept, List<EmployeeState> staff, List<TaskState> tasks, List<FileState> files,
                List<BuildRecord> builds, List<ChatEntry> chat, FailReason failReason, int score
ConceptState  : List<Block {id, BlockType type, paramId, float x, y}> blocks, List<ConceptLink {a, b}> links, bool approved, int nextId
EmployeeState : id (= CandidateDef.id), Discipline d, float energy, bool nightShift, int paidDay, int paidNightDay
TaskState     : id, Discipline d, title, int tier, float effort, float progress, float qualitySum,   // qualitySum = Σ качество × очки
                assigneeId, bool isFix, fileId, bool done                                            // порядок в списке = приоритет
FileState     : id, taskId, Discipline d, title, int revision, float quality, authorId, int day, bool night
BuildRecord   : int number, int day, List<BuiltFile {fileId, int revision, float quality}> files,
                int[] scores (3), int overall, List<string> unmetRequirements, bool passed
ChatEntry     : chatId ("boss" или CandidateDef.id), int day, string kind ("text"|"file"|"review"|"night"), string text, string refId
NightReport   : int day, List<string> lines, int spent                                              // не сохраняется

// Системы (статические), Scripts/Core/Systems/
LevelState.New(LevelSO, string nickname) -> LevelState                                   // P-19, Р1
ConceptRules.Place(LevelState, LevelSO, BlockType, float x, float y) -> string blockId   // null: тип уже есть или не этап Concept. P-20, Р2
ConceptRules.SetParam(LevelState, LevelSO, string blockId, string paramId) -> bool
ConceptRules.Move(LevelState, string blockId, float x, float y) -> bool
ConceptRules.Remove(LevelState, string blockId) -> bool                                   // удаляет и его линии
ConceptRules.Connect(LevelState, string a, string b) -> bool / Disconnect(...) -> bool
ConceptRules.PairEffects(LevelSO, string paramA, string paramB) -> List<Effect>
ConceptRules.LinkColor(LevelState, LevelSO, string a, string b) -> LinkColor              // Green только при плюсах > минусов
ConceptRules.Totals(LevelState, LevelSO) -> int[3]                                       // по всем парам, зажато ±conceptClamp
ConceptRules.CanApprove(LevelState, LevelSO, out string reason) -> bool
ConceptRules.Approve(LevelState, LevelSO) -> bool                                         // задачи по шаблонам, stage = Production
Staffing.DailyRate(CandidateDef, LevelTuning) -> int                                     // P-22, Р2
Staffing.CanHire(LevelState, LevelSO, string candidateId, out string reason) -> bool
Staffing.Hire(LevelState, LevelSO, string candidateId) -> bool                           // энергия 100, приветствие в чат
Staffing.SetNightShift(LevelState, LevelSO, string employeeId, bool on) -> bool
WorkSim.Assign(LevelState, string taskId, string employeeId) -> bool                     // P-21, Р1
WorkSim.SetPriority(LevelState, string taskId, int index) -> bool
WorkSim.Status(LevelState, LevelSO, string taskId, out WaitReason why) -> WorkStatus
WorkSim.Tick(LevelState, LevelSO, float dt) -> NightReport                               // не null, если в этом тике кончился день
WorkSim.EndDay(LevelState, LevelSO, int expectedDay) -> NightReport                      // null: не тот день или не этап Production
Review.CanBuild(LevelState, LevelSO, out string reason) -> bool                          // P-22, Р2
Review.Build(LevelState, LevelSO) -> BuildRecord                                         // 70+ -> Passed
Review.Score(LevelState, LevelSO) -> BuildRecord                                         // расчёт без записи, для тестов и подсказок
Review.CanFix(LevelState, LevelSO, string fileId, string employeeId, out string reason) -> bool
Review.SendToFix(LevelState, LevelSO, string fileId, string employeeId) -> bool          // задача исправления первой
Review.CheckBudgetDeadlock(LevelState, LevelSO) -> bool                                  // true -> Failed(NoBudget); зовёт GameLoop сразу после EndDay
SaveSystem.SaveLevel(LevelState) / LoadLevel() -> LevelState / HasLevelSave() / DeleteLevel()   // ключ save_v2; P-19, Р1
```

События наружу не публикуются: UI после каждой операции и тика перерисовывается по `LevelState`. Новые записи `chat` – источник уведомлений (непрочитанные считает UI по индексу последней просмотренной записи, индекс хранится в UI-настройках, не в сохранении).

Владельцы: типы `Enums`, `Data/LevelSO`, `State/Level*`, `WorkSim`, `SaveSystem` – Р1; `ConceptRules`, `Staffing`, `Review` – Р2; UI – см. `TEAM.md`.

## Существующий API прототипа (реализован, устаревает в P-28)

Пространство имён `Studio.Core`. P-1 создал их заглушками, P-2…P-17 наполнили. Новые карточки не расширяют этот API; P-28 отключает его использование и удаляет устаревшее по списку.

```csharp
public enum Axis { Interest, Coherence, Clarity, Tech }          // индекс в массивах int[4]
public enum Stage { Idea, Production, Release, Done }
public enum Slot { Genre, Setting, Audience, Mechanic, Hook, Reference }
public enum Sender { Boss, Artist, Programmer, Producer, Nephew, Ai, Team }
public enum ThreadKind { Unknown, Match, Conflict, Neutral }
public enum DifficultyLevel { Intern, Designer, Lead }

[Serializable] public struct Cost { public int days; public int budget; }

// Данные (ScriptableObject)
CardSO    : id, slot, title, tags[], productionCost (Cost), goalDeltas int[4], popularity (0–100)
ComboSO   : a, b (CardSO), kind (Match | Conflict), title, deltas int[4]
OptionSO  : id, text, cost, deltas int[4], tags[], followUp (MessageSO), followUpDelayDays
MessageSO : id, sender, stage, day, text, images (Sprite[]), options (OptionSO[])
QuoteSO   : id, axis, text, author, praise (похвала оси на цели и выше)
ContentDB : brief (MessageSO), cards, combos, messages, quotes, tagConflicts (пары строк), startBudget, deadline, releaseStartDay (0 = день первого сообщения Release)

// Состояние (сериализуется JsonUtility целиком)
GameState : nickname, difficulty, day, deadline, budget, stage,
            axes int[4], goals int[4], board (BoardState), log (List<DecisionRecord>),
            versions (List<VersionRecord>), inbox (List<string> id сообщений), answered (List<string>),
            pending (List<Pending {messageId, day}>)
BoardState     : placed (List<SlotEntry {slot, cardId}>), links (List<Link {a, b}>), revealed (List<string> cardId)
DecisionRecord : messageId, optionId, stage, day, deltas int[4], cost (Cost варианта), replaced (переоткрыто, есть запись новее)
VersionRecord  : number, day, axes int[4], approved, quoteIds (List<string>), blamed (List<QuoteBlame {quoteId, messageId}>)

// Системы (чистый C#)
DecisionResolver.CanAfford(GameState, OptionSO) -> bool
DecisionResolver.Apply(GameState, MessageSO, OptionSO, ContentDB) -> DecisionRecord
BoardRules.Place(GameState, CardSO) / Remove(GameState, Slot)       // замена в слоте стирает связи старой карточки
BoardRules.Connect(GameState, string cardIdA, string cardIdB) -> bool  // false: дубль, связь с собой, карточки нет на доске
BoardRules.Disconnect(GameState, string cardIdA, string cardIdB) -> bool
BoardRules.Thread(CardSO a, CardSO b, ContentDB, BoardState, bool showThreads) -> ThreadKind   // showThreads = DifficultySettings.showThreads
BoardRules.Reveal(GameState, string cardId) / Research(GameState, string cardId, Cost)        // Research списывает только budget, InvalidOperationException без денег
BoardRules.Goals(BoardState, ContentDB) -> int[4]
BoardRules.Commit(GameState, ContentDB)                                // конец этапа 1: goals, эффекты связанных комбо в axes, stage = Production
DayClock.EndDay(GameState, ContentDB) -> List<MessageSO>      // новые сообщения на утро
DayClock.IsOverDeadline(GameState) -> bool                     // day > deadline
ReviewSystem.Review(GameState, ContentDB, int tolerance = 0) -> VersionRecord   // tolerance = DifficultySettings.reviewTolerance; сборка = 1 день
ReviewSystem.Blame(GameState, VersionRecord, string quoteId, ContentDB) -> string messageId   // виновное решение; ReviewSystem.BoardId = "board" для доски и решений этапа Idea
ReviewSystem.CheckBlame(GameState, VersionRecord, string quoteId, string guessedMessageId, ContentDB) -> bool   // угадал – version.blamed, нет – day += 1
ReviewSystem.BoardRecord(GameState, ContentDB) -> DecisionRecord   // зафиксированная доска как запись, для Reopen доски
ReviewSystem.ReopenCost(DecisionRecord) -> Cost                    // cost × 2 для Idea, × 1 для Production, минимум 1 день; доска = 2 дня
ReviewSystem.Reopen(GameState, DecisionRecord, OptionSO newOption, ContentDB)   // откат deltas, цена, replaced = true; доска – stage = Idea
ReviewSystem.Recommit(GameState, ContentDB)                        // питч переоткрытой доски: Commit, затем stage = Release
Difficulty.For(DifficultyLevel) -> DifficultySettings { budgetMul, daysBonus, aiErrorChance, showThreads, reviewTolerance }
SaveSystem.Save(GameState) / Load() -> GameState / HasSave()
```

Если в работе контракт надо поменять – правка сначала здесь, отдельной строкой в `DECISIONS.md`, потом в коде.

## Вехи и задачи (история прототипа)

| Веха | Р1 · Ядро | Р2 · Правила | Р3 · Чат | Р4 · Доска и версии | Дни |
|---|---|---|---|---|---|
| **M0. Каркас [x]** | P-1 (каркас + заглушки) | читает доки, черновик контента | читает доки, UI-кит по макету | читает доки, нитки-прототип | 0.5 |
| **M1. Этап 1 и 2 серые** | P-2 решения | P-3 доска | P-4 мессенджер | P-5 доска | 3 |
| **M2. Дни и ревью** | P-6 цикл дня | P-7 ревью | доводка P-4 под дни | P-8 версии и финал | 3 |
| **M3. Звёздочки** | P-9 сохранение, сложность | P-12 Нейрон (логика) | P-9 меню, P-12 панель | доводка P-8 | 2 |
| **M3b. Оболочка игры** | P-14 новый UI [x] | – | P-15 главное меню как в PC-игре, P-16 пауза и настройки | – | 2 |
| **Серый проход** | ведёт сборку | чинит своё | чинит своё | чинит своё | 1 |
| **M4. Контент и арт** | Web-сборка на каждый мерж | P-10 контент | встраивает арт чата | встраивает арт доски | 3 |
| **M5. Сборка и тест** | P-11 выкладка | баланс | тест на 3 сложностях | тест на 3 сложностях | 2 |

Художники (если есть в команде), параллельно с M0–M3:
- **A-1 UI-кит** (художник 1): панели, кнопки, пузыри сообщений, вкладки, верхняя панель, иконки ресурсов. По макету экранов Мессенджер и Версии.
- **A-2 Доска** (художник 1): пробковая доска, карточка-шаблон, 6 иконок слотов, кнопки-булавки, 3 вида нитки (сплошная, двойная, пунктир).
- **A-3 Персонажи** (художник 2): 6 аватаров (Борис Петрович, Лера, Костя, Марина, Гоша, Нейрон), 3 варианта героя от Леры (мультяшный кот-рыцарь, мрачный странник, пиксельный робот).
- **A-4 Сцены** (художник 2): фон меню (вечерний офис), арт для 4 концовок.

Формат арта: PNG, UI в 2x к макету (макет 1440×810, игра 1920×1080), без текста на картинках – текст рисует Unity.

## Риски

- **Нитки на доске** – самая хитрая UI-часть. Актуальное требование автора (2026-10-24) – адаптивные кривые между креплениями стикеров. Запасной способ рендеринга – сегменты или UI-mesh вдоль вычисленной кривой; одна прямая растянутая `Image` требование не закрывает.
- **Баланс чисел** – после серого прохода, таблицей в `docs/balance/`.
- **WebGL-вес** – держать билд до 30 МБ, сжатие Brotli + Decompression Fallback. Сборку проверить уже на M1, а не в конце.
- **Текста много** – каждое сообщение до 2 строк, ответ до 5 слов.
