using System;
using System.IO;
using System.Collections.Generic;
using System.Drawing;

using Microsoft.Xna.Framework.Graphics;
using StardewValley;
using StardewValley.TerrainFeatures;
using Microsoft.Xna.Framework;
using StardewValley.Objects;
using StardewValley.Locations;
using System.Text.RegularExpressions;

namespace TheJunimoExpress
{
    class LoadData
    {
        public string name = "JunimoFarm";
        public string tmp;
        public static GameLocation previousLocation;
        public static List<string> recipes = new List<string>();
        public static List<int> craftables = new List<int>();
     
        public static List<StardewValley.Object> objectlist = new List<StardewValley.Object>();
        public static List<TerrainFeature> terrainFeatureList = new List<TerrainFeature>();


        public void LoadFromString(string savestring)
        {
            string[] savedata = Regex.Split(savestring, "--");
            List<Item> itemList = new List<Item>();
            SerializableDictionary<Vector2, StardewValley.Object> objects = new SerializableDictionary<Vector2, StardewValley.Object>();
            for (int i = 1; i < savedata.Length; i++)
            {

                string[] entry = Regex.Split(savedata[i], ";;;");
                int row = (int)Math.Floor(craftables[2] / 24.0);
                int col = craftables[2] % 24;
                if (entry[0] == "Tracks")
                {
                    if (Game1.getLocationFromName(entry[1]).terrainFeatures.ContainsKey(new Vector2(int.Parse(entry[4]), int.Parse(entry[5])))) { 
                    Game1.getLocationFromName(entry[1]).terrainFeatures.Remove(new Vector2(int.Parse(entry[4]), int.Parse(entry[5])));
                    }
                    Game1.getLocationFromName(entry[1]).terrainFeatures.Add(new Vector2(int.Parse(entry[4]), int.Parse(entry[5])),new RailroadTrack(row,col));
                    continue;
                }

                if (entry[3] != "-1")
                {
                    if (entry[1] == "Inventory")
                    {
                        itemList = Game1.player.items;
                    }
                    else if (entry[2] == "-1")
                    {
                        itemList = (Game1.getLocationFromName(entry[1]).objects[new Vector2(int.Parse(entry[4]), int.Parse(entry[4]))] as Chest).items;
                    }
                    else
                    {
                        itemList = ((Game1.getLocationFromName(entry[1]) as BuildableGameLocation).buildings[int.Parse(entry[2])].indoors.objects[new Vector2(int.Parse(entry[4]), int.Parse(entry[5]))] as Chest).items;
                    }

                    Item slot = itemList[int.Parse(entry[3])];

                    if (entry[0] == "CraftedTracks")
                    {
                        itemList[int.Parse(entry[3])] = new StardewValley.Object(craftables[0], slot.Stack);
                    }
                    else if(entry[0] == "CraftedJunimo")
                    {
                        itemList[int.Parse(entry[3])] = new StardewValley.Object(craftables[1], slot.Stack);
                    }
                    if (entry[0] == "LinkedChest")
                    {
                        
                        LinkedChest newLinkedChest =  new LinkedChest(Game1.getLocationFromName(entry[1]), true, true);
                        newLinkedChest.inList = (slot as Chest).items;
                        newLinkedChest.index = int.Parse(entry[3]);
                        newLinkedChest.location = Game1.getLocationFromName(entry[1]);
                        newLinkedChest.building = int.Parse(entry[2]);
                        newLinkedChest.tileLocation = new Vector2(int.Parse(entry[4]), int.Parse(entry[5]));
                        newLinkedChest.playerChoiceColor = (slot as Chest).playerChoiceColor;
                        Item[] items = new Item[(slot as Chest).items.Count];
                        (slot as Chest).items.CopyTo(items);
                        newLinkedChest.items = new List<Item>(items);
                        newLinkedChest.objectID = int.Parse(entry[6]);
                        if (int.Parse(entry[6]) > 0)
                        {
                            int findJunimo = objectlist.FindIndex(x => x is JunimoHelper && (x as JunimoHelper).objectID == int.Parse(entry[6]));
                            if (findJunimo >= 0)
                            {
                                newLinkedChest.linkedJunimo = (JunimoHelper)objectlist[findJunimo];
                                newLinkedChest.linkedJunimo.targetChest = newLinkedChest;
                            }
                        }
                        itemList[int.Parse(entry[3])] = newLinkedChest;
                        objectlist.Add(newLinkedChest);
                    }
                }
                else
                {
                    if(entry[2] == "-1")
                    {
                        objects = Game1.getLocationFromName(entry[1]).objects;

                    }
                    else
                    {
                        objects = (Game1.getLocationFromName(entry[1]) as BuildableGameLocation).buildings[int.Parse(entry[2])].indoors.objects;
                    }

                    Chest slot = (Chest) objects[new Vector2(int.Parse(entry[4]), int.Parse(entry[5]))];

                    if (entry[0] == "LinkedChest")
                    {
                        LinkedChest newLinkedChest = new LinkedChest(Game1.getLocationFromName(entry[1]), false, true);
                        newLinkedChest.inventory = false;
                        newLinkedChest.index = -1;
                        newLinkedChest.location = Game1.getLocationFromName(entry[1]);
                        newLinkedChest.building = int.Parse(entry[2]);
                        if (newLinkedChest.building > 0)
                        {
                            newLinkedChest.location = (Game1.getLocationFromName(entry[1]) as BuildableGameLocation).buildings[newLinkedChest.building].indoors;
                            newLinkedChest.bgl = (BuildableGameLocation) Game1.getLocationFromName(entry[1]);
                        }

                        newLinkedChest.tileLocation = new Vector2(int.Parse(entry[4]), int.Parse(entry[5]));
                        newLinkedChest.playerChoiceColor = (slot as Chest).playerChoiceColor;
                        Item[] items = new Item[(slot as Chest).items.Count];
                        (slot as Chest).items.CopyTo(items);
                        newLinkedChest.items = new List<Item>(items);
                        newLinkedChest.objectID = int.Parse(entry[6]);
                        if(int.Parse(entry[6]) > 0) { 
                        int findJunimo = objectlist.FindIndex(x => x is JunimoHelper && (x as JunimoHelper).objectID == int.Parse(entry[6]));
                        if (findJunimo >= 0)
                        {
                            newLinkedChest.linkedJunimo = (JunimoHelper)objectlist[findJunimo];
                            newLinkedChest.linkedJunimo.targetChest = newLinkedChest;
                        }
                        }
                        objects[new Vector2(int.Parse(entry[4]), int.Parse(entry[5]))] = newLinkedChest;
                        objectlist.Add(newLinkedChest);
                    } else if (entry[0] == "JunimoHelper")
                    {
                        JunimoHelper newJunimo = new JunimoHelper(Game1.getLocationFromName(entry[1]), new Vector2(int.Parse(entry[4]), int.Parse(entry[5])), craftables[3], false);
                        newJunimo.color = (slot as Chest).playerChoiceColor;
                        newJunimo.index = -1;
                        if(entry.Length > 7) { 
                        newJunimo.direction = int.Parse(entry[7]);
                        }
                        newJunimo.location = Game1.getLocationFromName(entry[1]);
                        newJunimo.building = int.Parse(entry[2]);
                        if(newJunimo.building > 0)
                        {
                            newJunimo.location = (Game1.getLocationFromName(entry[1]) as BuildableGameLocation).buildings[newJunimo.building].indoors;
                            newJunimo.bgl = (BuildableGameLocation)Game1.getLocationFromName(entry[1]);
                        }

                        newJunimo.tileLocation = new Vector2(int.Parse(entry[4]), int.Parse(entry[5]));
                        Item[] items = new Item[(slot as Chest).items.Count];
                        (slot as Chest).items.CopyTo(items);
                        newJunimo.itemChest.items = new List<Item>(items);
                        newJunimo.objectID = int.Parse(entry[6]);
                        
                        if (int.Parse(entry[6]) > 0)
                        {
                            int findChest = objectlist.FindIndex(x => x is LinkedChest && (x as LinkedChest).objectID == int.Parse(entry[6]));
                            if (findChest >= 0)
                            {
                                newJunimo.targetChest = (LinkedChest)objectlist[findChest];
                                newJunimo.targetChest.linkedJunimo = newJunimo;
                            }
                        }
                        objects[new Vector2(int.Parse(entry[4]), int.Parse(entry[5]))] = newJunimo;
                        objectlist.Add(newJunimo);
                        
                    }


                }



            }

        }


        public string SaveAndRemove(ulong GID, string PN)
        {
            string save = name;
            List<StardewValley.Object> craftedObjects = new List<StardewValley.Object>();
            List<GameLocation> checkLocations = new List<GameLocation>();
            checkLocations.Add(Game1.getFarm());
            checkLocations.Add(Game1.getLocationFromName("FarmHouse"));

            GameLocation gl = Game1.getFarm();

            for (int index = 0; index < Game1.player.items.Count; ++index)
            {
                if(Game1.player.items[index] == null)
                {
                    continue;
                }
                if (Game1.player.items[index].parentSheetIndex == craftables[0])
                {
                    save += "--" + "CraftedTracks" + ";;;" + "Inventory" + ";;;" + "-1" + ";;;" + index.ToString() + ";;;" + "0" + ";;;" + "0" + ";;;" + "-1";
                    Game1.player.items[index] = new StardewValley.Object(Vector2.Zero, 24, Game1.player.items[index].Stack);

                }
                else if (Game1.player.items[index].parentSheetIndex == craftables[1])
                {
                    save += "--" + "CraftedJunimo" + ";;;" + "Inventory" + ";;;" + "-1" + ";;;" + index.ToString() + ";;;" + "0" + ";;;" + "0" + ";;;" + "-1";
                    Game1.player.items[index] = new StardewValley.Object(Vector2.Zero, 24, Game1.player.items[index].Stack);
                }
            }


            for (int g = 0; g < checkLocations.Count; g++)
            {
                gl = checkLocations[g];
                foreach (Vector2 keyV in gl.objects.Keys)
                {
                    if (gl.objects[keyV] is Chest)
                    {
                        for (int i = 0; i < (gl.objects[keyV] as Chest).items.Count; i++)
                        {
                            if ((gl.objects[keyV] as Chest).items[i] == null)
                            {
                                continue;
                            }

                            if ((gl.objects[keyV] as Chest).items[i].parentSheetIndex == craftables[0])
                            {
                                save += "--" + "CraftedTracks" + ";;;" + gl.name + "/"+ "-1" + ";;;" + i.ToString() + ";;;" + keyV.X.ToString() + ";;;" + keyV.Y.ToString() + ";;;" + "-1";
                                (gl.objects[keyV] as Chest).items[i] = new StardewValley.Object(Vector2.Zero, 24, (gl.objects[keyV] as Chest).items[i].Stack);
                            }

                            if ((gl.objects[keyV] as Chest).items[i].parentSheetIndex == craftables[1])
                            {
                                save += "--" + "CraftedJunimo" + ";;;" + gl.name + ";;;" + "-1" + ";;;" + i.ToString() + ";;;" + keyV.X.ToString() + ";;;" + keyV.Y.ToString() + ";;;" + "-1";
                                (gl.objects[keyV] as Chest).items[i] = new StardewValley.Object(Vector2.Zero, 24, (gl.objects[keyV] as Chest).items[i].Stack);

                            }
                        }
                    }
                    
                }
                GameLocation bgl = new GameLocation();
                if (gl is BuildableGameLocation)
                {
                    for(int b = 0; b < (gl as BuildableGameLocation).buildings.Count; b++)
                    {
                        if ((gl as BuildableGameLocation).buildings[b] == null || (gl as BuildableGameLocation).buildings[b] == null || (gl as BuildableGameLocation).buildings[b].indoors == null)
                        {
                            continue;
                        }

                        bgl = (gl as BuildableGameLocation).buildings[b].indoors;
                        foreach (Vector2 keyV in bgl.objects.Keys)
                        {
                            if (bgl.objects[keyV] is Chest)
                            {
                                for (int j = 0; j < (bgl.objects[keyV] as Chest).items.Count; j++)
                                {
                                    if ((bgl.objects[keyV] as Chest).items[j] == null)
                                    {
                                        continue;
                                    }

                                    if ((bgl.objects[keyV] as Chest).items[j].parentSheetIndex == craftables[0])
                                    {
                                        save += "--" + "CraftedTracks" + ";;;" + gl.name + ";;;" + b.ToString() + ";;;" + j.ToString() + ";;;" + keyV.X.ToString() + ";;;" + keyV.Y.ToString() + ";;;" + "-1";
                                        (bgl.objects[keyV] as Chest).items[j] = new StardewValley.Object(Vector2.Zero, 24, (bgl.objects[keyV] as Chest).items[j].Stack);
                                    }

                                    if ((bgl.objects[keyV] as Chest).items[j].parentSheetIndex == craftables[1])
                                    {
                                        save += "--" + "CraftedJunimo" + ";;;" + gl.name + ";;;" + b.ToString() + ";;;" + j.ToString() + ";;;" + keyV.X.ToString() + ";;;" + keyV.Y.ToString() + ";;;" + "-1";
                                        (bgl.objects[keyV] as Chest).items[j] = new StardewValley.Object(Vector2.Zero, 24, (bgl.objects[keyV] as Chest).items[j].Stack);

                                    }
                                }
                            }

                        }
                    }

                }

            }

            


            for (int i = 0; i < terrainFeatureList.Count; i++)
            {
                Vector2 v = (terrainFeatureList[i] as RailroadTrack).tileLocation;
                GameLocation l = (terrainFeatureList[i] as RailroadTrack).location;
                save += "--" + "Tracks;;;"+ l.name + ";;;-1;;;-1;;;" + v.X.ToString() + ";;;" + v.Y.ToString() + ";;;-1";
                l.terrainFeatures.Remove(v);
            }

                for (int i = 0; i < objectlist.Count; i++)
            {
                if(objectlist[i] is LinkedChest)
                {
                    save += "--" + (objectlist[i] as LinkedChest).saveObject();
                    (objectlist[i] as LinkedChest).removeObject();
                }
                if (objectlist[i] is JunimoHelper)
                {
                    save += "--" + (objectlist[i] as JunimoHelper).saveObject();
                    (objectlist[i] as JunimoHelper).removeObject();
                }
            }
            objectlist = new List<StardewValley.Object>();

            for (int i = 0; i < recipes.Count; i++)
            {
                CraftingRecipe.craftingRecipes.Remove(recipes[i]);
                Game1.player.craftingRecipes.Remove(recipes[i]);
            }

           
            for (int i = 0; i < craftables.Count-2; i++)
            {
                Game1.objectInformation.Remove(craftables[i]);
            }

            Game1.bigCraftableSpriteSheet = Game1.bigCraftableSpriteSheet = Game1.content.Load<Texture2D>("TileSheets\\Craftables");
            Game1.objectSpriteSheet = Game1.content.Load<Texture2D>("Maps\\springobjects");
            Game1.bigCraftablesInformation = Game1.content.Load<Dictionary<int, string>>("Data\\BigCraftablesInformation");
            Game1.objectInformation = Game1.content.Load<Dictionary<int, string>>("Data\\ObjectInformation");
            CraftingRecipe.craftingRecipes = Game1.content.Load<Dictionary<string, string>>("Data//CraftingRecipes");
            craftables = new List<int>();
            recipes = new List<string>();
            return saveSavStringToFile(save,GID,PN);
        }


        public bool doesSavFileExist(ulong GID, string PN)
        {
            this.tmp = this.name + "_" + PN + "_" + GID + ".sav";
            string str = PN;
            foreach (char c in str)
            {
                if (!char.IsLetterOrDigit(c))
                    str = str.Replace(c.ToString() ?? "", "");
            }
            string path2 = Path.Combine(str + "_" + (object)GID, this.tmp);
            FileInfo fileInfo1 = new FileInfo(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"), "Saves"), path2));
            if (!fileInfo1.Exists)
            {
                return false;
            }
            else
            {
                return true;
            }



        }

        public string loadSavStringFromFile(ulong GID, string PN)
        {
            if (!(this.doesSavFileExist(Game1.uniqueIDForThisGame, Game1.player.name)))
            {
                return "";
            }
                this.tmp = this.name + "_" + PN + "_" + GID + ".sav";
            FileInfo fi = ensureFolderStructureExists(PN, GID, this.tmp);

            using (StreamReader sr = fi.OpenText())
            {
                return sr.ReadToEnd();

            }


        }

        public string saveSavStringToFile(string savstring, ulong GID, string PN)
        {
            this.tmp = this.name + "_" + PN + "_" + GID + ".sav";
            FileInfo fi = ensureFolderStructureExists(PN, GID, this.tmp);


            using (StreamWriter sw = fi.CreateText())
            {
                sw.WriteLine(savstring);
            }

            return savstring;

        }


        public static FileInfo ensureFolderStructureExists(string PN, ulong GID, string tmpString)
        {
            string str = PN;
            foreach (char c in str)
            {
                if (!char.IsLetterOrDigit(c))
                    str = str.Replace(c.ToString() ?? "", "");
            }
            string path2 = Path.Combine(str + "_" + (object)GID, tmpString);
            FileInfo fileInfo1 = new FileInfo(Path.Combine(Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "StardewValley"), "Saves"), path2));
            if (!fileInfo1.Directory.Exists)
                fileInfo1.Directory.Create();

            return fileInfo1;
        }

        public int setTracksTextures()
        {

            String src = "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";
            Texture2D objectSprite = Game1.objectSpriteSheet;

            Bitmap orgSpriteFirstLoad = this.Texture2Image(objectSprite);
            Texture2D orgSpriteSecondLoad = this.Bitmap2Texture(orgSpriteFirstLoad);
            Image orgSprite = this.Texture2Image(orgSpriteSecondLoad);
            int osH = orgSprite.Height;
            int osW = orgSprite.Width;
            Image newSprite = this.LoadImage(src);
            int nsH = newSprite.Height;

            Bitmap combSprite = this.combineImages(orgSprite, newSprite, 0);
            Texture2D fullTex = this.Bitmap2Texture(combSprite);
            Bitmap fullSprite = this.Texture2Image(fullTex);
            Game1.objectSpriteSheet = fullTex;

            combSprite.Dispose();
            fullSprite.Dispose();

            return (osW / 16) * ((osH) /16);




        }

        public int setHelperTextures()
        {

            //String src = "iVBORw0KGgoAAAANSUhEUgAAAIAAAAIACAYAAABKEWQ6AAAR6UlEQVR42u2dTahkRxmGGxEli+jSgFkYXZiV4CYjSCYIkSwMSEBHV4EQXEVwdrNTVDIEIiOISEIgziKjkDgLEWYRAiJGV8mAxkBAXCiIILgJJAgirdVJNXXrVtWpqq/O+aq6ngeK2923v/r53rfqnP6p07sdAAAAAAAAAAAAAAAAAAAAAGzOhQuP7F+9fXuv1gFp49IBqA5eIX8mX6Gi5r6amFYDkA5c2zyl/TfPt5i+u/dz64rlPzsXEgHXHMAoBqrtv82VafPq1WePcea2eSwnh+L8SyroYgDKBmqZP7fk5E+cf0kF9nlLA7D1nqKBWuTPPi+EW29zA0kFlA5gdAO1zF+qLI1/Kf+LBpAIuNR5W07RQD3lr8pALTqgHa9pIEn/3bN0+/9QjHs7VIeaAfwB5MSH6hjdQDXt29u2/bdeuHFsyy0G8z+3ffev2/+UgdyV7dwJT6mAbkWlDg6dbLVqP1eAVvGS/peaKBQjNdCuVsDQDCrpQGzwpe1LViB/9uSUc7OoQf5ylvHU+UutgZoJKB2AtoHcfi/Fh/rQIn+pl6A5L0WlBpJXoDgAiQBuu0vxOSeS0vxJ3pSK9SGnbbGArQYgMVDtEig9iWzVf+k7kyL9lp6U83+pQVr8vzYJreJr/9/igyfN9gEAAAAAAAAAAAAAAAAAAAAAYAike+y140GA9Dtk2vFMvop46f567fhTWr02n0DS/fXa8aewgqlNIOn+eu34U1jB1CZQqwscaMWfwgqmOoFa7a/Xih99BVOfQCU7ZCVXqFg7ftQVTG0C+fvLJfvrNeJPZQVTnUCle+RDe+O14kc3cC8T6FhBTiUpB24d75onNwHu8Vk7vocJeE6Amj3umvG1AvQS38MEDIoQKjnXyts6Ptc87slZTECN+B4mYPINjK3319fE55inxERbx/cwAaM88MADe1P4aGxCjPCPP/74oZjbFz++OxQMNJaG1fl3DWDK5c99pMgAGEhXwBb5P2cCW5n5i4H6FlCa/wO2UVvJjRs3DiXHALMbqAcBJfk/im8NYIR/7bXXDiXXADMbSF1ASf5NkO2waVAi/qwGUhVQkn9XfHcZLEk8BlIUUJJ/E/DjL33onNPtAErFn9lAKgJK8m+CQuKXnjRiICUBJfk3Db7yzTsODdeKj4EUBZTk3zYqFR8DKQkoyb8/WEnSZzeQioCS/LcSHwMpCdgi/z2IP7qBtARskTsxsxtIW0BV8Wc3EAJiIASc2UAAAAAAAAAAAAAAAAAA4/DQgxf3tpCNCcV//pmnj8WaIPYXA53QBLbiuxXY+489eulQmf8XA/W1Aovyb0T9zcsvHsS2HbD3cwzQi4FmXYHF+XcNYE1QYoAeDNTDClLbvnT80vzv/AB731ZmH7N/ezNQD4cgSfvS8Uvzf+y8CbLFH0QqMdoG0j4EtZzBteOX5P+M+10Xuv9LBfdkII1DUIv2JeOX5n+3JLSbkKXlU8NA2oegViuYREBJ/heFzjGAtoG0D0HSJbiFgJL8nxlIqILQ470YSPsQ1GoJlgoozX8TA2gZSPMQ1HIGSyeQJP/JDmxlAGm8xiGo5QzWzN+qS1DvK0gPS/Ca4882QOwYt9UMGN1AmgKKTwJLjdGjgbQPQVqHsBb5X/098i0M1OsKkhu/hoDDf8JaYiDtQ9AaSzDfsRAmastDUJdLMMbY5hDEDJ7cQAAAAAAAAAAAAAAAPXPPPffs3XLp0qVDITOTiG/Evnbt2v7mzZv7N95441DMbfMYBjrxCWwCjNAffPHNc8WYYKkiDKQroDj/pgLz5A//c7+/9s57xdw2BjAV5RhgdgNpCijN/9EAVvw//Oe9v9YA5v8YqF8Bpfk/ZwBbbAdyDTCrgbQFlOb/OIBQB8zjpQaY0UCaAkrzf8CdAbaY++bxr95796FgoD4FlOY/ehw0xQj/3S/cvb/+tU8c/mKgPgWU5D+JCTTiv/KdOw8lZYKZDdSDgJL8Z5vA/C1x0wwG6lnAkvwvVmSCTSldTk7dQL0LKMn/uYpqg0/dQL0LKMl/M07dQL0LKDoZ7IERDYSAGAgBMRAAAAAAAAAAAAAAAAAAAAAAAMDm/GR3194UMjGp+H/Zff5Qak2AgQaewCZw//a/DqWmEgykK2CL/Is7MLuBNAWU5r+LBIxsoB4EHH4FHdlAJyPgXRe/sveL+zgGOuEZaAR+8Lmf77/xu7f2z+33h2Jum8fN7RwDzGqgnjSsyr8V3/w1ottihLemWBJwdgNpr8Ci/NvgUHHNgYH6FLBF/oOdzx0ABtIVUJr/1dw/i4F6EFCS/26Pf6MYCAEnNxACTm4gAAAAAAAAAAAAAAAAAIABuP/++/duISOTif/Ety7vr1+/fijm9r13fOBQMNCJT2Arvg20t7/3xV22ATCQ7gosyr/foGsC+xcD9Stgi/wfMQFuRaYzOQaY3UCaAkrzf0b8xz71sWOjJjDHALMbqBsBK/N/wAjvd7i0A7MaqAsBJflvIf7MBlIXUJJ/ExByeslxcHYDqQoozV3piRIG6kvAFvkXgYF0BZTmX53RDTS9gLMbCAEnNxAAAAAAAAAAAAAAAAAAAAAAAAAAADTisUcv7ZcK4z/hwf/+17/c79/5W7KcahJOYvy1DjaP2wFe3j18LKH7Jkm9JqHF+E3588t3ninu/7odf62D3biDyF//0X7/g6cOf4P3O50FLcZvxd//46Nnim+C1cavMYP92IPY75dfffb7Z+6vbQCNGezGhsQPmWCV8WvN4JQB/LKmAbRmsB2/G/fwZz4ZvG3riY5/xBl8Lt4axi8Zh4ARZ7BvAPPXN4D7v+j4R53BfrxvGtdMKQOMOoP9ePf5fh1RA4w8g/3ZF6rDFT8m4qgzOGYgtyQPAaPPYF88vx5f/FD8yDNYfBI4+gx+6MGLwaXamsAX3xQTcyoz2B9/joHd8Q8/g5dMsHQcH34GR0wQO4QtrwCDzeBUHaHy/DNPB4//w85g4fiHn8EWMzBTjy1+wkyJnf0PPYOF4x9+BsewdfptLRlguBksHP/wM3gpCTnv4A09g4XjH34GSxl+Bq+RhJFm8FomGGYGr2mAYRzM+OUM7WDGv96sGHoQjF+ehN3EzD5+AAAAAAAAAACAfnj3r09csaXlc2FgA1y48Mg+VF69fXuPAU7YAG//6dNXjNAWI7h739zGAANiZ++SAazY5rlXrz57nPnmtnnMNQFZ7VzwUCk1gFswgKKYJcv40hK+FGuFDmHrwwAdzWBXxNwlPBZrn5cqtl5UWlH8khmcMkBsCccAnYtfMoOtiP7zYku4K6D71423hwK32MfcfqFYY/FNcpdmcGgGSmawPabb+LdeuHHsg1sM5n+sACsboGQGh17q5ZTUYaA0Fhq/Vq8R0H2nbmkJt7fdOnLPA5j9HRrAPY7XLuH+S8GQCWwcLwFXfMlXYoDQLK5dwn0DpAqKNSZ0/M1ZwpdeCZQs4b745jOBf//928di7rv/R7WVDCA5C5fMYP8DoZgBWP5XNECLkzBbR2oGY4ANj+ux+zEB7Mextcdge5LmC2iKFW/pHAADCAV3P1AJvTxbOgyY4ic/9xjsihgzQE4sBqh4+eafzYdIiZc6AXNXhloRcw0QMhAGWBDfnrS5f/138NwPd2L1mGO8SbI/e40A7vv0sXcRUyKax1LnEDkGQPGEAXzB3ffzUyuB+2mffa/fN4BfRyzeNYEvoluHb4Inn3zmSq4BzHNR3jOBj/sBTsggKQP4JiiNtyZwRUwZ6L77vnzl/w9d+cWNp26ZEjOA+Z8R3zzXxKD87vwHODHB3cdTH/W6z3NXj9L40CeJoXgjpBH1t6+/fhDW/DX3X/rp5Vs3f/bDYzH333zzpVvuczCB9/59aKkPzWz/7VzNeCOiEdSKagS25b/v/vGWLfYxaxZTMEDAAEsrwJKAW8f7BrC3fQOEnoMBvENA7pcxYku4Rrx7CHDFjRX3uRgg8BIuVwD/JE4z3q4CJQZA/MAqkCtA6CtZmvF2Fcg1ALO/0gSh5PcSn2MCxM/8PKD2wxzteCNsTkFpAAAAAAAAAAAAAAAAAAAAAJiX2AWafVLf1Mmto2VsKq7kCiHTjD82gNj395bul16TR9J+qs5YP5buTzX+pQEsVRDb5+dvOU/FtGw/ts19q/ZHG//iAHITGLtMrF9faQJL2vcHHLtM7Vrtjzj+xQHkJDD3MrGpBEjaD8WnLlPbuv2Rx180gNiFllOXifV3DMeuP1Tbfig+1o/QvoLZx784gJbX+V3aGCJpP2dn0dIVSmYdv+g6v8SPH9+0AzVXCaV93fabDiB2ldCtEkj75e03GYC9TKwpoQs65B5DJe2HtpKbYvu1dvujjn9xAIsVLJyI2PilWGn7fnzuZWpnH3/WmWTOjy2470L5deXEtmg/FJN7mdqZx598GzI3gam3JWvev25RR6x/jH9hAKX/L0nOWu2v2cdTHz8AAAAAAAAAAAAAAAAAAAAArIxkW3nLOtpUMpEAoR/jLvmWjuTrYKG6RElo8fUibQOVti/9Pl7O7t614psYqCcHSs1T2r5EgJLdvWvENzFQFw4UGqi2fYkApbt7W8c3MVAXDlRaglsJ2GJ3ryS+2kBdOFBxCd5qe/iam0Ml/dd3YAdL8NoCuq8OcuL9x0rjN98ermmgLWawVED/OkGpeNvP2vhSA62SgB6v0KEpYEn7a8Sn+p+VwFIHbm2glu3XCJDa2Onu2F0rXtL/YAWtBYgZKPf6NqmLJLQwsESAnDpy9v7Vxov771fgV1bjwFwDLV3cIOciCVIDtxAw9RK25L0Iye5kUf/9CkInEbnJKzFQ6KfjU/E5Jio1cCsBQ3VJ3sWsbbO6/y0dWGOgVHypCUvaX+tzEE3EW8W3duBS7BZL8KmZAAAAAAAAAAAAAAAAAAAAAOAM3exPBx2kv47Ran98zu/tYMCG4261OTP18+VrxbcwYA/GadF+1aSRCNDb/vhaA0lXwFYibr4CT78/vuEKonUIrZ4A0++P7+QQpLYCT78/voNDkOYK3EzA2Jbk2vitDTTy9Q3EF4hoIaD/w8e5P6Gait/KQKd+fYOs3cFSAUONbRkvNdDo1zeonQBNBYz9enVuAqTxkv5rHoJatB/66fliA9QI4J791v76tSS+lYFqEuhft2ApPtQHqYBLecv6FXOpgKmXQFvtj29hoNIEurEhAUPXPii5wIV0Bfa3zWf9gvhs++MlCfTFW4pfqqNWwNAEcO/nTOCmAo72gZYkgbHVx49fW8DW2+On+zTzlK5vMKXoAAAAAAAAAAAAAAAAAAAAAPA+qW+O1O5yleyOTcXmfsulpP2pxx/6gmLNpsbcjtZuslzqc237s4//zAOhH28OdSJ2O9TxpQH4t/2rg4Q2V67Z/mzjP96JbW70E5KbwNIBhJ4f21y5Vvszjn/nNrS0uTHVgZLdsTnxqc2VrdufefzHSmKbG91dL7EOlO6OXYqP9SO2qULSPuPf5W9u3OIKIT3vzj3V8XdxhQ/i9eJJIAYggdMbIOcYtOYVLmhfr/3sTuT+cnfrPfo5sS3an3n8595Fyr6yRGYncrc3h2Jqfr5eeoGL6cYfe31a+p64dHtz6u3XNbdXzz7+XUlyap7Too6122f8AAAAAAAAAAAAAAAAAAAAcLJItzbzxYMTQOPHj0M7cku/ydLCgNo/H686ibV+/LhFvLT/LSaA9gos6r/ar1fv9H+8ubWBpCJuvgK3FjC1vXqLeC0DaR5Cq/uv/uvVu/MXSAjF924gzUOoqP9bbW/eIl7j17+1BRT3v4fNjSMbSFvA6v7716cpETB0QaPceHdZjG3FSglY8tu7WxpY6xAq6n+tgC1moGuC0njfQNL+t1rBthRQOoGbJSC3jtwfcC756XaJgaQrWIsVVCpgiwmwyfbqnBMoafya5o3NwJLr+4QOYz2sYE0ESL0Eyn0pFOtDbtu1/ZdMgFoD9HQS3UTAlrtjJQaS9L/GQK0MoL0CL757tfZ7460MJOl/rYF6OIcST6DcBI386aXEYLn/l5ioRbxkAgEAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAEA5/wOxf2ZpS+7e8wAAAABJRU5ErkJggg==";
            String src = "iVBORw0KGgoAAAANSUhEUgAAAIAAAAIACAYAAABKEWQ6AAASxUlEQVR42u2dTahkRxmGGxEli+jSgFkYXZiV4CYjSCYIkSwMSEBHVwMhuIrg7Gan0ZAhEBlBRBICcRYZhcRZiDCLISBidJUMaAwExIWCCIKbYIIgcrQ6qaZudf1/dU5VdT0PFLe7b3/1871v1Tn9U6d3OwAAAAAAAAAAAAAAAAAAAADYnHPnHllevX17adYBaePSATQdfIP8qXy5SjP3lcTUGoB04K3Nk9t/9XyN6rt5P7UuX/6TcyERcM0BjGKg0v7rXKk2r1x57hCnbqvHUnIozr+kgi4G0NhANfNnlpT8ifMvqUA/LzYAXe8pGqhG/vTzXJj1VjeQVEDpAEY3UM38hUps/LH8Rw0gETDWeV1O0UA95a/IQDU60Dq+pYEk/TfP0vX/XTHmbVcdzQxgDyAl3lXH6AYqaV/f1u2/9eL1Q1tmUaj/me2bf83+hwxkrmxHJzy5ApoV5TrYdbJVq/1UAWrFS/qfayJXjNRAu1IBXTMopwO+wee2L1mB7NmTUo5mUYX8pSzjofOXUgNVE1A6gNYGMvsdi3f1oUb+Qi9BU16KSg0kr6DhACQCmO3G4lNOJKX5k7wp5etDSttiAWsNQGKg0iVQehJZq//SdyZF+sWelPJ/qUFq/L80CbXiS/9f44Onlu0DAAAAAAAAAAAAAAAAAAAAAMAQSPfYt44HAdLvkLWOZ/IVxEv317eOP6XVa/MJJN1f3zr+FFawZhNIur++dfwprGDNJlCtCxy0ij+FFazpBKq1v75V/OgrWPMJlLNDVnKFirXjR13Bmk0ge3+5ZH99i/hTWcGaTqDcPfKuvfGt4kc3cC8T6FBBSiUhB24db5onNQHm8bl1fA8T8EiAkj3uLeNLBeglvocJ6BTBVVKulbd1fKp5zJMzn4At4nuYgME3MLbeX18Sn2KeHBNtHd/DBPTywAMPLKrw0diEKOEfe+yxfVG3z398ty8YaCwNi/NvGkCVS5/7SJYBMFBbAWvk/8gEujL1FwP1LaA0/3t0o7qS69ev70uKAWY3UA8CSvJ/EF8bQAn/2muv7UuqAWY2UHMBJflXQbrDqkGJ+LMaqKmAkvyb4pvLYE7iMVBDASX5VwE/+tKHjpyuB5Ar/swGaiKgJP8qyCV+7kkjBmokoCT/qsFXvnnHvuFS8TFQQwEl+deNSsXHQI0ElOTfHqwk6bMbqImAkvzXEh8DNRKwRv57EH90A7USsEbuxMxuoNYCNhV/dgMhIAZCwJkNBAAAAAAAAAAAAAAAADAODz14ftGFbEwo/gvPPnMo2gS+vxjohCawFt+sQN9/9OKFfWX2XwzU1wosyr8S9de3XtqLrTug76cYoBcDzboCi/NvGkCbIMcAPRiohxWktH3p+KX539kB+r6uTD+m//ZmoB4OQZL2peOX5v/QeRWkiz2IUGJaG6j1IajmDC4dvyT/Z9xvutD8Xyi4JwO1OATVaF8yfmn+dzGhzYTEls8WBmp9CKq1gkkElOQ/KnSKAVobqPUhSLoE1xBQkv8zA3FV4Hq8FwO1PgTVWoKlAkrzX8UArQzU8hBUcwZLJ5Ak/8EObGUAaXyLQ1DNGdwyf6suQb2vID0swWuOP9kAvmPcVjNgdAO1FFB8EphrjB4N1PoQ1OoQViP/q79HvoWBel1BUuPXEHD4T1hzDNT6ELTGEsx3LISJ2vIQ1OUSjDG2OQQxgyc3EAAAAAAAAAAAAABAz9xzzz2LWS5cuLAvZGYS8ZXYV69eXW7cuLG88cYb+6Juq8cw0IlPYBWghP7gS28eFWWCWEUYqK2A4vyrCtSTP/yPZbn6zntF3VYGUBWlGGB2A7UUUJr/gwG0+L//z3t/tQHU/zFQvwJK839kAF10B1INMKuBWgsozf9hAK4OqMdzDTCjgVoKKM3/HnMG6KLuq8e/eu/d+4KB+hRQmn/vcVAVJfx3v3D3cu1rn9j/xUB9CijJfxAVqMR/5Tt37kvIBDMbqAcBJflPNoH6m+OmGQzUs4A5+Y9WpIJVyV1OTt1AvQsoyf9RRaXBp26g3gWU5L8ap26g3gUUnQz2wIgGQkAMhIAYCAAAAAAAAAAAAAAAAAAAAAAAYHN+vLtrUYVMTCr+n3ef35dSE2CggSewClze/ue+lFSCgdoKWCP/4g7MbqCWAkrz30UCRjZQDwIOv4KObKCTEfCu819Z7GI+joFOeAYqgR98/mfLN3771vL8suyLuq0eV7dTDDCrgXrSsCj/Wnz1V4muixJemyIm4OwGar0Ci/Kvg13FNAcG6lPAGvl3dj51ABiorYDS/K/m/lkM1IOAkvx3e/wbxUAIOLmBEHByAwEAAAAAAAAAAAAAAAAADMD999+/mIWMTCb+49+6tFy7dm1f1O177/jAvmCgE5/AWnwdqG9/74u7ZANgoLYrsCj/doOmCfRfDNSvgDXyf0AFmBWpzqQYYHYDtRRQmv8z4j/6qY8dGlWBKQaY3UDdCFiY/z1KeLvDuR2Y1UBdCCjJfw3xZzZQcwEl+VcBLqfnHAdnN1BTAaW5yz1RwkB9CVgj/yIwUFsBpflvzugGml7A2Q2EgJMbCAAAAAAAAAAAAAAAAAAAAAAAAAAAKvHoxQtLrDD+Ex787371i2V556/BcqpJOInxlzpYPa4HeGn38KG47qsk9ZqEGuNX5U+37jxTzP91O/5SB5txe5G//sNl+f7T+7/O+53Oghrj1+Ivf//omWKbYLXxt5jBduxe7PfLLz/75Jn7axugxQw2Y13iu0ywyvhbzeCQAeyypgFazWA9fjPu4c980nlb1+Md/4gz+CheG8YuCYeAEWewbQD11zaA+T/v+EedwXa8bRrTTCEDjDqD7Xjz+XYdXgOMPIPt2eeqwxTfJ+KoM9hnILMEDwGjz2BbPLseW3xX/MgzWHwSOPoMfujB886lWpvAFl8VFXMqM9gef4qBzfEPP4NjJogdx4efwR4T+A5h8RVgsBkcqsNVXnj2Gefxf9gZLBz/8DNYowam6tHFTpgqvrP/oWewcPzDz2Afuk67rZgBhpvBwvEPP4NjSUh5B2/oGSwc//AzWMrwM3iNJIw0g9cywTAzeE0DDONgxi9naAcz/vVmxdCDYPzyJOwmZvbxAwAAAAAAAAAA9MO7f3n8si41nwsDG+DcuUcWV3n19u0l1QAXby2Xn/zXe6W0bxefWC6bZYt8bNlWdwZ4+4+fvqyE1ijBzfvqdswASnhTfLMki/B+Hboe874qRcImxNrtnozIevbGDKDFVs+9cuW5w8xXt9VjpgliCbSNUDwjjTrWNIHLuEML7iq5BjBLzAA1ZmkI2wQlM1zHx4zWlfg+4WIi+pbwWKwW2oWuzzbAGoL7DOAScX/cdizd9vN8BtLx3QifM4NNEVOXcF+sfl6o6Ho3OSFzCOgSyRQwJGLIAM56b6U9Vl38nBkcMoBvCd/KAJJk6diUOmwDHAnsOI9wxcfaX+vwdiR+zgzWItrP8y3hpoDmXzNeHwrMoh8z+7XGEl9ioJQlPGaA1BVoNQNoAWIz2DUDJTNYH9N1/FsvXj/0wSwK9b8ahwDfElx6AuYzgNmO9FVE6AS0qgFyZrDrpV5KCR0GcmNzX9c7j6tPyF8yppzElQgYOvlc5bV6iYDmO3WxJVzfNutIPQ9Inf120mIzzz6LL0n2GrPTNY7uDGAex0uXcPuloMsEOi72NrCdtNTkud5Ayl2ya4u09nsbRy/5cgzgmsWlS7htgFApeeOmNOk9vCRfvS+u42/KEh57JZCzhNviq88E/v23bx+Kum/+fwfrGEByFi6ZwfYHQj4D8DHwigaocRKm6wjNYAyw4Vu5vvs+AfTHsaXHYH2SZguoihYvdg6AAYSCmx+ouF6exQ4DqtjJTz0GmyL6DJASiwEKXr7ZZ/MuQuKFTsDMlaFUxFQDuAyEASLi65M286/9Dp754Y6vHnWMV0m2Z68SwHyf3vcuYkhE9VjoHCLFACgeMIAtuPl+fmglMD/t0+/12waw6/DFmyawRTTrsE3w1FPPXk41gHouylsmsDE/wHEZJGQA2wS58doEpoghA91335cv//+hyz+//vRNVXwGUP9T4qvnqhiU3x1/gOMT3Hw89FGv+Txz9ciNd32S6IpXQipRf/P663th1V91/+WfXLp546c/OBR1/803X75pPgcTWO/fu5Z618y2385tGa9EVIJqUZXAuvz33T/c1EU/ps2iCgZwGCC2AsQE3DreNoC+bRvA9RwMYB0CUr+M4VvCW8SbhwBTXF8xn4sBHC/hUgWwT+JaxutVIMcAiO9YBVIFcH0lq2W8XgVSDcDsLzSBK/m9xKeYAPETPw8o/TCndbwSNqWgNAAAAAAAAAAAAAAAAAAAAADMi+8CzTahb+qk1lEzNhSXc4WQacbvG4Dv+3ux+7nX5JG0H6rT14/Y/anGHxtArALfPj97y3kopmb7vm3uW7U/2vijA0hNoO8ysXZ9uQnMad8esO8ytWu1P+L4owNISWDqZWJDCZC074oPXaa2dvsjjz9rAL4LLYcuE2vvGPZdf6i0fVe8rx+ufQWzjz86gJrX+Y1tDJG0n7KzKHaFklnHL7rOL/Hjx1ftQMlVQmm/bftVB+C7SuhWCaT9/ParDEBfJlYV1wUdUo+hkvZdW8lV0f1au/1Rxx8dQLSCyImIjo/FStu341MvUzv7+JPOJFN+bMF8F8quKyW2RvuumNTL1M48/uDbkKkJDL0tWfL+dY06fP1j/JEB5P4/Jzlrtb9mH099/AAAAAAAAAAAAAAAAAAAAACwMpJt5TXrqFPJRAK4fow751s6kq+DueoSJaHG14taGyi3fen38VJ2964VX8VAPTlQap7c9iUC5OzuXSO+ioG6cKDQQKXtSwTI3d1bO76KgbpwYKMluJaANXb3SuKLDdSFAxsuwVttD19zc6ik/+0d2MESvLaA5quDlHj7sdz4zbeHtzTQFjNYKqB9naBQvO5naXyugVZJQI9X6GgpYE77a8SH+p+UwFwHbm2gmu2XCBDa2Gnu2F0rXtJ/ZwW1BfAZKPX6NqGLJNQwsESAlDpS9v6Vxov7b1dgV1biwFQDxS5ukHKRBKmBawgYegmb816EZHeyqP92Ba6TiNTk5RjI9dPxofgUE+UauJaArrok72KWtlnc/5oOLDFQKD7XhDntr/U5SEvEW8W3dmAsdosl+NRMAAAAAAAAAAAAAAAAAAAAAHCGbvanQxukv45Ra398yu/tYMCK4661OTP08+VrxdcwYA/GqdF+0aSRCNDb/vhSA0lXwFoibr4CT78/vuIK0uoQWjwBpt8f38khqNkKPP3++A4OQS1X4GoC+rYkl8ZvbaCRr28gvkBEDQHtHz5O/QnVUPxWBjr16xsk7Q6WCuhqbMt4qYFGv75B6QSoKqDv16tTEyCNl/S/5SGoRvuun57PNkCJAObZb+mvX0viaxmoJIH2dQti8a4+SAWM5S3pV8ylAoZeAm21P76GgXITaMa6BHRd+yDnAhfSFdjeNp/0C+Kz7Y+XJNAWLxYfq6NUQNcEMO+nTOCqAo72gZYkgb7Vx45fW8Da2+On+zTzlK5vMKXoAAAAAAAAAAAAAAAAAAAAAPA+oW+OlO5yleyODcWmfsslp/2px+/6gmLJpsbUjpZusoz1ubT92cd/5gHXjze7OuG77ep4bAD2bfvqIK7NlWu2P9v4D3d8mxvthKQmMHcAruf7Nleu1f6M49+ZDcU2N4Y6kLM7NiU+tLmydvszj/9QiW9zo7nrxdeB3N2xsXhfP3ybKiTtM/5d+ubGLa4Q0vPu3FMdfxdX+CC+XTwJxAAkcHoDpByD1rzCBe23az+5E6m/3F17j35KbI32Zx7/0btIyVeWSOxE6vZmV0zJz9dLL3Ax3fh9r09z3xOXbm8Ovf265vbq2ce/y0lOyXNq1LF2+4wfAAAAAAAAAAAAAAAAAAAAThbp1ma+eHACtPjxY9eO3NxvstQwYOufj286iVv9+HGNeGn/a0yA1iuwqP/Nfr161/7Hm2sbSCri5itwbQFD26u3iG9loJaH0OL+N//16t3xBRJc8b0bqOUhVNT/rbY3bxHf4te/Wwso7n8PmxtHNlBrAYv7b1+fJkdA1wWNUuPNZdG3FSskYM5v725p4FaHUFH/SwWsMQNNE+TG2waS9r/WCralgNIJXC0BqXWk/oBzzk+3SwwkXcFqrKBSAWtMgE22V6ecQEnj1zSvbwbmXN/HdRjrYQWrIkDoJVDqSyFfH1LbLu2/ZAKUGqCnk+gqAtbcHSsxkKT/JQaqZYDWK3D03au13xuvZSBJ/0sN1MM5lHgCpSZo5E8vJQZL/b/ERDXiJRMIAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA8vkfP4eVSPuC46kAAAAASUVORK5CYII=";
            Texture2D bigSprite = StardewValley.Game1.bigCraftableSpriteSheet;

            Bitmap orgSpriteFirstLoad = this.Texture2Image(bigSprite);
            Texture2D orgSpriteSecondLoad = this.Bitmap2Texture(orgSpriteFirstLoad);
            Image orgSprite = this.Texture2Image(orgSpriteSecondLoad);
            int osH = orgSprite.Height;
            int osW = orgSprite.Width;
            Image newSprite = this.LoadImage(src);
            int nsH = newSprite.Height;

            Bitmap combSprite = this.combineImages(orgSprite, newSprite, 0);
            Texture2D fullTex = this.Bitmap2Texture(combSprite);
            Bitmap fullSprite = this.Texture2Image(fullTex);
            StardewValley.Game1.bigCraftableSpriteSheet = fullTex;

            combSprite.Dispose();
            fullSprite.Dispose();

           return (osW / 16) * ((osH) / 32);

        }


        

        private Bitmap combineImages(Image img1, Image img2, int offset)
        {

            int height = img1.Height + img2.Height + offset;
            int width = Math.Max(img1.Width, img2.Width);

            Bitmap img3 = new Bitmap(width, height);
            Graphics g = Graphics.FromImage(img3);

            g.Clear(System.Drawing.Color.Transparent);
            g.DrawImage(img1, new System.Drawing.Point(0, 0));
            g.DrawImage(img2, new System.Drawing.Point(0, img1.Height + offset));

            g.Dispose();
            img1.Dispose();
            img2.Dispose();

            return img3;

        }

        private Image LoadImage(String imageString)
        {

            byte[] bytes = Convert.FromBase64String(imageString);

            Image image;

            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
            }

            return image;
        }

        private Texture2D Bitmap2Texture(Bitmap bmp)
        {

            MemoryStream s = new MemoryStream();

            bmp.Save(s, System.Drawing.Imaging.ImageFormat.Png);
            s.Seek(0, SeekOrigin.Begin);
            Texture2D tx = Texture2D.FromStream(StardewValley.Game1.graphics.GraphicsDevice, s);

            return tx;

        }

        private Bitmap Texture2Image(Texture2D tex)
        {
            Texture2D texture = tex;
            byte[] textureData = new byte[4 * texture.Width * texture.Height];
            texture.GetData<byte>(textureData);

            Bitmap bmp = new Bitmap(
                           texture.Width, texture.Height,
                           System.Drawing.Imaging.PixelFormat.Format32bppArgb
                         );

            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(
                           new System.Drawing.Rectangle(0, 0, texture.Width, texture.Height),
                           System.Drawing.Imaging.ImageLockMode.WriteOnly,
                           System.Drawing.Imaging.PixelFormat.Format32bppArgb
                         );

            IntPtr safePtr = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(textureData, 0, safePtr, textureData.Length);
            bmp.UnlockBits(bmpData);

            return bmp;

        }
        

    }
}
