using System;

namespace Snake
{
    public class Highscore
    {
        public int Value { get; set; } = 0;
        public String Name { get; set; } = "None";

        public Highscore()
        {

        }

        public Highscore(string name, int value)
        {
            Value = value;
            Name = name;
        }
    }
}
