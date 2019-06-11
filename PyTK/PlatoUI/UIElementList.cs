using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PyTK.PlatoUI
{
    public class UIElementList : UIElement
    {
        public int Position { get; private set; } = 0;

        public Func<UIElement, UIElement, Rectangle> ElementPositioner { get; set; }

        public UIElement InnerList;
        protected bool IsVertical { get; set; } = true;

        protected int Margin = 0;

        public List<UIElement> ListElements = new List<UIElement>();

        protected bool Scrollable { get; set; } = true;

        public UIElementList(string id = "element", bool vertical = true, int z = 0, float opacity = 1f, int margin = 0, int startPosition = 0, bool scrollable = true, Func<UIElement, UIElement, Rectangle> positioner = null, Func<UIElement, UIElement, Rectangle> elementPositioner = null, params UIElement[] elements)
            : base(id, positioner, z, null, Color.White, opacity, true)
        {
            Margin = margin;
            Position = 0;
            Scrollable = scrollable;
            if (elementPositioner == null)
                ElementPositioner = UIHelper.Fill;
            else
                ElementPositioner = elementPositioner;

            IsVertical = vertical;
            Overflow = false;
            foreach (UIElement element in elements)
                Add(element);

            while (Position != startPosition)
                if (!ChangePosition(1))
                    break;
        }

        public virtual void Scroll(int direction)
        {
            if (direction > 0)
                direction = -1;
            else
                direction = 1;

            ChangePosition(direction);
        }

        public override UIElement Clone(string id = null)
        {
            if (id == null)
                id = Id;

            UIElement e = new UIElementList(id, IsVertical,Z,Opacity,Margin,Position,Scrollable, Positioner,ElementPositioner);
            CopyBasicAttributes(ref e);

            List<UIElement> elements = new List<UIElement>();
            foreach (UIElement child in ListElements)
                elements.Add(child.Clone());

            foreach (UIElement element in elements)
                e.Add(element);

            return e;
        }

        public override void Add(UIElement element, bool disattach = true)
        {

            if (disattach)
                element.Disattach();

            UIElement container;
            container = new UIElement(element.Id + "_Container", ElementPositioner, Z, null, null, 1f, true);

            if (Children.Count > 0) {
                UIElement last = Children.Last();
                if (!IsVertical)
                    container.OffsetX = last.OffsetX + Margin + last.Bounds.Width;
                else
                    container.OffsetY = last.OffsetY + Margin + last.Bounds.Height;
            }
            container.Add(element.WithBase(this));
            base.Add(container.WithBase(this));
            ListElements.Add(element);
        }

        public override void Remove(UIElement element)
        {
            base.Remove(element);
            ResetPositions();
        }

        public override void PerformScroll(int direction)
        {
            if (Scrollable && WasHover)
                Scroll(direction);

            base.PerformScroll(direction);
        }

        public override void PerformHover(Point point)
        {
            if ((HoverAction != null || Scrollable) && Bounds.Contains(point))
            {
                HoverAction?.Invoke(point, true, this);
                WasHover = true;
            }
            else if ((HoverAction != null || Scrollable) && WasHover)
            {
                HoverAction?.Invoke(point, false, this);
                WasHover = false;
            }

            foreach (UIElement child in Children.Where(c => c.Visible))
                child.PerformHover(point);
        }

        public virtual bool NextPosition()
        {
            return ChangePosition(1);
        }

        public virtual bool PreviousPosition()
        {
            return ChangePosition(-1);
        }

        public virtual bool ChangePosition(int steps)
        {
            if (steps == 0 || Children.Count < 2 || (steps > 0 && !Children.Last().OutOfBounds) || (steps < 0 && Position == 0))
                return false;

            UIElement first = Children.First();

            Position+= steps;
            

            foreach (UIElement child in Children)
                if (IsVertical)
                    child.OffsetY += steps * -1 * (first.Bounds.Height + Margin);
                else
                    child.OffsetX += steps * -1 * (first.Bounds.Width + Margin);

            Parent.UpdateBounds();

            return true;
        }

        public virtual void ResetPositions(bool children = true)
        {
            base.UpdateBounds(children);

            int t = Position;
            while (Position > 0)
                if (!PreviousPosition())
                    break;

            List<UIElement> elments = new List<UIElement>(ListElements);

            ListElements.Clear();
            Children.Clear();

            foreach (UIElement element in elments)
                Add(element);

            if (Position == 0)
                return;

            while (Position < t)
                if (!NextPosition())
                    break;

            base.UpdateBounds(children);
        }

        public override void UpdateBounds(bool children = true)
        {
            base.UpdateBounds(children);
        }
    }
}
