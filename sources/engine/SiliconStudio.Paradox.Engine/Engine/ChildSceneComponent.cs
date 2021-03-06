// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using SiliconStudio.Core;
using SiliconStudio.Paradox.Engine.Design;
using SiliconStudio.Paradox.Engine.Processors;

namespace SiliconStudio.Paradox.Engine
{
    /// <summary>
    /// A link to a scene that is rendered by a parent <see cref="Scene"/>.
    /// </summary>
    [DataContract("ChildSceneComponent")]
    [Display(112, "Child scene", Expand = ExpandRule.Once)]
    [DefaultEntityComponentProcessor(typeof(ChildSceneProcessor))]
    public sealed class ChildSceneComponent : ActivableEntityComponent
    {
        private Scene scene;

        // Used by the ChildSceneProcessor
        [DataMemberIgnore]
        internal SceneInstance SceneInstance;
        
        public readonly static PropertyKey<ChildSceneComponent> Key = new PropertyKey<ChildSceneComponent>("Key", typeof(ChildSceneComponent));

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildSceneComponent"/> class.
        /// </summary>
        public ChildSceneComponent()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ChildSceneComponent"/> class.
        /// </summary>
        /// <param name="scene">The scene.</param>
        public ChildSceneComponent(Scene scene)
        {
            Scene = scene;
        }

        /// <summary>
        /// Gets or sets the child scene.
        /// </summary>
        /// <value>The scene.</value>
        /// <userdoc>The reference to the scene to render. Any scene can be selected except the containing one.</userdoc>
        [DataMember(10)]
        public Scene Scene
        {
            get { return scene; }
            set
            {
                scene = value;
                if (SceneInstance != null)
                    SceneInstance.Scene = null; // unload the current scene, so that it can be unloaded from memory directly (without having to wait one frame)
            }
        }

        public override PropertyKey GetDefaultKey()
        {
            return Key;
        }
    }
}