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
        var packParameters = new[]
        {
            P("sixteen", BlockType.Audience, "Подростки 16+"),
            P("dark_fantasy", BlockType.Setting, "Тёмное фэнтези"),
            P("psych_hospital", BlockType.Setting, "Психбольница"),
            P("cozy_village", BlockType.Setting, "Уютная деревня"),
            P("abstract", BlockType.Setting, "Абстракция"),
            P("gothic_fantasy", BlockType.Setting, "Готика / фэнтези"),
            P("any_setting", BlockType.Setting, "Любой сеттинг"),
            P("modern", BlockType.Setting, "Современность"),
            P("school_romance", BlockType.Setting, "Школа / романтика"),
            P("gothic_farm", BlockType.Setting, "Готика (вампир-фермер)"),
            P("abandoned", BlockType.Setting, "Заброшенный город"),
            P("abandoned_place", BlockType.Setting, "Заброшка"),
            P("fantasy", BlockType.Setting, "Фэнтези"),
            P("history_fantasy", BlockType.Setting, "История / фэнтези"),
            P("post_apocalypse", BlockType.Setting, "Постапокалипсис"),
            P("festive", BlockType.Setting, "Весёлый мир"),
            P("modern_history", BlockType.Setting, "Современность / история"),
            P("real_world", BlockType.Setting, "Реальный мир"),
            P("fairy_tale", BlockType.Setting, "Сказка"),
            P("bright_casual", BlockType.Style, "Яркий казуал", 1),
            P("drawn_2d", BlockType.Style, "Рисованный 2D", 2),
            P("illustrations", BlockType.Style, "Иллюстрации", 2),
            P("isometric", BlockType.Style, "Изометрия", 2),
            P("anime", BlockType.Style, "Аниме", 2),
            P("stylized", BlockType.Style, "Стилизованный", 2),
            P("simple", BlockType.Style, "Простой", 1),
            P("realism_stylized", BlockType.Style, "Реализм / стилизация", 3),
            P("ui_realism", BlockType.Style, "UI / реализм", 2),
            P("bright_2d", BlockType.Style, "Яркий 2D", 1),
            P("stealth_explore", BlockType.Mechanics, "Стелс и исследование", 2),
            P("combat", BlockType.Mechanics, "Боевая система", 2),
            P("craft_farm", BlockType.Mechanics, "Крафт и ферма", 2),
            P("match3", BlockType.Mechanics, "Match-3", 1),
            P("hard_combat_stats", BlockType.Mechanics, "Сложный бой и статы", 3),
            P("explore_upgrades", BlockType.Mechanics, "Исследование и апгрейды", 3),
            P("deck_random", BlockType.Mechanics, "Deckbuilding и рандом", 2),
            P("pvp_loot", BlockType.Mechanics, "PvP и лут", 3),
            P("resource_management", BlockType.Mechanics, "Ресурсы и менеджмент", 3),
            P("choices_branching", BlockType.Mechanics, "Выборы и ветвление", 2),
            P("coop_hide", BlockType.Mechanics, "Кооп и прятки", 3),
            P("jumps_physics", BlockType.Mechanics, "Прыжки и физика", 2),
            P("army_control", BlockType.Mechanics, "Управление войсками", 3),
            P("automation", BlockType.Mechanics, "Автоматизация", 1),
            P("craft_build", BlockType.Mechanics, "Крафт и строительство", 3),
            P("minigames_rounds", BlockType.Mechanics, "Мини-игры и раунды", 2),
            P("tactic_hide", BlockType.Mechanics, "Тактика и прятки", 2),
            P("arcade_tracks", BlockType.Mechanics, "Аркада и трассы", 2),
            P("management_stats", BlockType.Mechanics, "Менеджмент и статистика", 2),
            P("physics_puzzle", BlockType.Mechanics, "Физика и головоломки", 2),
            P("solo_story", BlockType.Mechanics, "Одиночный сюжет", 2),
            P("pc_console", BlockType.Platform, "ПК / консоль"),
            P("pc_phone", BlockType.Platform, "ПК / телефон"),
            P("phone_switch", BlockType.Platform, "Телефон / Switch"),
            P("console_phone", BlockType.Platform, "Консоль / телефон"),
            P("action", BlockType.Genre, "Экшен"),
            P("simulator", BlockType.Genre, "Симулятор"),
            P("rpg_action", BlockType.Genre, "RPG / экшен"),
            P("card_strategy", BlockType.Genre, "Карточная стратегия"),
            P("shooter", BlockType.Genre, "Шутер"),
            P("strategy", BlockType.Genre, "Стратегия"),
            P("visual_novel", BlockType.Genre, "Визуальная новелла"),
            P("idle", BlockType.Genre, "Idle"),
            P("survival", BlockType.Genre, "Выживание"),
            P("party", BlockType.Genre, "Party"),
            P("stealth_action", BlockType.Genre, "Стелс-экшен"),
            P("racing", BlockType.Genre, "Гонки"),
            P("sports_sim", BlockType.Genre, "Спорт / симулятор"),
        };
        foreach (var parameter in packParameters) parameter.deriveFromPack = true;
        l.parameters.AddRange(packParameters);
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
            // Explicit conflicts from the card catalogue. Other new pairs are derived from cardPacks.
            R("phone", "resource_management", (M, -1)),
            R("visual_novel", "pvp_loot", (M, -1)),
            R("idle", "realism", (D, -1)),
            R("pixel", "adult", (D, -1)),
            R("party", "solo_story", (M, -1)),
        };
        l.cardPacks = new List<CardPackDef>
        {
            Pack("psych_horror", "Психологический хоррор", "horror", "adult", "psych_hospital", "realism", "stealth_explore", "pc"),
            Pack("slasher", "Слэшер", "action", "adult", "dark_fantasy", "realism", "combat", "pc_console"),
            Pack("cozy_farm", "Коззи-ферма", "simulator", "kids", "cozy_village", "cartoon", "craft_farm", "pc_phone"),
            Pack("mobile_puzzle", "Мобильная головоломка", "puzzle", "kids", "abstract", "bright_casual", "match3", "phone"),
            Pack("soulslike", "Соулслайк", "rpg_action", "adult", "dark_fantasy", "realism", "hard_combat_stats", "pc_console"),
            Pack("metroidvania", "Метроидвания", "platformer", "teen", "gothic_fantasy", "drawn_2d", "explore_upgrades", "pc_console"),
            Pack("roguelike_deckbuilder", "Рогалик-декбилдер", "card_strategy", "teen", "any_setting", "illustrations", "deck_random", "pc_phone"),
            Pack("battle_royale", "Королевская битва", "shooter", "sixteen", "modern", "realism", "pvp_loot", "pc_phone"),
            Pack("tycoon", "Тайкун-менеджер", "strategy", "teen", "modern", "isometric", "resource_management", "pc"),
            Pack("visual_novel", "Визуальная новелла", "visual_novel", "sixteen", "school_romance", "anime", "choices_branching", "pc_phone"),
            Pack("cozy_twist", "Коззи с твистом", "simulator", "teen", "gothic_farm", "stylized", "craft_farm", "pc"),
            Pack("coop_horror", "Кооп-хоррор", "horror", "sixteen", "abandoned_place", "realism", "coop_hide", "pc"),
            Pack("pixel_platformer", "Пиксельный платформер", "platformer", "kids", "fantasy", "pixel", "jumps_physics", "pc_phone"),
            Pack("rts", "RTS", "strategy", "teen", "history_fantasy", "realism", "army_control", "pc"),
            Pack("idle_clicker", "Idle / кликер", "idle", "kids", "any_setting", "simple", "automation", "phone"),
            Pack("sandbox_survival", "Песочница-выживание", "survival", "teen", "post_apocalypse", "realism", "craft_build", "pc"),
            Pack("party_minigames", "Партийные мини-игры", "party", "kids", "festive", "cartoon", "minigames_rounds", "phone_switch"),
            Pack("stealth_action", "Стелс-экшен", "stealth_action", "sixteen", "modern_history", "realism", "tactic_hide", "pc"),
            Pack("arcade_racing", "Гонки-аркада", "racing", "kids", "real_world", "realism_stylized", "arcade_tracks", "console_phone"),
            Pack("sports_manager", "Спортивный менеджер", "sports_sim", "teen", "real_world", "ui_realism", "management_stats", "pc_phone"),
            Pack("coop_horror_city", "Кооп-хоррор: заброшенный город", "horror", "adult", "abandoned", "realism", "coop_hide", "pc"),
            Pack("fairy_physics_puzzle", "Платформер-головоломка", "puzzle", "kids", "fairy_tale", "bright_2d", "physics_puzzle", "pc_phone"),
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
    static CardPackDef Pack(string id, string title, string genre, string audience, string setting, string style, string mechanics, string platform) =>
        new CardPackDef { id = id, title = title, genre = genre, audience = audience, setting = setting, style = style, mechanics = mechanics, platform = platform };
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
