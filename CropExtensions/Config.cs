using System.Collections.Generic;

namespace CropExtensions
{
    public class Config
    {
        public bool DetailedCropSeasons { get; set; } = true;

        public Dictionary<string, CropDetails> Presets { get; set; } = new Dictionary<string, CropDetails>();
    }
}
