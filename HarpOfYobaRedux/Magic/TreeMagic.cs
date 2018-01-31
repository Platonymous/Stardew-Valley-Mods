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
            if (Game1.currentLocation.isOutdoors || Game1.currentLocation.name.Equals("Greenhouse"))
            {
                GameLocation gls = Game1.currentLocation;

                foreach(KeyValuePair<Vector2,TerrainFeature> entry in gls.terrainFeatures)
                {
                    if (entry.Value is Tree tree)
                    {
                        if (!playedToday)
                            tree.growthStage = (tree.growthStage < 5) ? tree.growthStage + 1 : tree.growthStage;

                        tree.performUseAction(entry.Key);
                        continue;
                    }

                    if (entry.Value is FruitTree ftree)
                    {
                        if (!playedToday)
                        {
                            ftree.growthStage = (ftree.growthStage <= 5) ? ftree.growthStage + 1 : ftree.growthStage;
                            ftree.daysUntilMature = ftree.daysUntilMature - 7;
                        }
                        ftree.performUseAction(entry.Key);
                        continue;
                    }

                    if (entry.Value is Grass grass)
                    {
                        if (!playedToday)
                            grass.numberOfWeeds = Math.Min(grass.numberOfWeeds + Game1.random.Next(1, 4), 4);
                        grass.doCollisionAction(gls.terrainFeatures[entry.Key].getBoundingBox(entry.Key), 3, entry.Key, Game1.player, Game1.currentLocation);
                        continue;
                    }

                    if (entry.Value is Bush bush)
                    {
                        bush.performUseAction(entry.Key);
                        continue;
                    }
                }
                priorRadius = Game1.player.magneticRadius;
                Game1.player.magneticRadius += 2000;

                PyUtils.setDelayedAction(8000, () => Game1.player.magneticRadius = priorRadius);
            }
        }
    }
}
