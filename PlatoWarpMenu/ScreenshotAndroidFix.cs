using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.IO;
using Netcode;
using xTile.ObjectModel;
using Microsoft.Xna.Framework;
using StardewValley.BellsAndWhistles;
using StardewValley.Locations;
using StardewModdingAPI;
using PlatoTK;
using PlatoTK.Reflection;

namespace PlatoWarpMenu
{
    public class ScreenshotAndroidFix
    {

        public ScreenshotAndroidFix()
        {

        }

        public string InterceptScreenshotAndroid(Texture2D texture, string filename)
        {
            filename = Path.Combine(PlatoWarpMenuMod.tempFolder, Path.GetFileName(filename));

            if (PlatoWarpMenuMod.instance.config.UseTempFolder || Constants.TargetPlatform == GamePlatform.Android)
                return filename;

            using (var mem = new MemoryStream())
            {
                texture.SaveAsPng(mem, texture.Width, texture.Height);
                PlatoWarpMenuMod.LastScreen = Texture2D.FromStream(Game1.graphics.GraphicsDevice, mem);
            }

            return "";
        }

        public bool takeMapScreenshot(Game1 game, float scale, string screenshot_name)
        {
            if (Game1.currentLocation == null)
                return false;

            string path2_1 = screenshot_name + ".png";

            int mapX = 0;
            int mapY = 0;
            int mapWidth = Game1.currentLocation.map.DisplayWidth;
            int mapHeight = Game1.currentLocation.map.DisplayHeight;
            try
            {
                PropertyValue propertyValue = (PropertyValue)null;
                if (Game1.currentLocation.map.Properties.TryGetValue("ScreenshotRegion", out propertyValue))
                {
                    string[] strArray = propertyValue.ToString().Split(' ');
                    mapX = int.Parse(strArray[0]) * 64;
                    mapY = int.Parse(strArray[1]) * 64;
                    mapWidth = (int.Parse(strArray[2]) + 1) * 64 - mapX;
                    mapHeight = (int.Parse(strArray[3]) + 1) * 64 - mapY;
                }
            }
            catch
            {
                mapX = 0;
                mapY = 0;
                mapWidth = Game1.currentLocation.map.DisplayWidth;
                mapHeight = Game1.currentLocation.map.DisplayHeight;
            }

            Texture2D bitmap = null;
            bool flag1;
            int scaledMapWidth;
            int scaledMapHeight;
            do
            {
                flag1 = false;
                scaledMapWidth = (int)((double)mapWidth * (double)scale);
                scaledMapHeight = (int)((double)mapHeight * (double)scale);
                try
                {
                    bitmap = new Texture2D(game.GraphicsDevice, scaledMapWidth, scaledMapHeight);
                }
                catch
                {
                    flag1 = true;
                }
                if (flag1)
                    scale -= 0.25f;
                if ((double)scale <= 0.0)
                    return false;
            }
            while (flag1);

            int maxSize = 2048;
            int scaledMax = (int)(maxSize * (double)scale);
            xTile.Dimensions.Rectangle viewport = Game1.viewport;
            bool displayHud = Game1.displayHUD;
            game.takingMapScreenshot = true;
            float zoomLevel = Game1.options.zoomLevel;
            Game1.options.zoomLevel = 1f;
            RenderTarget2D lightmap = (RenderTarget2D)game.GetFieldValue("_lightmap");
            game.SetFieldValue(null, "_lightmap");
            try
            {
                typeof(Game1).CallAction("allocateLightmap", maxSize, maxSize);
                int cols = (int)Math.Ceiling((double)scaledMapWidth / (double)scaledMax);
                int rows = (int)Math.Ceiling((double)scaledMapHeight / (double)scaledMax);

                Texture2D[,] textureMap = new Texture2D[rows, cols];
                bitmap = new Texture2D(game.GraphicsDevice, cols * scaledMax, rows * scaledMax);
                for (int row = 0; row < rows; ++row)
                {
                    for (int col = 0; col < cols; ++col)
                    {
                        int captureWidth = scaledMax;
                        int captureHeight = scaledMax;
                        int x = col * scaledMax;
                        int y = row * scaledMax;
                        if (x + scaledMax > scaledMapWidth)
                            captureWidth += scaledMapWidth - (x + scaledMax);
                        if (y + scaledMax > scaledMapHeight)
                            captureHeight += scaledMapHeight - (y + scaledMax);
                        if (captureHeight > 0 && captureWidth > 0)
                        {
                            Rectangle rect = new Rectangle(x, y, captureWidth, captureHeight);
                            RenderTarget2D target_screen = new RenderTarget2D(Game1.graphics.GraphicsDevice, maxSize, maxSize, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
                            Game1.viewport = new xTile.Dimensions.Rectangle(col * maxSize + mapX, row * maxSize + mapY, maxSize, maxSize);

                            _draw(game, Game1.currentGameTime, target_screen);
                            RenderTarget2D renderTarget = new RenderTarget2D(Game1.graphics.GraphicsDevice, captureWidth, captureHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.DiscardContents);
                            game.GraphicsDevice.SetRenderTarget(renderTarget);
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Opaque, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
                            Color white = Color.White;
                            Game1.spriteBatch.Draw(target_screen, Vector2.Zero, new Microsoft.Xna.Framework.Rectangle?(target_screen.Bounds), white, 0.0f, Vector2.Zero, scale, SpriteEffects.None, 1f);
                            Game1.spriteBatch.End();
                            target_screen.Dispose();
                            game.GraphicsDevice.SetRenderTarget(null);

                            Color[] data = new Color[captureWidth * captureHeight];
                            renderTarget.GetData(data);
                            Texture2D texture = new Texture2D(game.GraphicsDevice, captureWidth, captureHeight);
                            texture.SetData(data);
                            textureMap[row, col] = texture;
                            renderTarget.Dispose();
                        }
                    }
                }

                int cx = 0;
                int cy = 0;
                Texture2D next = null;
                for (int r = 0; r < rows; r++)
                {
                    cx = 0;
                    for (int c = 0; c < cols; c++)
                    {
                        next = textureMap[r, c];
                        bitmap = PlatoWarpMenuMod.instance.Helper.GetPlatoHelper().Content.Textures.GetPatched(bitmap, new Point(cx, cy), next);
                        cx += next.Width;
                    }
                    cy += next.Height;
                }

                bitmap = PlatoWarpMenuMod.instance.Helper.GetPlatoHelper().Content.Textures.GetTrimed(bitmap);

                string filename = Path.Combine(PlatoWarpMenuMod.tempFolder, path2_1);

                if (InterceptScreenshotAndroid(bitmap, filename) != "")
                    using (FileStream fs = File.Create(filename))
                        bitmap.SaveAsPng(fs, bitmap.Width, bitmap.Height);

                bitmap.Dispose();
            }
            catch
            {
                game.GraphicsDevice.SetRenderTarget((RenderTarget2D)null);
            }

            if (game.GetFieldValue("_lightmap") != null)
            {
                game.GetFieldValue<RenderTarget2D>("_lightmap").Dispose();
                game.SetFieldValue((RenderTarget2D)null, "_lightmap");
            }

            game.SetFieldValue(lightmap, "_lightmap");
            Game1.options.zoomLevel = zoomLevel;
            game.takingMapScreenshot = false;
            Game1.displayHUD = displayHud;
            Game1.viewport = viewport;
            return false;
        }

        public void _draw(Game1 tthis, GameTime gameTime, RenderTarget2D target_screen)
        {
            Game1.showingHealthBar = false;
            Color bgColor = Color.Black;
            {
                if (target_screen != null)
                    tthis.GraphicsDevice.SetRenderTarget(target_screen);
                {
                    tthis.GraphicsDevice.Clear(bgColor);

                    if (Game1.gameMode == (byte)11)
                    {
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                        Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3685"), new Vector2(16f, 16f), Microsoft.Xna.Framework.Color.HotPink);
                        Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3686"), new Vector2(16f, 32f), new Microsoft.Xna.Framework.Color(0, (int)byte.MaxValue, 0));
                        Game1.spriteBatch.DrawString(Game1.dialogueFont, Game1.parseText(Game1.errorMessage, Game1.dialogueFont, Game1.graphics.GraphicsDevice.Viewport.Width), new Vector2(16f, 48f), Microsoft.Xna.Framework.Color.White);
                        Game1.spriteBatch.End();
                    }
                    else if (Game1.gameMode == (byte)6 || Game1.gameMode == (byte)3 && Game1.currentLocation == null)
                    {
                        Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                        string str1 = "";
                        for (int index = 0; (double)index < gameTime.TotalGameTime.TotalMilliseconds % 999.0 / 333.0; ++index)
                            str1 += ".";
                        string str2 = Game1.content.LoadString("Strings\\StringsFromCSFiles:Game1.cs.3688");
                        string s = str2 + str1;
                        string str3 = str2 + "... ";
                        int widthOfString = SpriteText.getWidthOfString(str3, 999999);
                        int height = 64;
                        int x = 64;
                        int y = Game1.graphics.GraphicsDevice.Viewport.GetTitleSafeArea().Bottom - height;
                        SpriteText.drawString(Game1.spriteBatch, s, x, y, 999999, widthOfString, height, 1f, 0.88f, false, 0, str3, -1, SpriteText.ScrollTextAlignment.Left);
                        Game1.spriteBatch.End();
                        if (target_screen != null)
                        {
                            tthis.GraphicsDevice.SetRenderTarget((RenderTarget2D)null);
                            tthis.GraphicsDevice.Clear(bgColor);
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
                            Game1.spriteBatch.Draw((Texture2D)target_screen, Vector2.Zero, new Microsoft.Xna.Framework.Rectangle?(target_screen.Bounds), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, Game1.options.zoomLevel, SpriteEffects.None, 1f);
                            Game1.spriteBatch.End();
                        }
                        if (Game1.overlayMenu != null)
                        {
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            Game1.overlayMenu.draw(Game1.spriteBatch);
                            Game1.spriteBatch.End();
                        }
                    }
                    else
                    {
                        Microsoft.Xna.Framework.Rectangle rectangle;
                        Viewport viewport;
                        if (Game1.gameMode == (byte)0)
                        {
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                        }
                        else
                        {
                            if (Game1.drawLighting)
                            {
                                tthis.GraphicsDevice.SetRenderTarget(Game1.lightmap);
                                tthis.GraphicsDevice.Clear(Microsoft.Xna.Framework.Color.White * 0.0f);
                                Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                                Microsoft.Xna.Framework.Color color = !Game1.currentLocation.Name.StartsWith("UndergroundMine") || !(Game1.currentLocation is MineShaft) ? (Game1.ambientLight.Equals(Microsoft.Xna.Framework.Color.White) || Game1.isRaining && (bool)(NetFieldBase<bool, NetBool>)Game1.currentLocation.isOutdoors ? Game1.outdoorLight : Game1.ambientLight) : (Game1.currentLocation as MineShaft).getLightingColor(gameTime);
                                Game1.spriteBatch.Draw(Game1.staminaRect, Game1.lightmap.Bounds, color);
                                foreach (LightSource currentLightSource in Game1.currentLightSources)
                                {
                                    if (!Game1.isRaining && !Game1.isDarkOut() || currentLightSource.lightContext.Value != LightSource.LightContext.WindowLight)
                                    {
                                        if (currentLightSource.PlayerID != 0L && currentLightSource.PlayerID != Game1.player.UniqueMultiplayerID)
                                        {
                                            Farmer farmerMaybeOffline = Game1.getFarmerMaybeOffline(currentLightSource.PlayerID);
                                            if (farmerMaybeOffline == null || farmerMaybeOffline.currentLocation != null && farmerMaybeOffline.currentLocation.Name != Game1.currentLocation.Name || (bool)(NetFieldBase<bool, NetBool>)farmerMaybeOffline.hidden)
                                                continue;
                                        }
                                        if (Utility.isOnScreen((Vector2)(NetFieldBase<Vector2, NetVector2>)currentLightSource.position, (int)((double)(float)(NetFieldBase<float, NetFloat>)currentLightSource.radius * 64.0 * 4.0)))
                                            Game1.spriteBatch.Draw(currentLightSource.lightTexture, Game1.GlobalToLocal(Game1.viewport, (Vector2)(NetFieldBase<Vector2, NetVector2>)currentLightSource.position) / (float)(Game1.options.lightingQuality / 2), new Microsoft.Xna.Framework.Rectangle?(currentLightSource.lightTexture.Bounds), (Microsoft.Xna.Framework.Color)(NetFieldBase<Microsoft.Xna.Framework.Color, NetColor>)currentLightSource.color, 0.0f, new Vector2((float)currentLightSource.lightTexture.Bounds.Center.X, (float)currentLightSource.lightTexture.Bounds.Center.Y), (float)(NetFieldBase<float, NetFloat>)currentLightSource.radius / (float)(Game1.options.lightingQuality / 2), SpriteEffects.None, 0.9f);
                                    }
                                }
                                Game1.spriteBatch.End();
                                tthis.GraphicsDevice.SetRenderTarget(target_screen);
                            }
                            if (Game1.bloomDay && Game1.bloom != null)
                                Game1.bloom.BeginDraw();
                            tthis.GraphicsDevice.Clear(bgColor);
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            if (Game1.background != null)
                                Game1.background.draw(Game1.spriteBatch);
                            Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                            Game1.currentLocation.Map.GetLayer("Back").Draw(Game1.mapDisplayDevice, Game1.viewport, xTile.Dimensions.Location.Origin, false, 4);
                            Game1.currentLocation.drawWater(Game1.spriteBatch);

                            if (!Game1.currentLocation.shouldHideCharacters())
                            {
                                if (Game1.CurrentEvent == null)
                                {
                                    foreach (NPC character in Game1.currentLocation.characters)
                                    {
                                        if (!(bool)(NetFieldBase<bool, NetBool>)character.swimming && !character.HideShadow && (!character.IsInvisible && !Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(character.getTileLocation())))
                                            Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, character.Position + new Vector2((float)(character.Sprite.SpriteWidth * 4) / 2f, (float)(character.GetBoundingBox().Height + (character.IsMonster ? 0 : 12)))), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Microsoft.Xna.Framework.Color.White, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), (float)(4.0 + (double)character.yJumpOffset / 40.0) * (float)(NetFieldBase<float, NetFloat>)character.scale, SpriteEffects.None, Math.Max(0.0f, (float)character.getStandingY() / 10000f) - 1E-06f);
                                    }
                                }
                                else
                                {
                                    foreach (NPC actor in Game1.CurrentEvent.actors)
                                    {
                                        if (!(bool)(NetFieldBase<bool, NetBool>)actor.swimming && !actor.HideShadow && !Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(actor.getTileLocation()))
                                            Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, actor.Position + new Vector2((float)(actor.Sprite.SpriteWidth * 4) / 2f, (float)(actor.GetBoundingBox().Height + (actor.IsMonster ? 0 : (actor.Sprite.SpriteHeight <= 16 ? -4 : 12))))), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Microsoft.Xna.Framework.Color.White, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), (float)(4.0 + (double)actor.yJumpOffset / 40.0) * (float)(NetFieldBase<float, NetFloat>)actor.scale, SpriteEffects.None, Math.Max(0.0f, (float)actor.getStandingY() / 10000f) - 1E-06f);
                                    }
                                }
                            }
                            xTile.Layers.Layer layer1 = Game1.currentLocation.Map.GetLayer("Buildings");
                            layer1.Draw(Game1.mapDisplayDevice, Game1.viewport, xTile.Dimensions.Location.Origin, false, 4);
                            Game1.mapDisplayDevice.EndScene();
                            Game1.spriteBatch.End();
                            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            if (!Game1.currentLocation.shouldHideCharacters())
                            {
                                if (Game1.CurrentEvent == null)
                                {
                                    foreach (NPC character in Game1.currentLocation.characters)
                                    {
                                        if (!(bool)(NetFieldBase<bool, NetBool>)character.swimming && !character.HideShadow && (!(bool)(NetFieldBase<bool, NetBool>)character.isInvisible && Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(character.getTileLocation())))
                                            Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, character.Position + new Vector2((float)(character.Sprite.SpriteWidth * 4) / 2f, (float)(character.GetBoundingBox().Height + (character.IsMonster ? 0 : 12)))), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Microsoft.Xna.Framework.Color.White, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), (float)(4.0 + (double)character.yJumpOffset / 40.0) * (float)(NetFieldBase<float, NetFloat>)character.scale, SpriteEffects.None, Math.Max(0.0f, (float)character.getStandingY() / 10000f) - 1E-06f);
                                    }
                                }
                                else
                                {
                                    foreach (NPC actor in Game1.CurrentEvent.actors)
                                    {
                                        if (!(bool)(NetFieldBase<bool, NetBool>)actor.swimming && !actor.HideShadow && Game1.currentLocation.shouldShadowBeDrawnAboveBuildingsLayer(actor.getTileLocation()))
                                            Game1.spriteBatch.Draw(Game1.shadowTexture, Game1.GlobalToLocal(Game1.viewport, actor.Position + new Vector2((float)(actor.Sprite.SpriteWidth * 4) / 2f, (float)(actor.GetBoundingBox().Height + (actor.IsMonster ? 0 : 12)))), new Microsoft.Xna.Framework.Rectangle?(Game1.shadowTexture.Bounds), Microsoft.Xna.Framework.Color.White, 0.0f, new Vector2((float)Game1.shadowTexture.Bounds.Center.X, (float)Game1.shadowTexture.Bounds.Center.Y), (float)(4.0 + (double)actor.yJumpOffset / 40.0) * (float)(NetFieldBase<float, NetFloat>)actor.scale, SpriteEffects.None, Math.Max(0.0f, (float)actor.getStandingY() / 10000f) - 1E-06f);
                                    }
                                }

                            }
                            if ((Game1.eventUp || Game1.killScreen) && (!Game1.killScreen && Game1.currentLocation.currentEvent != null))
                                Game1.currentLocation.currentEvent.draw(Game1.spriteBatch);
                            if (Game1.player.currentUpgrade != null && Game1.player.currentUpgrade.daysLeftTillUpgradeDone <= 3 && Game1.currentLocation.Name.Equals("Farm"))
                                Game1.spriteBatch.Draw(Game1.player.currentUpgrade.workerTexture, Game1.GlobalToLocal(Game1.viewport, Game1.player.currentUpgrade.positionOfCarpenter), new Microsoft.Xna.Framework.Rectangle?(Game1.player.currentUpgrade.getSourceRectangle()), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, (float)(((double)Game1.player.currentUpgrade.positionOfCarpenter.Y + 48.0) / 10000.0));
                            Game1.currentLocation.draw(Game1.spriteBatch);
                            foreach (Vector2 key in Game1.crabPotOverlayTiles.Keys)
                            {
                                xTile.Tiles.Tile tile = layer1.Tiles[(int)key.X, (int)key.Y];
                                if (tile != null)
                                {
                                    Vector2 local = Game1.GlobalToLocal(Game1.viewport, key * 64f);
                                    xTile.Dimensions.Location location = new xTile.Dimensions.Location((int)local.X, (int)local.Y);
                                    Game1.mapDisplayDevice.DrawTile(tile, location, (float)(((double)key.Y * 64.0 - 1.0) / 10000.0));
                                }
                            }
                            if (Game1.eventUp && Game1.currentLocation.currentEvent != null)
                            {
                                string messageToScreen = Game1.currentLocation.currentEvent.messageToScreen;
                            }

                            if (Game1.currentLocation.Name.Equals("Farm"))
                            {
                                if (Game1.player.CoopUpgradeLevel > 0)
                                    Game1.spriteBatch.Draw(Game1.currentCoopTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(1280f, 320f)), new Microsoft.Xna.Framework.Rectangle?(Game1.currentCoopTexture.Bounds), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, Math.Max(0.0f, 0.0576f));
                                switch (Game1.player.BarnUpgradeLevel)
                                {
                                    case 1:
                                        Game1.spriteBatch.Draw(Game1.currentBarnTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(768f, 320f)), new Microsoft.Xna.Framework.Rectangle?(Game1.currentBarnTexture.Bounds), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, Math.Max(0.0f, 0.0576f));
                                        break;
                                    case 2:
                                        Game1.spriteBatch.Draw(Game1.currentBarnTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(640f, 256f)), new Microsoft.Xna.Framework.Rectangle?(Game1.currentBarnTexture.Bounds), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, Math.Max(0.0f, 0.0576f));
                                        break;
                                }
                                if (Game1.player.hasGreenhouse)
                                    Game1.spriteBatch.Draw(Game1.greenhouseTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(64f, 320f)), new Microsoft.Xna.Framework.Rectangle?(Game1.greenhouseTexture.Bounds), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, Math.Max(0.0f, 0.0576f));
                            }

                                Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                            Game1.currentLocation.Map.GetLayer("Front").Draw(Game1.mapDisplayDevice, Game1.viewport, xTile.Dimensions.Location.Origin, false, 4);
                            Game1.mapDisplayDevice.EndScene();
                            Game1.currentLocation.drawAboveFrontLayer(Game1.spriteBatch);
                            Game1.spriteBatch.End();
                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            if (Game1.displayFarmer && Game1.player.ActiveObject != null && ((bool)(NetFieldBase<bool, NetBool>)Game1.player.ActiveObject.bigCraftable && tthis.checkBigCraftableBoundariesForFrontLayer()) && Game1.currentLocation.Map.GetLayer("Front").PickTile(new xTile.Dimensions.Location(Game1.player.getStandingX(), Game1.player.getStandingY()), Game1.viewport.Size) == null)
                                Game1.drawPlayerHeldObject(Game1.player);
                            else if (Game1.displayFarmer && Game1.player.ActiveObject != null)
                            {
                                if (Game1.currentLocation.Map.GetLayer("Front").PickTile(new xTile.Dimensions.Location((int)Game1.player.Position.X, (int)Game1.player.Position.Y - 38), Game1.viewport.Size) == null || Game1.currentLocation.Map.GetLayer("Front").PickTile(new xTile.Dimensions.Location((int)Game1.player.Position.X, (int)Game1.player.Position.Y - 38), Game1.viewport.Size).TileIndexProperties.ContainsKey("FrontAlways"))
                                {
                                    xTile.Layers.Layer layer2 = Game1.currentLocation.Map.GetLayer("Front");
                                    rectangle = Game1.player.GetBoundingBox();
                                    xTile.Dimensions.Location mapDisplayLocation1 = new xTile.Dimensions.Location(rectangle.Right, (int)Game1.player.Position.Y - 38);
                                    xTile.Dimensions.Size size1 = Game1.viewport.Size;
                                    if (layer2.PickTile(mapDisplayLocation1, size1) != null)
                                    {
                                        xTile.Layers.Layer layer3 = Game1.currentLocation.Map.GetLayer("Front");
                                        rectangle = Game1.player.GetBoundingBox();
                                        xTile.Dimensions.Location mapDisplayLocation2 = new xTile.Dimensions.Location(rectangle.Right, (int)Game1.player.Position.Y - 38);
                                        xTile.Dimensions.Size size2 = Game1.viewport.Size;
                                        if (layer3.PickTile(mapDisplayLocation2, size2).TileIndexProperties.ContainsKey("FrontAlways"))
                                            goto label_139;
                                    }
                                    else
                                        goto label_139;
                                }
                                Game1.drawPlayerHeldObject(Game1.player);
                            }
                        label_139:
                            if ((Game1.player.UsingTool || Game1.pickingTool) && Game1.player.CurrentTool != null && ((!Game1.player.CurrentTool.Name.Equals("Seeds") || Game1.pickingTool) && (Game1.currentLocation.Map.GetLayer("Front").PickTile(new xTile.Dimensions.Location(Game1.player.getStandingX(), (int)Game1.player.Position.Y - 38), Game1.viewport.Size) != null && Game1.currentLocation.Map.GetLayer("Front").PickTile(new xTile.Dimensions.Location(Game1.player.getStandingX(), Game1.player.getStandingY()), Game1.viewport.Size) == null)))
                                Game1.drawTool(Game1.player);
                            if (Game1.currentLocation.Map.GetLayer("AlwaysFront") != null)
                            {
                                Game1.mapDisplayDevice.BeginScene(Game1.spriteBatch);
                                Game1.currentLocation.Map.GetLayer("AlwaysFront").Draw(Game1.mapDisplayDevice, Game1.viewport, xTile.Dimensions.Location.Origin, false, 4);
                                Game1.mapDisplayDevice.EndScene();
                            }
                            if ((double)Game1.toolHold > 400.0 && Game1.player.CurrentTool.UpgradeLevel >= 1 && Game1.player.canReleaseTool)
                            {
                                Microsoft.Xna.Framework.Color color = Microsoft.Xna.Framework.Color.White;
                                switch ((int)((double)Game1.toolHold / 600.0) + 2)
                                {
                                    case 1:
                                        color = Tool.copperColor;
                                        break;
                                    case 2:
                                        color = Tool.steelColor;
                                        break;
                                    case 3:
                                        color = Tool.goldColor;
                                        break;
                                    case 4:
                                        color = Tool.iridiumColor;
                                        break;
                                }
                                Game1.spriteBatch.Draw(Game1.littleEffect, new Microsoft.Xna.Framework.Rectangle((int)Game1.player.getLocalPosition(Game1.viewport).X - 2, (int)Game1.player.getLocalPosition(Game1.viewport).Y - (Game1.player.CurrentTool.Name.Equals("Watering Can") ? 0 : 64) - 2, (int)((double)Game1.toolHold % 600.0 * 0.0799999982118607) + 4, 12), Microsoft.Xna.Framework.Color.Black);
                                Game1.spriteBatch.Draw(Game1.littleEffect, new Microsoft.Xna.Framework.Rectangle((int)Game1.player.getLocalPosition(Game1.viewport).X, (int)Game1.player.getLocalPosition(Game1.viewport).Y - (Game1.player.CurrentTool.Name.Equals("Watering Can") ? 0 : 64), (int)((double)Game1.toolHold % 600.0 * 0.0799999982118607), 8), color);
                            }

                            if ((double)Game1.currentLocation.LightLevel > 0.0 && Game1.timeOfDay < 2000)
                                Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Microsoft.Xna.Framework.Color.Black * Game1.currentLocation.LightLevel);
                            if (Game1.screenGlow)
                                Game1.spriteBatch.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Game1.screenGlowColor * Game1.screenGlowAlpha);
                            Game1.currentLocation.drawAboveAlwaysFrontLayer(Game1.spriteBatch);

                            Game1.spriteBatch.End();
                            Game1.spriteBatch.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            if (Game1.eventUp && Game1.currentLocation.currentEvent != null)
                            {
                                foreach (NPC actor in Game1.currentLocation.currentEvent.actors)
                                {
                                    if (actor.isEmoting)
                                    {
                                        Vector2 localPosition = actor.getLocalPosition(Game1.viewport);
                                        localPosition.Y -= 140f;
                                        if (actor.Age == 2)
                                            localPosition.Y += 32f;
                                        else if (actor.Gender == 1)
                                            localPosition.Y += 10f;
                                        Game1.spriteBatch.Draw(Game1.emoteSpriteSheet, localPosition, new Microsoft.Xna.Framework.Rectangle?(new Microsoft.Xna.Framework.Rectangle(actor.CurrentEmoteIndex * 16 % Game1.emoteSpriteSheet.Width, actor.CurrentEmoteIndex * 16 / Game1.emoteSpriteSheet.Width * 16, 16, 16)), Microsoft.Xna.Framework.Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)actor.getStandingY() / 10000f);
                                    }
                                }
                            }
                            Game1.spriteBatch.End();

                            Game1.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, (DepthStencilState)null, (RasterizerState)null);
                            if (Game1.drawGrid)
                            {
                                int num1 = -Game1.viewport.X % 64;
                                float num2 = (float)(-Game1.viewport.Y % 64);
                                int num3 = num1;
                                while (true)
                                {
                                    int num4 = num3;
                                    viewport = Game1.graphics.GraphicsDevice.Viewport;
                                    int width = viewport.Width;
                                    if (num4 < width)
                                    {
                                        SpriteBatch spriteBatch = Game1.spriteBatch;
                                        Texture2D staminaRect = Game1.staminaRect;
                                        int x = num3;
                                        int y = (int)num2;
                                        viewport = Game1.graphics.GraphicsDevice.Viewport;
                                        int height = viewport.Height;
                                        Microsoft.Xna.Framework.Rectangle destinationRectangle = new Microsoft.Xna.Framework.Rectangle(x, y, 1, height);
                                        Microsoft.Xna.Framework.Color color = Microsoft.Xna.Framework.Color.Red * 0.5f;
                                        spriteBatch.Draw(staminaRect, destinationRectangle, color);
                                        num3 += 64;
                                    }
                                    else
                                        break;
                                }
                                float num5 = num2;
                                while (true)
                                {
                                    double num4 = (double)num5;
                                    viewport = Game1.graphics.GraphicsDevice.Viewport;
                                    double height = (double)viewport.Height;
                                    if (num4 < height)
                                    {
                                        SpriteBatch spriteBatch = Game1.spriteBatch;
                                        Texture2D staminaRect = Game1.staminaRect;
                                        int x = num1;
                                        int y = (int)num5;
                                        viewport = Game1.graphics.GraphicsDevice.Viewport;
                                        int width = viewport.Width;
                                        Microsoft.Xna.Framework.Rectangle destinationRectangle = new Microsoft.Xna.Framework.Rectangle(x, y, width, 1);
                                        Microsoft.Xna.Framework.Color color = Microsoft.Xna.Framework.Color.Red * 0.5f;
                                        spriteBatch.Draw(staminaRect, destinationRectangle, color);
                                        num5 += 64f;
                                    }
                                    else
                                        break;
                                }
                            }
                        }
                        Game1.spriteBatch.End();
                        tthis.CallAction("renderScreenBuffer", target_screen);
                    }
                }
            }
        }
    }
}