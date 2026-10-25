using System.Collections.Generic;
using Studio.Core;
using UnityEditor;
using UnityEngine;

// One-shot generator of Data/Level0/Level0.asset (docs/MVP_RULES.md, DEMO.md). After the asset exists it is the
// source of truth and is edited in the inspector; rerunning overwrites inspector edits.
public static class Level0Builder
{
    public const string Path = "Assets/_Project/Data/Level0/Level0.asset";

    [MenuItem("Tools/Level0/Build asset")]
    public static void Build()
    {
        var l = AssetDatabase.LoadAssetAtPath<LevelSO>(Path);
        if (l == null)
        {
            if (!AssetDatabase.IsValidFolder("Assets/_Project/Data/Level0")) AssetDatabase.CreateFolder("Assets/_Project/Data", "Level0");
            l = ScriptableObject.CreateInstance<LevelSO>();
            AssetDatabase.CreateAsset(l, Path);
        }
        l.id = "level0";
        l.title = "0 · Обучение";
        l.briefText = "{name}, привет! У нас заказ от банка «Южный капитал»: игра на телефон для подростков 12+. Срок – {days} дня, бюджет – {budget}. Нужна оценка 70 или выше.\n\nСобери концепт на доске: поставь шесть блоков, выбери в каждом параметр и соедини их линиями. Зелёная линия – сочетание помогает игре.";
        l.deadlineDays = 3; l.startBudget = 100000; l.passScore = 70;
        l.tuning = new LevelTuning();
        l.requirements = new List<Requirement>
        {
            new Requirement { type = BlockType.Platform, paramId = "phone" },
            new Requirement { type = BlockType.Audience, paramId = "teen" },
        };
        l.parameters = new List<ParamDef>
        {
            P("platformer", BlockType.Genre, "Платформер"), P("puzzle", BlockType.Genre, "Головоломка"), P("horror", BlockType.Genre, "Хоррор"),
            P("kids", BlockType.Audience, "Дети 6+"), P("teen", BlockType.Audience, "Подростки 12+"), P("adult", BlockType.Audience, "Взрослые 18+"),
            P("space", BlockType.Setting, "Космос"), P("underwater", BlockType.Setting, "Подводный мир"), P("hospital", BlockType.Setting, "Заброшенная больница"),
            P("cartoon", BlockType.Style, "Мультяшный", 2), P("pixel", BlockType.Style, "Пиксельный", 2), P("realism", BlockType.Style, "Реализм", 3),
            P("jumps", BlockType.Mechanics, "Прыжки", 1), P("gravity", BlockType.Mechanics, "Гравитация", 2), P("craft", BlockType.Mechanics, "Крафт", 3),
            P("phone", BlockType.Platform, "Телефон"), P("pc", BlockType.Platform, "ПК"), P("console", BlockType.Platform, "Консоль"),
        };
        const Discipline D = Discipline.Design, M = Discipline.Mechanics, S = Discipline.Sound;
        // Rules exist only for linkable block types (ConceptRules.CanLink); the three paper routes are tuned on their chain links.
        l.pairRules = new List<PairRule>
        {
            // Genre - Audience
            R("horror", "kids", (M, -1)), R("horror", "teen", (S, 1), (D, -1)), R("horror", "adult", (S, 1), (D, 1)),
            R("platformer", "kids", (D, 1)), R("platformer", "teen", (D, -1)), R("platformer", "adult", (M, -1)),
            R("puzzle", "kids", (D, 1), (M, 1)), R("puzzle", "teen", (D, 1)), R("puzzle", "adult", (M, 1)),
            // Audience - Setting
            R("kids", "space", (D, 1)), R("kids", "underwater", (D, 1), (S, 1)), R("kids", "hospital", (S, 1)),
            R("teen", "space", (D, 1), (S, 1), (M, 1)), R("teen", "hospital", (S, 1), (D, -1)),
            R("adult", "space", (S, 1)), R("adult", "underwater", (D, -1)), R("adult", "hospital", (S, 1), (D, 1)),
            // Setting - Style
            R("space", "cartoon", (D, -1)), R("space", "pixel", (D, 1)), R("space", "realism", (D, 1), (S, 1)),
            R("underwater", "cartoon", (D, 1), (S, 1)), R("underwater", "pixel", (D, 1)),
            R("hospital", "cartoon", (D, -1)), R("hospital", "pixel", (S, 1)),
            // Style - Mechanics
            R("cartoon", "jumps", (M, 1)), R("cartoon", "gravity", (D, 1)), R("cartoon", "craft", (M, -1)),
            R("pixel", "jumps", (M, 1)), R("pixel", "gravity", (M, 1)), R("pixel", "craft", (M, 1), (D, 1)),
            R("realism", "jumps", (M, 1)), R("realism", "gravity", (D, 1)),
            // Mechanics - Platform
            R("jumps", "pc", (M, 1)), R("jumps", "console", (M, 1)),
            R("gravity", "phone", (M, 1)), R("gravity", "pc", (M, 1)), R("gravity", "console", (M, 1)),
            R("craft", "phone", (M, -1)), R("craft", "console", (M, 1), (D, 1)),
            // Genre - Mechanics
            R("platformer", "jumps", (M, 1)), R("platformer", "gravity", (M, 1), (D, 1)), R("platformer", "craft", (M, -1)),
            R("puzzle", "gravity", (M, 1)), R("puzzle", "craft", (M, -1)),
            R("horror", "gravity", (M, -1)), R("horror", "craft", (M, 1)),
            // Genre - Style
            R("platformer", "cartoon", (D, 1)), R("platformer", "pixel", (D, 1)), R("platformer", "realism", (D, -1)),
            R("puzzle", "pixel", (D, 1)),
            R("horror", "cartoon", (D, -1)), R("horror", "pixel", (S, 1)), R("horror", "realism", (D, 1), (S, 1)),
            // Audience - Platform
            R("kids", "phone", (D, 1), (M, 1)), R("kids", "pc", (D, -1)), R("kids", "console", (D, -1)),
            R("teen", "phone", (D, 1), (M, 1)), R("teen", "pc", (M, 1)), R("teen", "console", (D, 1)),
            R("adult", "phone", (D, -1)), R("adult", "pc", (M, 1)), R("adult", "console", (D, 1), (S, 1)),
        };
        l.tasks = new List<TaskTemplate>
        {
            new TaskTemplate { id = "art", d = D, titleFormat = "Арт: {Style}, {Setting}", tierFrom = BlockType.Style, tierFromBlock = true },
            new TaskTemplate { id = "mech", d = M, titleFormat = "Механика: {Mechanics} на {Platform}", tierFrom = BlockType.Mechanics, tierFromBlock = true },
            new TaskTemplate { id = "levels", d = M, titleFormat = "Уровни: {Genre}", fixedTier = 2 },
            new TaskTemplate { id = "sound", d = S, titleFormat = "Звук: {Setting}", fixedTier = 1 },
        };
        l.candidates = new List<CandidateDef>
        {
            C("lera", "Лера", D, 2, 3), C("vika", "Вика", D, 3, 1),
            C("kostya", "Костя", M, 2, 2), C("dima", "Дима", M, 1, 3),
            C("anya", "Аня", S, 2, 3), C("gosha", "Гоша", S, 1, 2),
        };
        l.events = Events();
        l.texts = new List<TextDef>
        {
            T("approved", "Концепт принят. Теперь хедхантер: найми людей на дизайн, механики и звук, потом раздай задачи в таск-листе."),
            T("morning", "Утро, день {day}. До дедлайна дней: {left}. Уставшие отдохнули – проверь задачи."),
            T("last_day", "Последний день! Собери игру до конца дня, иначе заказчику нечего показать."),
            T("passed", "«Южный капитал» принял игру. Уровень пройден!"),
            T("rework", "Ниже 70 – заказчик не примет. Сверь концепт с ТЗ и доработай слабый файл."),
            T("deadline_fail", "Срок вышел, а игры с оценкой 70+ нет. Уровень не пройден."),
            T("budget_fail", "Деньги кончились: платить команде нечем. Уровень не пройден."),
            T("hello_lera", "Привет, я Лера, художница. Рисую аккуратно, но не быстро."),
            T("hello_vika", "Хай! Вика, арт. Делаю очень быстро, по качеству – как получится."),
            T("hello_kostya", "Костя, программист. Средне по скорости и качеству."),
            T("hello_dima", "Дима, программист. Медленно, зато почти без багов."),
            T("hello_anya", "Аня, звук. Быстро и хорошо, но и стою недёшево."),
            T("hello_gosha", "Гоша, звук. Недорого, неспешно, нормально."),
        };
        EditorUtility.SetDirty(l);
        AssetDatabase.SaveAssets();
        Debug.Log("Level0 asset built: " + Path);
    }

    static ParamDef P(string id, BlockType t, string title, int tier = 0) => new ParamDef { id = id, type = t, title = title, tier = tier };
    static CandidateDef C(string id, string n, Discipline d, int sp, int q) => new CandidateDef { id = id, name = n, d = d, speed = sp, quality = q };
    // Rewrites only the event list: Build() would also reset candidates, portraits and every other inspector edit.
    [MenuItem("Tools/Level0/Rebuild events only")]
    public static void RebuildEvents()
    {
        var l = AssetDatabase.LoadAssetAtPath<LevelSO>(Path);
        l.events = Events();
        EditorUtility.SetDirty(l);
        AssetDatabase.SaveAssets();
        Debug.Log("Level0 events rebuilt: " + l.events.Count);
    }

    // docs/EVENTS.md describes the same list. Senders: an employee of discipline d, anyone, or the boss.
    static List<EventDef> Events()
    {
        const Discipline D = Discipline.Design, M = Discipline.Mechanics, S = Discipline.Sound;
        return new List<EventDef>
        {
            // Company-wide
            Ev("power", D, "Привет, слушай, подскажи, у нас вырубили электричество, что нам делать? Компы не работают, ну и мы тоже не можем.",
                O("Выходной", "Ну и славно. Отдыхаем, а завтра с новыми силами.", endDay: true),
                O("Коворкинг", "Супер, я уже собираю ноутбук. Там хоть розетки есть.", budget: -5000),
                O("Скоро починят", "Ждём тогда. Надеюсь, файлы сохранились...", failChance: 0.5f, failAll: -0.05f)),
            Ev("internet", "Сообщение от компании: беспилотная опасность! Интернета до завтра не будет. Возможно, полностью.",
                O("Работайте на мобильном", "Раздаём с телефонов. Медленно, но работаем.", speed: 0.5f, speedAll: true),
                O("Работайте без интернета", "Без сети так без сети. Только справочников под рукой нет.", speed: 0.25f, speedAll: true)),
            Ev("slop", M, "Это ужасно, наша игра – кусок сырого и вообще ни капли не проработанного нейрослопа! Ты попробуй диалоги почитать, я клянусь тебе, немой лучше скажет.",
                O("Я проверю работы команды", "Понял. Команда, всем показать, что уже сделано.", teamEnergy: -60, progressAll: 0.10f, disc: new[] { (D, -0.20f) })),
            // Artist
            Ev("art_detail", D, "Я всё поняла, но кое-что не поняла – какой у нас уровень детализации в прологе? Как в Last Night, Replaced и такое всё или попроще? Если так, то нужно сразу поговорить о доплате.",
                O("Да, сделай красиво", "Тогда рисую по максимуму. Пролог будет выглядеть как надо.", budget: -30000, progress: 0.15f),
                O("Нет, делаем всё «по-стандарту»", "Поняла, без излишеств.")),
            Ev("art_pics", D, "Охаёшечки-даттебаешечки! Сэмпай должен мне помочь выбрать. Я набросала разные вариантики, посмотри, что тут больше каваииии.",
                O("Просмотрю позже", "Хорошо, сэмпай! Тогда пока оставлю все три."),
                O("Выбери на своё усмотрение", "Сама выберу? Тогда третий, он самый каваииии!")),
            Ev("art_ai", D, "Я нагенерила пикч, но они слишком ИИ-шные. Давай я их просто зашакалю, испорчу и вставлю типо я сделала? Ок, да?",
                O("Ок, да", "Зашакалю так, что никто не отличит. Время сэкономлю.", progress: 0.10f),
                O("Нет/Нет", "Ладно... Нарисую всё сама, придётся начинать заново.", progress: -0.10f)),
            // Programmer
            MustEv(Ev("croak", M, "У меня проблема (не прикол). В проекте вирус, не могу убрать. Время от времени после билда я слышу в проекте чьё-то квакание. Это не критично, но мне кажется, рано или поздно это может начать надоедать.",
                O("Пусть квакает", "Ладно, пусть будет. Ква.", croak: true),
                O("Убирай немедленно", "Понял, лезу в логи. Это надолго.", energy: -20))),
            MustEv(Ev("yoda", M, "У меня снова проблема (и это снова не прикол). Я добавил для теста модельку Йоды из «Звёздных войн», но вместо зелёного чудища тут какой-то бородатый мужик. Пытаюсь удалить – недостаточно прав и смешной (как по мне) мем в логи про IT.", after: "croak",
                O("Кажется, я знаю, кто это (YODA остаётся)", "Ладно, пусть живёт. Бородатый Йода – тоже Йода."),
                O("Избавься от него", "Ещё раз и ещё раз... Всё, удалил. Силы ушли.", energy: -50))),
            // Musician
            Ev("fl", S, "Привет, у меня траблы небольшие – мне нужно fl'ку оплатить, чтоб работать, это 1,5к. Закинешь вечнозелёных?",
                O("Отказать", "Раскусил, да? Ладно, работаю на том, что есть.", progress: 0.10f),
                O("Ладно", "Спасибо! Ты попался в финансовую ловушку, но трек теперь пойдёт.", budget: -1500, progress: 0.10f)),
            Ev("half_track", S, "Ещё вопросик один обкашлять нужно – я не успеваю закончить трек, я чётко половину набросал.", after: "fl",
                O("Просто продублируй имеющееся", "Скопирую половину в зацикленном виде. Никто не заметит.", progress: 0.05f),
                O("Останься и доработай", "Эх... Остаюсь. Но это будет стоить денег и сил.", budget: -2000, energy: -30, progress: 0.20f)),
            // Writer
            Ev("writer1", M, "«Кусок нейрослопа». Сам пиши, если тебя что-то не устраивает. Либо я делаю свою работу так, как захочу, либо я её не делаю.", after: "slop",
                O("Занимайся. Делай", "Вот и славно. Сейчас будет шедевр.", progress: 0.15f),
                O("Я сказал «Нет»", "Ах так?! Хорошо, сделаю, но потом не жалуйся.", energy: -40, progress: 0.20f)),
            Ev("writer2", M, "Всё. На***. Я за******. Я не могу больше этим заниматься, я увольняюсь.", after: "writer1",
                O("Давай я просто буду больше тебе платить?", "Хм. Это другой разговор. Остаюсь.", rate: 100),
                O("Давай ты просто отдохнёшь?", "Наверное, ты прав. Сегодня просто поваляюсь.", speed: 0.5f),
                O("Ладно.", "Вот и поговорили. Всего хорошего.", fire: M)),
        };
    }

    static EventOption O(string label, string reply, float energy = 0, float progress = 0, int budget = 0, float teamEnergy = 0, float progressAll = 0,
        (Discipline d, float share)[] disc = null, float speed = 0, bool speedAll = false, float failChance = 0, float failAll = 0, int rate = 0,
        bool endDay = false, bool croak = false, Discipline? fire = null)
    {
        var o = new EventOption { label = label, reply = reply, energy = energy, progress = progress, budget = budget, teamEnergy = teamEnergy, progressAll = progressAll,
            speed = speed, speedAll = speedAll, failChance = failChance, failAll = failAll, rate = rate, endDay = endDay, croak = croak, fire = fire.HasValue ? (int)fire.Value : -1 };
        if (disc != null) foreach (var x in disc) o.disc.Add(new DiscShare { d = x.d, share = x.share });
        return o;
    }
    static EventDef Ev(string id, Discipline d, string text, params EventOption[] opts) => Ev(id, d, text, null, null, opts);
    static EventDef Ev(string id, Discipline d, string text, string after, params EventOption[] opts) => Ev(id, d, text, null, after, opts);
    static EventDef Ev(string id, Discipline d, string text, Discipline[] needs, params EventOption[] opts) => Ev(id, d, text, needs, null, opts);
    static EventDef Ev(string id, string text, params EventOption[] opts)   // from the boss
    {
        var e = Ev(id, Discipline.Design, text, null, null, opts);
        e.boss = true;
        return e;
    }
    static EventDef Ev(string id, Discipline d, string text, Discipline[] needs, string after, EventOption[] opts)
    {
        var e = new EventDef { id = id, d = d, text = text, after = after };
        if (needs != null) e.needs.AddRange(needs);
        e.options.AddRange(opts);
        return e;
    }
    static EventDef MustEv(EventDef e) { e.must = true; return e; }
    static TextDef T(string k, string t) => new TextDef { key = k, text = t };
    static PairRule R(string a, string b, params (Discipline d, int s)[] fx)
    {
        var r = new PairRule { a = a, b = b };
        foreach (var f in fx) r.effects.Add(new Effect { d = f.d, sign = f.s });
        return r;
    }
}
