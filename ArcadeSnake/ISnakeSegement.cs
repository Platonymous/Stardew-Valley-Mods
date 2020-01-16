using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Snake
{
    public interface ISnakeSegement
    {
        ISnakeSegement ChildSegment { get; }
        void AddNewTailSegment();
    }
}
