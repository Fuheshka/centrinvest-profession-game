using System;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

// Editor-only asset authoring. All resulting UI remains serialized and artist-editable.
public static class UIReferenceSprites
{
    readonly struct SpriteBinding
    {
        public readonly string Prefab, Node, Sprite;
        public SpriteBinding(string prefab, string node, string sprite)
        { Prefab = prefab; Node = node; Sprite = sprite; }
    }

    [MenuItem("Tools/UI/Apply reference sprites")]
    public static void Apply()
    {
        var bindings = new[]
        {
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/VersionRow.prefab", "", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/ReviewCard.prefab", "", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/EndingPanel.prefab", "Content/Left/Buttons/About", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/EndingPanel.prefab", "Content/Right", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/EndingPanel.prefab", "Content/Left/Buttons/NewProject", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/EndingPanel.prefab", "Content/Left/Score/Ring/Inner", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/EndingPanel.prefab", "Content", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/EndingPanel.prefab", "Content/Left/Score/Ring", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/VersionsPanel.prefab", "Content/Toast", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/VersionsPanel.prefab", "Content/Right/ReopenBox/ReopenButton", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/VersionsPanel.prefab", "Content", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/VersionsPanel.prefab", "Content/Modal", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/VersionsPanel.prefab", "Content/Right", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/VersionsPanel.prefab", "Content/Center", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/VersionsPanel.prefab", "Content/Right/ReopenBox", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/VersionsPanel.prefab", "Content/Left", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/VersionsPanel.prefab", "Content/Modal/Box/Cancel", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/VersionsPanel.prefab", "Content/Modal/Box", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/AxisCard.prefab", "", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/JournalRow.prefab", "", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/BuildButton.prefab", "", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/QuoteRow.prefab", "", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/ReopenOption.prefab", "", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Common/TopBar.prefab", "Background", "Assets/_Project/Art/UI/studio-background.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Common/TopBar.prefab", "Bar/Tabs/TabBoard", "Assets/_Project/Art/UI/panel-round.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Common/TopBar.prefab", "Bar/Tabs/TabChats", "Assets/_Project/Art/UI/panel-round.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Common/TopBar.prefab", "Bar/Tabs/TabVersions", "Assets/_Project/Art/UI/panel-round.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Common/TopBar.prefab", "Bar/Status", "Assets/_Project/Art/UI/panel-diagonal.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Common/TopBar.prefab", "Bar/EndDay", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Messenger/MessengerPanel.prefab", "Body/ChatsTab/Left", "Assets/_Project/Art/UI/panel-left.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Messenger/MessengerPanel.prefab", "Body/ChatsTab/Center", "Assets/_Project/Art/UI/panel-right.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Messenger/MessengerPanel.prefab", "Body/ChatsTab/Center/HeaderBg", "Assets/_Project/Art/UI/panel-right.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Messenger/MessengerPanel.prefab", "Body/ChatsTab/Center/OptionBar", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Board/BoardPanel.prefab", "Content/Cork", "Assets/_Project/Art/UI/panel-bottom.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Board/BoardPanel.prefab", "Content/Side/Brief", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Board/BoardPanel.prefab", "Content/Side/Research", "Assets/_Project/Art/UI/panel-round.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Board/BoardPanel.prefab", "Content/Side/Ask", "Assets/_Project/Art/UI/panel-round.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Board/BoardPanel.prefab", "PitchButton", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Board/CardView.prefab", "Body", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/VersionsPanel.prefab", "Content/Left", "Assets/_Project/Art/UI/panel-left.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/VersionsPanel.prefab", "Content/Center", "Assets/_Project/Art/UI/panel-bottom.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/VersionsPanel.prefab", "Content/Right", "Assets/_Project/Art/UI/panel-right.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Menu/MenuScreen.prefab", "Bg", "Assets/_Project/Art/UI/menu-background.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Menu/MenuScreen.prefab", "Header", "Assets/_Project/Art/UI/menu-title-plate.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Menu/MenuScreen.prefab", "Levels/LevelСтажёр", "Assets/_Project/Art/UI/panel-round.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Menu/MenuScreen.prefab", "Levels/LevelДизайнер", "Assets/_Project/Art/UI/panel-round.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Menu/MenuScreen.prefab", "Levels/LevelВедущий", "Assets/_Project/Art/UI/panel-round.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Menu/MenuScreen.prefab", "ContinueButton", "Assets/_Project/Art/UI/panel-round.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Versions/EndingPanel.prefab", "Content/Right", "Assets/_Project/Art/UI/panel-round.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Board/BoardPanel.prefab", "Content/Cork/Surface/Slot_Genre/Frame", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Board/BoardPanel.prefab", "Content/Cork/Surface/Slot_Setting/Frame", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Board/BoardPanel.prefab", "Content/Cork/Surface/Slot_Audience/Frame", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Board/BoardPanel.prefab", "Content/Cork/Surface/Slot_Mechanic/Frame", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Board/BoardPanel.prefab", "Content/Cork/Surface/Slot_Hook/Frame", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Board/BoardPanel.prefab", "Content/Cork/Surface/Slot_Reference/Frame", "Assets/_Project/Art/UI/rounded.png"),
            new SpriteBinding("Assets/_Project/Prefabs/UI/Board/BoardPanel.prefab", "Content/Cork/Surface/SlotRail", "Assets/_Project/Art/UI/rounded.png"),
        };
        foreach (var group in bindings.GroupBy(b => b.Prefab))
        {
            var root = PrefabUtility.LoadPrefabContents(group.Key);
            try
            {
                foreach (var b in group)
                {
                    var node = b.Node.Length == 0 ? root.transform : root.transform.Find(b.Node);
                    if (node == null) throw new InvalidOperationException("Missing UI node: " + b.Node);
                    var image = node.GetComponent<Image>();
                    var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(b.Sprite);
                    if (image == null || sprite == null) throw new InvalidOperationException("Missing UI image/sprite: " + b.Sprite);
                    image.sprite = sprite;
                }
                if (group.Key.EndsWith("TopBar.prefab"))
                {
                    var bar = root.GetComponent<Studio.UI.TopBarView>();
                    bar.statusBackground = root.transform.Find("Bar/Status").GetComponent<Image>();
                    bar.statusDivider = root.transform.Find("Bar/Status/Divider").GetComponent<Image>();
                    bar.budgetLabel = root.transform.Find("Bar/Status/BudgetLabel").GetComponent<TMPro.TMP_Text>();
                }
                if (group.Key.EndsWith("MenuScreen.prefab"))
                {
                    root.transform.Find("ArtPlaceholder").gameObject.SetActive(false);
                    foreach (Transform button in root.transform.Find("Levels"))
                    {
                        var border = button.GetComponent<Outline>() ?? button.gameObject.AddComponent<Outline>();
                        border.effectColor = new Color32(216, 226, 166, 255);
                        border.effectDistance = new Vector2(8, -8);
                        var shadow = button.GetComponents<Shadow>().FirstOrDefault(s => s.GetType() == typeof(Shadow)) ?? button.gameObject.AddComponent<Shadow>();
                        shadow.effectColor = new Color32(0, 0, 0, 255);
                        shadow.effectDistance = new Vector2(0, -20);
                    }
                }
                if (group.Key.EndsWith("MessengerPanel.prefab"))
                    root.transform.Find("Body/ChatsTab/Center/OptionBar/ReplyLabel").gameObject.SetActive(false);
                PrefabUtility.SaveAsPrefabAsset(root, group.Key);
            }
            finally { PrefabUtility.UnloadPrefabContents(root); }
        }
        Debug.Log("Reference UI sprite bindings applied to " + bindings.Select(b => b.Prefab).Distinct().Count() + " prefabs.");
    }
}
