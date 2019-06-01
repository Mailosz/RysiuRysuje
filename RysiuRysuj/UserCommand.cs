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

        virtual public void AppendPath(CanvasPathBuilder cpb, Level level, ref float dir, ref Vector2 pos) { }


        public override string ToString() => command;
    }

    public class MoveForward : UserCommand
    {
        double arg;

        public MoveForward(double arg)
        {
            this.arg = arg;
        }

        public override void AppendPath(CanvasPathBuilder cpb, Level level, ref float dir, ref Vector2 pos)
        {
            pos = pos + (new Vector2(0, (float)arg)).Rotate(dir);
            cpb.AddLine(pos);
        }

        public override string ToString() => "Move forward " + arg.ToString("0.##");
    }

    public class MoveHorizontal : UserCommand
    {
        double arg;

        public MoveHorizontal(double arg)
        {
            this.arg = arg;
        }

        public override void AppendPath(CanvasPathBuilder cpb, Level level, ref float dir, ref Vector2 pos)
        {
            pos = pos + new Vector2((float)arg, 0);
            cpb.AddLine(pos);
        }

        public override string ToString() => "Move horizontal " + arg.ToString("0.##");
    }

    public class MoveVertical : UserCommand
    {
        double arg;

        public MoveVertical(double arg)
        {
            this.arg = arg;
        }

        public override void AppendPath(CanvasPathBuilder cpb, Level level, ref float dir, ref Vector2 pos)
        {
            pos = pos + new Vector2(0, (float)arg);
            cpb.AddLine(pos);
        }

        public override string ToString() => "Move vertical " + arg.ToString("0.##");
    }

    public class RotateCommand : UserCommand
    {
        double arg;

        public RotateCommand(double arg)
        {
            this.arg = -arg;
        }

        public override void AppendPath(CanvasPathBuilder cpb, Level level, ref float dir, ref Vector2 pos)
        {
            //pos = pos + (new Vector2(0, (float)arg)).Rotate(dir);
            //cpb.AddLine(pos);
            
            dir += (float)(arg * Math.PI / 180);
        }

        public override string ToString() => "Rotate by " + Math.Abs(arg).ToString("0.##") + " deg " + ((arg > 0) ? "left" : "right");
    }

    public class RepeatCommand : UserCommand
    {
        int CommandCount;
        int RepeatCount;

        public RepeatCommand(int commandCount, int repeatCount)
        {
            CommandCount = commandCount;
            RepeatCount = repeatCount;
        }

        public override void AppendPath(CanvasPathBuilder cpb, Level level, ref float dir, ref Vector2 pos)
        {
            int id = level.Commands.IndexOf(this);
            for (int i = 0; i < RepeatCount; i++)
            {
                for (int com = Math.Min(CommandCount, id); com > 0; com--)
                { 
                    level.Commands[id - com].AppendPath(cpb, level, ref dir, ref pos);
                }
            }
        }

        public override string ToString() => "Repeat " + CommandCount.ToString() + " last commands " + RepeatCount.ToString() + " times";
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
