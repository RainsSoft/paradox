﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Collections.Generic;

namespace SiliconStudio.Paradox.Rendering.Images
{
    /// <summary>
    /// Keys used by <see cref="ToneMap"/> and ToneMapEffect pdxfx
    /// </summary>
    internal static class ColorTransformGroupKeys
    {
        public static readonly ParameterKey<List<ColorTransform>> Transforms = ParameterKeys.New<List<ColorTransform>>();
    }
}