using Harmony;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Text;

namespace Capitalism.Components.CapitalismCurrencyPatch
{
    internal struct StringProxy
    {
        private string textString;
        private StringBuilder textBuilder;
        public readonly int Length;

        public char this[int index]
        {
            get
            {
                if (this.textString != null)
                    return this.textString[index];
                return this.textBuilder[index];
            }
        }

        public StringProxy(string text)
        {
            this.textString = text;
            this.textBuilder = (StringBuilder)null;
            this.Length = text.Length;
        }

        public StringProxy(StringBuilder text)
        {
            this.textBuilder = text;
            this.textString = (string)null;
            this.Length = text.Length;
        }
    }

    [HarmonyPatch(typeof(SpriteFont), "InternalDraw")]
    internal class SpriteFontFix
    {
        internal static bool Prefix(ref SpriteFont __instance, ref StringProxy text)
        {
            /*
            int num = 0;
            int.TryParse(text.ToString(), out num);

            if (num <= 0)
                return true;

            string money = string.Format("{0:0.00}", Convert.ToDecimal(num) / 100);

            text = new StringProxy(money);
            */
            return true;
        }
    }
}
