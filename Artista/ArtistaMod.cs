using Artista.Artpieces;
using Artista.Furniture;
using Artista.Menu;
using Artista.Objects;
using Artista.Online;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using SpaceShared.APIs;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Artista
{
    public class ArtistaMod : Mod
    {
        public static ArtistaMod Singleton { get; private set; }
        public static Texture2D White { get; set; }

        public static Config Config { get; set; }

        public Artpiece DisplayCPF { get; set; }

        public float ScaleCPF { get; set; } = 4f;

        public int RotateCPF { get; set; } = 0;

        public Texture2D Curtain { get; set; }

        public OnlineArtAPI OnlineApi { get; set; }

        public override void Entry(IModHelper helper)
        {
            Config = Helper.ReadConfig<Config>();

            White = new Texture2D(Game1.graphics.GraphicsDevice, 1, 1);
            Color[] white = new Color[1] { Color.White };
            White.SetData(white);
            Singleton = this;

            OnlineApi = new OnlineArtAPI(helper);
            helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Display.MenuChanged += Display_MenuChanged;
            helper.Events.Display.RenderedWorld += Display_RenderedWorld;
            helper.Events.Multiplayer.ModMessageReceived += Multiplayer_ModMessageReceived;



        }

        private void Multiplayer_ModMessageReceived(object sender, StardewModdingAPI.Events.ModMessageReceivedEventArgs e)
        {
            if(Game1.activeClickableMenu is PaintMenu pm && e.Type == "Platonymous.Artista.Paint")
                pm.ReceivePaintMPInfo(e.ReadAs<PaintMPInfo>());
        }

        private void Display_RenderedWorld(object sender, StardewModdingAPI.Events.RenderedWorldEventArgs e)
        {
            if (DisplayCPF != null)
            {
                var tex = DisplayCPF.GetFullTexture();

                var angle = (float)(Math.PI / 2.0f) * RotateCPF;

                var vp = new Rectangle(0, 0, Game1.viewport.Width, Game1.viewport.Height);
                var cpfvp = new Rectangle((int)((vp.Width - (tex.Width * ScaleCPF)) / 2) + (int)((tex.Width * ScaleCPF)/2), (int)((vp.Height - (tex.Height * ScaleCPF)) / 2) + (int)((tex.Height * ScaleCPF) / 2), (int)(tex.Width * ScaleCPF), (int)(tex.Height * ScaleCPF));
                e.SpriteBatch.Draw(White,vp, DisplayCPF.CanvasColor);
                e.SpriteBatch.Draw(tex, cpfvp, new Rectangle(0, 0, tex.Width, tex.Height), Color.White, angle, new Vector2((tex.Width) / 2, (tex.Height) / 2), SpriteEffects.None, 5f);
            }
        }

        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            if(e.NewMenu is ShopMenu shop){
                if (shop.ShopId == "SeedShop")
                {
                    Dictionary<ISalable, int> newItemsToSell = new Dictionary<ISalable, int>();
                    shop.AddForSale(new Easel(), new ItemStockInformation(700, int.MaxValue, null, null));
                    shop.AddForSale(new PaintingFurniture(new Painting(1, 1, 1)), new ItemStockInformation(100, int.MaxValue, null, null));
                    shop.AddForSale(new PaintingFurniture(new Painting(1, 2, 1)), new ItemStockInformation(200, int.MaxValue, null, null));
                    shop.AddForSale(new PaintingFurniture(new Painting(2, 2, 1)), new ItemStockInformation(300, int.MaxValue, null, null));
                    shop.AddForSale(new PaintingFurniture(new Painting(1, 1, 2)), new ItemStockInformation(400, int.MaxValue, null, null));
                    shop.AddForSale(new PaintingFurniture(new Painting(1, 2, 2)), new ItemStockInformation(500, int.MaxValue, null, null));
                    shop.AddForSale(new PaintingFurniture(new Painting(2, 2, 2)), new ItemStockInformation(600, int.MaxValue, null, null));
                }
                else
                    Monitor.Log(shop.ShopId, LogLevel.Warn);

            }
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            var spaceCore = this.Helper.ModRegistry.GetApi<ISpaceCoreApi>("spacechase0.SpaceCore");
           spaceCore.RegisterSerializerType(typeof(Easel));
           spaceCore.RegisterSerializerType(typeof(SavedArtpiece));
           spaceCore.RegisterSerializerType(typeof(PaintingFurniture));

        }


        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!Context.IsWorldReady)
                return;

            if (!Config.CPFCompatibility)
                return;

            if(e.Button == Config.OpenOnlineMenu)
            {
                Game1.activeClickableMenu = new OnlineArtMenu(Helper, Monitor, OnlineApi);
                return;
            }

            if (e.Button == Config.ReverseButton && Game1.activeClickableMenu is PaintMenu paint)
            {
                paint.Reverse();
                return;
            }

            if (DisplayCPF != null)
            {

                if (e.Button != Config.CPFSSwitchFrameKey && e.Button != Config.ChangeCPFScaleUP && e.Button != Config.ChangeCPFScaleDown && e.Button != Config.ChangeCPFRotation)
                {
                    DisplayCPF = null;
                }
                else if (e.Button == Config.ChangeCPFScaleUP)
                {
                    ScaleCPF += 0.5f;
                    if (ScaleCPF > 20)
                        ScaleCPF = 1f;
                }
                else if (e.Button == Config.ChangeCPFScaleDown)
                {
                    ScaleCPF -= 0.5f;
                    if (ScaleCPF < 1f)
                        ScaleCPF = 20;
                }
                else if (e.Button == Config.ChangeCPFRotation)
                {
                    RotateCPF += 1;
                    if (RotateCPF > 3)
                        RotateCPF = 0;
                }

            }
            else if (Game1.activeClickableMenu is PaintMenu pm && e.Button == Config.CPFStartFramingKey)
            {
                Game1.activeClickableMenu.exitThisMenu();
                DisplayCPF = pm.Art;
                ScaleCPF = 4;
                RotateCPF = 0;
            }


        }
    }
}
