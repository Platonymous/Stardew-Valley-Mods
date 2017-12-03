using System.Collections.Generic;

namespace CustomNPC
{
    public class NPCBlueprint
    {
        public string name { get; set; } = "Name";
        public string displayName { get; set; } = "none";
        public string author { get; set; } = "none";
        public string version { get; set; } = "1.0.0";
        public string sprite { get; set; } = "sprite.png";
        public string portrait { get; set; } = "portrait.png";
        public string map { get; set; } = "Town";
        public string age { get; set; } = "adult";
        public string birthdaySeason { get; set; } = "summer";
        public int birthday { get; set; } = 9;
        public string gender { get; set; } = "male";
        public int[] position { get; set; } = new int[] { 25, 70 };
        public int facing { get; set; } = 3;
        public string homeRegion { get; set; } = "Town";
        public int[] loves { get; set; } = new int[0];
        public int[] likes { get; set; } = new int[0];
        public int[] dislikes { get; set; } = new int[0];
        public int[] hates { get; set; } = new int[0];
        public string datable { get; set; } = "not-datable";
        public string manners { get; set; } = "neutral";
        public string socialAnxiety { get; set; } = "neutral";
        public string optimism { get; set; } = "neutral";
        public string crush { get; set; } = "null";
        public string relations { get; set; } = "";
        public string dialogue { get; set; } = "none";
        public string marriageDialogue { get; set; } = "none";
        public string schedule { get; set; } = "none";
        public string animations { get; set; } = "none";
        public string events { get; set; } = "none";
        public string fileDirectory { get; set; } = "NPC";
        public string specialPositions { get; set; } = "none";
        public string[] customLocations { get; set; } = new string[] { "PetShop" };
        public int firstDay { get; set; } = 0;
        public string conditions { get; set; } = "none";
        public string spouseRoom { get; set; } = "none";
        public int[] spouseRoomPos { get; set; } = new int[] { 29, 1, 35, 10 };
        public string shopLocation { get; set; } = "none";
        public int[] shopPosition { get; set; } = new int[] { 20, 14 };
        public int[] shopkeeperPosition { get; set; } = new int[] { 21, 14 };
        public string shopConditions { get; set; } = "none";
        public List<ForSaleItem> inventory { get; set; } = new List<ForSaleItem>();
        public string[] translations { get; set; } = new string[] { "en" };
        public bool translateMarriage { get; set; } = true;
        public bool translateAnimations { get; set; } = false;
        public bool translateSchedule { get; set; } = false;
        public bool translateEvents { get; set; } = false;
        public string mail { get; set; } = "none";
        public bool translateMail { get; set; } = true;
        public List<CustomBuilding> buildings { get; set; } = new List<CustomBuilding>();
        public List<CustomRoom> rooms { get; set; } = new List<CustomRoom>();
        public List<AdditionalWarp> warps { get; set; } = new List<AdditionalWarp>();

        public NPCBlueprint()
        {
     

        }

    }
}
