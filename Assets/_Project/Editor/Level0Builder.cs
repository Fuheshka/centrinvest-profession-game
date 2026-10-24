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
        l.events = new List<EventDef>
        {
            Ev("ref", D, false, "Нашёл(ла) крутой референс, но придётся пересобрать часть арта. Пересобираем?", ("Пересобрать", "Принято, пересобираю. Будет красивее.", -10, 0.12f, 0), ("Не надо", "Ок, остаёмся на текущем.", 3, 0, 0)),
            Ev("pack", D, false, "Затык на детали. Можно купить готовый пак ассетов за 2 000, а можно нарисовать самому.", ("Купить пак", "Беру пак, сэкономлю время.", 0, 0.15f, -2000), ("Рисуй сам(а)", "Нарисую, но это займёт силы.", -8, 0.05f, 0)),
            Ev("bug", M, false, "Нашёл(ла) баг в старом коде. Чинить сейчас?", ("Чинить", "Починил(а). Теперь спокойнее.", -6, 0.08f, 0), ("Потом", "Ладно, оставлю на потом.", 0, 0.04f, 0)),
            Ev("physics", M, false, "Предлагаю упростить физику: быстрее, но менее красиво.", ("Упростить", "Упростил(а), дело пошло.", 5, 0.2f, 0), ("Оставить", "Оставляем как задумано.", 0, 0, 0)),
            Ev("track", S, false, "Есть классный трек у знакомого композитора, 3 000. Берём?", ("Купить трек", "Беру трек, ставлю в игру.", 0, 0.2f, -3000), ("Сделаю сам(а)", "Сделаю сам(а), выйдет дешевле.", -6, 0.06f, 0)),
            Ev("headache", S, false, "Голова гудит от наушников. Нужен перерыв?", ("Отдохни", "Спасибо, схожу на воздух.", 12, 0, 0), ("Дожми", "Ладно, дожму.", -8, 0.08f, 0)),
            Ev("coffee", D, true, "Устал(а). Можно кофе за счёт студии, 500.", ("Кофе", "Спасибо! Бодрее.", 10, 0, -500), ("Терпи", "Хорошо, потерплю.", -5, 0.05f, 0)),
            Ev("noise", D, true, "В офисе шумно. Надеть наушники и ускориться или позвать всех на обед за 1 000?", ("Наушники", "Надел(а), работаю.", 0, 0.06f, 0), ("Обед", "Все довольны.", 8, 0, -1000)),
        };
        l.texts = new List<TextDef>
        {
            T("approved", "Концепт принят. Теперь хедхантер: найми людей на дизайн, механики и звук, потом раздай задачи в таск-листе. С этой минуты пошёл рабочий день – 5 минут."),
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
    static EventDef Ev(string id, Discipline d, bool anyone, string text, params (string label, string reply, float energy, float progress, int budget)[] opts)
    {
        var e = new EventDef { id = id, d = d, anyone = anyone, text = text };
        foreach (var o in opts) e.options.Add(new EventOption { label = o.label, reply = o.reply, energy = o.energy, progress = o.progress, budget = o.budget });
        return e;
    }
    static TextDef T(string k, string t) => new TextDef { key = k, text = t };
    static PairRule R(string a, string b, params (Discipline d, int s)[] fx)
    {
        var r = new PairRule { a = a, b = b };
        foreach (var f in fx) r.effects.Add(new Effect { d = f.d, sign = f.s });
        return r;
    }
}
