using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using xTile;
using xTile.Layers;
using xTile.Tiles;

namespace TMXLoader
{
    public enum EditType
    {
        Merge,
        Warps,
        Replace,
        Festival,
        SpouseRoom
    }
    public class TMXAssetEditor
    {
        private MapEdit edit;
        private NPCPlacement npcedit;
        private Map newMap;
        private EditType type;
        public string assetName;
        public string conditions;
        public string inLocation;
        public SaveBuildable saveBuildable = null;

        public bool lastCheck = true;

        public string ContentPackId { get; }

        public override bool Equals(object obj)
        {
            return obj is TMXAssetEditor tmxe && edit == tmxe.edit && inLocation == tmxe.inLocation && assetName == tmxe.assetName;
        }

        public override int GetHashCode()
        {
            return (edit.GetHashCode() + ":" + inLocation + ":" + assetName + ":" + edit.position).GetHashCode();
        }

        public TMXAssetEditor(string contentPackId, MapEdit edit, Map map, EditType type)
        {
            this.ContentPackId = contentPackId;
            this.edit = edit;
            this.type = type;
            this.newMap = map;
            this.assetName = edit is BuildableEdit be? be._mapName : edit.name;
            this.inLocation = null;
            if (edit is BuildableEdit b)
                this.inLocation = b._location;

            this.conditions = edit is BuildableEdit ? "" : edit.conditions;
            lastCheck = conditions == "";
        }

        public TMXAssetEditor(string contentPackId, NPCPlacement npcedit, EditType type)
        {
            this.ContentPackId = contentPackId;

            this.npcedit = npcedit;
            this.type = type;
            this.assetName = npcedit.map;
            this.conditions = npcedit.conditions;
            this.inLocation = null;
            lastCheck = conditions == "";
        }
        public bool CanEdit(IAssetName asset)
        {
            if (saveBuildable != null && !TMXLoaderMod.buildablesBuild.Contains(saveBuildable))
                return false;

            return asset.IsEquivalentTo(edit is BuildableEdit ? assetName : $"Maps/{assetName}");
        }

        public void Edit(IAssetData asset)
        {
            if (saveBuildable != null && !TMXLoaderMod.buildablesBuild.Contains(saveBuildable))
                return;

            if (!lastCheck)
                return;
            Map map = newMap;
            Map original = (Map) asset.Data ;


            if (type == EditType.Merge)
            {
                if (edit.sourceArea.Length > 4)
                {
                    Map merged = original;
                    for (int i = 0, j = 0; i < edit.sourceArea.Length && j < edit.position.Length; i += 4, j += 2)
                        merged = map.mergeInto(merged, new Vector2(edit.position[j], edit.position[j + 1]), new Rectangle(edit.sourceArea[i], edit.sourceArea[i + 1], edit.sourceArea[i + 2], edit.sourceArea[i + 3]), edit.removeEmpty);
                    map = merged;
                }
                else
                {
                    Rectangle? sourceArea = null;
                    if (edit.sourceArea.Length == 4)
                        sourceArea = new Rectangle(edit.sourceArea[0], edit.sourceArea[1], edit.sourceArea[2], edit.sourceArea[3]);
                    map = map.mergeInto(original, new Vector2(edit.position[0], edit.position[1]), sourceArea, edit.removeEmpty);
                }
                editWarps(map, edit.addWarps, edit.removeWarps, original);
            }
            else if(type == EditType.Warps)
                editWarps(original, edit.addWarps, edit.removeWarps, original);
            else if(type == EditType.Replace)
            {
                original = edit.retainWarps ? original : map;
                editWarps(map, edit.addWarps, edit.removeWarps, original);
            }else if(type == EditType.Festival)
            {
                Texture2D springTex = TMXLoaderMod.helper.GameContent.Load<Texture2D>("Maps/spring_outdoorsTileSheet");
                Dictionary<string, string> source = TMXLoaderMod.helper.GameContent.Load<Dictionary<string, string>>("Data/NPCDispositions");
                int index = source.Keys.ToList().IndexOf(npcedit.name);
                TileSheet spring = original.GetTileSheet("ztemp");
                if (spring == null)
                {
                    spring = new TileSheet("ztemp", original, "Maps/spring_outdoorsTileSheet", new xTile.Dimensions.Size(springTex.Width, springTex.Height), original.TileSheets[0].TileSize);
                    original.AddTileSheet(spring);
                }
                if (index >= 0)
                {
                    original.GetLayer("Set-Up").Tiles[npcedit.position[0], npcedit.position[1]] = new StaticTile(original.GetLayer("Set-Up"), spring, BlendMode.Alpha, (index * 4) + npcedit.direction);
                    if (original.GetLayer("MainEvent") is Layer mLayer)
                    {
                        if(npcedit.position2[0] == -1 || npcedit.position2[1] == -1)
                            mLayer.Tiles[npcedit.position[0], npcedit.position[1]] = new StaticTile(original.GetLayer("MainEvent"), spring, BlendMode.Alpha, (index * 4) + npcedit.direction);
                        else
                            mLayer.Tiles[npcedit.position2[0], npcedit.position2[1]] = new StaticTile(original.GetLayer("MainEvent"), spring, BlendMode.Alpha, (index * 4) + npcedit.direction2);
                    }
                }
            }else if(type == EditType.SpouseRoom)
            {
                if (edit.info != "none")
                    foreach (Layer layer in map.Layers)
                        layer.Id = layer.Id.Replace("Spouse", edit.info);

                string eAction = "Lua Platonymous.TMXLoader.SpouseRoom entry";

                if (map.Properties.ContainsKey("EntryAction"))
                    map.Properties["EntryAction"] = eAction + ";" + map.Properties["EntryAction"];
                else
                    map.Properties.Add("EntryAction", eAction);

                map = map.mergeInto(original, new Vector2(edit.position[0], edit.position[1]), null, true);
            }

            if (map == null)
                map = original;

            asset.ReplaceWith(map);
        }

        public static void editWarps(Map map, string[] addWarps, string[] removeWarps, Map original = null)
        {
            if (!map.Properties.ContainsKey("Warp"))
                map.Properties.Add("Warp", "");

            string warps = "";

            if (original != null && original.Properties.ContainsKey("Warp") && !(removeWarps.Length > 0 && removeWarps[0] == "all"))
                warps = original.Properties["Warp"];

            if (addWarps.Length > 0)
                warps = (warps.Length > 9 ? warps + " " : "") + String.Join(" ", addWarps);

            if (removeWarps.Length > 0 && removeWarps[0] != "all")
            {
                foreach (string warp in removeWarps)
                {
                    warps = warps.Replace(warp + " ", "");
                    warps = warps.Replace(" " + warp, "");
                    warps = warps.Replace(warp, "");
                }
            }

            map.Properties["Warp"] = warps;
        }
    }


}
