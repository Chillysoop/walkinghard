using System;
using UnityEngine;

namespace VectorGame
{
    public sealed partial class PrototypeHUD
    {
        internal void CheckAstralChart(Action<bool, string> require)
        {
            int sparks = game.Sparks, owned = game.ConstellationOwned;
            foreach (float width in new[] { 1280f, 1706.667f })
            {
                const float height = 720;
                FitOfficeChart(); var map = OfficeMap(width, height);
                for (int i = 0; i < Constellation.Nodes.Length; i++)
                {
                    var r = OfficeStar(i, width, height);
                    require(OfficeVisible(r, map), "fit contains node / " + width + " / " + i);
                    require(HandleOfficePointer(EventType.MouseDown, r.center, width, height) && OfficeSelection == i,
                        "all branches selectable on one chart / " + width + " / " + i);
                    for (int j = 0; j < i; j++)
                        require(Vector2.Distance(r.center, OfficeStar(j, width, height).center) > (r.width + OfficeStar(j, width, height).width) / 2,
                            "distinct circular node hit areas / " + width + " / " + i + ":" + j);
                }
                var anchor = map.center + new Vector2(45, -30);
                var world = (anchor - map.center) / OfficeScale(width, height) - officePan;
                ZoomOfficeChart(1.5f, anchor, width, height);
                require(Vector2.Distance(OfficePoint(world, width, height), anchor) < .01f, "zoom preserves the point under the cursor");
                ZoomOfficeChart(.1f, map.center, width, height); require(Mathf.Approximately(officeZoom, .8f), "minimum zoom bounded");
                ZoomOfficeChart(100, map.center, width, height); require(Mathf.Approximately(officeZoom, 3.4f), "maximum zoom bounded");
                FitOfficeChart(); var start = map.min + Vector2.one * 12;
                require(HandleOfficePointer(EventType.MouseDown, start, width, height), "empty chart starts drag");
                var buy = OfficeNodeButton(officeSelection, width, height).center;
                require(HandleOfficePointer(EventType.MouseDrag, buy, width, height), "pan tracks drag outside the chart");
                require(HandleOfficePointer(EventType.MouseUp, buy, width, height) && !officePanning, "release over purchase cannot buy");
                require(game.Sparks == sparks && game.ConstellationOwned == owned, "navigation never spends Sparks");
                FitOfficeChart(); require(officePan == Vector2.zero && officeZoom == 1, "fit restores the whole chart");
                for (int branch = 0; branch < 7; branch++)
                {
                    require(HandleOfficePointer(EventType.MouseDown, OfficeBranchButton(branch, width, height).center, width, height), "branch focus click handled");
                    require(officeBranch == branch && OfficeVisible(OfficeStar(officeSelection, width, height), map), "focused branch selects a visible entry");
                }
            }
            var a = new Vector2(-50, 50); var b = new Vector2(150, 50); var clip = new Rect(0, 0, 100, 100);
            require(OfficeClip(ref a, ref b, clip) && a == new Vector2(0, 50) && b == new Vector2(100, 50), "off-chart links clip to panel bounds");
            a = new Vector2(-50, -50); b = new Vector2(150, -50);
            require(!OfficeClip(ref a, ref b, clip), "fully off-chart links are omitted");
            SelectOfficeBranch(0); FitOfficeChart();
        }
    }
}
