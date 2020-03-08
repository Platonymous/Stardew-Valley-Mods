using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using TMXTile;
using xTile.Dimensions;
using xTile.Display;
using xTile.Tiles;

namespace PyTK.Types
{
    public class PyDisplayDevice : IDisplayDevice
    {
        private ContentManager m_contentManager;
        private GraphicsDevice m_graphicsDevice;
        private Vector2 m_tilePosition;
        private Microsoft.Xna.Framework.Rectangle m_sourceRectangle;
        private SpriteBatch m_spriteBatchAlpha;
        private Color m_modulationColour;
        private DrawInstructions m_instructions;
        private Dictionary<string, Texture2D> m_tileSheetTextures;

        public PyDisplayDevice(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            this.m_contentManager = contentManager;
            this.m_graphicsDevice = graphicsDevice;
            this.m_spriteBatchAlpha = new SpriteBatch(graphicsDevice);
            this.m_tileSheetTextures = new Dictionary<string, Texture2D>();
            this.m_tilePosition = new Vector2();
            this.m_sourceRectangle = new Microsoft.Xna.Framework.Rectangle();
            this.m_modulationColour = Color.White;
        }

        public void Clear()
        {
            m_tileSheetTextures.Clear();
        }

        public void BeginScene(SpriteBatch b)
        {
            m_spriteBatchAlpha = b;
        }

        private void LoadTileSheet2(TileSheet tileSheet)
        {
            if(string.IsNullOrWhiteSpace(Path.GetDirectoryName(tileSheet.ImageSource)))
                    tileSheet.ImageSource = Path.Combine("Maps", Path.GetFileName(tileSheet.ImageSource));

            if (m_contentManager.Load<Texture2D>(tileSheet.ImageSource) is Texture2D texture)
                if (m_tileSheetTextures.ContainsKey(tileSheet.ImageSource))
                    m_tileSheetTextures[tileSheet.ImageSource] = texture;
                else
                    m_tileSheetTextures.Add(tileSheet.ImageSource, texture);
        }

        public Texture2D GetTexture(TileSheet tilesheet)
        {
            if (m_tileSheetTextures.TryGetValue(tilesheet.ImageSource, out Texture2D texture))
                return texture;
            else
            {
                LoadTileSheet2(tilesheet);
                if (m_tileSheetTextures.TryGetValue(tilesheet.ImageSource, out Texture2D texture2))
                    return texture2;
                else
                    return null;
            }
        }

        public void DisposeTileSheet(TileSheet tileSheet)
        {
            m_tileSheetTextures.Remove(tileSheet.ImageSource);
        }

        public void DrawTile(Tile tile, Location location, float layerDepth)
        {
            if (tile == null)
                return;

            xTile.Dimensions.Rectangle tileImageBounds = tile.TileSheet.GetTileImageBounds(tile.TileIndex);
            Texture2D tileSheetTexture = GetTexture(tile.TileSheet);
            if (tileSheetTexture == null || tileSheetTexture.IsDisposed)
                return;

            this.m_instructions = tile.GetDrawInstructions();

            this.m_tilePosition.X = (float)location.X + this.m_instructions.Offset.X;
            this.m_tilePosition.Y = (float)location.Y + this.m_instructions.Offset.Y;
            this.m_sourceRectangle.X = tileImageBounds.X;
            this.m_sourceRectangle.Y = tileImageBounds.Y;
            this.m_sourceRectangle.Width = tileImageBounds.Width;
            this.m_sourceRectangle.Height = tileImageBounds.Height;

            if (this.m_instructions.Color is TMXColor color)
                this.m_modulationColour = new Color(color.R, color.G, color.B, color.A);
            else
                this.m_modulationColour = Color.White;

            var origin = new Vector2(tileImageBounds.Width / 2, tileImageBounds.Height / 2);
            m_tilePosition += new Vector2(tileImageBounds.Width * Game1.pixelZoom / 2, tileImageBounds.Height * Game1.pixelZoom / 2);
            this.m_spriteBatchAlpha.Draw(
                texture:tileSheetTexture, 
                destinationRectangle:new Microsoft.Xna.Framework.Rectangle((int)this.m_tilePosition.X, (int) this.m_tilePosition.Y, Game1.tileSize, Game1.tileSize), 
                sourceRectangle: new Microsoft.Xna.Framework.Rectangle?(this.m_sourceRectangle), this.m_modulationColour * this.m_instructions.Opacity, 
                rotation: this.m_instructions.Rotation, 
                origin:origin, 
                effects:(SpriteEffects) m_instructions.Effect, 
                layerDepth);
        }

        public void EndScene()
        {
            
        }

        public void LoadTileSheet(TileSheet tileSheet)
        {
            LoadTileSheet2(tileSheet);
        }

        public void SetClippingRegion(xTile.Dimensions.Rectangle clippingRegion)
        {
            int backBufferWidth = this.m_graphicsDevice.PresentationParameters.BackBufferWidth;
            int backBufferHeight = this.m_graphicsDevice.PresentationParameters.BackBufferHeight;
            int x = this.Clamp(clippingRegion.X, 0, backBufferWidth);
            int y = this.Clamp(clippingRegion.Y, 0, backBufferHeight);
            int num1 = this.Clamp(clippingRegion.X + clippingRegion.Width, 0, backBufferWidth);
            int num2 = this.Clamp(clippingRegion.Y + clippingRegion.Height, 0, backBufferHeight);
            int width = num1 - x;
            int height = num2 - y;
            this.m_graphicsDevice.Viewport = new Viewport(x, y, width, height);
        }
        private int Clamp(int nValue, int nMin, int nMax)
        {
            return Math.Min(Math.Max(nValue, nMin), nMax);
        }
    }
}
