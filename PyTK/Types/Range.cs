using Microsoft.Xna.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PyTK.Types
{
    public class Range : IEnumerator<int>, IEnumerable
    {
        public int X = 0;
        public int Y = 0;
        private int[] _array
        {
            get
            {
                return toArray();
            }
        }

        public int this[int i]
        {
            get { return _array[i]; }
        }

        private int _index = 0;

        public Range(int from, int to)
        {
            X = from;
            Y = to;
        }

        public Range(int to)
        {
            Y = to;
        }

        public List<int> toList()
        {
            List<int> list = new List<int>(toArray());
            return list;
        }

        public int[] toArray()
        {
            int[] arr = new int[length];
            for (int i = 0; i < length; i++)
                arr[i] = X + i;
            return arr;
        }

        public bool overlaps(Range range)
        {
            return !(X < range.Y || Y < range.X);
        }

        public int length
        {
            get
            {
                return (Y - X);
            }
            set
            {
                Y += (value - length);
            }
        }

        public Range clone()
        {
            Range newRange = new Range(0, 3) * 1;
            return new Range(X, Y);
        }

        public int find(int i)
        {
            if (contains(i))
                return (i - X);
            else
                return -1;
        }

        public bool contains(int i)
        {
            return i >= X && i < Y;
        }

        public bool MoveNext()
        {
            _index++;
            return _index < length;
        }

        public void Reset()
        {
            _index = 0;
        }

        public void Dispose()
        {

        }

        public IEnumerator GetEnumerator()
        {
            return clone();
        }
        
        public static Range Zero {
            get
            {
                return new Range(0);
            }
         }

        public static Range Max
        {
            get
            {
                return new Range(int.MinValue,int.MaxValue);
            }
        }

        public int Current => _array[_index];

        object IEnumerator.Current => _array[_index];

        public static Range operator -(Range value1, Range value2)
        {
            return new Range(Math.Max(value1.X, value2.X), Math.Min(value1.Y, value2.Y));
        }

        public static Range operator +(Range value1, Range value2)
        {
            return new Range(Math.Min(value1.X, value2.X), Math.Max(value1.Y, value2.Y));
        }

        public static Range operator -(Range value1, int value2) {
            return new Range(value1.X - value2, value1.Y - value2);
        }

        public static Range operator +(Range value1, int value2)
        {
            return new Range(value1.X + value2, value1.Y + value2);
        }

        public static Range operator *(Range value1, int value2)
        {
            return new Range(value1.X * value2, value1.Y * value2);
        }

        public static Range operator /(Range value1, int value2)
        {
            return new Range(value1.X / value2, value1.Y / value2);
        }


    }
}
