// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;

namespace SiliconStudio.Paradox.Rendering.Lights
{
    /// <summary>
    /// Size hint of a shadow map. See remarks.
    /// </summary>
    /// <remarks>This is a hint to determine the size of a shadow map</remarks>
    [DataContract("LightShadowMapSize")]
    public enum LightShadowMapSize
    {
        /// <summary>
        /// Use a xtra-small size.
        /// </summary>
        /// <userodc>A smaller shadow map (1/8 of the reference size)</userodc>
        [Display("/ 8")]
        XSmall = 0, // NOTE: Number are used to compute the size, do not change them

        /// <summary>
        /// Use a small size.
        /// </summary>
        /// <userodc>A small shadow map (1/4 of the reference size)</userodc>
        [Display("/ 4")]
        Small = 1, // NOTE: Number are used to compute the size, do not change them

        /// <summary>
        /// Use a medium size.
        /// </summary>
        /// <userodc>A medium shadow map (1/2 of the reference size)</userodc>
        [Display("/ 2")]
        Medium = 2,
            
        /// <summary>
        /// Use a large size.
        /// </summary>
        /// <userodc>A large shadow map(x 1 of the reference size)</userodc>
        [Display("x 1")]
        Large = 3,

        /// <summary>
        /// Use a xtra-large size.
        /// </summary>
        /// <userodc>A larger shadow map (x 2 of the reference size)</userodc>
        [Display("x 2")]
        XLarge = 4,
    }
}