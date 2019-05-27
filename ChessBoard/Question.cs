using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChessBoard
{
    public class Question
    {
        public string Text { get; set; }
        public List<Choice> Choices { get; set; } = new List<Choice>();

        public Question(string text, params Choice[] choices)
        {
            Text = text;

            foreach (Choice choice in choices)
                Choices.Add(choice);
        }
    }
}
