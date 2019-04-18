﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reactive.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using MathNet.Numerics.LinearAlgebra.Double;
using Reactive.Bindings;

namespace HomographyVisualizer
{
    /// <summary>
    /// Rxらいくに書こうとして断念。Rx難しい。。。
    /// ↑成長したのでRxとすこし友達になった。
    /// </summary>
    public class VisualizerViewModelReactive : INotifyPropertyChanged
    {
        private Canvas _drawCanvas;

        private ReactiveProperty<bool> _drawingSrc = new ReactiveProperty<bool>(false);
        private ReactiveProperty<bool> _drawingDst = new ReactiveProperty<bool>(false);

        public ReactiveProperty<string> PointNumString { get; }

        public ReactiveProperty<bool> EnableTextBox { get; } = new ReactiveProperty<bool>(true);
        public ReactiveCommand DrawSrcAreaCommand { get; }
        public ReactiveCommand DrawDstAreaCommand { get; }
        public ReactiveCommand CreateTranslatePointCommand { get; } = new ReactiveCommand();
        public ReactiveCommand ClearCommand { get; } = new ReactiveCommand();

        private readonly List<DenseVector> _srcPoints = new List<DenseVector>();
        private readonly List<DenseVector> _dstPoints = new List<DenseVector>();

        public event PropertyChangedEventHandler PropertyChanged;

        private DenseMatrix _homo;
        private int _pointNum;

        private IObservable<MouseButtonEventArgs> _mouseDown;
        private IObservable<MouseEventArgs> _mouseMove;

        List<Line> _srcLines = new List<Line>(4);
        List<Line> _dstLines = new List<Line>(4);

        public VisualizerViewModelReactive(Canvas drawCanvas)
        {
            _drawCanvas = drawCanvas;

            DrawSrcAreaCommand = _drawingSrc.Select(x => x == false).ToReactiveCommand();
            DrawDstAreaCommand = _drawingDst.Select(x => x == false).ToReactiveCommand();

            PointNumString = new ReactiveProperty<string>("4");

            _mouseDown = Observable.FromEvent<MouseButtonEventHandler, MouseButtonEventArgs>(
                h => (s, e) => h(e),
                h => _drawCanvas.MouseDown += h,
                h => _drawCanvas.MouseDown -= h);

            _mouseMove = Observable.FromEvent<MouseEventHandler, MouseEventArgs>(
                h => (s, e) => h(e),
                h => _drawCanvas.MouseMove += h,
                h => _drawCanvas.MouseMove -= h);

            PointNumString.Subscribe(s =>
            {
                var success = int.TryParse(s?.ToString(), out var result);

                if (success && 4 <= result)
                {
                    _pointNum = result;
                    return;
                }
                PointNumString.Value = "4";
            });

            DrawSrcAreaCommand.Subscribe(() =>
            {
                _drawingSrc.Value = true;
                EnableTextBox.Value = false;
                CreateDrawingStream(_srcPoints, _srcLines, Brushes.Blue, Brushes.Aqua);
            });

            DrawDstAreaCommand.Subscribe(() =>
            {
                _drawingDst.Value = true;
                EnableTextBox.Value = false;
                CreateDrawingStream(_dstPoints, _dstLines, Brushes.Crimson, Brushes.Coral);
            });

            CreateTranslatePointCommand.Subscribe(() =>
            {
                CreateTranslatePoint();
            });

            ClearCommand.Subscribe(() =>
            {
                _drawCanvas.Children.RemoveRange(0, _drawCanvas.Children.Count);
                _drawingDst.Value = false;
                _drawingSrc.Value = false;
                _srcPoints.Clear();
                _dstPoints.Clear();
                _srcLines.Clear();
                _dstLines.Clear();
                _homo = null;
                EnableTextBox.Value = true;
            });
        }

        private Line CreateLine(double x0, double y0, double x1, double y1, double strokeThickness, Brush strokeBrush)
        {
            return new Line
            {
                X1 = x0,
                Y1 = y0,
                X2 = x1,
                Y2 = y1,
                StrokeThickness = strokeThickness,
                Stroke = strokeBrush,
            };
        }

        public void CreateDrawingStream(List<DenseVector> pointsList, List<Line> lineList, Brush pointBrush, Brush strokeBrush)
        {

            _mouseDown.Take(_pointNum)
                .Subscribe(x =>
                {
                    var point = x.GetPosition(_drawCanvas);

                    var v = DenseVector.OfArray(new double[] { point.X, point.Y });
                    pointsList.Add(v);

                    var elipse = new Ellipse
                    {
                        Width = 7,
                        Height = 7,
                        Fill = pointBrush
                    };

                    Canvas.SetLeft(elipse, point.X - elipse.Width / 2);
                    Canvas.SetTop(elipse, point.Y - elipse.Height / 2);

                    _drawCanvas.Children.Add(elipse);

                    var line = CreateLine(point.X, point.Y, point.X + 10, point.Y + 10, 3, strokeBrush);

                    lineList.Add(line);
                    _drawCanvas.Children.Add(line);
                },
                () =>
                {
                    var firstPoint = pointsList.First();
                    var lastPoint = pointsList.Last();

                    var latestLine = lineList.Last();

                    latestLine.X2 = firstPoint[0];
                    latestLine.Y2 = firstPoint[1];
                });

            //ドラッグする時の描画
            var dragStream = _mouseDown
                .SelectMany(_mouseMove).Publish();
            dragStream.Connect();

            dragStream.TakeUntil(_mouseDown)
            .Repeat(_pointNum)
            .Subscribe(x =>
            {
                if (!lineList.Any()) return;

                var point = x.GetPosition(_drawCanvas);

                var latestLine = lineList.Last();

                latestLine.X2 = point.X;
                latestLine.Y2 = point.Y;
            },
            () =>
            {
                Console.WriteLine("Complete Dracking");
            });
        }

        public void CreateTranslatePoint()
        {
            try
            {
                _homo = HomographySharp.HomographyHelper.FindHomography(_srcPoints, _dstPoints);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
                return;
            }

            var pointX = _srcPoints.Average(v => v[0]);
            var pointY = _srcPoints.Average(v => v[1]);

            Console.WriteLine(pointX);
            Console.WriteLine(pointY);

            var srcEllipse = new Ellipse
            {
                Name = "SrcTarget",
                Width = 10,
                Height = 10,
                Fill = Brushes.DarkViolet,
            };

            var dstEllipse = new Ellipse
            {
                Name = "DstTarget",
                Width = 10,
                Height = 10,
                Fill = Brushes.DarkSlateBlue,
            };

            Canvas.SetLeft(srcEllipse, pointX - srcEllipse.Width / 2);
            Canvas.SetTop(srcEllipse, pointY - srcEllipse.Height / 2);

            (var translateX, var translateY) = HomographySharp.HomographyHelper.Translate(_homo, pointX, pointY);

            Canvas.SetLeft(dstEllipse, translateX - srcEllipse.Width / 2);
            Canvas.SetTop(dstEllipse, translateY - srcEllipse.Height / 2);

            _drawCanvas.Children.Add(srcEllipse);
            _drawCanvas.Children.Add(dstEllipse);

            var ellipseDown = Observable.FromEvent<MouseButtonEventHandler, MouseButtonEventArgs>(
                h => (s, e) => h(e),
                h => srcEllipse.MouseDown += h,
                h => srcEllipse.MouseDown -= h);

            var ellipseMove = Observable.FromEvent<MouseEventHandler, MouseEventArgs>(
                h => (s, e) => h(e),
                h => srcEllipse.MouseMove += h,
                h => srcEllipse.MouseMove -= h);

            var ellipseUp = Observable.FromEvent<MouseButtonEventHandler, MouseEventArgs>(
                h => (s, e) => h(e),
                h => srcEllipse.MouseUp += h,
                h => srcEllipse.MouseUp -= h);

            ellipseDown
                .Do(_ => srcEllipse.CaptureMouse())
                .SelectMany(ellipseMove)
                .TakeUntil(ellipseUp.Do(_ => srcEllipse.ReleaseMouseCapture()))
                .Repeat()
                .Subscribe(x =>
                {
                    var newPoint = x.GetPosition(_drawCanvas);
                    Canvas.SetLeft(srcEllipse, newPoint.X - srcEllipse.Width / 2);
                    Canvas.SetTop(srcEllipse, newPoint.Y - srcEllipse.Height / 2);
                    (var newTranslateX, var newTranslateY) = HomographySharp.HomographyHelper.Translate(_homo, newPoint.X, newPoint.Y);
                    Canvas.SetLeft(dstEllipse, newTranslateX - srcEllipse.Width / 2);
                    Canvas.SetTop(dstEllipse, newTranslateY - srcEllipse.Height / 2);
                });
        }
    }
}