# ROADMAP: план реализации демо

Основа: `GDD.md` (что за игра), `DEMO.md` (стек механик и уровень), макет UI – `docs/mockup/` (5 экранов PNG + HTML), кто что делает – `TEAM.md`. Карточки задач с полными промптами – `PROMPTS.md`.

Дедлайн хакатона: [ДАТА]. Оценки ниже – в рабочих днях одного человека.

## Принципы

- Сначала серый проход от брифа до финала, потом арт и контент. Пока серого прохода нет, арт рисуется в стол по макету, но не встраивается.
- Логика – чистый C# в сборке `Studio.Core` с EditMode-тестами. UI только читает `GameState` и зовёт методы систем. Так два разработчика работают параллельно по контрактам ниже, не мешая друг другу.
- Контент – ScriptableObject в `Assets/_Project/Data/`. Тексты правятся без кода.
- Одна карточка = одна ветка и один PR в `dev` (squash). Правила – `TEAM.md`.

## Контракты (общие для всех карточек)

Пространство имён `Studio.Core`. P-1 создаёт их все заглушками (`NotImplementedException`), UI пишется против них с первого дня. Меняет контракты только Р1.

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
QuoteSO   : id, axis, text, author
ContentDB : brief (MessageSO), cards, combos, messages, quotes, tagConflicts (пары строк), startBudget, deadline

// Состояние (сериализуется JsonUtility целиком)
GameState : nickname, difficulty, day, deadline, budget, stage,
            axes int[4], goals int[4], board (BoardState), log (List<DecisionRecord>),
            versions (List<VersionRecord>), inbox (List<string> id сообщений), answered (List<string>)
BoardState     : placed (List<SlotEntry {slot, cardId}>), links (List<Link {a, b}>), revealed (List<string> cardId)
DecisionRecord : messageId, optionId, stage, day, deltas int[4]
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
ReviewSystem.Review(GameState, ContentDB) -> VersionRecord
ReviewSystem.Blame(GameState, VersionRecord, string quoteId) -> string messageId   // виновное решение
ReviewSystem.ReopenCost(DecisionRecord) -> Cost
ReviewSystem.Reopen(GameState, DecisionRecord, OptionSO newOption, ContentDB)
Difficulty.For(DifficultyLevel) -> DifficultySettings { budgetMul, daysBonus, aiErrorChance, showThreads, reviewTolerance }
SaveSystem.Save(GameState) / Load() -> GameState / HasSave()
```

Если в работе контракт надо поменять – правка сначала здесь, отдельной строкой в `DECISIONS.md`, потом в коде.

## Вехи и задачи

| Веха | Р1 · Ядро | Р2 · Правила | Р3 · Чат | Р4 · Доска и версии | Дни |
|---|---|---|---|---|---|
| **M0. Каркас [x]** | P-1 (каркас + заглушки) | читает доки, черновик контента | читает доки, UI-кит по макету | читает доки, нитки-прототип | 0.5 |
| **M1. Этап 1 и 2 серые** | P-2 решения | P-3 доска | P-4 мессенджер | P-5 доска | 3 |
| **M2. Дни и ревью** | P-6 цикл дня | P-7 ревью | доводка P-4 под дни | P-8 версии и финал | 3 |
| **M3. Звёздочки** | P-9 сохранение, сложность | P-12 Нейрон (логика) | P-9 меню, P-12 панель | доводка P-8 | 2 |
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

- **Нитки на доске** – самая хитрая UI-часть. Если линия в uGUI не взлетит за день – рисуем нитку растянутым и повёрнутым `Image`.
- **Баланс чисел** – после серого прохода, таблицей в `docs/balance/`.
- **WebGL-вес** – держать билд до 30 МБ, сжатие Brotli + Decompression Fallback. Сборку проверить уже на M1, а не в конце.
- **Текста много** – каждое сообщение до 2 строк, ответ до 5 слов.
