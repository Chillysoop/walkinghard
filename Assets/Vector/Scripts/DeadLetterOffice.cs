using System;
using System.Linq;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        int officeBranch, officeSelection, officeRank = 1;
        Vector2 officeDetailScroll, officePan, officeDragOrigin, officePanOrigin;
        float officeZoom = 1;
        bool officePanning;
        string officeFlashItem = "";
        float officeFlashAt;
        static Texture2D officeDisc, officeHalo;
        internal int OfficeBranch => officeBranch;
        internal int OfficeSelection => officeSelection;
        internal float OfficeZoom => officeZoom;
        internal Vector2 OfficePan => officePan;
        internal Rect OfficeMap(float w, float h) => new Rect(28, 142, w - 390, h - 260);
        Rect OfficeDetail(float w, float h) => new Rect(w - 350, 142, 322, h - 260);
        internal Rect OfficeBranchButton(int branch, float w, float h)
        {
            float width = (w - 68) / 7;
            return new Rect(28 + branch * (width + 2), 99, width, 30);
        }
        // Geography is presentation only. Solid links are real parent gates;
        // dotted links connect freely accessible entries to the Office hub.
        static Vector2 OfficePolar(float radius, float angle)
        {
            float a = angle * Mathf.Deg2Rad;
            return new Vector2(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius * .67f);
        }
        internal static Vector2 OfficePosition(int index)
        {
            var id = Constellation.Nodes[index].item;
            int branch = ConstellationContent.Branch(id);
            int i = Array.IndexOf(ConstellationContent.BranchItems[branch], id);
            if (branch == 0) return OfficePolar(i % 2 == 0 ? 120 : 225, -120 + i / 2 * 120);
            float angle = -150 + (branch - 1) * 60;
            return OfficePolar(i == 0 ? 285 : i < 3 ? 405 : 532,
                angle + (i == 0 ? 0 : i == 1 || i == 3 ? -12 : 12));
        }
        float OfficeScale(float w, float h) => Mathf.Min(OfficeMap(w, h).width / 1250, OfficeMap(w, h).height / 900) * officeZoom;
        Vector2 OfficePoint(Vector2 world, float w, float h) => OfficeMap(w, h).center + (world + officePan) * OfficeScale(w, h);
        internal Rect OfficeStar(int index, float w, float h)
        {
            var point = OfficePoint(OfficePosition(index), w, h);
            float size = Mathf.Clamp(66 * OfficeScale(w, h), 32, 76);
            if (LoadoutCatalog.Find(Constellation.Nodes[index].item).kind == ItemKind.Ultimate) size *= 1.15f;
            return new Rect(point.x - size / 2, point.y - size / 2, size, size);
        }
        internal Rect OfficeNodeButton(int index, float w, float h)
        {
            var d = OfficeDetail(w, h);
            return index == officeSelection ? new Rect(d.x + 14, d.yMax - 58, d.width - 28, 44) : new Rect(-2000, -2000, 1, 1);
        }
        internal void FitOfficeChart() { officePan = Vector2.zero; officeZoom = 1; officePanning = false; }
        internal void SelectOfficeBranch(int branch)
        {
            officeBranch = Mathf.Clamp(branch, 0, 6);
            officeSelection = Array.FindIndex(Constellation.Nodes, n => n.item == ConstellationContent.BranchItems[officeBranch][0]);
            officeDetailScroll = Vector2.zero;
            officeZoom = officeBranch == 0 ? 1.5f : 1.8f;
            officePan = officeBranch == 0 ? Vector2.zero : -OfficePolar(360, -150 + (officeBranch - 1) * 60);
            officePanning = false;
        }
        internal void ZoomOfficeChart(float factor, Vector2 anchor, float w, float h)
        {
            var map = OfficeMap(w, h);
            if (!map.Contains(anchor)) return;
            var world = (anchor - map.center) / OfficeScale(w, h) - officePan;
            officeZoom = Mathf.Clamp(officeZoom * factor, .8f, 3.4f);
            officePan = (anchor - map.center) / OfficeScale(w, h) - world;
            ClampOfficePan();
        }
        void ClampOfficePan() { officePan.x = Mathf.Clamp(officePan.x, -630, 630); officePan.y = Mathf.Clamp(officePan.y, -420, 420); }
        bool OfficeVisible(Rect r, Rect map) => map.Contains(r.min - Vector2.one * 5) && map.Contains(r.max + Vector2.one * 5);
        int OfficeHit(Vector2 point, float w, float h)
        {
            var map = OfficeMap(w, h);
            if (!map.Contains(point)) return -1;
            for (int i = 0; i < Constellation.Nodes.Length; i++)
            { var r = OfficeStar(i, w, h); if (OfficeVisible(r, map) && Vector2.Distance(r.center, point) <= r.width / 2) return i; }
            return -1;
        }
        internal bool HandleOfficePointer(EventType type, Vector2 point, float w, float h)
        {
            if (!game.OfficeOpen) return false;
            if (type == EventType.MouseUp && officePanning) { officePanning = false; return true; }
            if (type == EventType.MouseDrag && officePanning)
            {
                officePan = officePanOrigin + (point - officeDragOrigin) / OfficeScale(w, h);
                ClampOfficePan(); return true;
            }
            if (type != EventType.MouseDown) return false;
            for (int b = 0; b < 7; b++)
                if (OfficeBranchButton(b, w, h).Contains(point)) { SelectOfficeBranch(b); return true; }
            int hit = OfficeHit(point, w, h);
            if (hit >= 0)
            {
                officeSelection = hit; officeBranch = ConstellationContent.Branch(Constellation.Nodes[hit].item);
                officeDetailScroll = Vector2.zero; return true;
            }
            if (OfficeNodeButton(officeSelection, w, h).Contains(point))
            {
                string item = Constellation.Nodes[officeSelection].item;
                if (game.BuyConstellation(item)) { officeFlashItem = item; officeFlashAt = Time.unscaledTime; }
                return true;
            }
            if (OfficeMap(w, h).Contains(point))
            { officePanning = true; officeDragOrigin = point; officePanOrigin = officePan; return true; }
            return false;
        }
        // Clip endpoints before drawing rotated IMGUI lines, avoiding nested scroll transforms.
        static bool OfficeClip(ref Vector2 a, ref Vector2 b, Rect clip)
        {
            Vector2 delta = b - a; float lo = 0, hi = 1;
            bool Cut(float p, float q)
            {
                if (Mathf.Abs(p) < .00001f) return q >= 0;
                float t = q / p;
                if (p < 0) { if (t > hi) return false; lo = Mathf.Max(lo, t); }
                else { if (t < lo) return false; hi = Mathf.Min(hi, t); }
                return true;
            }
            if (!Cut(-delta.x, a.x - clip.x) || !Cut(delta.x, clip.xMax - a.x) || !Cut(-delta.y, a.y - clip.y) || !Cut(delta.y, clip.yMax - a.y)) return false;
            b = a + hi * delta; a += lo * delta; return true;
        }
        static void OfficeLine(Vector2 a, Vector2 b, Rect clip, Color color, float width = 1)
        { if (OfficeClip(ref a, ref b, clip)) Stroke(a, b, color, width); }
        static void OfficeCircle(Vector2 point, float radius, Rect clip, Color color, float width = 1)
        {
            for (int i = 0; i < 48; i++)
            {
                float a = i * Mathf.PI / 24, b = (i + 1) * Mathf.PI / 24;
                OfficeLine(point + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * radius,
                    point + new Vector2(Mathf.Cos(b), Mathf.Sin(b)) * radius, clip, color, width);
            }
        }
        static void OfficeTextures()
        {
            if (officeDisc != null) return;
            officeDisc = new Texture2D(96, 96, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave, wrapMode = TextureWrapMode.Clamp };
            officeHalo = new Texture2D(96, 96, TextureFormat.RGBA32, false) { hideFlags = HideFlags.HideAndDontSave, wrapMode = TextureWrapMode.Clamp };
            for (int y = 0; y < 96; y++) for (int x = 0; x < 96; x++)
            {
                float radius = new Vector2(x - 47.5f, y - 47.5f).magnitude / 47;
                officeDisc.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01((1 - radius) * 40)));
                officeHalo.SetPixel(x, y, new Color(1, 1, 1, Mathf.Pow(Mathf.Clamp01(1 - radius), 2)));
            }
            officeDisc.Apply(); officeHalo.Apply();
        }
        static void OfficeTexture(Rect rect, Texture2D texture, Color tint)
        { var before = GUI.color; GUI.color = tint; GUI.DrawTexture(rect, texture); GUI.color = before; }
        bool OfficeAncestor(int possible, int child)
        {
            string id = Constellation.Nodes[child].item;
            while (id != null)
            {
                if (Constellation.Nodes[possible].item == id) return true;
                id = Constellation.Nodes.First(n => n.item == id).parent;
            }
            return false;
        }
        void DrawOfficeChart(float w, float h)
        {
            var map = OfficeMap(w, h); OfficeTextures();
            Box(map, new Color(.012f, .022f, .039f)); Border(map, line, 1);
            // Never consume the draft/combat random stream for decorative stars.
            for (int i = 0; i < 165; i++)
            {
                float x = map.x + 5 + (i * 271 % 997) / 997f * (map.width - 10);
                float y = map.y + 5 + (i * 163 % 541) / 541f * (map.height - 10);
                float size = i % 17 == 0 ? 2 : 1;
                Box(new Rect(x, y, size, size), new Color(.55f, .7f, .78f, i % 3 == 0 ? .34f : .14f));
            }
            for (int ring = 0; ring < 3; ring++) for (int i = 0; i < 96; i++)
                OfficeLine(OfficePoint(OfficePolar(220 + ring * 190, i * 3.75f), w, h),
                    OfficePoint(OfficePolar(220 + ring * 190, (i + 1) * 3.75f), w, h), map, new Color(.28f, .44f, .52f, .16f));
            var center = OfficePoint(Vector2.zero, w, h);
            int hovered = officePanning ? -1 : OfficeHit(Pointer, w, h), path = hovered >= 0 ? hovered : officeSelection;
            for (int i = 0; i < Constellation.Nodes.Length; i++)
            {
                var node = Constellation.Nodes[i]; var end = OfficeStar(i, w, h).center;
                int parent = node.parent == null ? -1 : Array.FindIndex(Constellation.Nodes, n => n.item == node.parent);
                Vector2 start = parent < 0 ? center : OfficeStar(parent, w, h).center;
                bool traced = OfficeAncestor(i, path), owned = game.HasConstellation(node.item);
                Color color = traced ? new Color(.55f, .96f, .98f, .85f) : owned ? new Color(.3f, .75f, .78f, .55f) : new Color(.42f, .51f, .58f, .45f);
                if (parent < 0)
                {
                    for (int step = 0; step < 16; step += 2)
                        OfficeLine(Vector2.Lerp(start, end, step / 16f), Vector2.Lerp(start, end, (step + 1) / 16f), map, color);
                }
                else
                {
                    if (traced) OfficeLine(start, end, map, new Color(.25f, .9f, 1, .08f), 7);
                    OfficeLine(start, end, map, color, traced || owned ? 2 : 1);
                }
            }
            if (map.Contains(center - Vector2.one * 23) && map.Contains(center + Vector2.one * 23))
            {
                OfficeTexture(new Rect(center.x - 23, center.y - 23, 46, 46), officeDisc, new Color(.04f, .12f, .16f));
                OfficeCircle(center, 23, map, Accent); OfficeCircle(center, 17, map, line);
                Label(new Rect(center.x - 20, center.y - 13, 40, 26), "DLO", 12, white, true, TextAnchor.MiddleCenter);
            }
            for (int branch = 1; branch < 7; branch++)
            {
                Vector2 point = OfficePoint(OfficePolar(632, -150 + (branch - 1) * 60), w, h);
                var label = new Rect(point.x - 65, point.y - 9, 130, 19);
                if (map.Contains(label.min) && map.Contains(label.max))
                    Label(label, ConstellationContent.BranchNames[branch], 11, branch == officeBranch ? white : muted, true, TextAnchor.MiddleCenter);
            }
            for (int i = 0; i < Constellation.Nodes.Length; i++)
            {
                var node = Constellation.Nodes[i]; var def = LoadoutCatalog.Find(node.item); var r = OfficeStar(i, w, h);
                if (!OfficeVisible(r, map)) continue;
                bool owned = game.HasConstellation(node.item), ready = game.UnlockRequirement(node).StartsWith("UNLOCK ·");
                bool selected = i == officeSelection, hover = i == hovered;
                var edge = selected || owned ? Accent : ready || hover ? white : muted;
                if (selected || hover) OfficeTexture(new Rect(r.x - 7, r.y - 7, r.width + 14, r.height + 14), officeHalo, new Color(.4f, 1, 1, .7f));
                OfficeTexture(r, officeDisc, new Color(.035f, .075f, .10f));
                var image = new Rect(r.x + r.width * .19f, r.y + r.height * .19f, r.width * .62f, r.height * .62f);
                var before = GUI.color; GUI.color = owned || ready || selected ? Color.white : new Color(.6f, .64f, .68f, .65f);
                ActionGlyph(image, def, white); GUI.color = before;
                OfficeCircle(r.center, r.width / 2, map, edge, selected || owned ? 2 : 1);
                if (selected) OfficeCircle(r.center, r.width / 2 + 4, map, white);
                var badge = new Rect(r.xMax - 11, r.yMax - 13, 20, 20);
                OfficeTexture(badge, officeDisc, new Color(.025f, .055f, .075f));
                if (owned) { Stroke(badge.center + new Vector2(-4, 0), badge.center + new Vector2(-1, 3), Accent, 2); Stroke(badge.center + new Vector2(-1, 3), badge.center + new Vector2(5, -4), Accent, 2); }
                else if (ready) Label(badge, node.cost.ToString(), 11, white, true, TextAnchor.MiddleCenter);
                else LockIcon(badge.center, muted);
                if (node.item == officeFlashItem && Time.unscaledTime - officeFlashAt < 1.5f)
                {
                    float t = (Time.unscaledTime - officeFlashAt) / 1.5f;
                    OfficeCircle(r.center, r.width / 2 + (game.ReducedMotion ? 6 : 6 + t * 20), map, new Color(.6f, 1, 1, 1 - t), 2);
                }
            }
            if (hovered >= 0)
            {
                var node = Constellation.Nodes[hovered];
                var hint = new Rect(Mathf.Clamp(Pointer.x + 16, map.x + 6, map.xMax - 224), Mathf.Clamp(Pointer.y + 20, map.y + 4, map.yMax - 46), 218, 40);
                Plate(hint, ink, line, 5); Label(new Rect(hint.x + 9, hint.y + 3, hint.width - 18, 19), LoadoutCatalog.Find(node.item).name, 13, white, true);
                Label(new Rect(hint.x + 9, hint.y + 23, hint.width - 18, 14), game.HasConstellation(node.item) ? "RECOVERED" : node.cost + " SPARKS · CLICK FOR DETAILS", 10, muted);
            }
        }
        void DrawOfficeDetail(float w, float h)
        {
            var detail = OfficeDetail(w, h); var selected = Constellation.Nodes[officeSelection];
            var item = LoadoutCatalog.Find(selected.item); bool owned = game.HasConstellation(item.id);
            Panel(detail); ActionGlyph(new Rect(detail.x + 14, detail.y + 16, 62, 62), item, white);
            Label(new Rect(detail.x + 90, detail.y + 17, detail.width - 104, 44), item.name, 21, white, true);
            Label(new Rect(detail.x + 90, detail.y + 64, detail.width - 104, 16), Category(item).ToUpperInvariant(), 11, muted, true);
            Label(new Rect(detail.x + 14, detail.y + 94, detail.width - 28, 28), ItemSummary(new LoadoutItem(item)), 13, white);
            if (Button(new Rect(detail.x + 14, detail.y + 132, detail.width - 28, 27), "PREVIEW: " + ItemRarity.Names[officeRank].ToUpperInvariant() + "  >")) officeRank = officeRank == 5 ? 1 : officeRank + 1;
            var shown = new LoadoutItem(item, officeRank);
            string desc = ConstellationContent.IsNew(item.id) ? ConstellationRules(shown) : item.description + "\n\n" + EffectRules(shown) + "\n\n" + item.tradeoff;
            string requirement = selected.parent == null ? "ENTRY NODE · no prerequisite" : "REQUIRES\n" + (game.HasConstellation(selected.parent) ? "✓ " : "○ ") + LoadoutCatalog.Find(selected.parent).name + "\n" + (game.CompletionSeal ? "✓ " : "○ ") + "Completion Seal · defeat the Foreman";
            string body = ItemStats(item, officeRank) + "\n\n" + desc + "\n\n" + requirement;
            var viewport = new Rect(detail.x + 14, detail.y + 171, detail.width - 28, detail.height - 241);
            float bodyHeight = Style(13).CalcHeight(new GUIContent(body), viewport.width - 18) + 12;
            officeDetailScroll = GUI.BeginScrollView(viewport, officeDetailScroll, new Rect(0, 0, viewport.width - 18, bodyHeight));
            Label(new Rect(0, 0, viewport.width - 18, bodyHeight), body, 13, white); GUI.EndScrollView();
            string state = game.UnlockRequirement(selected); bool available = state.StartsWith("UNLOCK ·");
            var buy = OfficeNodeButton(officeSelection, w, h);
            Plate(buy, available ? new Color(.07f, .2f, .21f) : ink, available ? Accent : line, 5, available ? 2 : 1);
            Label(new Rect(buy.x + 6, buy.y + 3, buy.width - 12, buy.height - 6), owned ? "RECOVERED" : state, 12, available ? Accent : muted, true, TextAnchor.MiddleCenter);
        }
        void DeadLetterOffice(float w, float h)
        {
            var map = OfficeMap(w, h);
            Box(new Rect(0, 0, w, h), new Color(.008f, .017f, .028f));
            Label(new Rect(28, 23, 780, 32), "THE ASTRAL CHART", 27, white, true);
            Label(new Rect(29, 59, w - 370, 24), "DEAD LETTER OFFICE  /  " + game.ConstellationOwned + " OF 36 RECOVERED · Unlock patterns for future loot.", 13, muted);
            Label(new Rect(w - 338, 26, 310, 30), game.Sparks + " SPARKS", 25, Accent, true, TextAnchor.MiddleRight);
            Label(new Rect(w - 338, 61, 310, 20), game.CompletionSeal ? "COMPLETION SEAL RECOVERED" : "DEEPER NODES REQUIRE THE FOREMAN'S SEAL", 10, muted, true, TextAnchor.MiddleRight);
            DrawOfficeChart(w, h);
            for (int b = 0; b < 7; b++)
            {
                var r = OfficeBranchButton(b, w, h); var ids = ConstellationContent.BranchItems[b];
                Plate(r, b == officeBranch ? new Color(.06f, .13f, .17f) : ink, b == officeBranch ? Accent : line, 4);
                Label(r, ConstellationContent.BranchNames[b] + "  " + ids.Count(game.HasConstellation) + "/" + ids.Length, 11, b == officeBranch ? white : muted, true, TextAnchor.MiddleCenter);
            }
            DrawOfficeDetail(w, h);
            float toolsY = map.yMax + 10;
            Label(new Rect(map.x, toolsY + 5, map.width - 235, 20), "DRAG TO PAN · SCROLL TO ZOOM   /   ✓ OWNED   ○ AVAILABLE   ▣ LOCKED", 10, muted);
            if (Button(new Rect(map.xMax - 221, toolsY, 30, 27), "−")) ZoomOfficeChart(1 / 1.2f, map.center, w, h);
            Label(new Rect(map.xMax - 186, toolsY + 3, 52, 24), Mathf.RoundToInt(officeZoom * 100) + "%", 11, white, true, TextAnchor.MiddleCenter);
            if (Button(new Rect(map.xMax - 129, toolsY, 30, 27), "+")) ZoomOfficeChart(1.2f, map.center, w, h);
            if (Button(new Rect(map.xMax - 91, toolsY, 91, 27), "FIT CHART")) FitOfficeChart();
            var ev = Event.current;
            if (ev.type == EventType.ScrollWheel && map.Contains(Pointer)) { ZoomOfficeChart(Mathf.Exp(-ev.delta.y * .09f), Pointer, w, h); ev.Use(); }
            if (ev.button == 0 && HandleOfficePointer(ev.type, Pointer, w, h)) ev.Use();
            if (ev.type == EventType.KeyDown && ev.keyCode == KeyCode.Home) { FitOfficeChart(); ev.Use(); }
            if (ev.type == EventType.KeyDown && ev.keyCode == KeyCode.Tab)
            {
                officeSelection = (officeSelection + (ev.shift ? 35 : 1)) % 36;
                officeBranch = ConstellationContent.Branch(Constellation.Nodes[officeSelection].item);
                officePan = -OfficePosition(officeSelection); officeDetailScroll = Vector2.zero; ev.Use();
            }
            float foot = h - 66, width = (w - 80) / 3;
            if (Button(new Rect(28, foot, width, 38), "START JOURNEY · ~30 MIN", true)) game.StartChapter();
            if (ExpeditionButton(new Rect(40 + width, foot, width, 38), game.HasCheckpoint ? game.ResumeLabel : "NO SAVED JOURNEY", game.HasCheckpoint))
            { if (game.ResumeExpedition()) game.CloseOffice(); }
            if (Button(new Rect(52 + width * 2, foot, width, 38), "BACK TO MAIN MENU")) game.CloseOffice();
            Label(new Rect(28, h - 21, w - 56, 14), game.OfficeMessage.Length > 0 ? game.OfficeMessage : "Purchases unlock future loot. Saved journeys keep their pool.  ·  Dotted paths are open entries; solid paths require the previous node.", 10, muted);
        }
    }
}
