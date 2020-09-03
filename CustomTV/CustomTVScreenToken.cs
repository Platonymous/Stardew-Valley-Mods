using StardewModdingAPI;
using System.Collections.Generic;

namespace CustomTV
{
    internal class CustomTVScreenToken : CustomTVChannelToken
    {
        public CustomTVScreenToken()
        {
        }

        public override IEnumerable<string> GetValues(string input)
        {
            string[] parts = input.Replace(": ", ":").Split(' ');
            //Channel Id Bounds:XX YY WW HH Frames:FF Duration:DD Offset:XX YY
            string screenid = GetScreenId(parts[0], parts[1]);
            if (Screens.ContainsKey(screenid))
                return new[] { $"{ScreenTexturePrefix}{screenid}" };

            TVScreen screen = new TVScreen();
            screen.Id = screenid;

            if (parts.Length > 2)
                for (int i = 2; i < parts.Length; i++)
                {
                    string p = parts[i];
                    if (p.Split(':') is string[] parameter && parameter.Length == 2)
                    {
                        switch (parameter[0].ToLower())
                        {
                            case "bounds":
                                {
                                    if (parts.Length > i + 3
                                            && int.TryParse(parameter[1], out int x)
                                            && int.TryParse(parts[i + 1], out int y)
                                            && int.TryParse(parts[i + 2], out int width)
                                            && int.TryParse(parts[i + 3], out int height))
                                        screen.SourceBounds = new[] { x, y, width, height };
                                    break;
                                }
                            case "frames":
                                {
                                    if (int.TryParse(parameter[1], out int frames))
                                        screen.Frames = frames;
                                    break;
                                }
                            case "duration":
                                {
                                    if (int.TryParse(parameter[1], out int duration))
                                        screen.FrameDuration = duration;
                                    break;
                                }
                            case "offset":
                                {
                                    if (parts.Length > i + 1
                                        && int.TryParse(parameter[1], out int offsetX)
                                        && int.TryParse(parts[i + 1], out int offsetY))
                                        screen.Offset = new[] { offsetX, offsetY };
                                    break;
                                }
                        }
                    }
                }
            string output = $"{ScreenTexturePrefix}{screen.Id}";

            screen.Path = output;
            Screens.Add(screen.Id, screen);

            return new[] { output };
        }
    }
}
