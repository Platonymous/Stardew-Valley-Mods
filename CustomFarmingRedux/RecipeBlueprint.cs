using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using PyTK.Extensions;
using SObject = StardewValley.Object;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using PyTK;

namespace CustomFarmingRedux
{
    public class RecipeBlueprint
    {
        internal IModHelper Helper = CustomFarmingReduxMod._helper;
        internal IMonitor Monitor = CustomFarmingReduxMod._monitor;
        internal Random rnd = new Random();
        public string _name = "";
        public string _description = "";
        public string _category = "";
        public int _tileindex = -1;
        public int _index = -1;
        public int[] exclude;
        public int[] include;
        public string id => name + "." + index + "." + quality + "." + stack;
        public string price = "original";
        private int dropInQuality = -1;
        public string name
        {
            get
            {
                if (_name == "" && index > 0)
                    _name = item.StartsWith("random:") ? " " : Game1.objectInformation[index].Split('/')[4];

                return _name;
            }
            set
            {
                _name = value;
            }
        }
        public string description
        {
            get
            {
                if (_description == "" || _description == null)
                    return Game1.objectInformation[index].Split('/')[5];
                else
                    return _description;
            }
            set
            {
                _description = value;
            }
        }
        public string category
        {
            get
            {
                if (_category == "")
                    _category = Game1.objectInformation[index].Split('/')[3].Split(' ')[0];

                return _category;
            }
            set
            {
                _category = value;
            }
        }
        public int index
        {
            get
            {
                if (_index == -999)
                    return _index;

                if (bigcraftable)
                {
                    if (_index <= 0 && item != "")
                    {
                        if (item.StartsWith("random:"))
                        {
                            string[] items = item.Split(':')[1].Split(',');
                            return Game1.bigCraftablesInformation.getIndexByName(items[rnd.Next(0, items.Length)]);
                        }
                        else
                        _index = Game1.bigCraftablesInformation.getIndexByName(item);
                    }
                    else if (_index <= 0 && name != "")
                        _index = Game1.bigCraftablesInformation.getIndexByName(name);
                }
                else
                {
                    if (_index <= 0 && item != "")
                    {
                        if (item.StartsWith("random:"))
                        {
                            string[] items = item.Split(':')[1].Split(',');
                            string name = items[rnd.Next(0, items.Length)];
                            if (name.ToLower() == "stone")
                                return 390;
                            return Game1.objectInformation.getIndexByName(name);
                        }
                        else
                            _index = Game1.objectInformation.getIndexByName(item);
                    }
                    else if (_index <= 0 && name != "")
                        _index = Game1.objectInformation.getIndexByName(name);
                }
                return _index;
            }
            set
            {
                _index = value;
            }
        }

        public bool bigcraftable { get; set; } = false;

        public string item { get; set; } = "";

        public List<IngredientBlueprint> materials { get; set; }

        public string conditions { get; set; } = "";
        public string texture { get; set; }
        public int tileindex
        {
            get
            {
                if (_tileindex == -1)
                    return _index;
                else
                    return _tileindex;
            }
            set
            {
                _tileindex = value;
            }
        }
        public bool prefix { get; set; } = false;
        public bool suffix { get; set; } = false;
        public bool insert { get; set; } = false;
        public int insertpos { get; set; } = 0;
        public bool colored { get; set; } = false;
        public int[] color { get; set; } = null;
        public int time { get; set; }
        public int stack { get; set; } = 1;
        public int quality { get; set; } = 0;
        public bool custom { get => (texture != null && texture != "") || (_description != null && _description != ""); }
        public CustomMachineBlueprint mBlueprint;
        public Texture2D texture2d;

        public void consumeIngredients(List<IList<Item>> items, SObject dropin = null)
        {
            if (materials == null)
                return;

            if (dropin != null && quality == -1)
                dropInQuality = dropin.Quality;

            List<IngredientBlueprint> ingredients = materials.toList(p => p.clone());
        
            foreach (IngredientBlueprint i in materials)
            {
                for (int list = 0; list < items.Count; list++)
                    if (ingredients.Exists(e => e.index == i.index))
                        if (items[list].ToList().Find(p => fitsIngredient(p, i) && (dropin == null || p.ParentSheetIndex == dropin.ParentSheetIndex)) is Item j)
                        {
                            j.Stack -= i.stack;
                            int ii = ingredients.FindIndex(p => p.index == i.index);

                            ingredients[ii].stack = (j.Stack > 0) ? 0 : Math.Abs(j.Stack);
                            if (ingredients[ii].stack == 0)
                                ingredients.Remove(ingredients[ii]);
                            if (j.Stack <= 0)
                                items[list][items[list].IndexOf(j)] = (Item)null;
                        }
            }

            if (ingredients.Count > 0)
                foreach (IngredientBlueprint i in materials)
                    for (int list = 0; list < items.Count; list++)
                        if (ingredients.Exists(e => e.index == i.index))
                            if (items[list].ToList().Find(p => fitsIngredient(p, i)) is Item j)
                            {
                                j.Stack -= i.stack;
                                int ii = ingredients.FindIndex(p => p.index == i.index);

                                ingredients[ii].stack = (j.Stack > 0) ? 0 : Math.Abs(j.Stack);
                                if (ingredients[ii].stack == 0)
                                    ingredients.Remove(ingredients[ii]);
                                if (j.Stack <= 0)
                                    items[list].Remove(j);
                            }

            dropInQuality = -1;
        }

        public Texture2D getTexture(IModHelper helper = null)
        {
            if (texture2d != null)
                return texture2d;

            if (helper == null)
                helper = Helper;

            if (texture2d == null)
                if (texture == null || texture == "")
                    texture2d = Game1.objectSpriteSheet;
                else
                {
                    if (mBlueprint.pack.baseFolder != "ContentPack")
                        texture2d = Helper.Content.Load<Texture2D>($"{mBlueprint.pack.baseFolder}/{mBlueprint.folder}/{texture}");
                    else
                        texture2d = mBlueprint.pack.contentPack.LoadAsset<Texture2D>(texture);
                }
            return texture2d;
        }

        internal bool fitsIngredient(Item p, IngredientBlueprint i)
        {
            if (quality == -1 && dropInQuality > 0 && p is SObject sbj && sbj.Quality != dropInQuality)
                return false;

            if (p is SObject obj && i.index == -999 && (i.exactquality == -1 || obj.Quality == i.exactquality) && obj.Quality >= i.quality && (i.quality >= 0 || obj.Quality < (i.quality * -1)))
                return true;

            return p is SObject o && (exclude == null || !exclude.Contains(o.ParentSheetIndex)) && (o.ParentSheetIndex == i.index || o.Category == i.index || (i.context.Count > 0 && i.context.Exists(ic => o.HasContextTag(ic))) || (include != null && (include.Contains(o.ParentSheetIndex) || include.Contains(o.Category)))) && (i.exactquality == -1 || o.Quality == i.exactquality) && o.Quality >= i.quality && (i.quality >= 0 || o.Quality < (i.quality * -1));
        }

        public bool fitsIngredient(Item p, List<IngredientBlueprint> l)
        {
            foreach (IngredientBlueprint i in l)
            {
                if (p is SObject obj && i.index == -999)
                    return true;

                if (p is SObject o && (exclude == null || !exclude.Contains(o.ParentSheetIndex)) && (o.ParentSheetIndex == i.index || o.Category == i.index || (i.context.Count > 0 && i.context.Exists(ic => o.HasContextTag(ic)))|| (include != null && (include.Contains(o.ParentSheetIndex) || include.Contains(o.Category)))) && (i.exactquality == -1 || o.Quality == i.exactquality) && o.Quality >= i.quality && (i.quality >= 0 || o.Quality < (i.quality * -1)))
                    return true;
            }

            return false;
        }

        public bool hasIngredients(List<IList<Item>> items, Item dropInItem = null)
        {
            if (materials == null)
                return true;

            if (dropInItem is SObject dropin && quality == -1)
                dropInQuality = dropin.Quality;

            List<IngredientBlueprint> ingredients = materials.toList(p => p.clone());

            foreach (IngredientBlueprint i in materials)
                for (int list = 0; list < items.Count; list++)
                    if (ingredients.Exists(e => e.index == i.index))
                    {
                        if (items[list].ToList().Find(p => fitsIngredient(p,i)) is Item j)
                        {
                            int ii = ingredients.FindIndex(p => p.index == i.index);
                            ingredients[ii].stack = (j.Stack - ingredients[ii].stack > 0) ? 0 : Math.Abs(j.Stack - ingredients[ii].stack);
                            if (ingredients[ii].stack == 0)
                                ingredients.Remove(ingredients[ii]);
                        }
                    }

            dropInQuality = -1;

            if (ingredients.Count <= 0)
                return true;
            else
                return false;
        }

        public void consumeIngredients(List<Item> items,  SObject dropin)
        {
            consumeIngredients(new List<IList<Item>>() { items }, dropin);
        }

        public bool hasIngredients(List<Item> items)
        {
            return hasIngredients(new List<IList<Item>>() { items });
        }

        private Color getColor(SObject input)
        {
            if (color != null)
                return new Color(color.toVector<Vector4>());
            else
                return CustomObject.getColor(input);
        }

        public SObject createObject(SObject input)
        {
            if (bigcraftable)
                return new SObject(Vector2.Zero, index == -999 ? input.ParentSheetIndex : index);
            else
            {
                if (!custom && colored)
                    return setNameAndQuality(new ColoredObject(index == -999 ? input.ParentSheetIndex : index, stack, getColor(input)), input);
                else if (!custom)
                    return setNameAndQuality(new SObject(Vector2.Zero, index == -999 ? input.ParentSheetIndex : index, stack), input);
                else
                    return new CustomObject(index == -999 ? input.ParentSheetIndex : index, stack, name, input, this);
            }
        }

        private SObject setNameAndQuality(SObject s, SObject input)
        {
            string oName = s.name;
            s.Quality = quality == -1 ? input.Quality : quality;
            s.name = (prefix) && name != input.name + " " + name ? input.name + " " + name : name;
            s.name = (suffix) && name != s.name + " " + input.name ? s.name + " " + input.name : s.name;
            if (insert)
            {
                string[] namesplit = s.name.Split(' ');
                namesplit[insertpos] += " " + input.name;
                s.name = String.Join(" ", namesplit);
            }

            if (s.name == null || s.name == "" || s.name == " ")
                s.name = oName;

            int compPrice = (int)(PyUtils.calc(price, new KeyValuePair<string, object>("input", input == null ? 0 : input.Price), new KeyValuePair<string, object>("original", s.Price)));
            s.Price = compPrice;

            if(prefix || suffix || insert)
                s.preservedParentSheetIndex.Value = -1 * input.ParentSheetIndex;
            return s;
        }

    }
}
