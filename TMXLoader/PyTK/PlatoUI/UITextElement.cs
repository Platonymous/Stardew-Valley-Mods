using BmFont;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.IO;

namespace TMXLoader
{
    public class UITextElement : UIElement
    {
        protected string _text;
        public virtual Point TextSize { get; set; }

        public virtual string FontId { get; set; } = "";

        public virtual string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                MeasureString();
                UpdateBounds();
            }
        }

        protected float _scale = 1f;

        public virtual float Scale
        {
            get
            {
                return _scale;
            }
            set
            {
                _scale = value;
                MeasureString();
            }
        }

        public virtual SpriteFont Font { get; set; }
        public virtual Color TextColor { get; set; } = Color.Black;

        public UITextElement(string text, SpriteFont font, Color color, float scale = 1f, float opacity = 1f, string id = "element", int z = 0, Func<UIElement, UIElement, Rectangle> positioner = null)
            :base(id, positioner, z, null, null, opacity, false)
        {
            Scale = scale;
            Font = font;
            Text = text;
            TextColor = color;
        }

        public virtual string GetText()
        {
            if (!OutOfBounds || Text == null || Font == null || Text == "")
                return Text;

            string text = Text;

            while (OutOfBounds && Text.Length > 1)
                Text = Text.Substring(0, Text.Length - 1);

            if (OutOfBounds)
                Text = "";

            string r = Text;
            Text = text;

            return r;
        }

        public virtual Point MeasureString()
        {
            if (FontId == "" && Font != null)
            {
                var p = Font.MeasureString(_text).toPoint();
                TextSize = new Point((int)(p.X * Scale), (int)(p.Y * Scale));
            }
            else if(FontId != "")
                TextSize = UIFontRenderer.MeasureString(FontId, _text, Scale);

            return TextSize;
        }

        public override UIElement Clone(string id = null)
        {
            if (id == null)
                id = Id;

            UIElement e = new UITextElement(Text,Font,TextColor,Scale, Opacity,id,Z,Positioner);

            CopyBasicAttributes(ref e);

            foreach (UIElement child in Children)
                e.Add(child.Clone());

            return e;
        }

        public virtual UITextElement WithFont(string id)
        {
            FontId = id;
            MeasureString();
            return this;
        }
    }
}
