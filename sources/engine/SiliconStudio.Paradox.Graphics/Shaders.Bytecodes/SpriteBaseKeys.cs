﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Rendering
{
    public partial class SpriteBaseKeys
    {
        static SpriteBaseKeys()
        {
            MatrixTransform = ParameterKeys.New(Matrix.Identity);
        }

        public static readonly ParameterKey<bool> ColorIsSRgb = ParameterKeys.New(false);
    }
}