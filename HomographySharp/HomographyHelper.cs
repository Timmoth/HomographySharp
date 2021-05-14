﻿using System;
using System.Numerics;
using System.Collections.Generic;
using HomographySharp.Double;
using HomographySharp.Single;

namespace HomographySharp
{
    public static class HomographyHelper
    {
        public static HomographyMatrix<float> Find(IReadOnlyList<Point2<float>> srcPoints, IReadOnlyList<Point2<float>> dstPoints)
            => SingleHomographyHelper.Find(srcPoints, dstPoints);

        public static HomographyMatrix<float> Find(ReadOnlySpan<Point2<float>> srcPoints, ReadOnlySpan<Point2<float>> dstPoints)
            => SingleHomographyHelper.Find(srcPoints, dstPoints);

        public static HomographyMatrix<float> Find(Point2<float>[] srcPoints, Point2<float>[] dstPoints)
            => SingleHomographyHelper.Find(new ReadOnlySpan<Point2<float>>(srcPoints), new ReadOnlySpan<Point2<float>>(dstPoints));

        public static HomographyMatrix<float> Find(IReadOnlyList<Vector2> srcPoints, IReadOnlyList<Vector2> dstPoints)
            => SingleHomographyHelper.Find(srcPoints, dstPoints);

        public static HomographyMatrix<float> Find(ReadOnlySpan<Vector2> srcPoints, ReadOnlySpan<Vector2> dstPoints)
            => SingleHomographyHelper.Find(srcPoints, dstPoints);

        public static HomographyMatrix<float> Find(Vector2[] srcPoints, Vector2[] dstPoints)
            => SingleHomographyHelper.Find(new ReadOnlySpan<Vector2>(srcPoints), new ReadOnlySpan<Vector2>(dstPoints));

        public static HomographyMatrix<float> Create(ReadOnlySpan<float> elements)
        {
            if (elements.Length == 9)
            {
                return new SingleHomographyMatrix(elements.ToArray());
            }

            throw new ArgumentException("elements.Length must be 9.");
        }

        public static HomographyMatrix<double> Find(IReadOnlyList<Point2<double>> srcPoints, IReadOnlyList<Point2<double>> dstPoints)
            => DoubleHomographyHelper.Find(srcPoints, dstPoints);

        public static HomographyMatrix<double> Find(ReadOnlySpan<Point2<double>> srcPoints, ReadOnlySpan<Point2<double>> dstPoints)
            => DoubleHomographyHelper.Find(srcPoints, dstPoints);

        public static HomographyMatrix<double> Find(Point2<double>[] srcPoints, Point2<double>[] dstPoints)
            => DoubleHomographyHelper.Find(new ReadOnlySpan<Point2<double>>(srcPoints), new ReadOnlySpan<Point2<double>>(dstPoints));

        public static HomographyMatrix<double> Create(ReadOnlySpan<double> elements)
        {
            if (elements.Length == 9)
            {
                return new DoubleHomographyMatrix(elements.ToArray());
            }

            throw new ArgumentException("elements.Length must be 9.");
        }
    }
}