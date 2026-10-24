using System.Collections.Generic;
using Studio.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Studio.UI.Mvp
{
    // Free canvas: block types from the side panel, parameter chosen inside the block, manual links.
    public class DashboardWindow : MonoBehaviour
    {
        public GameLoop game;
        public Button[] palette = new Button[6];        // in BlockType order
        public RectTransform canvas, linkLayer, tagLayer;   // tagLayer is the last child of canvas: tags stay above blocks
        public BlockView blockPrefab;
        public LinkView linkPrefab;
        public TMP_Text totals, reason, mode;
        public Button approve, linkMode;
        public PopupMenu popup;

        readonly Dictionary<string, BlockView> views = new Dictionary<string, BlockView>();
        readonly List<(ConceptLink link, LinkView view)> linkViews = new List<(ConceptLink, LinkView)>();
        bool linking;
        string linkFrom;

        void Awake()
        {
            for (int i = 0; i < palette.Length; i++) { var t = (BlockType)i; palette[i].onClick.AddListener(() => Place(t)); }
            approve.onClick.AddListener(() => { if (ConceptRules.Approve(game.S, game.L)) { game.Commit(); FindAnyObjectByType<TopBarMvp>()?.Show(2); } });
            linkMode.onClick.AddListener(() => { linking = !linking; linkFrom = null; Redraw(); });
            game.Changed += OnChanged;
        }

        void OnEnable() => Redraw();
        int drawnHash;
        void OnChanged() { if (isActiveAndEnabled && Hash() != drawnHash) Redraw(); }
        int Hash() => game.S == null ? 0 : JsonUtility.ToJson(game.S.concept).GetHashCode() ^ (linking ? 1 : 0) ^ (linkFrom?.GetHashCode() ?? 0);

        void Place(BlockType t)
        {
            int n = game.S.concept.blocks.Count;
            float x = 160 + (n % 3) * 360, y = -130 - (n / 3) * 280;
            if (ConceptRules.Place(game.S, game.L, t, x, y) != null) { game.Commit(); Redraw(); }
        }

        public void Clicked(Block b)
        {
            if (game.S.concept.approved && !ConceptRules.CanRework(game.S)) return;
            if (linking && !game.S.concept.approved)
            {
                if (linkFrom == null) linkFrom = b.id;
                else if (linkFrom == b.id) linkFrom = null;
                else if (game.S.concept.links.Count < ConceptRules.MaxLinks && ConceptRules.CanLink(game.S.concept.blocks.Find(x => x.id == linkFrom).type, b.type))
                {
                    ConceptRules.Connect(game.S, linkFrom, b.id);
                    linkFrom = null;
                    game.Commit();
                }
                Redraw();
                return;
            }
            var opts = new List<(string, System.Action, int)>();
            foreach (var p in game.L.parameters)
                if (p.type == b.type) { var pid = p.id; opts.Add((p.title, () => { ConceptRules.SetParam(game.S, game.L, b.id, pid); game.Commit(); Redraw(); }, b.paramId == pid ? 1 : 0)); }
            if (!game.S.concept.approved) opts.Add(("Удалить блок", () => { ConceptRules.Remove(game.S, b.id); game.Commit(); Redraw(); }, 2));
            popup.Show((RectTransform)views[b.id].transform, Names.Of(b.type), opts.ToArray());
        }

        // Live while dragging: only the lines are re-aimed, blocks are not rebuilt (that would kill the drag).
        public void Dragging(Block b, Vector2 pos)
        {
            b.x = pos.x; b.y = pos.y;
            foreach (var (link, view) in linkViews) view.Place(views[link.a].Center, views[link.b].Center);
        }

        public void Moved(Block b, Vector2 pos) { ConceptRules.Move(game.S, b.id, pos.x, pos.y); SaveSystem.SaveLevel(game.S); Redraw(); }

        public void LinkClicked(ConceptLink link)
        {
            var s = game.S;
            if (s.concept.approved) return;
            var view = linkViews.Find(x => x.link == link).view;
            popup.Show(view.tag, "Линия", ("Удалить", () => { ConceptRules.Disconnect(s, link.a, link.b); game.Commit(); Redraw(); }, 2));
        }

        // "Д+1 З–1": net effect per discipline, green plus and red minus on the dark plate.
        static string Tag(List<Effect> fx, int step)
        {
            var net = new int[3];
            foreach (var e in fx) net[(int)e.d] += System.Math.Sign(e.sign);
            string[] letter = { "Д", "М", "З" };
            var parts = new List<string>();
            for (int i = 0; i < 3; i++)
                if (net[i] != 0) parts.Add("<color=#" + (net[i] > 0 ? "7BE0A4" : "FF8F8F") + ">" + letter[i] + (net[i] > 0 ? "+" : "–") + System.Math.Abs(net[i]) * step + "</color>");
            return parts.Count == 0 ? "±" : string.Join(" ", parts);
        }

        string Title(Block b) => b.paramId == null ? Names.Of(b.type) : game.L.Param(b.paramId).title;

        void Redraw()
        {
            if (game.S == null) return;
            drawnHash = Hash();
            var s = game.S; var l = game.L;
            foreach (Transform c in canvas) if (c != linkLayer && c != tagLayer) Destroy(c.gameObject);
            foreach (Transform c in linkLayer) Destroy(c.gameObject);
            foreach (Transform c in tagLayer) Destroy(c.gameObject);
            views.Clear(); linkViews.Clear();
            foreach (var b in s.concept.blocks)
            {
                var v = Instantiate(blockPrefab, canvas);
                v.Bind(this, b, Names.Of(b.type), b.paramId == null ? "выбрать" : l.Param(b.paramId).title, b.id == linkFrom, linkFrom != null && b.id != linkFrom
                    && (s.concept.links.Count >= ConceptRules.MaxLinks || !ConceptRules.CanLink(s.concept.blocks.Find(x => x.id == linkFrom).type, b.type)));
                views[b.id] = v;
            }
            foreach (var link in s.concept.links)
            {
                var v = Instantiate(linkPrefab, linkLayer);
                var a = s.concept.blocks.Find(x => x.id == link.a); var b = s.concept.blocks.Find(x => x.id == link.b);
                var fx = ConceptRules.PairEffects(l, a.paramId, b.paramId);
                bool neutral = fx.Count == 0;   // no rule for this pair: grey, not a red "bad" verdict
                linkViews.Add((link, v));
                v.Bind(this, link, views[link.a].Center, views[link.b].Center,
                       neutral ? Palette.Neutral : ConceptRules.LinkColor(s, l, link.a, link.b) == LinkColor.Green ? Palette.Green : Palette.Red,
                       neutral ? "–" : Tag(fx, l.tuning.conceptStep));
            }
            for (int i = 0; i < palette.Length; i++) palette[i].interactable = !s.concept.approved && s.BlockOf((BlockType)i) == null;
            var t = ConceptRules.Totals(s, l);
            totals.text = "Итог: Дизайн " + Sign(t[0] * l.tuning.conceptStep) + " · Механики " + Sign(t[1] * l.tuning.conceptStep) + " · Звук " + Sign(t[2] * l.tuning.conceptStep);
            bool ok = ConceptRules.CanApprove(s, l, out var why);
            approve.interactable = ok;
            reason.text = ConceptRules.CanRework(s) ? "Концепт можно пересмотреть: поменяй параметры и пересобери игру" : s.concept.approved ? "Концепт утверждён" : ok ? "Можно утверждать" : why;
            string count = " " + s.concept.links.Count + "/" + ConceptRules.MaxLinks;
            mode.text = linking ? (linkFrom == null ? "Выбери первый блок" : "Выбери второй блок") + count : "Связать блоки" + count;
            tagLayer.SetAsLastSibling();
            linkMode.interactable = !s.concept.approved;
        }

        static string Sign(int v) => v > 0 ? "+" + v : v.ToString();
    }
}
