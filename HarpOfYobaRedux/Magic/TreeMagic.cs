using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.TerrainFeatures;
using System;
using System.Collections.Generic;
using System.Linq;
using PyTK.Extensions;
using PyTK.Types;
using PyTK;

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
                List<Vector2> treetiles = new TerrainSelector<TerrainFeature>(t => (t is Tree || t is FruitTree || t is Grass || t is Bush)).keysIn(gls);

                for (int i = 0; i < treetiles.Count; i++)
                {
                    if (gls.terrainFeatures[treetiles[i]] is Tree)
                    {
                        if (!playedToday)
                        {
                            Tree tree = gls.terrainFeatures[treetiles[i]] as Tree;
                            tree.growthStage = (tree.growthStage < 4) ? tree.growthStage + 1 : tree.growthStage;
                            gls.terrainFeatures[treetiles[i]] = tree;
                        }
                        (gls.terrainFeatures[treetiles[i]] as Tree).performUseAction(treetiles[i]);
                    }

                    if (gls.terrainFeatures[treetiles[i]] is FruitTree)
                    {
                        if (!playedToday)
                        {
                            FruitTree tree = (gls.terrainFeatures[treetiles[i]] as FruitTree);
                            tree.growthStage = (tree.growthStage <= 4) ? tree.growthStage + 1 : tree.growthStage;
                            tree.daysUntilMature = tree.daysUntilMature - 7;
                            gls.terrainFeatures[treetiles[i]] = tree;
                        }
                       (gls.terrainFeatures[treetiles[i]] as FruitTree).performUseAction(treetiles[i]);
                    }

                    if (gls.terrainFeatures[treetiles[i]] is Grass)
                        if (!playedToday)
                        {
                            Grass grass = (gls.terrainFeatures[treetiles[i]] as Grass);
                            grass.numberOfWeeds = Math.Min(grass.numberOfWeeds + Game1.random.Next(1, 4), 4);
                            gls.terrainFeatures[treetiles[i]] = grass;
                        }
                        (gls.terrainFeatures[treetiles[i]] as Grass).doCollisionAction(gls.terrainFeatures[treetiles[i]].getBoundingBox(treetiles[i]), 3, treetiles[i], Game1.player, Game1.currentLocation);

                    if (gls.terrainFeatures[treetiles[i]] is Bush)
                        (gls.terrainFeatures[treetiles[i]] as Bush).performUseAction(treetiles[i]);
                }
                priorRadius = Game1.player.magneticRadius;
                Game1.player.magneticRadius += 2000;

                PyUtils.setDelayedAction(8000, () => Game1.player.magneticRadius = priorRadius);
            }
        }
    }
}
