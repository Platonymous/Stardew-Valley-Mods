using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using PyTK.Types;
using PyTK;
using PyTK.Extensions;

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

                        tree.performUseAction(entry.Key, Game1.currentLocation);
                        continue;
                    }

                    if (entry.Value.Value is FruitTree ftree)
                    {
                        if (!playedToday)
                        {
                            ftree.growthStage.Value = (ftree.growthStage.Value <= 5) ? ftree.growthStage.Value + 1 : ftree.growthStage.Value;
                            ftree.daysUntilMature.Value = ftree.daysUntilMature.Value - 7;
                        }
                        ftree.performUseAction(entry.Key, Game1.currentLocation);
                        continue;
                    }

                    if (entry.Value.Value is Grass grass)
                    {
                        if (!playedToday)
                            grass.numberOfWeeds.Value = Math.Min(grass.numberOfWeeds.Value + Game1.random.Next(1, 4), 4);
                        grass.doCollisionAction(gls.terrainFeatures[entry.Key].getBoundingBox(entry.Key), 3, entry.Key, Game1.player, Game1.currentLocation);
                        continue;
                    }

                    if (entry.Value.Value is Bush bush)
                    {
                        bush.performUseAction(entry.Key, Game1.currentLocation);
                        continue;
                    }
                }
                priorRadius = Game1.player.MagneticRadius;
                Game1.player.MagneticRadius += 2000;

                PyUtils.setDelayedAction(8000, () => Game1.player.MagneticRadius = priorRadius);
            }
        }
    }
}
