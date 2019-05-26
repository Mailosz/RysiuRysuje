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
    class Finish
    {
        public float Radius;
        public float RotationSpeed = 0.01f;
        public float Rotate = 0;
        public Vector2 Center;
        public Finish(Vector2 center, float r)
        {
            Radius = r;
            Center = center;
        }

        public void Update(Level plane)
        {
            Rotate += RotationSpeed;
            if (Rotate > Math.PI * 2) { Rotate -= (float)Math.PI * 2; }
            else if (Rotate < -Math.PI * 2) { Rotate += (float)Math.PI * 2; }
        }

        public void Draw(CanvasDrawingSession ds)
        {
            var temp = ds.Transform;

            ds.Transform = Matrix3x2.CreateRotation(Rotate, Center) * ds.Transform;

            ds.DrawCircle(Center, Radius, Colors.LimeGreen, 5, new CanvasStrokeStyle() { CustomDashStyle = new[] {2f,2f } });

            ds.Transform = temp;
        }
    }
}
