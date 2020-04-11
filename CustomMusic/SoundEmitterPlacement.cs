using Microsoft.Xna.Framework;

namespace CustomMusic
{
    public class SoundEmitterPlacement
    {
        public const float DefaultDistanceModifier = 0.01f;
        public const float DefaultVolumeModifier = 1f;
        public const int DefaultMaxDistance = 25;

        public float[] Position { get; set; } = new float[2] { 0, 0 };

        public string SoundId { get; set; } = "";

        public float DistanceModifier { get; set; } = DefaultDistanceModifier;

        public float VolumeModifier { get; set; } = DefaultVolumeModifier;

        public int MaxTileDistance { get; set; } = DefaultMaxDistance;

        public string Conditions { get; set; } = "";

        public Vector2 GetPosition()
        {
            return new Vector2(Position[0], Position[1]);
        }
    }
}