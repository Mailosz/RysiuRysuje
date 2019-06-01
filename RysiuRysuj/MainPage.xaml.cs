using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
using RysiuRysuj.Map;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Microsoft.Graphics.Canvas.Text;
using System.Drawing;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RysiuRysuj
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        CanvasGeometry userGeometry;
        Vector2 scroll = new Vector2();
        Vector2 centerPoint = new Vector2();
        Matrix3x2 Transform;
        float zoom = 1;
        float minzoom = 0.5f;
        float maxzoom = 2f;
        bool pressed = false;
        bool cheatline = true;
        List<Vector2> collisionPoints = new List<Vector2>();

        Level plane = new Level();
        int levelCounter = 1;
        int executedCommandsCount = 0;

        Vector2 lastPoint;
        public MainPage()
        {
            this.InitializeComponent();


            plane = GenerateLevel();
            UpdateGUI();
        }


        private void InputBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter || e.Key == VirtualKey.Execute)
            {
                if (ParseCommand(inputBox.Text, out UserCommand command, ref executedCommandsCount))
                {
                    inputBox.Text = "";

                    ChangePath(command);
                }
                else
                {
                    inputBox.SelectAll();
                }
                e.Handled = true;
            }
        }

        private void ChangePath(UserCommand command)
        {
            historyList.Items.Add(command);
            historyList.ScrollIntoView(command);
            plane.Commands.Add(command);

            PathChanged();
            UpdateGUI();
        }

        private void CheckCollisions(CanvasGeometry geometry)
        {
            geometry = geometry.Simplify(CanvasGeometrySimplification.Lines);

            CollisionChecker cc = new CollisionChecker(plane.Obstacles);

            List<Vector2> cp = new List<Vector2>(100);
            cc.OnCollision += (with, p, t) =>
            {
                cp.Add(p);

                if (with is Wall)
                {
                    CollisionWithWall();
                }
            };
            geometry.SendPathTo(cc);
            collisionPoints = cp;
        }

        private void CollisionWithWall()
        {
            var t = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    LostDialog lfd = new LostDialog();

                    lfd.PrimaryButtonClick += (s, e) => { RetryLevel(); };
                    lfd.SecondaryButtonClick += (s, e) => { GimmieNewLevel(); };

                    await lfd.ShowAsync();
                }
                catch
                {

                }
            });

        }

        private void LevelFinished()
        {

            var t = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, async () =>
            {
                try
                {
                    LevelFinishedDialog lfd = new LevelFinishedDialog();

                    lfd.PrimaryButtonClick += (s, e) => { RetryLevel(); };
                    lfd.SecondaryButtonClick += (s, e) => { levelCounter++; GimmieNewLevel(); };

                    await lfd.ShowAsync();
                }
                catch
                {

                }
            });

        }

        private void RetryLevel()
        {
            UpdateGUI();
            plane.PathGeometry = null;

            plane.MainActor.PointOnPath = 0f;
            plane.MainActor.Position = plane.StartPoint;
            plane.MainActor.CurrentDirection = plane.StartDir;
            plane.MainActor.Direction = plane.StartDir;
            plane.MainActor.Go = true;
            plane.MainActor.UpdateTransform();

            plane.Commands.Clear();
            historyList.Items.Clear();

            collisionPoints = new List<Vector2>();
        }

        private void GimmieNewLevel()
        {
            historyList.Items.Clear();
            collisionPoints = new List<Vector2>();
            Level nl = GenerateLevel();

            nl.CreateResources(viewBox);
            scroll = Vector2.Zero; //zeroing scroll

            plane = nl;
            UpdateGUI();
        }

        private void PathChanged()
        {
            CanvasPathBuilder cpb = new CanvasPathBuilder(viewBox);
            float dir = plane.StartDir;
            Vector2 pos = plane.StartPoint;
            Vector2 tangent;
            cpb.BeginFigure(pos);
            foreach (var command in plane.Commands)
            {
                command.AppendPath(cpb, plane, ref dir, ref pos);
            }
            cpb.EndFigure(CanvasFigureLoop.Open);
            plane.PathGeometry = CanvasGeometry.CreatePath(cpb);
            plane.MainActor.PathLength = plane.PathGeometry.ComputePathLength();
            plane.MainActor.Direction = dir;

            CheckCollisions(plane.PathGeometry);
        }

public bool ParseCommand(string text, out UserCommand command, ref int countCommand)
{
    text = text.Trim();
            
    int pos = 0;
    string token;
    command = null;
                         
    if (TryGetToken(text, ref pos, (c) => !char.IsLetter(c), out token))
    {
        double arg1 = 0;
        switch (token.ToUpperInvariant())
        {
            case "MH":
                if (TryGetToken(text, ref pos, (c) => !char.IsDigit(c), out token) && double.TryParse(token, out arg1))
                {
                    command = new MoveHorizontal(arg1);
                    countCommand++;
                    return true;
                }
                else return false;
            case "MV":
                if (TryGetToken(text, ref pos, (c) => !char.IsDigit(c), out token) && double.TryParse(token, out arg1))
                {
                    command = new MoveVertical(arg1);
                    countCommand++;
                    return true;
                }
                else return false;
            case "MF":
                if (TryGetToken(text, ref pos, (c) => !char.IsDigit(c), out token) && double.TryParse(token, out arg1))
                {
                    command = new MoveForward(arg1);
                    countCommand++;
                    return true;
                }
                else return false;
            case "RT":
                if (TryGetToken(text, ref pos, (c) => c != '-' && !char.IsDigit(c), out token) && double.TryParse(token, out arg1))
                {
                    command = new RotateCommand(arg1);
                    countCommand++;
                    return true;
                }
                else return false;
            case "REPEAT":
                if (TryGetToken(text, ref pos, (c) => !char.IsDigit(c), out token) && int.TryParse(token, out int comnum)
                    && TryGetToken(text, ref pos, (c) => !char.IsDigit(c), out token) && int.TryParse(token, out int ile))
                {
                    command = new RepeatCommand(comnum, ile);
                    countCommand++;
                    return true;
                }
                else return false;
        }
    }

    command = null;
    return false; 
}

bool TryGetToken(string text, ref int pos, Func<char, bool> until, out string token)
{
    for (int i = pos; i < text.Length; i++)
    {
        if (until(text[i]))
        {
            if (i - pos > 0)
            {
                token = text.Substring(pos, i - pos);
                pos = i;
                return true;
            }
        }
    }
    if (pos < text.Length)
    {
        token = text.Substring(pos);
        pos = text.Length;
        return true;
    }
    token = null;
    return false;
}

        private void ViewBox_CreateResources(CanvasAnimatedControl sender, CanvasCreateResourcesEventArgs args)
        {
            userGeometry = CanvasGeometry.CreatePolygon(sender, new[] { new Vector2(-10,-10), new Vector2(10,-10), new Vector2(0, 15), });

            plane?.CreateResources(sender);
        }

        private void ViewBox_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            Transform = Matrix3x2.CreateScale(1,-1, centerPoint) * Matrix3x2.CreateTranslation(scroll + centerPoint * new Vector2(1,-1)) * Matrix3x2.CreateScale(zoom);

            if (plane.MainActor != null)
            {
                plane.MainActor.Move(plane);
            }

            if (plane.Finish != null)
            {
                plane.Finish.Update(plane);
                if (plane.MainActor.Go && Vector2.Distance(plane.MainActor.Position, plane.Finish.Center) <= plane.Finish.Radius)
                {
                    plane.MainActor.Go = false;
                    LevelFinished();
                }
            }
        }

        private void ViewBox_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            if (plane == null) return;
            args.DrawingSession.Transform = Transform;

            foreach (var obstacle in plane.Obstacles)
            {
                obstacle.Draw(args.DrawingSession);
            }

            args.DrawingSession.FillCircle(plane.StartPoint, 6, Colors.CornflowerBlue);

            if (plane.PathGeometry != null)
            {
                args.DrawingSession.DrawGeometry(plane.PathGeometry, Colors.CornflowerBlue, 4);
            }

            if (plane.MainActor != null)
            {
                args.DrawingSession.Transform = plane.MainActor.Transform * Transform;
                args.DrawingSession.FillGeometry(userGeometry, Colors.Blue);
                args.DrawingSession.DrawGeometry(userGeometry, Colors.Black, 2);
            }

            args.DrawingSession.Transform = Transform;

            if (plane.Finish != null)  //drawing finish if it exists
            {
                plane.Finish.Draw(args.DrawingSession);
            }

            foreach (var p in collisionPoints)
            {
                args.DrawingSession.FillCircle(p, 5, Colors.Red);
            }

            if (cheatline)
            {
                args.DrawingSession.DrawLine(plane.MainActor.Position, plane.MainActor.Position + new Vector2(1000,0) * new Vector2((float)Math.Sin(plane.MainActor.Direction), (float)Math.Cos(plane.MainActor.Direction)), Colors.Lime);
            }
        }

        private void ViewBox_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            centerPoint = e.NewSize.ToVector2() / 2;
        }

        private void ViewBox_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            lastPoint = e.GetCurrentPoint(viewBox).Position.ToVector2();
            viewBox.CapturePointer(e.Pointer);
            pressed = true;
        }

        private void ViewBox_PointerMoved(object sender, PointerRoutedEventArgs e)
        {
            if (pressed)
            {
                var point = e.GetCurrentPoint(viewBox).Position.ToVector2();

                scroll += point - lastPoint;

                lastPoint = point;

                if (!e.Pointer.IsInContact)
                {
                    pressed = false;
                }
            }
        }

        private void ViewBox_PointerCaptureLost(object sender, PointerRoutedEventArgs e)
        {
            pressed = false;
        }

        private Level GenerateLevel()
        {
            Random random = new Random();
            Level nl = new Level();

            Rectangle bounds = new Rectangle(-500, -300, 1000, 600);
            float margin = 20;

            nl.StartPoint = new Vector2((float)bounds.X + margin + random.Next((int)(bounds.Width - margin * 2)),
                (float)bounds.Y + margin + random.Next((int)(bounds.Height - margin * 2)));


            nl.Obstacles.Clear();

            // Generate finish
            var finishPosition = new Vector2();

            for (int i = 0; i < 10; i++)
            {
                finishPosition = new Vector2((float)bounds.X + margin + random.Next((int)(bounds.Width - margin * 2)),
                (float)bounds.Y + margin + random.Next((int)(bounds.Height - margin * 2)));

                if (Vector2.Distance(nl.StartPoint, finishPosition) > 500) break;
            }

            nl.StartDir = (float)Math.Atan2((nl.StartPoint - finishPosition).X, -(nl.StartPoint - finishPosition).Y);

            nl.Finish = new Finish(finishPosition, 25);

            nl.MainActor = new Actor(nl.StartPoint);
            nl.MainActor.CurrentDirection = nl.StartDir;
            nl.MainActor.Direction = nl.StartDir;
            nl.MainActor.UpdateTransform();

            // Generate random polygons
            for (int i = 0; i < 10 + levelCounter * 5; i++)
            {
                var position = Vector2.Zero;
                while (true)
                {
                    position = new Vector2(random.Next(bounds.Left, bounds.Right), random.Next(bounds.Top, bounds.Bottom));
                    if (Vector2.Distance(position, finishPosition) < 70 || Vector2.Distance(position, nl.StartPoint) < 70) continue;
                    break;
                }

                var leftBottom = new Vector2(random.Next(-50, 0), random.Next(-50, 0)) + position;
                var rightBottom = new Vector2(random.Next(0, 50), random.Next(-50, 0)) + position;
                var rightTop = new Vector2(random.Next(0, 50), random.Next(0, 50)) + position;
                var leftTop = new Vector2(random.Next(-50, 0), random.Next(0, 50)) + position;

                nl.Obstacles.Add(new Wall(new List<Vector2>() { leftBottom, rightBottom, rightTop, leftTop }));
            }

            // Generate outer walls
            float wallsize = 20;
            nl.Obstacles.Add(new Wall(new List<Vector2> { new Vector2(bounds.X, bounds.Y - wallsize), new Vector2(bounds.X, bounds.Bottom + wallsize), new Vector2(bounds.X - wallsize, bounds.Bottom + wallsize), new Vector2(bounds.X - wallsize, bounds.Y - wallsize) }));
            nl.Obstacles.Add(new Wall(new List<Vector2> { new Vector2(bounds.X + bounds.Width, bounds.Y - wallsize), new Vector2(bounds.X + bounds.Width, bounds.Bottom + wallsize), new Vector2(bounds.Right + wallsize, bounds.Bottom + wallsize), new Vector2(bounds.Right + wallsize, bounds.Y - wallsize) }));

            nl.Obstacles.Add(new Wall(new List<Vector2> { new Vector2(bounds.X - wallsize, bounds.Y), new Vector2(bounds.Right + wallsize, bounds.Y), new Vector2(bounds.Right + wallsize, bounds.Y - wallsize), new Vector2(bounds.X - wallsize, bounds.Y - wallsize) }));
            nl.Obstacles.Add(new Wall(new List<Vector2> { new Vector2(bounds.X - wallsize, bounds.Bottom), new Vector2(bounds.Right + wallsize, bounds.Bottom), new Vector2(bounds.Right + wallsize, bounds.Bottom + wallsize), new Vector2(bounds.X - wallsize, bounds.Bottom + wallsize) }));

            //nl.Obstacles.Add(new Wall(new List<Vector2>() { new Vector2(-550, -300), new Vector2(-530, -300), new Vector2(-530, 300), new Vector2(-550, 300) }));
            //nl.Obstacles.Add(new Wall(new List<Vector2>() { new Vector2( 550, -300), new Vector2( 530, -300), new Vector2( 530, 300), new Vector2( 550, 300) }));
            //nl.Obstacles.Add(new Wall(new List<Vector2>() { new Vector2( 550, -300), new Vector2(-550, -300), new Vector2(-550,-280), new Vector2( 550, -280) }));
            //nl.Obstacles.Add(new Wall(new List<Vector2>() { new Vector2( 550,  300), new Vector2(-550,  300), new Vector2(-550, 280), new Vector2( 550,  280) }));

            return nl;
        }



        private void UpdateGUI()
        {
            ExecutedCommandsLabel.Text = "Wykonanych komend: " + executedCommandsCount;
            LevelLabel.Text = "Poziom: " + levelCounter;
        }

        private void ViewBox_ManipulationDelta(object sender, ManipulationDeltaRoutedEventArgs e)
        {
            scroll += e.Delta.Translation.ToVector2() / zoom;

            zoom = (float)Math.Min(Math.Max(zoom * e.Delta.Scale, minzoom), maxzoom);
        }

        private void ZoomInButton_Click(object sender, RoutedEventArgs e)
        {
            zoom = (float)Math.Min(zoom * 1.1, maxzoom);
        }

        private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
        {
            zoom = (float)Math.Max(zoom * 0.9, minzoom);
        }
    }
}


