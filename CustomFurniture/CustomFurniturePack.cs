namespace CustomFurniture
{
    class CustomFurniturePack
    {
        public string folderName { get; set; }
        public string fileName { get; set; }
        public CustomFurnitureData[] furniture { get; set; }
        
        public CustomFurniturePack()
        {
            furniture = new CustomFurnitureData[] { new CustomFurnitureData(), new CustomFurnitureData(), new CustomFurnitureData(), new CustomFurnitureData(), new CustomFurnitureData() };
        }
    }
}
