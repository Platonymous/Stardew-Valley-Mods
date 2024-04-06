using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;

namespace HarpOfYobaRedux
{
    class TreeMagic : IMagic
    {
        private int priorRadius;

        public TreeMagic()
        {

        }

        public void doMagic(bool playedToday)
        {
            if (Game1.currentLocation.IsOutdoors || Game1.currentLocation.Name.Equals("Greenhouse") || Game1.currentLocation.IsGreenhouse)
            {
                GameLocation gls = Game1.currentLocation;

                foreach(var entry in gls.terrainFeatures.FieldDict)
                {
                    if (entry.Value.Value is Tree tree)
                    {
                        if (!playedToday)
                            tree.growthStage.Value = (tree.growthStage.Value < 5) ? tree.growthStage.Value + 1 : tree.growthStage.Value;

                        tree.performUseAction(entry.Key);
                        continue;
                    }

                    if (entry.Value.Value is FruitTree ftree)
                    {
                        if (!playedToday)
                        {
                            ftree.growthStage.Value = (ftree.growthStage.Value <= 5) ? ftree.growthStage.Value + 1 : ftree.growthStage.Value;
                            ftree.daysUntilMature.Value = ftree.daysUntilMature.Value - 7;
                        }
                        ftree.performUseAction(entry.Key);
                        continue;
                    }

                    if (entry.Value.Value is Grass grass)
                    {
                        if (!playedToday)
                            grass.numberOfWeeds.Value = Math.Min(grass.numberOfWeeds.Value + Game1.random.Next(1, 4), 4);
                        grass.doCollisionAction(gls.terrainFeatures[entry.Key].getBoundingBox(), 3, entry.Key, Game1.player);
                        continue;
                    }

                    if (entry.Value.Value is Bush bush)
                    {
                        bush.performUseAction(entry.Key);
                        continue;
                    }
                }
                Buff magneticBuff = new Buff("hoy.magnetic", displayName: "Magnetism", duration: 8000 + Game1.random.Next(2000), description: "");

                magneticBuff.millisecondsDuration = 35000 + Game1.random.Next(30000);
                magneticBuff.iconSheetIndex = 1;
                magneticBuff.iconTexture = Game1.buffsIcons;

                magneticBuff.effects.FarmingLevel.Value = 1;
                    magneticBuff.description = "";
                    magneticBuff.displayName = "Magnetism";
                    magneticBuff.millisecondsDuration = 8000 + Game1.random.Next(2000);
                    magneticBuff.effects.MagneticRadius.Value = 2000;

                magneticBuff.glow = Color.YellowGreen;
                if (!Game1.player.hasBuff("hoy.magnetic"))
                    Game1.player.applyBuff(magneticBuff);
            }
        }
    }
}
