using Studio.Core;

namespace Studio.UI
{
    // What the Menu scene hands to the Game scene. Empty when Game is opened directly in the Editor.
    public static class Session
    {
        public static bool Continue;
        public static string Nickname;
        public static DifficultyLevel Level = DifficultyLevel.Designer;
    }
}
