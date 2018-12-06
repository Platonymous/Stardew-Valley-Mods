using Harmony;
using StardewModdingAPI;
using StardewValley;
using System.Reflection;
using PyTK;
using PyTK.CustomElementHandler;
using PyTK.Types;
using PyTK.Extensions;
using StardewValley.Menus;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using System;
using SObject = StardewValley.Object;
using System.Threading.Tasks;
using System.Linq;
using StardewValley.BellsAndWhistles;
using Netcode;
using StardewValley.Buildings;
using StardewValley.Locations;
using StardewModdingAPI.Events;

namespace SplitMoney
{
    public class SplitMoneyMod : Mod
    {
        internal static PyResponder<bool, int> moneyReceiver;
        internal static string moneyReceiverName = "Platonymous.MoneyReceiver";
        internal static int startMoney = 500;
        internal static List<Farmer> players = new List<Farmer>();
        internal static List<Farmer> top4 = new List<Farmer>();
        internal static IMonitor SMonitor;
        internal static IModHelper SHelper;
        internal static Dictionary<Farmer, int> moneyToSplitPerFarmer { get; set; } = new Dictionary<Farmer, int>();
        internal static int myMoney = -1;
        internal static bool lockMoney = true;

        public override void Entry(IModHelper helper)
        {
            SMonitor = Monitor;
            SHelper = Helper;
            moneyReceiver = new PyResponder<bool, int>(moneyReceiverName, (i) => { Game1.player.Money += i; return true; }, 30);
            moneyReceiver.start();
            SaveHandler.promiseType(typeof(GoldItem));

            SaveEvents.AfterReturnToTitle += (s, e) => { myMoney = -1; lockMoney = true; };
            SaveEvents.BeforeSave += (s, e) =>
            {
                if (Game1.IsMasterGame)
                {
                    Game1.player.teamRoot.Value.money.Value = myMoney;
                    Game1.player.teamRoot.Value.money.MarkClean();
                    convertMoney(true);
                    SaveHandler.ReplaceAll(Game1.player.items, Game1.player);
                }
            };

            PyTK.Events.PyTimeEvents.BeforeSleepEvents += (s, e) =>
            {
                if (!Game1.IsMasterGame)
                {
                    int getMoney = 0;
                    foreach (StardewValley.Object obj in Game1.getFarm().shippingBin.Where(i => i is StardewValley.Object o && o.owner == Game1.player.UniqueMultiplayerID))
                        getMoney += obj.sellToStorePrice() * obj.Stack;

                    myMoney += getMoney;
                    convertMoney(true);
                    myMoney -= getMoney;

                    SaveHandler.ReplaceAll(Game1.player.items, Game1.player);
                }
            };

            SaveEvents.AfterLoad += (s, e) =>
            {
                lockMoney = false;
            };

            #region moneyItem
            new CustomObjectData("Platonymous.G", "G/1/-300/Basic/G/The common currency of Pelican Town.", Game1.mouseCursors.getArea(new Rectangle(280, 410, 16, 16)), Color.White, type: typeof(GoldItem));
            ButtonClick.ActionButton.onClick((pos) => new List<IClickableMenu>(Game1.onScreenMenus).Exists(m => m is DayTimeMoneyBox && m.isWithinBounds(pos.X, pos.Y - 180) && m.isWithinBounds(pos.X, pos.Y) && m.isWithinBounds(pos.X, pos.Y + 50)), (p) => convertMoney());
            #endregion

            #region harmony
            HarmonyInstance instance = HarmonyInstance.Create("Platonymous.SplitMoney");
            instance.PatchAll(Assembly.GetExecutingAssembly());
            #endregion
        } 

        #region methods

        private static void markObject(Item item, Farmer who)
        {
            if (item is StardewValley.Object obj)
            {
                obj.name = obj.name + ">>" + who.Name;
                obj.owner.Value = who.UniqueMultiplayerID;
            }
        }

        private void convertMoney(bool forSaving = false)
        {
            if (Game1.player.ActiveObject != null && !forSaving)
            {
                if (Game1.player.ActiveObject is GoldItem s)
                {
                    if (s.forSaving && !Game1.IsMasterGame)
                        return;

                    Game1.player.money += s.forSaving ? s.Stack - 1 : s.Stack;
                    Game1.player.removeItemFromInventory(s);
                    Game1.playSound("sell");
                }
                return;
            }

            int a = forSaving ? Game1.player.money : Math.Min(999, Game1.player.money);

            if (a <= 0 && !forSaving)
                return;

            if(!forSaving)
                Game1.player.money -= a;
            GoldItem gold = (GoldItem) CustomObjectData.collection["Platonymous.G"].getObject();
            gold.forSaving = forSaving;
            gold.Stack = forSaving ? a + 1 : a;
            if (!forSaving)
            {
                Game1.player.addItemByMenuIfNecessary(gold);
                Game1.playSound("purchase");
            }
            else
            {
                while (Game1.player.items.ToList().Find(i => i is GoldItem go && go.forSaving) is GoldItem gItem)
                    Game1.player.items.Remove(gItem);

                Game1.player.items.Add(gold);
            }
            }

        internal static void sendMoney(Farmer receiver, int money)
        {
            if (receiver == Game1.player)
                Game1.player.Money += money;
            else
            {
                Task.Run(async () =>
                {
                    await PyNet.sendRequestToFarmer<bool>(moneyReceiverName, money, receiver);
                });
            }
        }

        public override object GetApi()
        {
            return new SplitMoneyAPI();
        }

        #endregion

        #region overrides

        [HarmonyPatch]
        internal class FriendShipFix
        {
            internal static MethodInfo TargetMethod()
            {
                return PyUtils.getTypeSDV("SaveGame").GetMethod("loadDataToFarmer", BindingFlags.Public | BindingFlags.Static);
            }

            internal static void Prefix(Farmer target, ref int __state)
            {
                if (target.friendships != null && target.friendships.ContainsKey("money"))
                {
                    __state = target.friendships["money"][0];
                    target.friendships.Remove("money");
                }
                else
                {
                    __state = -1;
                }
            }

            internal static void Postfix(Farmer target, ref int __state)
            {
                if (__state != -1)
                {
                    if (target.friendships == null)
                        target.friendships = new SerializableDictionary<string, int[]>();

                    target.friendships.Add("money", new int[] { __state });
                }
            }
        }

        [HarmonyPatch]
        internal class ShippingSplitFix2
        {
            internal static MethodInfo TargetMethod()
            {
                return PyUtils.getTypeSDV("Menus.ShippingMenu").GetMethod("parseItems");
            }

            public static void Prefix(ShippingMenu __instance, IList<Item> items)
            {
                players = Game1.getAllFarmers().ToList();
                moneyToSplitPerFarmer = new Dictionary<Farmer, int>();
                players.ForEach(p => moneyToSplitPerFarmer.Add(p, 0));

                foreach (Item item in items)
                    if (item is StardewValley.Object obj && obj.owner.Value is long u)
                    {
                        if (u != Game1.player.UniqueMultiplayerID && !moneyToSplitPerFarmer.Keys.ToList().Exists(k => k.UniqueMultiplayerID == u))
                        {
                            if (players.Find(f => f.Name == obj.netName.Value.Split(new string[] { ">>" }, StringSplitOptions.None)[1]) is Farmer otherFarmer)
                                u = otherFarmer.UniqueMultiplayerID;
                            else
                                u = Game1.MasterPlayer.UniqueMultiplayerID;

                            obj.owner.Value = u;
                        }

                        int price = obj.sellToStorePrice() * obj.Stack;

                        if(u == Game1.player.UniqueMultiplayerID)
                            myMoney += price;

                        Farmer up = moneyToSplitPerFarmer.Find(mts => mts.Key.UniqueMultiplayerID == u).Key;
                        moneyToSplitPerFarmer[up] += price;
                    }

                var msplit = moneyToSplitPerFarmer.clone();
                top4 = new List<Farmer>();
                while (top4.Count < 4 && msplit.Count > 0)
                {
                    var max = msplit.Find(msp => msp.Value == msplit.Values.Max());
                    if(max.Value > 0)
                        top4.Add(max.Key);
                    msplit.Remove(max.Key);
                }

                lockMoney = true;
            }

            public static void Postfix(ShippingMenu __instance, IList<Item> items)
            {
                lockMoney = false;
            }
        }

        [HarmonyPatch]
        internal class ObjectName
        {
            internal static MethodInfo TargetMethod()
            {
                return PyUtils.getTypeSDV("Object").GetProperty("name").GetGetMethod();
            }

            internal static void Postfix(StardewValley.Object __instance, ref string __result)
            {
                if (__result != null && __result.Contains(">>"))
                    __result = __result.Split(new string[] { ">>" },StringSplitOptions.None)[0];
            }
        }

        [HarmonyPatch]
        internal class CanStackWith
        {
            internal static MethodInfo TargetMethod()
            {
                return PyUtils.getTypeSDV("Item").GetMethod("canStackWith", BindingFlags.Public | BindingFlags.Instance);
            }

            internal static void Postfix(Item __instance, Item other, ref bool __result)
            {
                if (__result && __instance is StardewValley.Object obj1 && other is StardewValley.Object obj2)
                    __result = obj1.owner == obj2.owner;
            }           
        }

        [HarmonyPatch]
        internal class CategoryName
        {
            internal static MethodInfo TargetMethod()
            {
                return PyUtils.getTypeSDV("Menus.ShippingMenu").GetMethod("getCategoryName");
            }

            internal static void Postfix(int index, ref string __result)
            {
                if (!Game1.IsMultiplayer || index >= 4)
                    return;

                if (index >= top4.Count)
                    __result = "------------";
                else
                    __result = top4[index].Name;
            }
        }

        [HarmonyPatch]
        internal class CategoryId
        {
            internal static MethodInfo TargetMethod()
            {
                return PyUtils.getTypeSDV("Menus.ShippingMenu").GetMethod("getCategoryIndexForObject");
            }

            internal static void Postfix(StardewValley.Object o, ref int __result)
            {
                if (!Game1.IsMultiplayer)
                    return;

                if (Game1.IsMasterGame && (__result == -75 || __result == -79))
                    Game1.stats.CropsShipped += (uint)o.Stack;

                if (top4.FindIndex(f =>f.UniqueMultiplayerID == o.owner) is int i && i >= 0)
                    __result = i;
                else
                    __result = 4;
            }
        }

        [HarmonyPatch]
        internal class ShipItem
        {
            internal static MethodInfo TargetMethod()
            {
                return PyUtils.getTypeSDV("Buildings.ShippingBin").GetMethod("shipItem",BindingFlags.NonPublic | BindingFlags.Instance);
            }

            internal static void Prefix(ref Item i, Farmer who)
            {
                markObject(i, who);
            }
        }

        [HarmonyPatch]
        internal class ShipItem2
        {
            internal static MethodInfo TargetMethod()
            {
                return PyUtils.getTypeSDV("Buildings.ShippingBin").GetMethod("shipItem", BindingFlags.Public | BindingFlags.Instance);
            }

            internal static void Prefix(ref Item i)
            {
                markObject(i, Game1.player);
            }
        }


        [HarmonyPatch]
        internal class ShipItem3
        {
            internal static MethodInfo TargetMethod()
            {
                return PyUtils.getTypeSDV("Buildings.ShippingBin").GetMethod("showShipment", BindingFlags.Public | BindingFlags.Instance);
            }

            internal static void Prefix(ref StardewValley.Object o)
            {
                markObject(o, Game1.player);
            }
        }

        [HarmonyPatch]
        internal class ShipItem4
        {
            internal static MethodInfo TargetMethod()
            {
                return PyUtils.getTypeSDV("Farm").GetMethod("shipItem", BindingFlags.NonPublic | BindingFlags.Instance);
            }

            internal static void Prefix(ref Item i, Farmer who)
            {
                markObject(i, who);
            }
        }

        [HarmonyPatch]
        internal class ShipItem5
        {
            internal static MethodInfo TargetMethod()
            {
                return PyUtils.getTypeSDV("Farm").GetMethod("shipItem", BindingFlags.Public | BindingFlags.Instance);
            }

            internal static void Prefix(ref Item i)
            {
                markObject(i, Game1.player);
            }
        }

        [HarmonyPatch]
        internal class ShipItem6
        {
            internal static MethodInfo TargetMethod()
            {
                return PyUtils.getTypeSDV("Farm").GetMethod("showShipment", BindingFlags.Public | BindingFlags.Instance);
            }

            internal static void Prefix(ref StardewValley.Object o)
            {
                markObject(o, Game1.player);
            }
        }

        [HarmonyPatch]
        internal class MoneyGetter
        {
            internal static MethodInfo TargetMethod()
            {
                return PyUtils.getTypeSDV("Farmer").GetProperty("money").GetGetMethod();
            }

            internal static bool Prefix(Farmer __instance, ref int __result)
            {
                if (Game1.activeClickableMenu is TitleMenu)
                    return true;

                if (myMoney == -1)
                    __result = Game1.IsMasterGame ? Game1.player.teamRoot.Value.money.Value : startMoney;
                else
                    __result = myMoney;

                return false;
            }
        }

        [HarmonyPatch]
        internal class MoneySetter
        {
            internal static MethodInfo TargetMethod()
            {
                return PyUtils.getTypeSDV("Farmer").GetProperty("money").GetSetMethod();
            }

            internal static bool Prefix(Farmer __instance, int value)
            {
                if (Game1.activeClickableMenu is TitleMenu)
                    return true;

                if (lockMoney || __instance != Game1.player)
                    return false;

                myMoney = value;
                SMonitor.Log("Set Money to:" + value);

                return false;
            }
        }

        [HarmonyPatch]
        internal class ProposalFix
        {
            internal static MethodInfo TargetMethod()
            {
                return AccessTools.Method(PyUtils.getTypeSDV("FarmerTeam"), "SendProposal");
            }

            internal static bool Prefix(Farmer receiver, ProposalType proposalType, Item gift)
            {
                if (proposalType == ProposalType.Gift && gift is GoldItem)
                {
                    int money = Game1.player.ActiveObject.Stack;
                    sendMoney(receiver, money);
                    Game1.player.removeItemFromInventory(Game1.player.ActiveObject);
                    return false;
                }
                else
                    return true;
            }
        }
        #endregion

    }
}
