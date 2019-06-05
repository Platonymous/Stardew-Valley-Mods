using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Extensions;
using System;

namespace PyTK.PlatoUI
{
    public class UITextElement : UIElement
    {
        protected string _text;
        public virtual Point TextSize { get; set; }
        public virtual string Text
        {
            get
            {
                return _text;
            }
            set
            {
                _text = value;
                TextSize = Font.MeasureString(_text).toPoint();
                UpdateBounds();
            }
        }

        public virtual float Scale { get; set; } = 1f;

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

        public override UIElement Clone(string id = null)
        {
            if (id == null)
                id = Id;

            var e = new UITextElement(Text,Font,TextColor,Scale, Opacity,id,Z,Positioner).WithInteractivity(UpdateAction, HoverAction, ClickAction, KeyAction, ScrollAction).WithTypes(Types.ToArray());

            e.SelectAction = SelectAction;
            e.IsSelectable = IsSelectable;
            e.IsSelected = IsSelected;
            e.SelectionId = SelectionId;
            e.IsDraggable = IsDraggable;
            e.DragAction = DragAction;
            e.DragPoint = DragPoint;

            foreach (UIElement child in Children)
                e.Add(child.Clone());

            return e;
        }
    }
}
