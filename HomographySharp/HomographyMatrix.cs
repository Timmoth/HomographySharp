﻿using System;
using System.Collections.Generic;

namespace HomographySharp
{
    public abstract class HomographyMatrix<T> where T : unmanaged, IEquatable<T>
    {
        public abstract Point2<T> Translate(T srcX, T srcY);

        public abstract IReadOnlyList<T> Elements { get; }

        public abstract ReadOnlySpan<T> ElementsAsSpan();

        internal HomographyMatrix()
        {
        }
    }
}