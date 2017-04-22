using StardewValley;

namespace HarpOfYobaRedux
{
    class AnimalMagic : IMagic
    {
        public AnimalMagic()
        {

        }

        public void petAnimals()
        {
            if (Game1.currentLocation is AnimalHouse || Game1.currentLocation is Farm)
            {
                SerializableDictionary<long, FarmAnimal> animals = Game1.getFarm().animals;

                if (Game1.currentLocation is AnimalHouse)
                {
                    animals = (Game1.currentLocation as AnimalHouse).animals;
                }

                foreach (FarmAnimal animal in animals.Values)
                {
                    if (!animal.wasPet)
                    {
                        Game1.player.FarmerSprite.PauseForSingleAnimation = false;
                        animal.pet(Game1.player);

                    }
                }
            }
        }

        public void doMagic(bool playedToday)
        {
            DelayedAction animalAction = new DelayedAction(6000);
            animalAction.behavior = new DelayedAction.delayedBehavior(petAnimals);

            Game1.delayedActions.Add(animalAction);
        }       
    }
}
