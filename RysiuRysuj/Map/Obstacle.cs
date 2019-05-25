using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace RysiuRysuj.Map
{
    abstract public class Obstacle
    {
        //every obstacle will be polygon becouse collision system needs that
        public List<Vector2> Points = new List<Vector2>();

        //this function returns if a line collides with this obstacle, and if so, also where
        public bool TryGetCollisionPoint(Vector2 a, Vector2 b, out float coldis, out Vector2 point, out Vector2 tangent)
        {
            float min = float.PositiveInfinity;
            float pos = 1;
            int nearest = -1;
            float r, s;
            if (IsIntersecting(Points.Last(), Points.First(), a, b, out r, out s))
            {
                min = s;
                pos = r;
                nearest = 0;
            }
            for (int i = 1; i < Points.Count; i++)
            {
                if (IsIntersecting(Points[i -1], Points[i], a, b, out r, out s))
                {
                    if (s < min)
                    {
                        pos = r;
                        min = s;
                        nearest = i;
                    }
                }
            }

            if (nearest == 0)
            {
                point = Points.Last() + (Points.First() - Points.Last()) * pos;
                tangent = Points.First() - Points.Last();
                coldis = min;
                return true;
            }
            else if (nearest > 0)
            {
                point = Points[nearest - 1] + (Points[nearest] - Points[nearest - 1]) * pos;
                tangent = Points[nearest] - Points[nearest - 1]; 
                coldis = min;
                return true;
            }
            else
            {
                point = new Vector2();
                tangent = new Vector2();
                coldis = 0;
                return false;
            }
        }

        bool IsIntersecting(Vector2 a, Vector2 b, Vector2 c, Vector2 d, out float r, out float s)
        {
            float denominator = ((b.X - a.X) * (d.Y - c.Y)) - ((b.Y - a.Y) * (d.X - c.X));
            float numerator1 = ((a.Y - c.Y) * (d.X - c.X)) - ((a.X - c.X) * (d.Y - c.Y));
            float numerator2 = ((a.Y - c.Y) * (b.X - a.X)) - ((a.X - c.X) * (b.Y - a.Y));

            r = 0; s = 0;

            // Detect coincident lines (has a problem, read below)
            if (denominator == 0) return false;

            r = numerator1 / denominator;
            s = numerator2 / denominator;

            return (r >= 0 && r <= 1) && (s >= 0 && s <= 1);
        }

        public abstract void Draw(CanvasDrawingSession ds);

        public virtual void CreateResources(ICanvasResourceCreator device)
        {
        }
    }


    public class Wall : Obstacle
    {
        CanvasGeometry geometry;
        public Wall(List<Vector2> points)
        {
            Points = points;
        }

        public override void CreateResources(ICanvasResourceCreator device)
        {
            geometry = CanvasGeometry.CreatePolygon(device, Points.ToArray());
        }
        public override void Draw(CanvasDrawingSession ds)
        {
            if (geometry != null)
            ds.FillGeometry(geometry, Colors.Black);
        }
    }
}
