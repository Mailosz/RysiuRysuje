using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RysiuRysuj
{
    public abstract class UserCommand
    {
        string command;
        public UserCommand()
        {
        }

        virtual public void AppendPath(CanvasPathBuilder cpb, ref float dir, ref Vector2 pos) { }


        public override string ToString() => command;
    }

    public class MoveForward : UserCommand
    {
        double arg;

        public MoveForward(double arg)
        {
            this.arg = arg;
        }

        public override void AppendPath(CanvasPathBuilder cpb, ref float dir, ref Vector2 pos)
        {
            pos = pos + (new Vector2(0, (float)arg)).Rotate(dir);
            cpb.AddLine(pos);
        }

        public override string ToString() => "Move forward " + arg.ToString("0.##");
    }

    public class RotateCommand : UserCommand
    {
        double arg;

        public RotateCommand(double arg)
        {
            this.arg = arg;
        }

        public override void AppendPath(CanvasPathBuilder cpb, ref float dir, ref Vector2 pos)
        {
            //pos = pos + (new Vector2(0, (float)arg)).Rotate(dir);
            //cpb.AddLine(pos);
            
            dir += (float)(arg * Math.PI / 180);
        }

        public override string ToString() => "Rotate by " + arg.ToString("0.##") + " deg " + ((arg > 0) ? "right" : "left");
    }
}

public static class VectorHelper
{
    public static Vector2 Rotate(this Vector2 vector, double angle)
    {
        float cos = (float)Math.Cos(angle);
        float sin = (float)Math.Sin(angle);
        return new Vector2(vector.X * cos - vector.Y * sin, vector.Y * cos + vector.X * sin);
    }

    public static Vector2 Rotate(this Vector2 vector, double angle, Vector2 around)
    {
        float cos = (float)Math.Cos(angle);
        float sin = (float)Math.Sin(angle);
        return new Vector2(around.X + (vector.X - around.X) * cos - (vector.Y - around.Y) * sin, around.Y + (vector.Y - around.Y) * cos + (vector.X - around.X) * sin);
    }
}
