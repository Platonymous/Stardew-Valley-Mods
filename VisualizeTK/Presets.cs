using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VisualizeTK;

namespace VisualizeTK
{
    internal class Presets
    {
        public static void Test()
        {
            ApplyPreset("Grey");
        }

        public static void Custom()
        {
            VisualizeTKMod.Singleton.SetShaderParameters(1.0f, 1.0f, 1.0f, new Color[] { Color.White }, 0);
        }

        public static bool ApplyPreset(string name)
        {
            if (typeof(Presets).GetMethod(name) is MethodInfo m)
            {
                m.Invoke(null, new object[0]);
                return true;
            }

            return false;
        }

        public static void Sepia()
        {
            Set(0.03f, new Color[] { Color.BlanchedAlmond }, 0);
        }

        public static void Gray()
        {
            Set(0.0f);
        }

        public static void Grey()
        {
            Set(0.0f);
        }
        public static void Underwater()
        {
            Set(0.1f,0.2f,0.3f, new Color[] { Color.DodgerBlue, Color.Blue}, 0.2f);
        }
        public static void Garden()
        {
            Set(1.1f,1.6f,1.3f);
        }

        public static void Desaturated()
        {
            Set(0.5f);
        }

        public static void Danger()
        {
            Set(0.7f, new Color[] { Color.PaleVioletRed, Color.DarkRed }, 3f);
        }

        public static void Heat()
        {
            Set(0.6f, new Color[] { Color.Red, Color.Orchid, Color.OrangeRed }, 0.2f);
        }

        public static void Sunset()
        {
            Set(0.5f, new Color[] { Color.Orange, Color.PaleVioletRed, Color.MediumOrchid }, 0.2f);
        }

        public static void Midnight()
        {
            Set(0.3f, new Color[] { Color.DarkSlateBlue, Color.MediumBlue}, 0.1f);
        }

        public static void Set(float sat, Color[] tint, float speed)
        {
            VisualizeTKMod.Singleton.SetShaderParameters(sat,sat,sat,tint,speed);
        }

        public static void Set(float sat)
        {
            VisualizeTKMod.Singleton.SetShaderParameters(sat, sat, sat, new Color[] { Color.White }, 0);
        }

        public static void Set(float satr, float satg, float satb, Color[] tint, float speed)
        {
            VisualizeTKMod.Singleton.SetShaderParameters(satr, satg, satb, tint, speed);
        }
        public static void Set(float satr, float satg, float satb)
        {
            VisualizeTKMod.Singleton.SetShaderParameters(satr, satg, satb, new Color[] { Color.White }, 0);
        }
    }
}
