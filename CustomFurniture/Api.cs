using System.Collections.Generic;

namespace CustomFurniture
{
  public interface IApi
  {
    List<string> GetAllFurnitureFromContentPack(string cp);
  }

  public class Api : IApi
  {
    /// <summary>
    /// Given the unique id of a content pack, return a list of all furniture
    /// item names defined by that content pack.
    /// </summary>
    /// <param name="cpUniqueId">The unique id of a content pack,
    /// e.g. Platonymous.ExamplePack</param>
    /// <returns>List of furniture names, or null if the content pack was
    /// unknown to us.</returns>
    public List<string> GetAllFurnitureFromContentPack(string cpUniqueId)
    {
      foreach (var entry in ((CustomFurnitureMod)CustomFurnitureMod.instance).
                 furnitureByContentPack)
      {
        if (entry.Key.UniqueID == cpUniqueId)
        {
          return new List<string>(entry.Value);
        }
      }
      return null;
    }

  }
}
