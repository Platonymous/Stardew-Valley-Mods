using Microsoft.Xna.Framework;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Monsters;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HarpOfYobaRedux
{
    class MonsterMagic : IMagic
    {

        public MonsterMagic()
        {

        }

        public void switchMonsters()
        {
            List<int> glMonster = new List<int>();

            for (int i = 0; i < Game1.currentLocation.characters.Count(); i++)
            {
                Character c = Game1.currentLocation.characters[i];
                if (c is Monster && !(c is Duggy))
                    glMonster.Add(i);
            }

            if(glMonster.Count <= 0)
                return;

            Monster pickMonster = (Monster)Game1.currentLocation.characters[glMonster[Game1.random.Next(glMonster.Count())]];

            Type t = pickMonster.GetType();
            
            for (int j = 0; j < glMonster.Count(); j++)
            {
                Monster monster = (Monster)Game1.currentLocation.characters[glMonster[j]];
                if (monster == pickMonster)
                    continue;

                try
                {
                    if (Game1.currentLocation is MineShaft)
                    {
                        MineShaft gl = (MineShaft)Game1.currentLocation;

                        if (pickMonster is GreenSlime || pickMonster is Bat)
                            monster = (Monster)Activator.CreateInstance(t, new object[] { new Vector2(monster.position.X, monster.position.Y), gl.mineLevel });
                        else
                            monster = (Monster)Activator.CreateInstance(t, new object[] { new Vector2(monster.position.X, monster.position.Y) });
                    }
                    else if (Game1.currentLocation is SlimeHutch && pickMonster is GreenSlime)
                        monster = (Monster)Activator.CreateInstance(t, new object[] { new Vector2(monster.position.X, monster.position.Y), (pickMonster as GreenSlime).color.Value });
                }
                catch
                {

                }

                Game1.currentLocation.characters[glMonster[j]] = monster;
            }

            Game1.player.forceTimePass = true;
            Game1.currentLocation.damageMonster(new Rectangle(0, 0, Game1.currentLocation.map.DisplayWidth, Game1.currentLocation.map.DisplayHeight), 0, 0, false, 1.5f, 100, 0f, 1f, false, Game1.player);
            pickMonster.doEmote(20);

            Game1.delayedActions.Add(new DelayedAction(500, secondHit));
        }

        public void secondHit()
        {
            Game1.currentLocation.damageMonster(new Rectangle(0, 0, Game1.currentLocation.map.DisplayWidth, Game1.currentLocation.map.DisplayHeight), 0, 0, false, 1.5f, 100, 0f, 1f, false, Game1.player);
            Game1.player.forceTimePass = false;
        }

        public void doMagic(bool playedToday)
        {
            Game1.delayedActions.Add(new DelayedAction(5000, switchMonsters));
        }
    }
}
