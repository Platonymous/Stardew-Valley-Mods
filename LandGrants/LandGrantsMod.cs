using HarmonyLib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Netcode;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Buildings;
using StardewValley.Characters;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using static StardewValley.Menus.LoadGameMenu;

namespace LandGrants
{
    public class LandGrantsMod : Mod
    {
        private static Config Config;
        private Harmony HarmonyInstance;

        const int MinPlayers = 4;
        public static int MaxPlayers { get; set; } = 16;

        private static IModHelper ModHelper { get; set; }
        private static LandGrantsMod Singleton { get; set; }
        private static Dictionary<string, Farmer> ExtraFarmers { get; set; } = new Dictionary<string, Farmer>();

        private static bool UseLocationNameAsIs { get; set; } = false;

        private static bool CompareRealStrings { get; set; } = false;

        private static bool IsActive { get; set; } = false;

        private static bool FromMPMenu { get; set; } = false;

        private static bool OverrideLocations { get; set; } = false;

        private static GameLocation OverrideFarm { get; set; } = null;

        private static float CabinButtonScale { get; set; } = 0f;

        private static LocationRequest ReturnLocation { get; set; } = null;

        private static Farmer LastFarmHandLoaded { get; set; } = null;

        public override void Entry(IModHelper helper)
        {
            Singleton = this;
            ModHelper = helper;
            HarmonyInstance = new Harmony("Platonymous.LandGrants");
            Config = Helper.ReadConfig<Config>();
            MaxPlayers = Config.MaxPlayer;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;

        }

        private static bool ShouldBeActive()
        {
            return (IsActive && (Context.IsMultiplayer || FromMPMenu));
        }

        private void GameLoop_GameLaunched(object sender, StardewModdingAPI.Events.GameLaunchedEventArgs e)
        {
            Multiplayer multiplayer = (Multiplayer)AccessTools.Field(typeof(Game1), "multiplayer").GetValue(null);
            var maxPlayerField = AccessTools.Field(typeof(Multiplayer), nameof(Multiplayer.playerLimit));
            maxPlayerField.SetValue(multiplayer, MaxPlayers);
            
            HarmonyInstance.Patch(AccessTools.Method(typeof(NetFieldBase<string, NetString>), "Equals", new Type[] { typeof(object) }), null, new HarmonyMethod(this.GetType(), nameof(NetStringEquals)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(CoopMenu), "addSaveFiles"), new HarmonyMethod(this.GetType(), nameof(AddSaveFiles)));
            HarmonyInstance.Patch(AccessTools.PropertyGetter(typeof(FarmHouse), "owner"), null, new HarmonyMethod(this.GetType(), nameof(Owner)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(Game1), "performWarpFarmer", new Type[] { typeof(LocationRequest), typeof(int), typeof(int), typeof(int) }), new HarmonyMethod(this.GetType(), nameof(PerformWarpFarmer)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(Multiplayer), nameof(Multiplayer.isAlwaysActiveLocation)), new HarmonyMethod(this.GetType(), nameof(IsAlwaysActiveLocation)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(Multiplayer), "saveFarmhand"), new HarmonyMethod(this.GetType(), nameof(SaveFarmhand)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(Game1), nameof(Game1.getLocationFromName), new Type[] { typeof(string), typeof(bool) }), new HarmonyMethod(this.GetType(), nameof(GetLocationFromName)), new HarmonyMethod(this.GetType(), nameof(GetLocationFromName2)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(GameLocation), "loadMap"), new HarmonyMethod(this.GetType(), nameof(LoadMap)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(GameLocation), "reloadMap"), new HarmonyMethod(this.GetType(), nameof(ReloadMap)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(GameLocation), "BuildStartingCabins"), new HarmonyMethod(this.GetType(), nameof(BuildStartingCabins)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(GameLocation), "performTenMinuteUpdate"), new HarmonyMethod(this.GetType(), nameof(PerformTenMinuteUpdate), new Type[] { typeof(GameLocation) }));
            HarmonyInstance.Patch(AccessTools.Method(typeof(FarmCave), "performTenMinuteUpdate"), new HarmonyMethod(this.GetType(), nameof(PerformTenMinuteUpdate), new Type[] { typeof(FarmCave) }));
            HarmonyInstance.Patch(AccessTools.Method(typeof(FarmHouse), "performTenMinuteUpdate"), new HarmonyMethod(this.GetType(), nameof(PerformTenMinuteUpdate), new Type[] { typeof(FarmHouse) }));
            HarmonyInstance.Patch(AccessTools.Method(typeof(Farm), "performTenMinuteUpdate"), new HarmonyMethod(this.GetType(), nameof(PerformTenMinuteUpdate), new Type[] { typeof(Farm) }));
            HarmonyInstance.Patch(AccessTools.Method(typeof(GameLocation), "DayUpdate"), new HarmonyMethod(this.GetType(), nameof(PerformDayUpdate), new Type[] { typeof(GameLocation) }));
            HarmonyInstance.Patch(AccessTools.Method(typeof(FarmCave), "DayUpdate"), new HarmonyMethod(this.GetType(), nameof(PerformDayUpdate), new Type[] { typeof(FarmCave) }));
            HarmonyInstance.Patch(AccessTools.Method(typeof(Farm), "DayUpdate"), new HarmonyMethod(this.GetType(), nameof(PerformDayUpdate), new Type[] { typeof(Farm) }));
            HarmonyInstance.Patch(AccessTools.Method(typeof(Cabin), "DayUpdate"), new HarmonyMethod(this.GetType(), nameof(PerformDayUpdate), new Type[] { typeof(Cabin) }));
            HarmonyInstance.Patch(AccessTools.Method(typeof(Cellar), "DayUpdate"), new HarmonyMethod(this.GetType(), nameof(PerformDayUpdate), new Type[] { typeof(Cellar) }));

            HarmonyInstance.Patch(AccessTools.Method(typeof(GameLocation), "updateEvenIfFarmerIsntHere"), new HarmonyMethod(this.GetType(), nameof(PerformDayUpdate), new Type[] { typeof(GameLocation) }));
            HarmonyInstance.Patch(AccessTools.Method(typeof(Farm), "updateEvenIfFarmerIsntHere"), new HarmonyMethod(this.GetType(), nameof(PerformDayUpdate), new Type[] { typeof(Farm) }));
            HarmonyInstance.Patch(AccessTools.Method(typeof(Cabin), "updateEvenIfFarmerIsntHere"), new HarmonyMethod(this.GetType(), nameof(PerformDayUpdate), new Type[] { typeof(Cabin) }));

            HarmonyInstance.Patch(AccessTools.Method(typeof(CharacterCustomization), "selectionClick"), new HarmonyMethod(this.GetType(), nameof(SelectionClick)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(CharacterCustomization), "performHoverAction"), new HarmonyMethod(this.GetType(), nameof(PerformHoverAction)), new HarmonyMethod(this.GetType(), nameof(PerformHoverActionPost)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(Game1), nameof(Game1.warpCharacter), new Type[] { typeof(NPC), typeof(GameLocation), typeof(Vector2) }), new HarmonyMethod(this.GetType(), nameof(WarpCharacter)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(LoadGameMenu.SaveFileSlot), "drawSlotName"), new HarmonyMethod(this.GetType(), nameof(DrawSlotName)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(FarmhandMenu.FarmhandSlot), "Activate"), new HarmonyMethod(this.GetType(), nameof(Activate)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(CarpenterMenu), "setUpForBuildingPlacement"), null, new HarmonyMethod(this.GetType(), nameof(SetUpForBuildingPlacement)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(CarpenterMenu), "tryToBuild"), new HarmonyMethod(this.GetType(), nameof(TryToBuild)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(CarpenterMenu), "performHoverAction"), new HarmonyMethod(this.GetType(), nameof(PerformHoverActionCM)), new HarmonyMethod(this.GetType(), nameof(PerformHoverActionCMPost)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(CarpenterMenu), "tryToBuild"), new HarmonyMethod(this.GetType(), nameof(TryToBuild)));
            HarmonyInstance.Patch(AccessTools.Constructor(typeof(CarpenterMenu)), new HarmonyMethod(this.GetType(), nameof(CarpenterMenuConstructor)), new HarmonyMethod(this.GetType(), nameof(CarpenterMenuConstructorPost)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(AnimalQueryMenu), "prepareForAnimalPlacement"), null, new HarmonyMethod(this.GetType(), nameof(PrepareForAnimalPlacement)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(AnimalQueryMenu), "performHoverAction"), new HarmonyMethod(this.GetType(), nameof(PerformHoverActionCM)), new HarmonyMethod(this.GetType(), nameof(PerformHoverActionCMPost)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(PurchaseAnimalsMenu), "setUpForAnimalPlacement"), null, new HarmonyMethod(this.GetType(), nameof(SetUpForAnimalPlacement)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(PurchaseAnimalsMenu), "receiveLeftClick"), new HarmonyMethod(this.GetType(), nameof(PerformHoverActionCM)), new HarmonyMethod(this.GetType(), nameof(PerformHoverActionCMPost)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(PurchaseAnimalsMenu), "performHoverAction"), new HarmonyMethod(this.GetType(), nameof(PerformHoverActionCM)), new HarmonyMethod(this.GetType(), nameof(PerformHoverActionCMPost)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(FarmAnimal), "updateWhenNotCurrentLocation"), new HarmonyMethod(this.GetType(), nameof(UpdateWhenNotCurrentLocation)), new HarmonyMethod(this.GetType(), nameof(UpdateWhenNotCurrentLocationPost)));

            HarmonyInstance.Patch(AccessTools.Method(typeof(Dialogue), "checkForSpecialCharacters"), new HarmonyMethod(this.GetType(), nameof(ParseDialogueString)), new HarmonyMethod(this.GetType(), nameof(ParseDialogueStringPost)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(Utility), nameof(Utility.getHomeOfFarmer)), new HarmonyMethod(this.GetType(), nameof(GetHomeOfFarmer)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(AnimalHouse), "getBuilding"), null, new HarmonyMethod(this.GetType(), nameof(GetBuilding)));

            HarmonyInstance.Patch(AccessTools.PropertyGetter(typeof(FarmAnimal), "home"),null, new HarmonyMethod(this.GetType(), nameof(GetAnimalHome)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(Farm), "resetSharedState"), null, new HarmonyMethod(this.GetType(), nameof(ResetSharedState)));
            HarmonyInstance.Patch(AccessTools.Method(typeof(FarmHouse), "GetCellarName"), null, new HarmonyMethod(this.GetType(), nameof(GetCellarName)));

            Helper.Events.GameLoop.DayStarted += GameLoop_DayStarted;
            Helper.Events.GameLoop.UpdateTicked += GameLoop_UpdateTicked;
            Helper.Events.Input.ButtonPressed += Input_ButtonPressed;
            Helper.Events.GameLoop.TimeChanged += GameLoop_TimeChanged;
        }

        public static void GetCellarName(FarmHouse __instance, ref string __result)
        {
            if(__instance.modData.ContainsKey("Platonymous.LandGrants.SaveCabin") && __instance.modData.TryGetValue("Platonymous.LandGrants.Cellar", out string cellar))
            {
                __result = cellar;
            }
        }

        public static void ResetSharedState(Farm __instance)
        {
            if(__instance.modData.TryGetValue("Platonymous.LandGrants.SaveCabin", out string cabinloc)
               && Game1.getLocationFromName(cabinloc, true) is Cabin cabin && cabin.getFarmhand().Value is Farmer farmer)
            __instance.houseSource.Value = new Microsoft.Xna.Framework.Rectangle(0, 144 * (((int)farmer.HouseUpgradeLevel == 3) ? 2 : ((int)farmer.HouseUpgradeLevel)), 160, 144);
        }

        private void GameLoop_TimeChanged(object sender, StardewModdingAPI.Events.TimeChangedEventArgs e)
        {
            if (Context.IsWorldReady && Game1.player is Farmer farmer && !IsOwnedByActiveFarmer(Game1.currentLocation))
                Game1.warpFarmer(new LocationRequest("Town", false, Game1.getLocationFromName("Town")), 100, 60, 1);
        }


        public static void GetAnimalHome(FarmAnimal __instance, ref Building __result)
        {
            if (!ShouldBeActive())
                return;

            if (__result == null)
            {
                foreach (Farm farm in Game1.locations.Where(l => l is Farm))
                    if (farm.buildings.FirstOrDefault(b => b.indoors.Value is AnimalHouse a && a.animalsThatLiveHere.Contains(__instance.myID.Value)) is Building bld)
                    {
                        __result = bld;
                        break;
                    }
            }
        }

        public static void GetBuilding(AnimalHouse __instance, ref Building __result)
        {
            if (!ShouldBeActive())
                return;

            if (__result == null)
            foreach(Farm farm in Game1.locations.Where(l => l is Farm))
            foreach (Building b in farm.buildings)
            {
                if (b.indoors.Value != null && b.indoors.Value.Equals(__instance))
                {
                   __result = b;
                }
            }
        }

        public static void GetHomeOfFarmer(Farmer who, ref FarmHouse __result)
        {
        }

        public static void ParseDialogueString(Dialogue __instance, string str, ref string __result)
        {
            if (!ShouldBeActive())
                return;

            CompareRealStrings = true;
        }


        public static void ParseDialogueStringPost()
        {
            if (!ShouldBeActive())
                return;

            CompareRealStrings = false;
        }

        public static void NetStringEquals(NetFieldBase<string, NetString> __instance, object obj, ref bool __result)
        {
            if (CompareRealStrings)
                return;

            if (!ShouldBeActive())
                return;

            if (UseLocationNameAsIs)
                return;


            if (obj is string compare && __instance.Value is string org)
            {
                bool result = __result;
                if (org.Contains("_LG_") && !compare.Contains("_LG_"))
                {
                    if (org.Contains("Farm_LG_") && compare.Equals("Farm"))
                        __result = true;

                    if (org.Contains("FarmCave_LG_") && compare.Equals("FarmCave"))
                        __result = true;

                    if (org.Contains("FarmHouse_LG_") && compare.Equals("FarmHouse"))
                        __result = true;

                    if (org.Contains("Greenhouse_LG_") && compare.Equals("Greenhouse"))
                        __result = true;

                    if (org.Contains("Cellar_LG_") && compare.Equals("Cellar"))
                        __result = true;
                }

            }
        }


        public static void SetUpForAnimalPlacement()
        {
            if (!ShouldBeActive())
                return;

            if (Context.IsMainPlayer)
                return;

            if (Game1.player.modData.TryGetValue("Platonymous.LandGrants.Farm", out string farm))
            {
                Game1.currentLocation = Game1.getLocationFromName(farm);
                Game1.player.viewingLocation.Value = farm;
                Game1.currentLocation.resetForPlayerEntry();
            }
        }

        public static void PrepareForAnimalPlacement()
        {
            if (!ShouldBeActive())
                return;

            if (Context.IsMainPlayer)
                return;

            if (Game1.player.modData.TryGetValue("Platonymous.LandGrants.Farm", out string farm))
            {
                Game1.currentLocation = Game1.getLocationFromName(farm);
                Game1.currentLocation.resetForPlayerEntry();
            }

        }

        public static void CarpenterMenuConstructor()
        {
            if (!ShouldBeActive())
                return;

            if (Context.IsMainPlayer)
                return;
            OverrideLocations = true;

        }
        public static void CarpenterMenuConstructorPost()
        {
            if (!ShouldBeActive())
                return;

            if (Context.IsMainPlayer)
                return;
            OverrideLocations = false;

        }

        public static bool UpdateWhenNotCurrentLocation(FarmAnimal __instance, Building currentBuilding, GameTime time, GameLocation environment)
        {
            if (!ShouldBeActive())
                return true;

            if (environment is AnimalHouse ah)
            {
                if (__instance.home == null)
                    __instance.reload(ah.getBuilding());

                OverrideFarm = Game1.locations.FirstOrDefault(l => l is Farm f && f.buildings.Any(bl => bl.indoors.Value == environment));
            }

            if (environment is Farm farmenv)
            {
                if (__instance.home == null)
                {
                    Building b = null;
                    foreach (Farm farm in Game1.locations.Where(l => l is Farm))
                        if (farm.buildings.FirstOrDefault(b => b.indoors.Value is AnimalHouse a && a.animalsThatLiveHere.Contains(__instance.myID.Value)) is Building bld)
                            b = bld;

                     __instance.reload(b);
                }

                OverrideFarm = environment;
            }

            return true;
        }


        public static void UpdateWhenNotCurrentLocationPost(Building currentBuilding, GameTime time, GameLocation environment)
        {
            if (!ShouldBeActive())
                return;

            OverrideFarm = null;
        }


        public static void PerformHoverActionCM()
        {
            if (Context.IsMainPlayer)
                return;
            OverrideLocations = true;
        }

        public static void PerformHoverActionCMPost()
        {
            if (Context.IsMainPlayer)
                return;
            OverrideLocations = false;
        }

        public static void SetUpForBuildingPlacement()
        {
            if (Context.IsMainPlayer)
                return;

            if (Game1.player.modData.TryGetValue("Platonymous.LandGrants.Farm", out string farm))
            {
                Game1.currentLocation = Game1.getLocationFromName(farm);
                Game1.player.viewingLocation.Value = farm;
                Game1.currentLocation.resetForPlayerEntry();
            }
        }

        public static bool TryToBuild(CarpenterMenu __instance, ref bool __result)
        {
            if (Context.IsMainPlayer)
                return true;

            bool magical = (bool) AccessTools.Field(typeof(CarpenterMenu), "magicalConstruction").GetValue(__instance);
            if (Game1.player.modData.TryGetValue("Platonymous.LandGrants.Farm", out string farm))
            {
                __result = ((Farm)Game1.getLocationFromName(farm)).buildStructure(__instance.CurrentBlueprint, new Vector2((Game1.viewport.X + Game1.getOldMouseX(ui_scale: false)) / 64, (Game1.viewport.Y + Game1.getOldMouseY(ui_scale: false)) / 64), Game1.player, magical);
                return false;
            }

            return true;
        }

        public static void SaveFarmhand(NetFarmerRoot farmhand)
        {
            if (!ShouldBeActive())
                return;

            FarmHouse farmHouse = Utility.getHomeOfFarmer(farmhand);
            if (farmHouse.modData.TryGetValue("Platonymous.LandGrants.SaveCabin", out string cabin) && Game1.getLocationFromName(cabin, true) is Cabin c)
                c.saveFarmhand(farmhand);
        }

        public static bool DrawSlotName(LoadGameMenu.SaveFileSlot __instance, SpriteBatch b, int i)
        {
            if (__instance.Farmer.modData.TryGetValue("Platonymous.LandGrants.MultiFarm", out string num))
            {
                var farmerName = $"[LG {int.Parse(num) + 1}] " + Game1.content.LoadString("Strings\\UI:CoopMenu_HostFile", __instance.Farmer.Name, __instance.Farmer.farmName.Value);
                var menu = (LoadGameMenu)AccessTools.Field(typeof(MenuSlot), "menu").GetValue(__instance);
                SpriteText.drawString(b, farmerName, menu.slotButtons[i].bounds.X + 128 + 36, menu.slotButtons[i].bounds.Y + 36, 999999, -1, 999999, __instance.getSlotAlpha());
                return false;
            }
            return true;
        }

        private void Input_ButtonPressed(object sender, StardewModdingAPI.Events.ButtonPressedEventArgs e)
        {
            if (!ShouldBeActive() && Context.IsWorldReady && e.Button == Config.BuildCabinKey && Game1.activeClickableMenu == null)
            {
                var cm = new CarpenterMenu();
                List<BluePrint> bp = new List<BluePrint>() { new BluePrint("Stone Cabin"), new BluePrint("Plank Cabin"), new BluePrint("Log Cabin") };
                bp.ForEach(b =>
                {
                    b.woodRequired = 0;
                    b.stoneRequired = 0;
                    b.moneyRequired = 0;
                    b.IronRequired = 0;
                    b.IridiumRequired = 0;
                    b.GoldRequired = 0;
                    b.copperRequired = 0;
                    b.itemsRequired = new Dictionary<int, int>();
                });

                AccessTools.Field(typeof(CarpenterMenu), "blueprints").SetValue(cm, bp);
                AccessTools.Field(typeof(CarpenterMenu), "currentBlueprintIndex").SetValue(cm, 0);
                cm.setNewActiveBlueprint();

                ReturnLocation = new LocationRequest(Game1.currentLocation.NameOrUniqueName, Game1.currentLocation.isStructure.Value, Game1.currentLocation);
                Game1.activeClickableMenu = cm;
                Helper.Events.Display.MenuChanged += Display_MenuChanged;
            }
        }

        private void Display_MenuChanged(object sender, StardewModdingAPI.Events.MenuChangedEventArgs e)
        {
            if (e.NewMenu == null)
            {
                Game1.warpFarmer(ReturnLocation, Game1.player.getTileX(), Game1.player.getTileY(), Game1.player.facingDirection);
                Helper.Events.Display.MenuChanged -= Display_MenuChanged;
            }
        }

        public static void WarpCharacter(NPC character, ref GameLocation targetLocation, Vector2 position)
        {
            if (!ShouldBeActive())
                return;

            if (character is Horse horse)
            {
                if (horse.getOwner() is Farmer farmer && !farmer.IsMainPlayer && farmer.modData.TryGetValue("Platonymous.LandGrants." + targetLocation.Name, out string realTarget) && targetLocation.NameOrUniqueName != realTarget)
                    targetLocation = Game1.getLocationFromName(realTarget);
            }

            if (character is Pet pet)
            {
                if (pet.currentLocation is GameLocation location && location.modData.TryGetValue("Platonymous.LandGrants." + targetLocation.Name, out string realTarget) && targetLocation.NameOrUniqueName != realTarget)
                    targetLocation = Game1.getLocationFromName(realTarget);
            }

            if (character is NPC npc && npc.getSpouse() is Farmer spouse && !spouse.IsMainPlayer)
            {
                if (spouse.modData.TryGetValue("Platonymous.LandGrants." + targetLocation.Name, out string realTarget) && targetLocation.NameOrUniqueName != realTarget)
                    targetLocation = Game1.getLocationFromName(realTarget);
            }
        }

        public static void PerformHoverAction(CharacterCustomization __instance, int x, int y)
        {
            if (!FromMPMenu)
                return;

            foreach (ClickableTextureComponent c5 in __instance.rightSelectionButtons)
            {
                if (c5.containsPoint(x, y))
                {
                    c5.scale = Math.Min(c5.scale + 0.02f, c5.baseScale + 0.1f);
                }
                else
                {
                    c5.scale = Math.Max(c5.scale - 0.02f, c5.baseScale);
                }
                if (c5.name.Equals("Cabins") && Game1.startingCabins == MaxPlayers)
                {
                    c5.scale = 0f;
                }

                if (c5.name.Equals("Cabins"))
                {
                    CabinButtonScale = c5.scale;
                }
            }
        }

        public static void PerformHoverActionPost(CharacterCustomization __instance)
        {
            if (!FromMPMenu)
                return;

            foreach (ClickableTextureComponent c5 in __instance.rightSelectionButtons)
            {
                if (c5.name.Equals("Cabins"))
                {
                    c5.scale = CabinButtonScale;
                }
            }
        }

        public static bool SelectionClick(string name, int change)
        {
            if (!FromMPMenu)
                return true;

            if (name == "Cabins")
            {
                if ((Game1.startingCabins != 0 || change >= 0) && (Game1.startingCabins != MaxPlayers || change <= 0))
                {
                    Game1.playSound("axchop");
                }
                Game1.startingCabins += change;
                Game1.startingCabins = Math.Max(0, Math.Min(MaxPlayers, Game1.startingCabins));

                if (Game1.startingCabins > MinPlayers)
                    if (!Game1.player.modData.ContainsKey("Platonymous.LandGrants.MultiFarm"))
                        Game1.player.modData.Add("Platonymous.LandGrants.MultiFarm", Game1.startingCabins.ToString());
                    else
                        Game1.player.modData["Platonymous.LandGrants.MultiFarm"] = Game1.startingCabins.ToString();

                return false;
            }

            return true;
        }

        private void GameLoop_UpdateTicked(object sender, StardewModdingAPI.Events.UpdateTickedEventArgs e)
        {
            if (Game1.activeClickableMenu is TitleMenu && TitleMenu.subMenu is CoopMenu)
                FromMPMenu = true;
            else if (Game1.activeClickableMenu is TitleMenu && TitleMenu.subMenu == null)
            {
                FromMPMenu = false;
                IsActive = false;
            }
        }

        public static bool PerformTenMinuteUpdate(GameLocation __instance)
        {
            return ShouldPerformUpdate(__instance);
        }

        public static bool PerformTenMinuteUpdate(FarmCave __instance)
        {
            return ShouldPerformUpdate(__instance);
        }

        public static bool PerformTenMinuteUpdate(FarmHouse __instance)
        {
            return ShouldPerformUpdate(__instance);
        }

        public static bool PerformTenMinuteUpdate(Farm __instance)
        {
            return ShouldPerformUpdate(__instance);
        }

        public static bool PerformTenMinuteUpdate(Cellar __instance)
        {
            return ShouldPerformUpdate(__instance);
        }

        public static bool PerformTenMinuteUpdate(Cabin __instance)
        {
            return ShouldPerformUpdate(__instance);
        }

        public static bool PerformDayUpdate(GameLocation __instance)
        {
            return ShouldPerformUpdate(__instance);
        }

        public static bool PerformDayUpdate(FarmCave __instance)
        {
            return ShouldPerformUpdate(__instance);
        }

        public static bool PerformDayUpdate(FarmHouse __instance)
        {
            return ShouldPerformUpdate(__instance);
        }

        public static bool PerformDayUpdate(Farm __instance)
        {
            return ShouldPerformUpdate(__instance);
        }

        public static bool PerformDayUpdate(Cellar __instance)
        {
            return ShouldPerformUpdate(__instance);
        }

        public static bool PerformDayUpdate(Cabin __instance)
        {
            return ShouldPerformUpdate(__instance);
        }

        public static bool ShouldPerformUpdate(GameLocation location)
        {
            if (Config.KeepFarmsActive || !ShouldBeActive() || IsOwnedByActiveFarmer(location))
                return true;

            return false;
        }
        
        public static void Activate(FarmhandMenu.FarmhandSlot __instance)
        {
            FarmhandMenu menu = (FarmhandMenu)AccessTools.Field(typeof(FarmhandMenu.FarmhandSlot), "menu").GetValue(__instance);
            if (menu.client != null)
                LastFarmHandLoaded = __instance.Farmer;
        }

        public static bool BuildStartingCabins()
        {
            if (LastFarmHandLoaded == null)
                LastFarmHandLoaded = Game1.player;

            if (LastFarmHandLoaded == null)
                return true;

            int maxPlayers = LastFarmHandLoaded.modData.TryGetValue("Platonymous.LandGrants.MultiFarm", out string mfstring) && int.TryParse(mfstring, out int mf) ? mf : -1;
            LastFarmHandLoaded = null;
            if (maxPlayers == -1)
            {
                maxPlayers = Game1.startingCabins;

                if (SaveGame.loaded?.player?.modData is ModDataDictionary md && md.TryGetValue("Platonymous.LandGrants.MultiFarm", out string cnum) && int.TryParse(cnum, out int cabinsnum))
                    maxPlayers = Math.Max(Game1.startingCabins, cabinsnum);

                if (maxPlayers < MinPlayers)
                {
                    IsActive = false;
                    return true;
                }
            }

            if (!Game1.player.modData.ContainsKey("Platonymous.LandGrants.MultiFarm"))
                Game1.player.modData.Add("Platonymous.LandGrants.MultiFarm", maxPlayers.ToString());

            IsActive = true;

            var count = GetMasterFarm().buildings.Where(b => b.isCabin).Count() + 1;
            while (count < maxPlayers + 1)
            {
                CreateNewFarmHandFarm(count, maxPlayers);
                count++;
            }

            return false;
        }

        public static void ReloadMap(GameLocation __instance)
        {
            if (!ShouldBeActive())
                return;

            if (__instance.mapPath.Value.Contains("_LG_"))
                __instance.mapPath.Value = __instance.mapPath.Value.Split("_LG_", StringSplitOptions.None)[0];

            if (__instance.mapPath.Value.StartsWith(@"Maps\Cabin"))
                __instance.mapPath.Value = @"Maps\Cabin";

        }

        public static void LoadMap(GameLocation __instance, ref string mapPath)
        {
            if (!ShouldBeActive())
                return;

            if (mapPath.Contains("_LG_"))
                mapPath = mapPath.Split("_LG_", StringSplitOptions.None)[0];

            if (mapPath.StartsWith(@"Maps\Cabin"))
                mapPath = @"Maps\Cabin";

            __instance.mapPath.Value = mapPath;
        }

        private void GameLoop_DayStarted(object sender, StardewModdingAPI.Events.DayStartedEventArgs e)
        {
            if (!ShouldBeActive())
                return;

            if (!Context.IsMainPlayer && Game1.player.homeLocation.Value.StartsWith("Cabin") && Game1.player.modData.TryGetValue("Platonymous.LandGrants.FarmHouse", out string farmhouse))
                Game1.player.homeLocation.Value = farmhouse;

            if (Game1.netWorldState?.Value?.CurrentPlayerLimit is not null)
                Game1.netWorldState.Value.CurrentPlayerLimit.Value = MaxPlayers;

            if (Context.IsMainPlayer)
            {
                SetWarpFarmData(Game1.getFarm(), 0);
                SetModDataMain(Game1.getFarm().modData, Game1.getLocationFromName("FarmHouse").modData, Game1.getLocationFromName("Greenhouse").modData, Game1.getLocationFromName("FarmCave").modData, Game1.getLocationFromName("Cellar").modData);

                if (Game1.getLocationFromName("Forest") is Forest forest
                    && !forest.modData.ContainsKey("Platonymous.FarmGrants.Farm")
                    && Game1.player.modData.TryGetValue("Platonymous.LandGrants.MultiFarm", out string numstring)
                    && int.TryParse(numstring, out int num))
                {
                    if (!forest.modData.ContainsKey("Platonymous.LandGrants.Farm"))
                    {
                        forest.modData.Add("Platonymous.LandGrants.Farm", "WarpFarmUp");
                        forest.modData.Add("Platonymous.LandGrants.Position", (num + 2).ToString());
                    }
                }
                CompareRealStrings = true;
                foreach (Farm farm in Game1.locations.Where(l => l is Farm && l.modData.ContainsKey("Platonymous.LandGrants.SaveCabin")))
                {

                    if (farm.modData.TryGetValue("Platonymous.LandGrants.SaveCabin", out string cabinloc) && farm.modData.TryGetValue("Platonymous.LandGrants.FarmHouse", out string fhloc) && Game1.getLocationFromName(fhloc) is FarmHouse fh && fh.NameOrUniqueName != "FarmHouse")
                    {
                        if (farm.modData.TryGetValue("Platonymous.LandGrants.Position", out string posstr) && int.TryParse(posstr, out int pos) && Game1.getLocationFromName(cabinloc, true) is Cabin cabin)
                        {
                            Farmer farmer = cabin.getFarmhand();
                            if (farmer.homeLocation.Value.StartsWith("Cabin") && farmer.modData.TryGetValue("Platonymous.LandGrants.FarmHouse", out string fhname))
                                farmer.homeLocation.Value = fhname;
                            SetModData(cabin, pos, fh.modData);

                            if (fh.owner != Game1.player && cabin.owner != Game1.player)
                            {
                                int level = Math.Max(cabin.upgradeLevel, fh.upgradeLevel);
                                cabin.upgradeLevel = level;
                                fh.upgradeLevel = level;
                                fh.updateFarmLayout();
                            }
                        }
                    }
                }
                
                (Game1.getLocationFromName("FarmHouse") as FarmHouse).updateFarmLayout();

                CompareRealStrings = false;

                foreach (Farm farm in Game1.locations.Where(l => l is Farm))
                {
                    if (farm.modData.TryGetValue("Platonymous.LandGrants.SaveCabin", out string cabinloc))
                        foreach (Building b in farm.buildings.Where(b => b.indoors.Value is GameLocation))
                        {
                            if (farm.modData.TryGetValue("Platonymous.LandGrants.Position", out string posstr) && int.TryParse(posstr, out int pos) && Game1.getLocationFromName(cabinloc, true) is Cabin cabin)
                                SetModData(cabin, pos, b.indoors.Value.modData);
                        }
                    else
                        foreach (Building b in farm.buildings.Where(b => b.indoors.Value is GameLocation))
                        {
                            if (farm.modData.TryGetValue("Platonymous.LandGrants.Position", out string posstr) && int.TryParse(posstr, out int pos) && pos == 0)
                                SetModDataMain(b.indoors.Value.modData);
                        }
                }
            }
            else
                Game1.warpFarmer(new LocationRequest(Game1.currentLocation.NameOrUniqueName, false, Game1.currentLocation), Game1.player.getTileX(), Game1.player.getTileY(), Game1.player.FacingDirection);
        }

        public static Farm GetMasterFarm()
        {
            UseLocationNameAsIs = true;
            CompareRealStrings = true;
            Farm farm = Game1.getFarm();
            CompareRealStrings = false;
            UseLocationNameAsIs = false;
            return farm;
        }

        public static void GetLocationFromName(ref string name)
        {
            CompareRealStrings = true;

            if (!ShouldBeActive())
                return;

            if (!Context.IsMainPlayer && Game1.player.modData.TryGetValue("Platonymous.LandGrants." + name, out string location) && ((name == "Farm" && Game1.currentLocation?.Name == "AnimalShop") || OverrideLocations))
            {
                name = location;
            }

            if (OverrideFarm is Farm && name == "Farm")
                name = OverrideFarm.NameOrUniqueName;

        }

        public static void GetLocationFromName2(string name, ref GameLocation __result)
        {
            CompareRealStrings = !ShouldBeActive();
        }

        public static void PerformWarpFarmer(ref LocationRequest locationRequest, ref int tileX, ref int tileY)
        {
            if (!ShouldBeActive())
                return;

            if (Game1.currentLocation?.modData is ModDataDictionary md && md.TryGetValue("Platonymous.LandGrants." + locationRequest.Name, out string location) && location != locationRequest.Name)
            {
                UseLocationNameAsIs = true;

                if ((location == "WarpFarmDown" || location == "WarpFarmUp")
                    && Game1.currentLocation.modData.TryGetValue("Platonymous.LandGrants.Position", out string posstring)
                    && int.TryParse(posstring, out int pos))
                {
                    bool down = location == "WarpFarmDown";
                    GameLocation warpfarm = null;
                    int farms = Game1.locations.Where(l => l is Farm).Count();
                    while (warpfarm == null && farms > -1)
                    {
                        pos = down ? pos + 1 : pos - 1;
                        farms--;
                        if (Game1.locations.FirstOrDefault(l => l is Farm && l.modData.TryGetValue("Platonymous.LandGrants.Position", out string npos)
                            && int.TryParse(npos, out int nposi) && nposi == pos) is GameLocation farm)
                        {
                            if (IsOwnedByActiveFarmer(farm))
                                warpfarm = farm;
                        }
                    }


                    if (warpfarm != null)
                    {
                        locationRequest = new LocationRequest(pos == 0 ? warpfarm.Name : warpfarm.uniqueName.Value, locationRequest.IsStructure, warpfarm);
                        tileX = down ? 40 : 41;
                        tileY = down ? 0 : 64;
                    }

                    return;
                }

                locationRequest = new LocationRequest(location, locationRequest.IsStructure, Game1.getLocationFromName(location, locationRequest.IsStructure));
                UseLocationNameAsIs = false;
            }
            else if (Game1.player?.modData is ModDataDictionary pmd && pmd.TryGetValue("Platonymous.LandGrants." + locationRequest.Name, out string playerlocation) && playerlocation != locationRequest.Name)
            {
                UseLocationNameAsIs = true;
                locationRequest = new LocationRequest(playerlocation, locationRequest.IsStructure, Game1.getLocationFromName(playerlocation, locationRequest.IsStructure));
                UseLocationNameAsIs = false;
            }

        }

        public static bool IsOwnedByActiveFarmer(GameLocation location)
        {
            if (location == null || Config.KeepFarmsActive || !location.modData.ContainsKey("Platonymous.LandGrants.SaveCabin"))
                return true;

            bool isactive = (location.modData.TryGetValue("Platonymous.LandGrants.SaveCabin", out string fhlocation) && Game1.getLocationFromName(fhlocation) is Cabin fh && (fh.getFarmhand().Value.isActive()));
            return isactive;
        }

        public static bool IsAlwaysActiveLocation(GameLocation location, ref bool __result)
        {
            if (!ShouldBeActive())
                return true;

            if (location.Name.StartsWith("Farm_") || location.Name.StartsWith("FarmHouse_") || location.Name.StartsWith("Greenhouse_") || location.Name.StartsWith("Cellar_"))
            {
                __result = true;
                return false;
            }

            return true;
        }

        public static void Owner(FarmHouse __instance, ref Farmer __result)
        {
            if (!ShouldBeActive())
                return;

            if (__instance.modData.TryGetValue("Platonymous.LandGrants.SaveCabin", out string cabinName)
                && Game1.getLocationFromName(cabinName, true) is Cabin c)
                __result = c.getFarmhand().Value;
        }

        public static void AddSaveFiles(List<Farmer> files)
        {
            foreach (Farmer file in files)
                file.slotCanHost = file.modData.ContainsKey("Platonymous.LandGrants.MultiFarm") || file.slotCanHost;
        }

        public static Cabin BuildNewCabin(Farmer player)
        {
            Building b = new Building(new BluePrint("Log Cabin")
            {
                magical = true
            }, Vector2.Zero);
            b.daysOfConstructionLeft.Value = 0;
            b.load();
            GetMasterFarm().buildStructure(b, Vector2.Zero, player, skipSafetyChecks: true);

            if (Game1.getFarm() is Farm farm && !farm.buildings.Contains(b))
                farm.buildings.Add(b);
            b.modData.Add("IsFakeCabin", "true");

            if (b.indoors.Value is Cabin cabin)
                return cabin;

            return null;
        }

        public static void CreateNewFarmHandFarm(int position, int maxPlayers)
        {
            var newCabin = BuildNewCabin(Game1.MasterPlayer);

            var newFarmHouse = new FarmHouse("Maps\\FarmHouse", "FarmHouse_LG_" + position);
            var newGreenhouse = new GameLocation("Maps\\Greenhouse", "Greenhouse_LG_" + position);
            var newFarm = new Farm("Maps\\" + Farm.getMapNameFromTypeInt(Game1.whichFarm), "Farm_LG_" + position);
            var newCellar = new Cellar("Maps\\Cellar", "Cellar_LG_" + position);
            var newFarmCave = new FarmCave("Maps\\FarmCave", "FarmCave_LG_" + position);

            newFarm.uniqueName.Value = "Farm_LG_" + position;
            newGreenhouse.uniqueName.Value = "Greenhouse_LG_" + position;
            newFarmHouse.uniqueName.Value = "FarmHouse_LG_" + position;
            newCellar.uniqueName.Value = "Cellar_LG_" + position;
            newFarmCave.uniqueName.Value = "FarmCave_LG_" + position;

            SetModData(newCabin, position, newFarmCave, newFarm, newFarmHouse, newGreenhouse, newCellar, newCabin);

            SetWarpFarmData(newFarm, position);

            if (Context.IsMainPlayer)
            {
                var newFarmer = newCabin.getFarmhand().Value;
                SetModData(newCabin, position, newFarmer);
                newFarmer.modData.Add("Platonymous.LandGrants.MultiFarm", maxPlayers.ToString());
                newFarmer.currentLocation = newFarmHouse;
                newFarmer.homeLocation.Value = newFarmHouse.uniqueName.Value;
                newFarmer.lastSleepLocation.Value = newFarmer.homeLocation.Value;
                newFarmer.lastSleepPoint.Value = newFarmHouse.getBedSpot();
                newFarmer.Position = Utility.PointToVector2(newFarmer.lastSleepPoint.Value) * 64f;
            }
        }

        public static void SetWarpFarmData(GameLocation location, int position)
        {
            if (!location.modData.ContainsKey("Platonymous.LandGrants.Backwoods"))
                location.modData.Add("Platonymous.LandGrants.Backwoods", "WarpFarmUp");
            if (!location.modData.ContainsKey("Platonymous.LandGrants.Forest"))
                location.modData.Add("Platonymous.LandGrants.Forest", "WarpFarmDown");
            if (!location.modData.ContainsKey("Platonymous.LandGrants.Position"))
                location.modData.Add("Platonymous.LandGrants.Position", position.ToString());

            if (!Game1.locations.Contains(location))
                Game1.locations.Add(location);
        }

        public static void SetModData(Cabin cabin, int position, params GameLocation[] locations)
        {
            SetModData(cabin, position, locations.Select(l => l.modData).ToArray());
            SetModData(cabin, position, cabin.getFarmhand().Value.modData);

            foreach (var location in locations.Where(l => !Game1.locations.Contains(l)))
                Game1.locations.Add(location);
        }

        public static void SetModData(Cabin cabin, int position, Farmer farmer)
        {
            SetModData(cabin, position, farmer.modData);
        }

        public static void SetModData(Cabin cabin, int position, params ModDataDictionary[] modDataSet)
        {
            foreach (var modData in modDataSet)
            {
                if (!modData.ContainsKey("Platonymous.LandGrants.SaveCabin"))
                    modData.Add("Platonymous.LandGrants.SaveCabin", cabin.uniqueName.Value);
                if (!modData.ContainsKey("Platonymous.LandGrants.Cellar"))
                    modData.Add("Platonymous.LandGrants.Cellar", "Cellar_LG_" + position);
                if (!modData.ContainsKey("Platonymous.LandGrants.Greenhouse"))
                    modData.Add("Platonymous.LandGrants.Greenhouse", "Greenhouse_LG_" + position);
                if (!modData.ContainsKey("Platonymous.LandGrants.Farm"))
                    modData.Add("Platonymous.LandGrants.Farm", "Farm_LG_" + position);
                if (!modData.ContainsKey("Platonymous.LandGrants.FarmHouse"))
                    modData.Add("Platonymous.LandGrants.FarmHouse", "FarmHouse_LG_" + position);
                if (!modData.ContainsKey("Platonymous.LandGrants.FarmCave"))
                    modData.Add("Platonymous.LandGrants.FarmCave", "FarmCave_LG_" + position);
            }
        }

        public static void SetModDataMain(params ModDataDictionary[] modDataSet)
        {
            foreach (var modData in modDataSet)
            {
                if (!modData.ContainsKey("Platonymous.LandGrants.Greenhouse"))
                    modData.Add("Platonymous.LandGrants.Greenhouse", "Greenhouse");
                if (!modData.ContainsKey("Platonymous.LandGrants.Farm"))
                    modData.Add("Platonymous.LandGrants.Farm", "Farm");
                if (!modData.ContainsKey("Platonymous.LandGrants.FarmHouse"))
                    modData.Add("Platonymous.LandGrants.FarmHouse", "FarmHouse");
                if (!modData.ContainsKey("Platonymous.LandGrants.FarmCave"))
                    modData.Add("Platonymous.LandGrants.FarmCave", "FarmCave");
                if (!modData.ContainsKey("Platonymous.LandGrants.Cellar"))
                    modData.Add("Platonymous.LandGrants.Cellar", "Cellar");
            }
        }
    }
}
