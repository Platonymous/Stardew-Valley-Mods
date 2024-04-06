using Artista.Artpieces;
using Microsoft.Xna.Framework;
using System;

namespace Artista.Menu
{
    public class SizeChoice
    {
        public Rectangle Rectangle { get; set; }

        public string Text { get; set; }

        public Painting Art { get; set; }
    
        public SizeChoice(int w, int h, int s)
        {
            Text = $"{w} x {h}";
            if(s > 1)
            {
                Text += $" (X{s})";
            }
            Art = new Painting(w, h, s);
        }



    }

    public class TextChoice
    {
        public Rectangle Rectangle { get; set; }

        public string Text { get; set; }

        public Action Action { get; set; }

        public float Opacity { get; set; } = 1.0f;

        public TextChoice(string text, Action action, float opacity = 1f)
        {
            Opacity = opacity;
            Text = text;
            Action = action;
        }



    }
}
