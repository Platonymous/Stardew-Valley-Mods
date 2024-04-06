using StardewValley;
using StardewValley.TerrainFeatures;
using StardewModdingAPI;
using HarmonyLib;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace NoSoilDecayRedux
{
    public class NoSoilDecayReduxMod : Mod
    {
        private static CodeInstruction replacement = null;
        private static bool IsDayUpdate = false;

        public override void Entry(IModHelper helper)
        {
            var harmony = new Harmony("Platonymous.NoSoilDecay");

            harmony.Patch(AccessTools.Method(typeof(NoSoilDecayReduxMod), nameof(NoSoilDecayReduxMod.SpawnWeedsAndStonesReplacer)),
                transpiler: new HarmonyMethod(typeof(NoSoilDecayReduxMod), nameof(SpawnWeedsAndStonesReplacerTranspiler)));

            harmony.Patch(AccessTools.Method(typeof(GameLocation), nameof(GameLocation.spawnWeedsAndStones)),
                transpiler: new HarmonyMethod(typeof(NoSoilDecayReduxMod), nameof(SpawnWeedsAndStones)));

            harmony.Patch(AccessTools.Method(typeof(Farm), nameof(Farm.DayUpdate)),
                prefix: new HarmonyMethod(typeof(NoSoilDecayReduxMod), nameof(DayUpdateFarm1)),
                postfix: new HarmonyMethod(typeof(NoSoilDecayReduxMod), nameof(DayUpdateFarm2)));

            harmony.Patch(AccessTools.PropertyGetter(typeof(HoeDirt), nameof(HoeDirt.crop)),
                postfix: new HarmonyMethod(typeof(NoSoilDecayReduxMod), nameof(GetCrop)));

        }

        public static void GetCrop(HoeDirt __instance, ref Crop __result)
        {
            if (!IsDayUpdate || __result != null)
                return;

                __result = new FakeCrop();
        }

        public static void DayUpdateFarm1()
        {
            IsDayUpdate = true;
        }

        public static void DayUpdateFarm2()
        {
            IsDayUpdate = false;
        }
        public static IEnumerable<CodeInstruction> SpawnWeedsAndStonesReplacerTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            string search = "callvirt:111:Boolean ContainsKey(Microsoft.Xna.Framework.Vector2)";
            foreach (var instruction in instructions)
            {
                var line = instruction.opcode.Name + ":" + instruction.opcode.Value + ":" + instruction.operand?.ToString();

                if (line == search)
                    replacement = instruction;

                yield return instruction;
            }
        }

        public static void SpawnWeedsAndStonesReplacer()
        {
            var terrainFeatures = Game1.currentLocation.terrainFeatures;
            var vector = Vector2.One;
            var vector2 = Vector2.One;

            terrainFeatures.ContainsKey(vector + vector2);
        }

        public static IEnumerable<CodeInstruction> SpawnWeedsAndStones(IEnumerable<CodeInstruction> instructions)
        {
            string initLine = "ldloca.s:18:StardewValley.TerrainFeatures.TerrainFeature (19)";
            string foundLine = "callvirt:111:Boolean Remove(Microsoft.Xna.Framework.Vector2)";
            bool init = false;
                foreach (var instruction in instructions)
                {
                    bool skip = false;

                        var line = instruction.opcode.Name + ":" + instruction.opcode.Value + ":" + instruction.operand?.ToString();

                        if (line == initLine)
                            init = true;

                        if (init && line == foundLine)
                            skip = true;

                    if (!skip)
                    {
                        yield return instruction;
                    }
                    else
                    {
                        if (replacement != null)
                        {
                            yield return replacement;
                        }
                    }
                }
        }
    }

    public class FakeCrop : Crop
    {
        public FakeCrop()
        {

        }
    }

}
