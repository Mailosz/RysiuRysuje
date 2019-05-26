using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using RysiuRysuj.Map;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;

namespace RysiuRysuj
{
    class Level
    {
        public List<UserCommand> Commands = new List<UserCommand>();
        public float StartDir = 0;
        public Vector2 StartPoint;

        public Finish Finish;

        public Actor MainActor;
        public Rect MapBounds = new Rect(-1000, -1000, 2000, 2000);
        public List<Obstacle> Obstacles = new List<Obstacle>();

        public CanvasGeometry PathGeometry;

        public void CreateResources(ICanvasResourceCreator rc)
        {

            foreach (var obstacle in Obstacles)
            {
                obstacle.CreateResources(rc);
            }
        }

    }

    class Actor
    {
        public Vector2 Position;
        public float Direction = (float)Math.PI/2;

        float Speed;
        public float PathLength;
        public float PointOnPath;
        public float EndDir = (float)Math.PI / 2;
        public bool Go = true;

        public Matrix3x2 Transform = Matrix3x2.Identity;

        public Actor(Vector2 pos)
        {
            Transform = Matrix3x2.CreateTranslation(pos);
            Position = pos;
        }

        public void UpdateTransform()
        {
            Transform = Matrix3x2.CreateTranslation(Position) * Matrix3x2.CreateRotation(Direction, Position);
        }

        public void Move(Level level)
        {
            if (PointOnPath < PathLength && level.PathGeometry != null)
            {
                if (Go)
                {
                    Speed = (float)Math.Min(Speed + 0.1, (PathLength - PointOnPath) / 15 + 0.1);
                }
                else
                {
                    Speed = (float)Math.Max(Speed - 0.1 - Speed / 15, 0);
                }
                //move
                PointOnPath = Math.Min(PointOnPath + Speed, PathLength);

                //set variables
                Position = level.PathGeometry.ComputePointOnPath(PointOnPath, out Vector2 tangent);
                Direction = (float)(Math.Atan2(tangent.Y, tangent.X) - Math.PI / 2);
                UpdateTransform();
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
