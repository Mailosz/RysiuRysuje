using Microsoft.Graphics.Canvas.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RysiuRysuj.Map
{
    class CollisionChecker : ICanvasPathReceiver
    {
        public delegate void CollisionEvent(Obstacle with, Vector2 point, Vector2 tangent);
        public event CollisionEvent OnCollision;

        Vector2 last = new Vector2();
        public List<Obstacle> Obstacles { get; }
        public CollisionChecker(List<Obstacle> obstacles)
        {
            Obstacles = obstacles;
        }

        public void BeginFigure(Vector2 startPoint, CanvasFigureFill figureFill)
        {
            last = startPoint;
        }

        public void AddArc(Vector2 endPoint, float radiusX, float radiusY, float rotationAngle, CanvasSweepDirection sweepDirection, CanvasArcSize arcSize)
        {
            throw new NotImplementedException();
        }

        public void AddCubicBezier(Vector2 controlPoint1, Vector2 controlPoint2, Vector2 endPoint)
        {
            throw new NotImplementedException();
        }

        public void AddLine(Vector2 endPoint)
        {
            //float mindis = float.PositiveInfinity;
            foreach (var obstacle in Obstacles)
            {
                if (obstacle.TryGetCollisionPoint(last, endPoint, out float coldis, out Vector2 point, out Vector2 tangent))
                {
                    OnCollision?.Invoke(obstacle, point, tangent / tangent.Length());
                    break;
                }
            }

            last = endPoint;
        }

        public void AddQuadraticBezier(Vector2 controlPoint, Vector2 endPoint)
        {
            throw new NotImplementedException();
        }

        public void SetFilledRegionDetermination(CanvasFilledRegionDetermination filledRegionDetermination)
        {

        }

        public void SetSegmentOptions(CanvasFigureSegmentOptions figureSegmentOptions)
        {

        }

        public void EndFigure(CanvasFigureLoop figureLoop)
        {

        }
    }
}
