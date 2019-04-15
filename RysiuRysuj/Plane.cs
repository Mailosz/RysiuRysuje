using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RysiuRysuj
{
    class Plane
    {
        public List<UserCommand> Commands = new List<UserCommand>();
        public float StartDir = 0;
        public Vector2 StartPoint;

        public Actor MainActor;


        public CanvasGeometry PathGeometry;

    }

    class Actor
    {
        public Vector2 Position;
        public float Direction = (float)Math.PI/2;

        float Speed;
        public CanvasGeometry PathGeometry;
        public float PathLength;
        public float PointOnPath;
        public float EndDir = (float)Math.PI / 2;

        public Matrix3x2 Transform = Matrix3x2.Identity;

        public void Move()
        {
            if (PointOnPath < PathLength && PathGeometry != null)
            {
                Speed = (float)Math.Min(Speed + 0.1, (PathLength - PointOnPath) / 15 + 0.1);
                //move
                PointOnPath = Math.Min(PointOnPath + Speed, PathLength);

                //set variables
                Position = PathGeometry.ComputePointOnPath(PointOnPath, out Vector2 tangent);
                Direction = (float)(Math.Atan2(tangent.Y, tangent.X) - Math.PI / 2);
                Transform = Matrix3x2.CreateTranslation(Position) * Matrix3x2.CreateRotation(Direction, Position);
            }
            else
            {
                if (Math.Abs(Direction - EndDir) > float.Epsilon)
                {
                    Direction = Math.Max(Math.Min(Direction + 0.05f, EndDir), Direction - 0.05f);

                    Transform = Matrix3x2.CreateTranslation(Position) * Matrix3x2.CreateRotation(Direction, Position);
                }
            }
        }

    }
}
