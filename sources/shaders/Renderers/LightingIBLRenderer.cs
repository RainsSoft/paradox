﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;

using SiliconStudio.Core;
using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Effects.Modules.Processors;
using SiliconStudio.Paradox.EntityModel;
using SiliconStudio.Paradox.Graphics;

namespace SiliconStudio.Paradox.Effects.Modules.Renderers
{
    /// <summary>
    /// Computes cubemaps contribution of the ambient specular.
    /// </summary>
    public class LightingIBLRenderer : Renderer
    {
        #region Public static members

        public static readonly ParameterKey<Texture> IBLLightingTexture = ParameterKeys.New<Texture>();

        #endregion

        #region Private members

        private bool clearTarget;

        private bool externRenderTarget;

        private RenderTarget IBLRenderTarget;

        private DepthStencilBuffer readOnlyDepthBuffer;

        private GeometricPrimitive cubemapMesh;

        private Effect IBLEffect;

        private BlendState IBLBlendState;

        private DepthStencilState IBLDepthStencilState;

        private ParameterCollection parameters;

        #endregion

        #region Public properties

        /// <summary>
        /// The texture the lighting will be rendered into.
        /// </summary>
        public Texture IBLTexture
        {
            get
            {
                return IBLRenderTarget == null ? null : IBLRenderTarget.Texture;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// This renderer will compute the cubemap influence on the scene. It supposes a deferred shading/rendering pipeline. Will set a RenderTarget and a DepthStencilBuffer.
        /// </summary>
        /// <param name="services">The services.</param>
        /// <param name="depthBuffer">The depth buffer. Should be read only.</param>
        /// <param name="renderTarget">The render target. If null, a new render target will be created.</param>
        /// <param name="clearRenderTarget">A flag to enable the clear of the render target.</param>
        public LightingIBLRenderer(IServiceRegistry services, DepthStencilBuffer depthBuffer, RenderTarget renderTarget = null, bool clearRenderTarget = true) : base(services)
        {
            if (depthBuffer == null)
                throw new ArgumentNullException("depthBuffer");

            readOnlyDepthBuffer = depthBuffer;

            if (renderTarget != null)
            {
                if (renderTarget.Width != readOnlyDepthBuffer.Description.Width
                    || renderTarget.Height != readOnlyDepthBuffer.Description.Height)
                    throw new Exception("Size of readOnlyDepthBuffer and renderTarget do not match.");
                IBLRenderTarget = renderTarget;
                externRenderTarget = true;
            }

            clearTarget = clearRenderTarget;
        }

        #endregion

        #region Public methods

        /// <inheritdoc/>
        public override void Load()
        {
            // Create necessary objects
            if (IBLRenderTarget == null)
                IBLRenderTarget = Texture2D.New(GraphicsDevice, readOnlyDepthBuffer.Description.Width, readOnlyDepthBuffer.Description.Height, PixelFormat.R16G16B16A16_Float, TextureFlags.ShaderResource | TextureFlags.RenderTarget).ToRenderTarget();

            cubemapMesh = GeometricPrimitive.Cube.New(GraphicsDevice);

            var blendStateDescr = new BlendStateDescription()
            {
                RenderTargets = new[]
                {
                    new BlendStateRenderTargetDescription()
                    {
                        BlendEnable = true,
                        ColorSourceBlend = Blend.SourceAlpha,
                        ColorDestinationBlend = Blend.One,
                        ColorBlendFunction = BlendFunction.Add,
                        AlphaSourceBlend = Blend.One,
                        AlphaDestinationBlend = Blend.One,
                        AlphaBlendFunction = BlendFunction.Add,
                        ColorWriteChannels = ColorWriteChannels.All
                    }
                }
            };
            IBLBlendState = BlendState.New(GraphicsDevice, blendStateDescr);

            // depth state to test z-fail of backfaces
            IBLDepthStencilState = DepthStencilState.New(GraphicsDevice, new DepthStencilStateDescription(true, false)
                {
                    StencilEnable = false,
                    DepthBufferFunction = CompareFunction.GreaterEqual,
                });

            // effect
            IBLEffect = EffectSystem.LoadEffect("CubemapIBLSpecular");

            parameters = new ParameterCollection();
            parameters.Set(RenderTargetKeys.DepthStencilSource, readOnlyDepthBuffer.Texture);

            // Add to pipeline
            Pass.StartPass += RenderIBL;
        }

        /// <inheritdoc/>
        public override void Unload()
        {
            // Remove from pipeline
            Pass.StartPass -= RenderIBL;

            parameters.Clear();

            IBLEffect.Dispose();
            IBLDepthStencilState.Dispose();
            IBLBlendState.Dispose();
            cubemapMesh.Dispose();
            if (!externRenderTarget)
                IBLRenderTarget.Dispose();
        }

        #endregion

        #region Private methods

        private void RenderIBL(RenderContext context)
        {
            var entitySystem = Services.GetServiceAs<EntitySystem>();
            var cubemapSourceProcessor = entitySystem.GetProcessor<CubemapSourceProcessor>();
            if (cubemapSourceProcessor == null)
                return;

            // clear render target
            if (clearTarget)
                GraphicsDevice.Clear(IBLRenderTarget, new Color4(0, 0, 0, 0));

            // if no cubemap, exit
            if (cubemapSourceProcessor.Cubemaps.Count <= 0)
                return;

            // set render target
            GraphicsDevice.SetRenderTarget(readOnlyDepthBuffer, IBLRenderTarget);
            
            // set depth state
            GraphicsDevice.SetDepthStencilState(IBLDepthStencilState);

            // set culling
            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.CullFront);

            // set blend state
            GraphicsDevice.SetBlendState(IBLBlendState);

            foreach (var cubemap in cubemapSourceProcessor.Cubemaps)
            {
                // set world matrix matrices
                // TODO: rotation of cubemap & cube mesh
                parameters.Set(TransformationKeys.World, ComputeTransformationMatrix(cubemap.Value.InfluenceRadius, cubemap.Key.Transformation.Translation));
                parameters.Set(CubemapIBLBaseKeys.CubemapRadius, cubemap.Value.InfluenceRadius);
                parameters.Set(CubemapIBLBaseKeys.Cubemap, cubemap.Value.Texture);
                parameters.Set(CubemapIBLBaseKeys.CubemapPosition, cubemap.Key.Transformation.Translation);

                // apply effect
                IBLEffect.Apply(parameters, context.CurrentPass.Parameters);

                // render cubemap
                cubemapMesh.Draw(GraphicsDevice);
            }
            
            IBLEffect.UnbindResources();

            // TODO: revert to correct old settings
            GraphicsDevice.SetDepthStencilState(GraphicsDevice.DepthStencilStates.None);
            GraphicsDevice.SetRasterizerState(GraphicsDevice.RasterizerStates.CullBack);
            GraphicsDevice.SetBlendState(GraphicsDevice.BlendStates.Default);
        }

        #endregion

        #region Helpers

        private static Matrix ComputeTransformationMatrix(float size, Vector3 position)
        {
            // x2 because the size is a radius
            return Matrix.Scaling(2 * size) * Matrix.Translation(position);
        }

        #endregion
    }
}