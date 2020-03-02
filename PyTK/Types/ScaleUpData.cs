using System.Collections.Generic;

namespace PyTK.Types
{
    public class ScaleUpData
    {
        public float Scale { get; set; } = 1f;
        public Animation Animation {get; set;} = null;
        public int[] SourceArea { get; set; } = null;
    }

    public class Animation
    {
        public int FrameWidth { get; set; } = -1;
        public int FrameHeight { get; set; } = -1;
        public int FPS { get; set; } = 30;

        public bool Loop { get; set; } = true;
    }
}
