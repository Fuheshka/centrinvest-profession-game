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
                if (p.type == b.type) { var pid = p.id; opts.Add((p.title, () =>
                {
                    if (!ConceptRules.SetParam(game.S, game.L, b.id, pid, out var reason))
                    {
                        if (!string.IsNullOrEmpty(reason)) game.modal.Show("Концепт не изменён", reason, ("Понятно", null));
                        Redraw();
                        return;
                    }
                    game.Commit();
                    Redraw();
                }, b.paramId == pid ? 1 : 0)); }
            if (!game.S.concept.approved) opts.Add(("Удалить блок", () => { ConceptRules.Remove(game.S, b.id); game.Commit(); Redraw(); }, 2));
            popup.Show((RectTransform)views[b.id].transform, Names.Of(b.type), opts.ToArray());
        }

        // Live while dragging: only the lines are re-aimed, blocks are not rebuilt (that would kill the drag).
        public void Dragging(Block b, Vector2 pos)
        {
            b.x = pos.x; b.y = pos.y;
            foreach (var (link, view) in linkViews)
                PlaceLink(link, view);
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
                var param = b.paramId == null ? null : l.Param(b.paramId);
                var source = linkFrom == null ? null : s.concept.blocks.Find(x => x.id == linkFrom);
                v.Bind(this, b, Names.Of(b.type), param?.title ?? "выбрать", b.id == linkFrom, source != null && b.id != linkFrom
                    && (s.concept.links.Count >= ConceptRules.MaxLinks || !ConceptRules.CanLink(source.type, b.type)));
                views[b.id] = v;
            }
            foreach (var link in s.concept.links)
            {
                var v = Instantiate(linkPrefab, linkLayer);
                var blockA = s.concept.blocks.Find(x => x.id == link.a); var blockB = s.concept.blocks.Find(x => x.id == link.b);
                var fx = ConceptRules.PairEffects(l, blockA.paramId, blockB.paramId);
                bool neutral = fx.Count == 0;   // no rule for this pair: grey, not a red "bad" verdict
                linkViews.Add((link, v));
                var viewA = views[link.a]; var viewB = views[link.b];
                LinkPorts(viewA, viewB, out var from, out var to, out _);
                v.Bind(this, link, from, to,
                       neutral ? Palette.Neutral : ConceptRules.LinkColor(s, l, link.a, link.b) == LinkColor.Green ? Palette.Green : Palette.Red,
                       neutral ? "–" : Tag(fx, l.tuning.conceptStep));
                PlaceLink(link, v);
            }
            for (int i = 0; i < palette.Length; i++) palette[i].interactable = !s.concept.approved && s.BlockOf((BlockType)i) == null;
            var t = ConceptRules.Totals(s, l);
            totals.text = "Итог: Дизайн " + Sign(t[0] * l.tuning.conceptStep) + " · Механики " + Sign(t[1] * l.tuning.conceptStep) + " · Звук " + Sign(t[2] * l.tuning.conceptStep);
            bool ok = ConceptRules.CanApprove(s, l, out var why);
            approve.interactable = ok;
            reason.text = ConceptRules.CanRework(s)
                ? (s.reworkChargedBuild == s.builds.Count
                    ? "Пересмотр оплачен: можно менять параметры. Сложность задач обновится."
                    : "Первое изменение: один день и дневные ставки команды. Сложность задач обновится.")
                : s.concept.approved ? "Концепт утверждён" : ok ? "Можно утверждать" : why;
            string count = " " + s.concept.links.Count + "/" + ConceptRules.MaxLinks;
            mode.text = linking ? (linkFrom == null ? "Выбери первый блок" : "Выбери второй блок") + count : "Связать блоки" + count;
            tagLayer.SetAsLastSibling();
            linkMode.interactable = !s.concept.approved;
        }

        void LinkPorts(BlockView a, BlockView b, out Vector2 from, out Vector2 to, out bool vertical)
        {
            var delta = b.Center - a.Center;
            vertical = Mathf.Abs(delta.x) < ((RectTransform)a.transform).rect.width * 0.6f
                && Mathf.Abs(delta.y) > ((RectTransform)a.transform).rect.height * 0.6f;
            if (vertical)
            {
                float side = (a.Center.x + b.Center.x) * 0.5f < canvas.rect.width * 0.5f ? 1f : -1f;
                from = a.PortOnSide(side);
                to = b.PortOnSide(side);
            }
            else
            {
                from = a.PortToward(b.Center);
                to = b.PortToward(a.Center);
            }
        }

        void PlaceLink(ConceptLink link, LinkView view)
        {
            var a = views[link.a]; var b = views[link.b];
            LinkPorts(a, b, out var from, out var to, out var vertical);
            if (vertical)
            {
                float side = Mathf.Sign(from.x - a.Center.x);
                float normalX = -(to.y - from.y);
                view.Place(from, to, 45f * Mathf.Sign(side * normalX));
            }
            else view.Place(from, to, NeededBend(link, from, to));
        }

        float NeededBend(ConceptLink link, Vector2 from, Vector2 to)
        {
            var chord = to - from;
            float lengthSq = chord.sqrMagnitude;
            if (lengthSq < 1f) return 0f;

            bool obstructed = false;
            foreach (var pair in views)
            {
                if (pair.Key == link.a || pair.Key == link.b) continue;
                var rt = (RectTransform)pair.Value.transform;
                float t = Mathf.Clamp01(Vector2.Dot(pair.Value.Center - from, chord) / lengthSq);
                var nearest = from + chord * t;
                if (Vector2.Distance(pair.Value.Center, nearest) < Mathf.Min(rt.rect.width, rt.rect.height) * 0.49f)
                {
                    obstructed = true;
                    break;
                }
            }
            if (!obstructed) return 0f;

            var normal = new Vector2(-chord.y, chord.x).normalized;
            const float amount = 150f;
            float positive = CurveClearance(from, to, normal * amount, link);
            float negative = CurveClearance(from, to, -normal * amount, link);
            return positive >= negative ? amount : -amount;
        }

        float CurveClearance(Vector2 from, Vector2 to, Vector2 controlOffset, ConceptLink link)
        {
            float clearance = 0f;
            for (int step = 1; step < 10; step++)
            {
                float t = step / 10f;
                var point = Vector2.Lerp(from, to, t) + controlOffset * (4f * t * (1f - t));
                foreach (var pair in views)
                {
                    if (pair.Key == link.a || pair.Key == link.b) continue;
                    var rt = (RectTransform)pair.Value.transform;
                    var delta = point - pair.Value.Center;
                    float dx = Mathf.Max(0f, Mathf.Abs(delta.x) - rt.rect.width * 0.5f);
                    float dy = Mathf.Max(0f, Mathf.Abs(delta.y) - rt.rect.height * 0.5f);
                    clearance += Mathf.Sqrt(dx * dx + dy * dy);
                }
            }
            return clearance;
        }

        static string Sign(int v) => v > 0 ? "+" + v : v.ToString();
    }
}
