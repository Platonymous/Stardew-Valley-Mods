using Microsoft.Xna.Framework;
using PyTK.Types;
using PyTK.Extensions;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using PyTK.Tiled;
using System.IO;
using StardewValley.Menus;

namespace ATM
{
    public class ATMMod : Mod
    {
        internal static Config config;
        internal ITranslationHelper i18n => Helper.Translation;
        internal BankAccount bankAccount;
        internal List<Response> responses;

        public override void Entry(IModHelper helper)
        {
            config = helper.ReadConfig<Config>();

            helper.Events.GameLoop.Saving += OnSaving;
            helper.Events.GameLoop.SaveLoaded += OnSaveLoaded;
            helper.Events.GameLoop.DayStarted += OnDayStarted;

            responses = new List<Response>();
            responses.Add(new Response("ATM_Deposit", i18n.Get("ATM_Deposit")));
            responses.Add(new Response("ATM_Daily_Deposit", i18n.Get("ATM_Daily_Deposit")));
            responses.Add(new Response("ATM_Withdraw", i18n.Get("ATM_Withdraw")));
            responses.Add(new Response("ATM_Close", i18n.Get("ATM_Close")));

            var openATMAction = new TileAction("OpenATM", openATM);
            openATMAction.register();

            var atmWinterCheck = new TileAction("ATMWinterCheck", checkSeason);
            atmWinterCheck.register();

            var atm = TMXContent.Load(Path.Combine("Assets", "atm.tmx"), Helper);
            atm.injectInto(@"Maps/" + config.Map, new Vector2(config.Position[0], config.Position[1]), null);
        }

        private bool checkSeason(string action, GameLocation location, Vector2 tileposition, string layer)
        {
            var tile1 = location.Map.GetLayer("Front").Tiles[config.Position[0], config.Position[1]];
            var tile2 = location.Map.GetLayer("Buildings").Tiles[config.Position[0], config.Position[1] + 1];
            bool winter = Game1.currentSeason.ToLower() == "winter";
            tile1.TileIndex = winter ? 1 : 0;
            tile2.TileIndex = winter ? 3 : 2;

            return true;
        }

        private bool openATM(string action, GameLocation location, Vector2 tileposition, string layer)
        {
            if (!Game1.IsMasterGame)
            {
                Game1.addHUDMessage(new HUDMessage(i18n.Get("Fail_Main")));
                return true;
            }

            string text = i18n.Get("Account_Ballance") + ": " + bankAccount.Balance + "g. " + (config.Credit ? i18n.Get("Line_Credit") + ": " + bankAccount.CreditLine + "g" : "");
            Game1.currentLocation.createQuestionDialogue(text, responses.ToArray(), nextMenu);
            return true;
        }

        private void nextMenu(Farmer who, string key)
        {
            if (key == "ATM_Close")
                return;

            var text = responses.Find(r => r.responseKey == key).responseText;

            Game1.activeClickableMenu = new NumberSelectionMenu(text, (number, price, farmer) => processOrder(number, price, farmer, key), -1, 0, (key != "ATM_Withdraw") ? (key == "ATM_Daily_Deposit") ? Math.Max(Game1.player.money, bankAccount.DailyMoneyOrder) : Game1.player.Money : bankAccount.AvailableMoney, (key == "ATM_Daily_Deposit") ? bankAccount.DailyMoneyOrder : Math.Min((key != "ATM_Withdraw") ? Game1.player.money : bankAccount.AvailableMoney, 100));
        }

        private void processOrder(int number, int price, Farmer who, string key)
        {
            if (key == "ATM_Deposit")
            {
                Game1.player.Money -= number;
                bankAccount.ActualBalance += number;
            }
            else if (key == "ATM_Daily_Deposit")
                bankAccount.DailyMoneyOrder = number;
            else if (key == "ATM_Deposit")
            {
                Game1.player.Money -= number;
                bankAccount.ActualBalance += number;
            }
            else if (key == "ATM_Withdraw")
            {
                Game1.player.Money += number;
                bankAccount.ActualBalance -= number;
            }

            Game1.activeClickableMenu = null;
        }

        private void OnDayStarted(object sender, DayStartedEventArgs e)
        {
            if (Game1.IsMasterGame)
            {
                if (bankAccount.DailyMoneyOrder > 0 && bankAccount.DailyMoneyOrder < Game1.player.Money)
                {
                    Game1.player.Money -= bankAccount.DailyMoneyOrder;
                    bankAccount.ActualBalance += bankAccount.DailyMoneyOrder;
                    Game1.addHUDMessage(new HUDMessage(i18n.Get("Daily_Deposite") + ": " + bankAccount.DailyMoneyOrder + "g", 2));
                }

                setInterest();

                if (Game1.dayOfMonth == 1)
                {
                    if (Game1.currentSeason.ToLower() == "spring")
                        setCreditLine();

                    payInterest();
                }
            }
        }

        private void OnSaving(object sender, SavingEventArgs e)
        {
            if (Game1.IsMasterGame)
                Helper.Data.WriteSaveData("Platonymous.ATM.BankAccount", bankAccount);
        }

        private void OnSaveLoaded(object sender, SaveLoadedEventArgs e)
        {
            if (!Game1.IsMasterGame)
                return;

            bankAccount = Helper.Data.ReadSaveData<BankAccount>("Platonymous.ATM.BankAccount");
            if (bankAccount == null)
            {
                bankAccount = new BankAccount();
                setCreditLine();
            }
        }

        private void setCreditLine()
        {
            int line = (int)(Math.Floor(Math.Floor(PyTK.PyUtils.calc(config.CreditLine, new KeyValuePair<string, object>("value", (Math.Max(0, bankAccount.Balance) + Game1.player.Money)))) / 1000) * 1000);
            if (line > bankAccount.CreditLine)
            {
                bankAccount.CreditLine = line;
                if (config.Credit)
                    Game1.addHUDMessage(new HUDMessage(i18n.Get("New_Credit") + ": " + line + "g", 2));
            }
        }

        private void setInterest()
        {
            float value = bankAccount.ActualBalance;

            if (value == 0)
                return;

            float interest = value * (value < 0 ? config.CreditInterest : config.GainInterest);
            bankAccount.UnpaidInterest += (interest / 28);
        }

        private void payInterest()
        {
            int charge = (int)Math.Floor(Math.Abs(bankAccount.UnpaidInterest));
            int value = (int)Math.Floor(bankAccount.UnpaidInterest);

            if (bankAccount.UnpaidInterest == 0 || charge == 0)
                return;

            if (bankAccount.UnpaidInterest > 0 || bankAccount.AvailableMoney >= Math.Abs(bankAccount.UnpaidInterest))
            {
                Game1.addHUDMessage(new HUDMessage(i18n.Get("Interest") + ": " + value + "g", 2));
                bankAccount.ActualBalance += bankAccount.UnpaidInterest;
                bankAccount.UnpaidInterest = 0;
            }
            else
            {
                bankAccount.UnpaidInterest = bankAccount.AvailableMoney - Math.Abs(bankAccount.UnpaidInterest);
                bankAccount.ActualBalance = 0 - bankAccount.CreditLine;
                if (Game1.player.Money > charge)
                {
                    Game1.player.Money -= charge;
                    bankAccount.UnpaidInterest += charge;
                    Game1.addHUDMessage(new HUDMessage(i18n.Get("Interest") + ": " + value + "g", 2));
                }
                else
                {
                    bankAccount.UnpaidInterest += Game1.player.Money;
                    Game1.player.Money = 0;
                    Game1.addHUDMessage(new HUDMessage(i18n.Get("Interest") + ": " + (Game1.player.Money * -1) + "g", 2));
                }
            }
        }
    }
}
