using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomFurniture
{

    public class WallpaperData
    {

    public Texture2D texture;
    public int index;
    public int width;
    public int height;
    public int parentSheetIndex;
    
    public WallpaperData(Texture2D texture, int index, int width, int height, int parentSheetIndex)
    {
            this.texture = texture;
            this.index = index;
            this.width = width;
            this.height = height;
            this.parentSheetIndex = parentSheetIndex;
    }

}
}
