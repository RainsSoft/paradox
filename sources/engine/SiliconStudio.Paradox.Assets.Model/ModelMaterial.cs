// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using SiliconStudio.Assets.Diff;
using SiliconStudio.Core;
using SiliconStudio.Paradox.Rendering;

namespace SiliconStudio.Paradox.Assets.Model
{
    /// <summary>
    /// Represents a <see cref="MaterialInstance"/> in a 
    /// </summary>
    [DataContract]
    public class ModelMaterial : IDiffKey
    {
        /// <summary>
        /// Gets or sets the material slot name in a <see cref="ModelAsset"/>.
        /// </summary>
        /// <value>
        /// The material slot name.
        /// </value>
        /// <userdoc>The .</userdoc>
        /// <userdoc>The name of the material as written in the imported model and the reference to the corresponding material asset.</userdoc>
        [DataMember(10), DiffMember(Diff3ChangeType.MergeFromAsset2)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the material stored in this slot.
        /// </summary>
        /// <value>
        /// The material.
        /// </value>
        [DataMember(20)]
        public MaterialInstance MaterialInstance { get; set; }

        /// <summary>
        /// Gets the difference key.
        /// </summary>
        /// <returns></returns>
        object IDiffKey.GetDiffKey()
        {
            return Name;
        }
    }
}