using StardewModdingAPI;
namespace GifRecorder
{
    public class Config
    {
        internal int Delay
        {
            get
            {
                int delay = 90;

                if (FPS >= 30)
                    delay = 60;

                if (FPS >= 60)
                    delay = 30;

                return delay;
            }
        }
        public int FPS { get; set; } = 15;

        public int Scale { get; set; } = 2;

        public int MaxFrames { get; set; } = 300;

        public float FrameOpacity { get; set; } = 0.7f;

        public int DelayGifEncoder { get; set; } = 2000;

        public int DelayGifEncoderStep { get; set; } = 100;

        public SButton RecordButton {get;set;} = SButton.R;
        public bool WithCtrlButton { get; set; } = true;

        public SButton FrameButton { get; set; } = SButton.NumPad5;
        public SButton FrameUp { get; set; } = SButton.NumPad8;
        public SButton FrameDown { get; set; } = SButton.NumPad2;
        public SButton FrameLeft { get; set; } = SButton.NumPad4;
        public SButton FrameRight { get; set; } = SButton.NumPad6;
        public SButton FrameWider { get; set; } = SButton.NumPad7;
        public SButton FrameThiner { get; set; } = SButton.NumPad9;
        public SButton FrameTaller { get; set; } = SButton.NumPad1;
        public SButton FrameFlatter { get; set; } = SButton.NumPad3;

        public SButton FixViewport { get; set; } = SButton.Home;

        public SButton RemoveBackground { get; set; } = SButton.NumPad0;



    }
}
