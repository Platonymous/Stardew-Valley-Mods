using HarmonyLib;
using Microsoft.Xna.Framework;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Movies;
using StardewValley.Locations;
using StardewValley.Minigames;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CustomMovies
{
    public class CustomMoviesMod : Mod
    {
        public static Dictionary<string, CustomMovieData> allTheMovies = new Dictionary<string, CustomMovieData>();
        public static List<TranslatableMovieReactions> allTheReactions = new List<TranslatableMovieReactions>();

        public static CustomMovieData lastMovie = null;
        private static IModHelper cmHelper = null;
        public override void Entry(IModHelper helper)
        {
            cmHelper = helper;
            helper.Events.GameLoop.GameLaunched += GameLoop_GameLaunched;
            helper.Events.Content.AssetRequested += CMVAssetEditor.OnAssetRequested;

            helper.Events.GameLoop.UpdateTicked += (s, e) => AnimatedTexture2D.ticked = e.Ticks;
            OvSpritebatchNew.initializePatch(new Harmony("Platonymous.CustomMovies"));
        }

        public static CraneGame.Prize getCustomMoviePrize(CustomMovieData movieData, CraneGame game, Vector2 pos, float z)
        {
            if (getItem(movieData.CranePrizeType, name: movieData.CranePrizeName) is Item prize)
                return new CraneGame.Prize(game, prize)
                {
                    position = pos,
                    zPosition = z,
                    isLargeItem = (prize is StardewValley.Object o && o.bigCraftable.Value) || !(prize is StardewValley.Object)
                };

            return null;
        }

        private void GameLoop_GameLaunched(object sender, GameLaunchedEventArgs e)
        {
            loadContentPacks();
            harmonyFix();
        }

        public static Type getTypeSDV(string type)
        {
            string prefix = "StardewValley.";
            Type defaulSDV = Type.GetType(prefix + type + ", Stardew Valley");

            if (defaulSDV != null)
                return defaulSDV;
            else
                return Type.GetType(prefix + type + ", StardewValley");
        }

        public static Item getItem(string type, int index = -1, string name = "none")
        {
            Item item = null;

            if (type == "Object")
            {
                if (index != -1)
                    item = new StardewValley.Object(index, 1);
                else if (name != "none")
                    item = new StardewValley.Object(Game1.objectInformation.getIndexByName(name), 1);
            }
            else if (type == "BigObject")
            {
                if (index != -1)
                    item = new StardewValley.Object(Vector2.Zero, index);
                else if (name != "none")
                    item = new StardewValley.Object(Vector2.Zero, Game1.bigCraftablesInformation.getIndexByName(name));
            }
            else if (type == "Ring")
            {
                if (index != -1)
                    item = new Ring(index);
                else if (name != "none")
                    item = new Ring(Game1.objectInformation.getIndexByName(name));
            }
            else if (type == "Hat")
            {
                if (index != -1)
                    item = new Hat(index);
                else if (name != "none")
                    item = new Hat(cmHelper.GameContent.Load<Dictionary<int, string>>("Data/hats").getIndexByName(name));
            }
            else if (type == "Boots")
            {
                if (index != -1)
                    item = new Boots(index);
                else if (name != "none")
                    item = new Boots(cmHelper.GameContent.Load<Dictionary<int, string>>("Data/Boots").getIndexByName(name));
            }
            else if (type == "TV")
            {
                if (index != -1)
                    item = new StardewValley.Objects.TV(index, Vector2.Zero);
                else if (name != "none")
                    item = new TV(cmHelper.GameContent.Load<Dictionary<int, string>>("Data/Furniture").getIndexByName(name), Vector2.Zero);
            }
            else if (type == "IndoorPot")
                item = new StardewValley.Objects.IndoorPot(Vector2.Zero);
            else if (type == "CrabPot")
                item = new StardewValley.Objects.CrabPot(Vector2.Zero);
            else if (type == "Chest")
                item = new StardewValley.Objects.Chest(true);
            else if (type == "Cask")
                item = new StardewValley.Objects.Cask(Vector2.Zero);
            else if (type == "Cask")
                item = new StardewValley.Objects.Cask(Vector2.Zero);
            else if (type == "Furniture")
            {
                if (index != -1)
                    item = new StardewValley.Objects.Furniture(index, Vector2.Zero);
                else if (name != "none")
                    item = new Furniture(cmHelper.GameContent.Load<Dictionary<int, string>>("Data/Furniture").getIndexByName(name), Vector2.Zero);
            }
            else if (type == "Sign")
                item = new StardewValley.Objects.Sign(Vector2.Zero, index);
            else if (type == "Wallpaper")
                item = new StardewValley.Objects.Wallpaper(Math.Abs(index), false);
            else if (type == "Floors")
                item = new StardewValley.Objects.Wallpaper(Math.Abs(index), true);
            else if (type == "MeleeWeapon")
            {
                if (index != -1)
                    item = new MeleeWeapon(index);
                else if (name != "none")
                    item = new MeleeWeapon(cmHelper.GameContent.Load<Dictionary<int, string>>("Data/weapons").getIndexByName(name));

            }
            else if (type == "SDVType")
            {
                try
                {
                    if (index == -1)
                        item = Activator.CreateInstance(getTypeSDV(name)) is Item i ? i : null;
                    else
                        item = Activator.CreateInstance(getTypeSDV(name), index) is Item i ? i : null;
                }
                catch
                {
                }
            }
            else if (type == "ByType")
            {
                try
                {
                    if (index == -1)
                        item = Activator.CreateInstance(Type.GetType(name)) is Item i ? i : null;
                    else
                        item = Activator.CreateInstance(Type.GetType(name), index) is Item i ? i : null;
                }
                catch
                {
                }
            }

            return item;
        }
        public void loadContentPacks()
        {
            Dictionary<string, MovieData> movieData = MovieTheater.GetMovieData();
            List<MovieCharacterReaction> genericReactions = MovieTheater.GetMovieReactions();
            foreach (var pack in Helper.ContentPacks.GetOwned())
            {
                CustomMoviePack cPack = pack.ReadJsonFile<CustomMoviePack>("content.json");
                foreach (var movie in cPack.Movies)
                {
                    movie.LoadTexture(pack);
                    movie._pack = pack;

                    int y = 0;

                    while (movieData.ContainsKey(movie.Season + "_movie_" + y))
                        y++;

                    movie.FixedMovieID = movie.FixedMovieID == null ? movie.Season + "_movie_" + y : movie.FixedMovieID;
                    Monitor.Log("Added " + movie.Title + " as " + movie.FixedMovieID);
                    MovieData fixedData = movie.GetData(y);
                    fixedData.ID = movie.FixedMovieID;
                    movieData.Remove(fixedData.ID);
                    movieData.Add(fixedData.ID, fixedData);
                    allTheMovies.Remove(movie.Id);
                    allTheMovies.Add(movie.Id, movie);
                }

                typeof(MovieTheater).GetField("_movieData", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, movieData);

                foreach (var reaction in cPack.Reactions)
                {
                    foreach (var r in reaction.Reactions)
                        if (r.Tag != null && allTheMovies.ContainsKey(r.Tag))
                            r.Tag = "CMovieID:" + r.Tag;

                    allTheReactions.Remove(new TranslatableMovieReactions(reaction, pack));
                    allTheReactions.Add(new TranslatableMovieReactions(reaction, pack));
                }

            }
        }

        private void harmonyFix()
        {
            Harmony instance = new Harmony("Platonymous.CustomMovies");
            instance.Patch(typeof(MovieTheater).GetMethod("GetMovieData", BindingFlags.Public | BindingFlags.Static), null, new HarmonyMethod(typeof(CustomMoviesMod).GetMethod("GetMovieData", BindingFlags.Public | BindingFlags.Static)), null);
            instance.Patch(typeof(MovieTheater).GetMethod("GetMovieForDate", BindingFlags.Public | BindingFlags.Static), null, new HarmonyMethod(typeof(CustomMoviesMod).GetMethod("GetMovieForDate", BindingFlags.Public | BindingFlags.Static)), null);
            instance.Patch(typeof(MovieTheater).GetMethod("GetMovieReactions", BindingFlags.Public | BindingFlags.Static), null, new HarmonyMethod(typeof(CustomMoviesMod).GetMethod("GetMovieReactions", BindingFlags.Public | BindingFlags.Static)), null);
            instance.Patch(AccessTools.Constructor(typeof(CraneGame)), postfix: new HarmonyMethod(this.GetType().GetMethod("craneGameInstantiated", BindingFlags.Public | BindingFlags.Static)));
        }

        public static void craneGameInstantiated(ref CraneGame __instance)
        {
            if (CMVAssetEditor.CurrentMovie == null || CMVAssetEditor.CurrentMovie.CranePrizeType == "None")
                return;

            var game = __instance;
            List<CraneGame.CraneGameObject> gameObjects = new List<CraneGame.CraneGameObject>((List<CraneGame.CraneGameObject>)game.GetType().GetField("_gameObjects", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(game));
            Random rnd = new Random();

            foreach (CraneGame.Prize prize in gameObjects.Where(go => go is CraneGame.Prize p && !p.isLargeItem))
            {
                if (rnd.NextDouble() < 0.2)
                {
                    game.UnregisterGameObject(prize);
                    if (getCustomMoviePrize(CMVAssetEditor.CurrentMovie, game, prize.position, prize.zPosition) == null)
                        game.RegisterGameObject(prize);
                    break;
                }
            }
        }

        public static void GetMovieReactions(ref List<MovieCharacterReaction> __result)
        {
            foreach (var reaction in allTheReactions)
            {
                __result.Remove(translateReactions(reaction).Reaction);
                __result.Add(translateReactions(reaction).Reaction);
            }
        }

        public static CustomMovieData translateMovie(CustomMovieData movie)
        {
            if (movie._pack.Translation == null)
                return movie;

            if (movie._pack.Translation.Get(movie.Id + "_title") is Translation t && t.HasValue() && t.ToString() is string title && title != "")
                movie.Title = title;

            if (movie._pack.Translation.Get(movie.Id + "_title") is Translation td && td.HasValue() && td.ToString() is string description && description != "")
                movie.Description = description;

            foreach (var scene in movie.Scenes)
            {
                if (movie._pack.Translation.Get(movie.Id + "_" + scene.ID + "_text") is Translation translation && translation.HasValue() && translation.ToString() is string text && text != "")
                    scene.Text = text;

                if (movie._pack.Translation.Get(movie.Id + "_" + scene.ID + "_script") is Translation translationScript && translationScript.HasValue() && translationScript.ToString() is string script && script != "")
                    scene.Script = script;
            }

            return movie;
        }

        public static TranslatableMovieReactions translateReactions(TranslatableMovieReactions reactions)
        {
            if (reactions._pack.Translation == null)
                return reactions;

            foreach (MovieReaction reaction in reactions.Reaction.Reactions)
            {
                if (reactions._pack.Translation.Get("reaction_" + reaction.ID + "_beforeMovie_text") is Translation translation && translation.HasValue() && translation.ToString() is string text && text != "")
                    reaction.SpecialResponses.BeforeMovie.Text = text;

                if (reactions._pack.Translation.Get("reaction_" + reaction.ID + "_beforeMovie_script") is Translation translationScript && translationScript.HasValue() && translationScript.ToString() is string script && script != "")
                    reaction.SpecialResponses.BeforeMovie.Script = script;

                if (reactions._pack.Translation.Get("reaction_" + reaction.ID + "_afterMovie_text") is Translation translationAM && translationAM.HasValue() && translationAM.ToString() is string textaf && textaf != "")
                    reaction.SpecialResponses.AfterMovie.Text = textaf;

                if (reactions._pack.Translation.Get("reaction_" + reaction.ID + "_afterMovie_script") is Translation translationScriptAM && translationScriptAM.HasValue() && translationScriptAM.ToString() is string scriptaf && scriptaf != "")
                    reaction.SpecialResponses.AfterMovie.Script = scriptaf;

                if (reactions._pack.Translation.Get("reaction_" + reaction.ID + "_duringMovie_text") is Translation translationDM && translationDM.HasValue() && translationDM.ToString() is string textdm && textdm != "")
                    reaction.SpecialResponses.DuringMovie.Text = textdm;

                if (reactions._pack.Translation.Get("reaction_" + reaction.ID + "_duringMovie_script") is Translation translationScriptDM && translationScriptDM.HasValue() && translationScriptDM.ToString() is string scriptdm && scriptdm != "")
                    reaction.SpecialResponses.DuringMovie.Script = scriptdm;
            }

            return reactions;
        }

        public static void GetMovieForDate(ref MovieData __result, WorldDate date)
        {
            bool next = false;
            if (date != new WorldDate(Game1.Date))
            {
                date.TotalDays -= 21;
                next = true;
            }

            int r = date.TotalDays / 7;

            var data = MovieTheater.GetMovieData();

            string season = Game1.currentSeason;

            if (next && Game1.dayOfMonth > 21)
                switch (season)
                {
                    case "spring": season = "summer"; break;
                    case "summer": season = "fall"; break;
                    case "fall": season = "winter"; break;
                    case "winter": season = "spring"; break;
                }

            List<MovieData> movies = data.Values.Where(m => m.ID.StartsWith(season)).ToList();

            __result = movies[r % movies.Count];

            if (__result.Tags.Contains("CustomMovie"))
                CMVAssetEditor.CurrentMovie = allTheMovies[__result.Tags.Find(t => t.StartsWith("CMovieID:")).Split(':')[1]];
            else
                CMVAssetEditor.CurrentMovie = null;

            if (lastMovie != CMVAssetEditor.CurrentMovie)
                cmHelper.GameContent.InvalidateCache("LooseSprites/Movies");

            lastMovie = CMVAssetEditor.CurrentMovie;



        }

        public static bool checkPlayerConditions(string conditions)
        {
            return cmHelper.Reflection.GetField<bool>(Game1.player, conditions).GetValue();
        }

        public static bool checkEventConditions(string conditions, object caller, GameLocation location)
        {
            bool result = false;
            bool comparer = true;

            if (conditions == null || conditions == "")
                return true;

            if (conditions.StartsWith("NOT "))
            {
                conditions = conditions.Replace("NOT ", "");
                comparer = false;
            }

            if (!Context.IsWorldReady)
            {
                if (conditions.StartsWith("r "))
                {
                    string[] cond = conditions.Split(' ');
                    return comparer == Game1.random.NextDouble() <= double.Parse(cond[1]);
                }

                if (conditions.StartsWith("LC ") || conditions.StartsWith("!LC "))
                {
                    try
                    {
                        result =false;
                    }
                    catch
                    {
                        result = false;
                    }
                    return result == comparer;
                }

                return result;
            }

            if (conditions.StartsWith("PC "))
                result = checkPlayerConditions(conditions.Replace("PC ", ""));
            else if (conditions.StartsWith("LC ") || conditions.StartsWith("!LC "))
                result = false;
            else
            {
                if (location == null)
                    location = Game1.currentLocation;

                if (!(location is GameLocation))
                    location = Game1.getFarm();

                if (location == null)
                {
                    if (conditions.StartsWith("r "))
                    {
                        string[] cond = conditions.Split(' ');
                        return comparer == Game1.random.NextDouble() <= double.Parse(cond[1]);
                    }

                    if (conditions.StartsWith("LC ") || conditions.StartsWith("!LC "))
                    {
                        result = false;
                        return result == comparer;
                    }

                    result = false;
                }
                else
                {
                    try
                    {
                        result = cmHelper.Reflection.GetMethod(location, "checkEventPrecondition").Invoke<int>("9999999/" + conditions) > 0;
                    }
                    catch
                    {
                        try
                        {
                            var m = typeof(GameLocation).GetMethod("checkEventPrecondition", BindingFlags.NonPublic | BindingFlags.Instance);
                            result = (int)m.Invoke(location, new string[] { ("9999999/" + conditions) }) > 0;
                        }
                        catch
                        {
                            result = false;
                        }
                    }
                }
            }

            return result == comparer;
        }

        public static void GetMovieData(ref Dictionary<string, MovieData> __result)
        {
            foreach (var movie in allTheMovies.Values)
            {
                if (__result.Any(d => d.Value.Tags.Contains("CMovieID:" + movie.Id)))
                {
                    if (__result.FirstOrDefault(d => d.Value.Tags.Contains("CMovieID:" + movie.Id)) is KeyValuePair<string, MovieData> md && !checkEventConditions(movie.Conditions, Game1.MasterPlayer, Game1.currentLocation))
                        __result.Remove(md.Key);

                    continue;
                }

                if (!checkEventConditions(movie.Conditions, Game1.MasterPlayer, Game1.currentLocation))
                    continue;

                if (movie.FixedMovieID != null)
                {
                    MovieData fixedData = movie.GetFixedData();
                    fixedData.ID = movie.FixedMovieID;
                    __result.Remove(fixedData.ID);
                    __result.Add(fixedData.ID, fixedData);
                    continue;
                }
            }
        }

    }

    public static class CustomMovieExtensions{
        public static int getIndexByName(this IDictionary<int, string> dictionary, string name)
        {
            int found = 0;

            if (name.StartsWith("startswith:"))
                found = (dictionary.Where(d => d.Value.Split('/')[0].StartsWith(name.Split(':')[1])).FirstOrDefault()).Key;
            else if (name.StartsWith("endswith:"))
                found = (dictionary.Where(d => d.Value.Split('/')[0].EndsWith(name.Split(':')[1])).FirstOrDefault()).Key;
            else if (name.StartsWith("contains:"))
                found = (dictionary.Where(d => d.Value.Split('/')[0].Contains(name.Split(':')[1])).FirstOrDefault()).Key;
            else
                found = (dictionary.Where(d => d.Value.Split('/')[0] == name).FirstOrDefault()).Key;

            return found;
        }

    }
}
