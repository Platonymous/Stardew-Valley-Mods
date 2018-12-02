namespace CustomFurniture
{
    class CustomFurniturePack
    {
        public string useid { get; set; } = "none";
        public string folderName { get; set; }
        public string fileName { get; set; }
        public string author { get; set; } = "none";
        public string version { get; set; } = "1.0.0";
        public string name { get; set; } = "Furniture Pack";
        public CustomFurnitureData[] furniture { get; set; }
        
        public CustomFurniturePack()
        {
            furniture = new CustomFurnitureData[] { new CustomFurnitureData(), new CustomFurnitureData(), new CustomFurnitureData(), new CustomFurnitureData(), new CustomFurnitureData() };
        }
    }
}
