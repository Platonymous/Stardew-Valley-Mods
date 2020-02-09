using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Objects;
using PyTK.CustomElementHandler;
using System;
using Netcode;

namespace CustomFurnitureAnywhere
{
    class AnywhereCustomFurniture : CustomFurniture.CustomFurniture, ISaveElement
    {
        public AnywhereCustomFurniture()
        {

        }

        public AnywhereCustomFurniture(CustomFurniture.CustomFurniture item) : base(item.data, item.id, item.TileLocation)
        {
            this.defaultBoundingBox.Value = item.defaultBoundingBox.Value;
            this.boundingBox.Value = item.boundingBox.Value;
            this.currentRotation.Value = item.currentRotation.Value;
            this.rotations.Value = item.rotations.Value;
            this.furniture_type.Value = item.furniture_type.Value;
            this.rotate();
            this.rotate();
            this.rotate();
            this.rotate();
        }

        public Furniture Revert()
        {
            CustomFurniture.CustomFurniture self = new CustomFurniture.CustomFurniture(this.data, this.id, this.TileLocation);
            self.defaultBoundingBox.Value = this.defaultBoundingBox.Value;
            self.boundingBox.Value = this.boundingBox.Value;
            self.currentRotation.Value = this.currentRotation.Value;
            self.rotations.Value = this.rotations.Value;
            self.sourceRect.Value = this.sourceRect.Value;
            self.rotate();
            self.rotate();
            self.rotate();
            self.rotate();
            return self;
        }
        public override bool isPassable()
        {
            return this.furniture_type.Value == 12;
        }
        public override string getCategoryName()
        {
            return "CustomFurnitureAnywhere";
        }
        public override bool performObjectDropInAction(Item dropInItem, bool probe, Farmer who)
        {
            return false;
        }
        public override bool canBePlacedHere(GameLocation l, Vector2 tile)
        {
            for (int index1 = 0; index1 < this.boundingBox.Width / Game1.tileSize; ++index1)
            {
                for (int index2 = 0; index2 < this.boundingBox.Height / Game1.tileSize; ++index2)
                {
                    Vector2 key = tile + new Vector2(index1, index2);
                    if (l.Objects.ContainsKey(key))
                    {
                        if (l.objects[key] is CustomFurniture.CustomFurniture)
                        {
                            Vector2 vector2 = key * Game1.tileSize - new Vector2(Game1.tileSize / 2);
                            CustomFurniture.CustomFurniture furniture = (CustomFurniture.CustomFurniture)l.objects[key];
                            if (furniture.furniture_type.Value == 11 && (furniture.getBoundingBox(furniture.TileLocation).Contains((int)vector2.X, (int)vector2.Y) && furniture.heldObject.Value == null && this.getTilesWide() == 1))
                                return true;
                            if ((furniture.furniture_type.Value != 12 || this.furniture_type.Value == 12) && furniture.getBoundingBox(furniture.TileLocation).Contains((int)vector2.X, (int)vector2.Y))
                                return false;
                        }
                        return false;
                    }
                }
            }
            if (this.ParentSheetIndex == 710 && l.doesTileHaveProperty((int)tile.X, (int)tile.Y, "Water", "Back") != null && (!l.objects.ContainsKey(tile) && l.doesTileHaveProperty((int)tile.X + 1, (int)tile.Y, "Water", "Back") != null) && l.doesTileHaveProperty((int)tile.X - 1, (int)tile.Y, "Water", "Back") != null || l.doesTileHaveProperty((int)tile.X, (int)tile.Y + 1, "Water", "Back") != null && l.doesTileHaveProperty((int)tile.X, (int)tile.Y - 1, "Water", "Back") != null || (this.parentSheetIndex == 105 && this.bigCraftable && (l.terrainFeatures.ContainsKey(tile) && l.terrainFeatures[tile] is StardewValley.TerrainFeatures.Tree) && !l.objects.ContainsKey(tile) || this.name != null && this.name.Contains("Bomb") && (!l.isTileOccupiedForPlacement(tile, this) || l.isTileOccupiedByFarmer(tile) != null)))
                return true;
            return !l.isTileOccupiedForPlacement(tile, this);
        }
        public override bool placementAction(GameLocation location, int x, int y, StardewValley.Farmer who = null)
        {
            Point point = new Point(x / Game1.tileSize, y / Game1.tileSize);
            this.TileLocation = new Vector2(point.X, point.Y);
            if (this.furniture_type.Value == 6 || this.furniture_type.Value == 13 || this.ParentSheetIndex == 1293)
            {
                Game1.showRedMessage("Can only be placed in House");
                return false;
            }
            for (int index1 = point.X; index1 < point.X + this.getTilesWide(); ++index1)
            {
                for (int index2 = point.Y; index2 < point.Y + this.getTilesHigh(); ++index2)
                {
                    if (location.doesTileHaveProperty(index1, index2, "NoFurniture", "Back") != null)
                    {
                        Game1.showRedMessage("Furniture can't be placed here");
                        return false;
                    }
                    if (location.getTileIndexAt(index1, index2, "Buildings") != -1)
                        return false;
                }
            }
            this.boundingBox.Value = new Rectangle(x / Game1.tileSize * Game1.tileSize, y / Game1.tileSize * Game1.tileSize, this.boundingBox.Width, this.boundingBox.Height);
            foreach (Character character in Game1.getAllFarmers())
            {
                if (character.currentLocation == location && character.GetBoundingBox().Intersects(this.boundingBox.Value))
                {
                    Game1.showRedMessage("Can't place on top of a person.");
                    return false;
                }
            }
            foreach (StardewValley.Object i in location.objects.Values)
            {
                if (i is CustomFurniture.CustomFurniture)
                {
                    CustomFurniture.CustomFurniture furniture = (CustomFurniture.CustomFurniture)i;
                    
                    if (furniture.getBoundingBox(furniture.TileLocation).Intersects(this.boundingBox.Value))
                    {
                        Game1.showRedMessage("Furniture can't be placed here");
                        return false;
                    }
                }
            }
            this.updateDrawPosition();
            if (!this.performDropDownAction(who))
            {
                StardewValley.Object @object = (StardewValley.Object)this.getOne();
                @object.shakeTimer = 50;
                @object.TileLocation = this.TileLocation;
                if (location.objects.ContainsKey(this.TileLocation))
                {
                    if (location.objects[this.TileLocation].ParentSheetIndex != this.ParentSheetIndex)
                    {
                        Game1.createItemDebris(location.objects[this.TileLocation], this.TileLocation * Game1.tileSize, Game1.random.Next(4), null);
                        location.objects[this.TileLocation] = @object;
                    }
                }
                else
                    location.objects.Add(this.TileLocation, @object);
                (@object as AnywhereCustomFurniture).sourceRect.Value = this.sourceRect.Value;
                (@object as AnywhereCustomFurniture).boundingBox.Value = this.boundingBox.Value;
                @object.initializeLightSource(this.TileLocation);
            }

            Game1.playSound("woodyStep");
            return true;
        }

        public override Rectangle getBoundingBox(Vector2 tileLocation)
        {
            this.boundingBox.X = (int)tileLocation.X * Game1.tileSize;
            this.boundingBox.Y = (int)tileLocation.Y * Game1.tileSize;
            return boundingBox;
        }

        public override bool performToolAction(Tool t, GameLocation location)
        {
            return base.performToolAction(t, location);
        }

        public override Item getOne()
        {
            AnywhereCustomFurniture furniture = new AnywhereCustomFurniture(this);
            furniture.drawPosition.Value = this.drawPosition.Value;
            furniture.defaultBoundingBox.Value = this.defaultBoundingBox.Value;
            furniture.boundingBox.Value = this.boundingBox.Value;
            furniture.currentRotation.Value = this.currentRotation.Value;
            furniture.rotations.Value = this.rotations.Value;
            furniture.furniture_type.Value = this.furniture_type.Value;
            furniture.sourceRect.Value = this.sourceRect.Value;
            furniture.rotate();
            furniture.rotate();
            furniture.rotate();
            furniture.rotate();
            return furniture;
        }
        public override bool clicked(StardewValley.Farmer who)
        {
            Console.Write("Clicked");

            Game1.haltAfterCheck = false;
            if (furniture_type.Value == 11 && who.ActiveObject != null && (who.ActiveObject != null && this.heldObject.Value == null))
                return false;
            if (this.heldObject.Value == null && (who.ActiveObject == null || !(who.ActiveObject is Furniture)))
            {
                if (who.addItemToInventoryBool((Item)this.Revert(), false))
                {
                    Game1.playSound("coin");
                    return true;
                }
                return true;
            }
            if (this.heldObject.Value != null)
            {
                StardewValley.Object @object = this.heldObject.Value;
                this.heldObject.Value = (StardewValley.Object)null;
                if (who.addItemToInventoryBool((Item)this.Revert(), false))
                {
                    @object.performRemoveAction(TileLocation, who.currentLocation);
                    Game1.playSound("coin");
                    return true;
                }
                this.heldObject.Value = @object;
            }
            return false;
        }
    }
}