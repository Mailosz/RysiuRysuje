using Microsoft.Graphics.Canvas;
using Microsoft.Graphics.Canvas.Geometry;
using Microsoft.Graphics.Canvas.UI;
using Microsoft.Graphics.Canvas.UI.Xaml;
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
        bool pressed = false;

        Plane plane = new Plane();

        Vector2 lastPoint;
        public MainPage()
        {
            this.InitializeComponent();

            plane.MainActor = new Actor();
        }

        private void InputBox_KeyDown(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Enter || e.Key == VirtualKey.Execute)
            {
                if (ParseCommand(inputBox.Text, out UserCommand command))
                {
                    inputBox.Text = "";
                    historyList.Items.Add(command);
                    historyList.ScrollIntoView(command);
                    plane.Commands.Add(command);
                    PathChanged();
                }
                else
                {
                    inputBox.SelectAll();
                }
                e.Handled = true;
            }
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
                command.AppendPath(cpb, ref dir, ref pos);
            }
            cpb.EndFigure(CanvasFigureLoop.Open);
            plane.PathGeometry = CanvasGeometry.CreatePath(cpb);
            plane.MainActor.PathGeometry = plane.PathGeometry;
            plane.MainActor.PathLength = plane.PathGeometry.ComputePathLength();
            plane.MainActor.EndDir = dir;
        }

        public bool ParseCommand(string text, out UserCommand? command)
        {
            text = text.Trim();
            
            int pos = 0;
            string token;
                         
            if (TryGetToken(text, ref pos, (c) => !char.IsLetter(c), out token))
            {
                double arg1 = 0;
                switch (token.ToUpperInvariant())
                {
                    case "MF":
                        if (TryGetToken(text, ref pos, (c) => !char.IsDigit(c), out token) && double.TryParse(token, out arg1))
                        {

                        }
                        command = new MoveForward(arg1);
                        return true;
                    case "RT":
                        if (TryGetToken(text, ref pos, (c) => c != '-' && !char.IsDigit(c), out token) && double.TryParse(token, out arg1))
                        {

                        }
                        command = new RotateCommand(arg1);
                        return true;
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
                        pos += i;
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
        }

        private void ViewBox_Update(ICanvasAnimatedControl sender, CanvasAnimatedUpdateEventArgs args)
        {
            Transform = Matrix3x2.CreateRotation((float)Math.PI, centerPoint) * Matrix3x2.CreateTranslation(scroll - centerPoint);

            if (plane.MainActor != null)
            {
                plane.MainActor.Move();
            }
        }

        private void ViewBox_Draw(ICanvasAnimatedControl sender, CanvasAnimatedDrawEventArgs args)
        {
            args.DrawingSession.Transform = Transform;

            if (plane.PathGeometry != null)
            {
                args.DrawingSession.DrawGeometry(plane.PathGeometry, Colors.Blue, 3);
            }

            if (plane.MainActor != null)
            {
                args.DrawingSession.Transform = plane.MainActor.Transform * Transform;
                args.DrawingSession.FillGeometry(userGeometry, Colors.Blue);
                args.DrawingSession.DrawGeometry(userGeometry, Colors.Black, 2);
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
    }
}


