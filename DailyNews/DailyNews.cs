using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;
using xTile;

namespace DailyNews
{

	public class ModEntry : Mod
	{
		GameLocation.afterQuestionBehavior old_afterQuestion;
		int dailyNews;
		ModConfig config;
		Texture2D newsScreen;


		//tv overloading
		private static FieldInfo Field = typeof(GameLocation).GetField("afterQuestion", BindingFlags.Instance | BindingFlags.NonPublic);
		private static FieldInfo TVScreen = typeof(TV).GetField("screen", BindingFlags.Instance | BindingFlags.NonPublic);
		private static GameLocation.afterQuestionBehavior Callback;
		private static TV Target;


		public override void Entry(IModHelper helper)
		{
			// submit to events in StardewModdingAPI
			TimeEvents.DayOfMonthChanged += CheckIfNews;
			this.config = this.Helper.ReadConfig<ModConfig>();
			this.newsScreen = this.Helper.Content.Load<Texture2D>(@"assets\news.png", ContentSource.ModFolder);
		}

		private void CheckIfNews(object sender, EventArgs e)
		{
			string str = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
			if (str.Equals("Tue") || str.Equals("Fri") || str.Equals("Sat"))
			{

				MenuEvents.MenuChanged += Event_MenuChanged;
				Random randomNews = new Random();
				this.dailyNews = randomNews.Next(0,this.config.newsItems.Count);
				showMessage("Breaking News for " + UppercaseFirst(Game1.currentSeason) + " " + Game1.dayOfMonth);
			}
			else
			{
				MenuEvents.MenuChanged -= Event_MenuChanged;
			}
		}

		private void Event_MenuChanged(object sender, EventArgsClickableMenuChanged e)
		{
            TryHookTelevision();
			string day = Game1.shortDayNameFromDayOfSeason(Game1.dayOfMonth);
			if (day.Equals("Tue") || day.Equals("Fri") || day.Equals("Sat"))
			{
				if (e.NewMenu is StardewValley.Menus.DialogueBox)
				{
					
					List<string> dialogues = this.Helper.Reflection.GetPrivateValue<List<string>>(e.NewMenu, "dialogues");
					if (dialogues.Count == 1 && dialogues[0] == Game1.content.LoadString("Strings\\StringsFromCSFiles:TV.cs.13120"))
					{
						List<Response> responseList = this.Helper.Reflection.GetPrivateValue<List<Response>>(e.NewMenu, "responses");
						Response news = new Response("News", "News Report");
						responseList.Insert(responseList.Count - 1, news);
						old_afterQuestion = this.Helper.Reflection.GetPrivateValue<GameLocation.afterQuestionBehavior>(Game1.currentLocation, "afterQuestion");
						GameLocation.afterQuestionBehavior afterQuestion = new GameLocation.afterQuestionBehavior(this.overightChannel);
						this.Helper.Reflection.GetPrivateField<GameLocation.afterQuestionBehavior>(Game1.currentLocation, "afterQuestion").SetValue(afterQuestion);
					}

				}
			}
		}

		public static void showMessage(string msg)
		{
			var hudmsg = new HUDMessage(msg, Color.SeaGreen, 5250f, true);
			hudmsg.whatType = 2;
			Game1.addHUDMessage(hudmsg);
		}

		static string UppercaseFirst(string s)
		{
			// Check for empty string.
			if (string.IsNullOrEmpty(s))
			{
				return string.Empty;
			}
			// Return char and concat substring.
			return char.ToUpper(s[0]) + s.Substring(1);
		}

		public void overightChannel(Farmer who, string answer)
		{
			string str = answer.Split(' ')[0];
			if (str == "News")
			{
				TVScreen.SetValue(Target, new TemporaryAnimatedSprite(this.newsScreen, new Rectangle(0, 0, 42, 28), 150f, 2, 999999, Target.getScreenPosition(), false, false, (float)((double)(Target.boundingBox.Bottom - 1) / 10000.0 + 9.99999974737875E-06), 0.0f, Color.White, Target.getScreenSizeModifier(), 0.0f, 0.0f, 0.0f, false));
				Game1.drawObjectDialogue(Game1.parseText(this.config.newsItems[this.dailyNews]));
				Game1.afterDialogues = new Game1.afterFadeFunction(this.NextScene);

			}
			else
				this.old_afterQuestion(who, answer);
		}

		public void TryHookTelevision()
		{
			if (Game1.currentLocation != null && Game1.currentLocation is DecoratableLocation && Game1.activeClickableMenu != null && Game1.activeClickableMenu is DialogueBox)
			{
				Callback = (GameLocation.afterQuestionBehavior)Field.GetValue(Game1.currentLocation);
				if (Callback != null && Callback.Target.GetType() == typeof(TV))
				{
					Target = (TV)Callback.Target;
				}
			}
		}

		public void NextScene()
		{
			Target.turnOffTV();
		}

	}
}
