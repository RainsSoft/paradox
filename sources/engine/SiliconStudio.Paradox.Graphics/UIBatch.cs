﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using SiliconStudio.Core.Mathematics;

namespace SiliconStudio.Paradox.Graphics
{
    /// <summary>
    /// A utility class to batch and draw UI images.
    /// </summary>
    public class UIBatch : BatchBase<UIBatch.UIImageDrawInfo>
    {
        private static readonly List<short[]> PrimiteTypeToIndices = new List<short[]>(4);

        private const int MaxVerticesPerElement = 16;
        private const int MaxIndicesPerElement = 54;
        private const int MaxElementBatchNumber = 2048;
        private const int MaxVerticesCount = MaxVerticesPerElement * MaxElementBatchNumber;
        private const int MaxIndicesCount = MaxIndicesPerElement * MaxElementBatchNumber;

        /// <summary>
        /// The view projection matrix that will be used for the current begin/end draw calls.
        /// </summary>
        private Matrix viewProjectionMatrix;

        private Vector4 vector4LeftTop = new Vector4(-0.5f, -0.5f, -0.5f, 1);

        private readonly Vector4[] shiftVectorX = new Vector4[4];
        private readonly Vector4[] shiftVectorY = new Vector4[4];

        private readonly Texture whiteTexture;

        static UIBatch()
        {
            PrimiteTypeToIndices.Add(new short[6]);  // rectangle
            PrimiteTypeToIndices.Add(new short[54]); // border rectangle
            PrimiteTypeToIndices.Add(new short[36]); // cube
            PrimiteTypeToIndices.Add(new short[36]); // reverse cube

            // rectangle
            var indices = PrimiteTypeToIndices[(int)PrimitiveType.Rectangle];
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 1;
            indices[4] = 3;
            indices[5] = 2;

            // border rectangle
            indices = PrimiteTypeToIndices[(int)PrimitiveType.BorderRectangle];
            var count = 0;
            for (var j = 0; j < 3; ++j)
            {
                for (var l = 0; l < 3; ++l)
                {
                    indices[count++] = (short)((j << 2) + l + 0);
                    indices[count++] = (short)((j << 2) + l + 1);
                    indices[count++] = (short)((j << 2) + l + 4);

                    indices[count++] = (short)((j << 2) + l + 1);
                    indices[count++] = (short)((j << 2) + l + 5);
                    indices[count++] = (short)((j << 2) + l + 4);
                }
            }

            // cube
            count = 0;
            indices = PrimiteTypeToIndices[(int)PrimitiveType.Cube];
            indices[count++] = 0; //front
            indices[count++] = 1;
            indices[count++] = 2;
            indices[count++] = 1;
            indices[count++] = 3;
            indices[count++] = 2;

            indices[count++] = 1; // right
            indices[count++] = 5;
            indices[count++] = 7;
            indices[count++] = 1;
            indices[count++] = 7;
            indices[count++] = 3;

            indices[count++] = 5; // back
            indices[count++] = 4;
            indices[count++] = 6;
            indices[count++] = 5;
            indices[count++] = 6;
            indices[count++] = 7;

            indices[count++] = 4; // left
            indices[count++] = 0;
            indices[count++] = 2;
            indices[count++] = 4;
            indices[count++] = 2;
            indices[count++] = 6;

            indices[count++] = 1; // top 
            indices[count++] = 0;
            indices[count++] = 4;
            indices[count++] = 1;
            indices[count++] = 4;
            indices[count++] = 5;

            indices[count++] = 2; // bottom 
            indices[count++] = 3;
            indices[count++] = 6;
            indices[count++] = 3;
            indices[count++] = 7;
            indices[count  ] = 6;

            // reverse cube
            var cubeIndices = PrimiteTypeToIndices[(int)PrimitiveType.Cube];
            indices = PrimiteTypeToIndices[(int)PrimitiveType.ReverseCube];
            for( var i=0; i<cubeIndices.Length; i += 3)
            {
                indices[i+0] = cubeIndices[i+0];
                indices[i+1] = cubeIndices[i+2];
                indices[i+2] = cubeIndices[i+1];
            }
        }

        /// <summary>
        /// Creates a new instance of <see cref="UIBatch"/>.
        /// </summary>
        /// <param name="device">A valid instance of <see cref="GraphicsDevice"/>.</param>
        public UIBatch(GraphicsDevice device)
            : base(device, UIEffect.Bytecode, UIEffect.BytecodeSRgb,
            ResourceBufferInfo.CreateDynamicIndexBufferInfo("UIBatch.VertexIndexBuffers", MaxIndicesCount, MaxVerticesCount), 
            VertexPositionColorTextureSwizzle.Layout)
        {
            // Create a 1x1 pixel white texture
            whiteTexture = GraphicsDevice.GetSharedWhiteTexture();
        }

        /// <summary>
        /// Begins a image batch rendering using the specified blend state, depth stencil and a view-projection transformation matrix. 
        /// Passing null for any of the state objects selects the default default state objects (BlendState.AlphaBlend, DepthStencilState.None).
        /// </summary>
        /// <param name="blendState">Blending options.</param>
        /// <param name="depthStencilState">Depth and stencil options.</param>
        /// <param name="viewProjection">The view projection matrix used for this series of draw calls</param>
        /// <param name="stencilValue">The value of the stencil buffer to take as reference</param>
        public void Begin(ref Matrix viewProjection, BlendState blendState, DepthStencilState depthStencilState, int stencilValue)
        {
            Begin(ref viewProjection, blendState, null, null, depthStencilState, stencilValue);
        }        
        
        /// <summary>
        /// Begins a image batch rendering using the specified blend state, sampler, depth stencil, rasterizer state objects, and the view-projection transformation matrix. 
        /// Passing null for any of the state objects selects the default default state objects (BlendState.AlphaBlend, DepthStencilState.None, RasterizerState.CullCounterClockwise, SamplerState.LinearClamp). 
        /// </summary>
        /// <param name="blendState">Blending options.</param>
        /// <param name="samplerState">Texture sampling options.</param>
        /// <param name="depthStencilState">Depth and stencil options.</param>
        /// <param name="rasterizerState">Rasterization options.</param>
        /// <param name="viewProjection">The view projection matrix used for this series of draw calls</param>
        /// <param name="stencilValue">The value of the stencil buffer to take as reference</param>
        public void Begin(ref Matrix viewProjection, BlendState blendState, SamplerState samplerState, RasterizerState rasterizerState, DepthStencilState depthStencilState, int stencilValue)
        {
            viewProjectionMatrix = viewProjection;

            Begin(null, null, SpriteSortMode.BackToFront, blendState, samplerState, depthStencilState, rasterizerState, stencilValue);
        }

        /// <summary>
        /// Draw a rectangle of the provided size at the position specified by the world matrix having the provided color.
        /// </summary>
        /// <param name="worldMatrix">The world matrix specifying the position of the rectangle in the world</param>
        /// <param name="elementSize">The size of the rectangle</param>
        /// <param name="color">The color of the rectangle</param>
        /// <param name="depthBias">The depth bias to use when drawing the element</param>
        public void DrawRectangle(ref Matrix worldMatrix, ref Vector3 elementSize, ref Color color, int depthBias)
        {
            // Skip items with null size
            if (elementSize.Length() < MathUtil.ZeroTolerance)
                return;

            // Calculate the information needed to draw.
            var drawInfo = new UIImageDrawInfo
            {
                DepthBias = depthBias,
                Color = color,
                Primitive = PrimitiveType.Rectangle
            };

            var matrix = worldMatrix;
            matrix.M11 *= elementSize.X;
            matrix.M12 *= elementSize.X;
            matrix.M13 *= elementSize.X;
            matrix.M21 *= elementSize.Y;
            matrix.M22 *= elementSize.Y;
            matrix.M23 *= elementSize.Y;
            matrix.M31 *= elementSize.Z;
            matrix.M32 *= elementSize.Z;
            matrix.M33 *= elementSize.Z;

            Matrix worldViewProjection;
            Matrix.Multiply(ref matrix, ref viewProjectionMatrix, out worldViewProjection);
            drawInfo.UnitXWorld = worldViewProjection.Row1;
            drawInfo.UnitYWorld = worldViewProjection.Row2;
            drawInfo.UnitZWorld = worldViewProjection.Row3;
            Vector4.Transform(ref vector4LeftTop, ref worldViewProjection, out drawInfo.LeftTopCornerWorld);

            var elementInfo = new ElementInfo(4, 6, ref drawInfo, depthBias);

            Draw(whiteTexture, ref elementInfo);
        }

        /// <summary>
        /// Draw a cube of the provided size at the position specified by the world matrix having the provided color.
        /// </summary>
        /// <param name="worldMatrix">The world matrix specifying the position of the cube in the world</param>
        /// <param name="elementSize">The size of the cube</param>
        /// <param name="color">The color of the cube</param>
        /// <param name="depthBias">The depth bias to use when drawing the element</param>
        public void DrawCube(ref Matrix worldMatrix, ref Vector3 elementSize, ref Color color, int depthBias)
        {
            DrawCube(ref worldMatrix, ref elementSize, ref color, depthBias, false);
        }

        /// <summary>
        /// Draw a colored background having provided size at the position specified by the world matrix.
        /// </summary>
        /// <param name="worldMatrix">The world matrix specifying the position of the element in the world</param>
        /// <param name="elementSize">The size of the element</param>
        /// <param name="color">The color of the element</param>
        /// <param name="depthBias">The depth bias to use when drawing the element</param>
        public void DrawBackground(ref Matrix worldMatrix, ref Vector3 elementSize, ref Color color, int depthBias)
        {
            DrawCube(ref worldMatrix, ref elementSize, ref color, depthBias, true);
        }

        private void DrawCube(ref Matrix worldMatrix, ref Vector3 elementSize, ref Color color, int depthBias, bool isReverse)
        {
            // Skip items with null size
            if (elementSize.Length() < MathUtil.ZeroTolerance)
                return;

            // Calculate the information needed to draw.
            var drawInfo = new UIImageDrawInfo
            {
                DepthBias = depthBias,
                Color = color,
                Primitive = isReverse? PrimitiveType.ReverseCube: PrimitiveType.Cube
            };

            var matrix = worldMatrix;
            matrix.M11 *= elementSize.X;
            matrix.M12 *= elementSize.X;
            matrix.M13 *= elementSize.X;
            matrix.M21 *= elementSize.Y;
            matrix.M22 *= elementSize.Y;
            matrix.M23 *= elementSize.Y;
            matrix.M31 *= elementSize.Z;
            matrix.M32 *= elementSize.Z;
            matrix.M33 *= elementSize.Z;

            Matrix worldViewProjection;
            Matrix.Multiply(ref matrix, ref viewProjectionMatrix, out worldViewProjection);
            drawInfo.UnitXWorld = worldViewProjection.Row1;
            drawInfo.UnitYWorld = worldViewProjection.Row2;
            drawInfo.UnitZWorld = worldViewProjection.Row3;
            Vector4.Transform(ref vector4LeftTop, ref worldViewProjection, out drawInfo.LeftTopCornerWorld);

            var elementInfo = new ElementInfo(8, 6 * 6, ref drawInfo, depthBias);

            Draw(whiteTexture, ref elementInfo);
        }

        /// <summary>
        /// Batch a new border image draw to the draw list.
        /// </summary>
        /// <param name="texture">The texture to use during the draw</param>
        /// <param name="worldMatrix">The world matrix of the element</param>
        /// <param name="sourceRectangle">The rectangle indicating the source region of the texture to use</param>
        /// <param name="elementSize">The size of the ui element</param>
        /// <param name="borderSize">The size of the borders in the texture in pixels (left/right/top/bottom)</param>
        /// <param name="color">The color to apply to the texture image.</param>
        /// <param name="depthBias">The depth bias of the ui element</param>
        /// <param name="imageOrientation">The rotation to apply on the image uv</param>
        /// <param name="swizzle">Swizzle mode indicating the swizzle use when sampling the texture in the shader</param>
        /// <param name="snapImage">Indicate if the image needs to be snapped or not</param>
        public void DrawImage(Texture texture, ref Matrix worldMatrix, ref RectangleF sourceRectangle, ref Vector3 elementSize, ref Vector4 borderSize, 
            ref Color color, int depthBias, ImageOrientation imageOrientation = ImageOrientation.AsIs, SwizzleMode swizzle = SwizzleMode.None, bool snapImage = false)
        {
            // Check that texture is not null
            if (texture == null)
                throw new ArgumentNullException("texture");

            // Skip items with null size
            if (elementSize.Length() < MathUtil.ZeroTolerance)
                return;

            // Calculate the information needed to draw.
            var drawInfo = new UIImageDrawInfo
            {
                Source =
                {
                    X = sourceRectangle.X / texture.ViewWidth, 
                    Y = sourceRectangle.Y / texture.ViewHeight, 
                    Width = sourceRectangle.Width / texture.ViewWidth, 
                    Height = sourceRectangle.Height / texture.ViewHeight
                },
                DepthBias = depthBias,
                Color = color,
                Swizzle = swizzle,
                SnapImage = snapImage,
                Primitive = borderSize == Vector4.Zero? PrimitiveType.Rectangle : PrimitiveType.BorderRectangle,
                BorderSize = new Vector4(borderSize.X / sourceRectangle.Width, borderSize.Y / sourceRectangle.Height, borderSize.Z / sourceRectangle.Width, borderSize.W / sourceRectangle.Height),
            };

            var rotatedSize = imageOrientation == ImageOrientation.AsIs? elementSize: new Vector3(elementSize.Y, elementSize.X, 0);
            drawInfo.VertexShift = new Vector4(borderSize.X / rotatedSize.X, borderSize.Y / rotatedSize.Y, 1f - borderSize.Z / rotatedSize.X, 1f - borderSize.W / rotatedSize.Y);

            var matrix = worldMatrix;
            matrix.M11 *= elementSize.X;
            matrix.M12 *= elementSize.X;
            matrix.M13 *= elementSize.X;
            matrix.M21 *= elementSize.Y;
            matrix.M22 *= elementSize.Y;
            matrix.M23 *= elementSize.Y;
            matrix.M31 *= elementSize.Z;
            matrix.M32 *= elementSize.Z;
            matrix.M33 *= elementSize.Z;

            Matrix worldViewProjection;
            Matrix.Multiply(ref matrix, ref viewProjectionMatrix, out worldViewProjection);
            drawInfo.UnitXWorld = worldViewProjection.Row1;
            drawInfo.UnitYWorld = worldViewProjection.Row2;

            // rotate origin and unit axis if need.
            var leftTopCorner = vector4LeftTop;
            if (imageOrientation == ImageOrientation.Rotated90)
            {
                var unitX = drawInfo.UnitXWorld;
                drawInfo.UnitXWorld = -drawInfo.UnitYWorld;
                drawInfo.UnitYWorld = unitX;
                leftTopCorner = new Vector4(-0.5f, 0.5f, 0, 1);
            }
            Vector4.Transform(ref leftTopCorner, ref worldViewProjection, out drawInfo.LeftTopCornerWorld);

            var verticesPerElement = 4;
            var indicesPerElement = 6;
            if (drawInfo.Primitive == PrimitiveType.BorderRectangle)
            {
                verticesPerElement = 16;
                indicesPerElement = 54;
            }

            var elementInfo = new ElementInfo(verticesPerElement, indicesPerElement, ref drawInfo, depthBias);

            Draw(texture, ref elementInfo);
        }

        internal void DrawCharacter(Texture texture, ref Matrix worldViewProjectionMatrix, ref RectangleF sourceRectangle, ref Color color, int depthBias, SwizzleMode swizzle)
        {
            // Check that texture is not null
            if (texture == null)
                throw new ArgumentNullException("texture");

            // Calculate the information needed to draw.
            var drawInfo = new UIImageDrawInfo
            {
                Source =
                {
                    X = sourceRectangle.X / texture.ViewWidth,
                    Y = sourceRectangle.Y / texture.ViewHeight,
                    Width = sourceRectangle.Width / texture.ViewWidth,
                    Height = sourceRectangle.Height / texture.ViewHeight
                },
                DepthBias = depthBias,
                Color = color,
                Swizzle = swizzle,
                Primitive = PrimitiveType.Rectangle,
                VertexShift = Vector4.Zero,
                UnitXWorld = worldViewProjectionMatrix.Row1,
                UnitYWorld = worldViewProjectionMatrix.Row2,
                LeftTopCornerWorld = worldViewProjectionMatrix.Row4,
            };

            var elementInfo = new ElementInfo(4, 6, ref drawInfo, depthBias);

            Draw(texture, ref elementInfo);
        }

        /// <summary>
        /// Batch the draws required to display the provided text to the draw list.
        /// </summary>
        /// <param name="fontSize">The size to use when rendering the font in virtual pixels</param>
        /// <param name="internalScales">The internal scales to apply on the font glyphs</param>
        /// <param name="text">The text to draw on the screen</param>
        /// <param name="worldMatrix">The world matrix of the element</param>
        /// <param name="elementSize">The 2D size of the element to draw in virtual pixels</param>
        /// <param name="color">The color of the text to draw</param>
        /// <param name="alignment">The alignment of the text to draw</param>
        /// <param name="depthBias">The depth bias of the ui element</param>
        /// <param name="font">The fond to use to draw the text</param>
        /// <param name="snapText">Indicate if the rendered string should be snapped to the closed pixel.</param>
        public void DrawString(SpriteFont font, string text, float fontSize, ref Vector2 internalScales, ref Matrix worldMatrix, ref Vector2  elementSize, 
                                ref Color color, TextAlignment alignment, int depthBias, bool snapText)
        {
            var drawCommand = new SpriteFont.InternalUIDrawCommand
                {
                    Matrix = worldMatrix, 
                    Batch = this, 
                    Color = color, 
                    DepthBias = depthBias, 
                    FontScale = internalScales,
                    SnapText = snapText,
                    Alignment = alignment,
                    Size = elementSize,
                };

            DrawString(font, text, ref drawCommand);
        }

        internal void DrawString(SpriteFont font, string text, ref SpriteFont.InternalUIDrawCommand drawCommand)
        {
            if (font == null)
                throw new ArgumentNullException("font");

            if (text == null)
                throw new ArgumentNullException("text");

            var proxy = new SpriteFont.StringProxy(text);

            // shift the string position so that it is written from the left/top corner of the element
            var offsets = drawCommand.Size / 2;
            var worldMatrix = drawCommand.Matrix;
            worldMatrix.M41 -= worldMatrix.M11 * offsets.X + worldMatrix.M21 * offsets.Y;
            worldMatrix.M42 -= worldMatrix.M12 * offsets.X + worldMatrix.M22 * offsets.Y;
            worldMatrix.M43 -= worldMatrix.M13 * offsets.X + worldMatrix.M23 * offsets.Y;

            // transform the world matrix into the world view project matrix
            Matrix.MultiplyTo(ref worldMatrix, ref viewProjectionMatrix, out drawCommand.Matrix);

            // do not snap static fonts when real/virtual resolution does not match.
            if (!font.IsDynamic && (drawCommand.FontScale.X != 1 || drawCommand.FontScale.Y != 1)) 
            {
                drawCommand.SnapText = false;   // we don't want snapping of the resolution of the screen does not match virtual resolution. (character alignment problems)
                drawCommand.FontScale = Vector2.One; // ensure that static font are not scaled internally
            }

            // snap draw start position to prevent characters to be drawn in between two pixels
            if (drawCommand.SnapText)
            {
                var invW = 1.0f / drawCommand.Matrix.M44;
                var backBufferHalfWidth = GraphicsDevice.BackBuffer.ViewWidth / 2;
                var backBufferHalfHeight = GraphicsDevice.BackBuffer.ViewHeight / 2;

                drawCommand.Matrix.M41 *= invW;
                drawCommand.Matrix.M42 *= invW;
                drawCommand.Matrix.M41 = (float)(Math.Round(drawCommand.Matrix.M41 * backBufferHalfWidth) / backBufferHalfWidth);
                drawCommand.Matrix.M42 = (float)(Math.Round(drawCommand.Matrix.M42 * backBufferHalfHeight) / backBufferHalfHeight);
                drawCommand.Matrix.M41 /= invW;
                drawCommand.Matrix.M42 /= invW;
            }

            font.InternalUIDraw(ref proxy, ref drawCommand);
        }

        protected override unsafe void UpdateBufferValuesFromElementInfo(ref ElementInfo elementInfo, IntPtr vertexPtr, IntPtr indexPtr, int vertexOffset)
        {
            // the vertex buffer
            var vertex = (VertexPositionColorTextureSwizzle*)vertexPtr;
            fixed (UIImageDrawInfo* drawInfo = &elementInfo.DrawInfo)
            {
                switch (drawInfo->Primitive)
                {
                    case PrimitiveType.Rectangle:
                        CalculateRectangleVertices(drawInfo, vertex);
                        break;
                    case PrimitiveType.BorderRectangle:
                        CalculateBorderRectangleVertices(drawInfo, vertex);
                        break;
                    case PrimitiveType.Cube:
                    case PrimitiveType.ReverseCube:
                        CalculateCubeVertices(drawInfo, vertex);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                // the index buffer
                var index = (short*)indexPtr;
                var indices = PrimiteTypeToIndices[(int)drawInfo->Primitive];
                for (var i = 0; i < indices.Length; ++i)
                    index[i] = (short)(indices[i] + vertexOffset);
            }
        }

        private unsafe void CalculateCubeVertices(UIImageDrawInfo* drawInfo, VertexPositionColorTextureSwizzle* vertex)
        {
            var currentPosition = drawInfo->LeftTopCornerWorld;
            
            // set the two first line of vertices
            for (var l = 0; l < 2; ++l)
            {
                for (int r = 0; r < 2; r++)
                {
                    for (int c = 0; c < 2; c++)
                    {
                        vertex->Color = drawInfo->Color;
                        vertex->Swizzle = (int)drawInfo->Swizzle;
                        vertex->TextureCoordinate.X = 0; // cubes are used only for color
                        vertex->TextureCoordinate.Y = 0; // cubes are used only for color

                        vertex->Position.X = currentPosition.X;
                        vertex->Position.Y = currentPosition.Y;
                        vertex->Position.Z = currentPosition.Z - currentPosition.W * drawInfo->DepthBias * DepthBiasShiftOneUnit;
                        vertex->Position.W = currentPosition.W;

                        vertex++;

                        if (c == 0)
                            Vector4.Add(ref currentPosition, ref drawInfo->UnitXWorld, out currentPosition);
                        else
                            Vector4.Subtract(ref currentPosition, ref drawInfo->UnitXWorld, out currentPosition);
                    }

                    if (r == 0)
                        Vector4.Add(ref currentPosition, ref drawInfo->UnitYWorld, out currentPosition);
                    else
                        Vector4.Subtract(ref currentPosition, ref drawInfo->UnitYWorld, out currentPosition);
                }

                Vector4.Add(ref currentPosition, ref drawInfo->UnitZWorld, out currentPosition);
            }
        }

        private unsafe void CalculateBorderRectangleVertices(UIImageDrawInfo* drawInfo, VertexPositionColorTextureSwizzle* vertex)
        {
            // set the texture uv vectors
            var uvX = new Vector4();
            uvX[0] = drawInfo->Source.Left;
            uvX[3] = drawInfo->Source.Right;
            uvX[1] = uvX[0] + drawInfo->Source.Width * drawInfo->BorderSize.X;
            uvX[2] = uvX[3] - drawInfo->Source.Width * drawInfo->BorderSize.Z;
            var uvY = new Vector4();
            uvY[0] = drawInfo->Source.Top;
            uvY[3] = drawInfo->Source.Bottom;
            uvY[1] = uvY[0] + drawInfo->Source.Height * drawInfo->BorderSize.Y;
            uvY[2] = uvY[3] - drawInfo->Source.Height * drawInfo->BorderSize.W;

            // set the shift vectors
            shiftVectorX[0] = Vector4.Zero;
            Vector4.Multiply(ref drawInfo->UnitXWorld, drawInfo->VertexShift.X, out shiftVectorX[1]);
            Vector4.Multiply(ref drawInfo->UnitXWorld, drawInfo->VertexShift.Z, out shiftVectorX[2]);
            shiftVectorX[3] = drawInfo->UnitXWorld;

            shiftVectorY[0] = Vector4.Zero;
            Vector4.Multiply(ref drawInfo->UnitYWorld, drawInfo->VertexShift.Y, out shiftVectorY[1]);
            Vector4.Multiply(ref drawInfo->UnitYWorld, drawInfo->VertexShift.W, out shiftVectorY[2]);
            shiftVectorY[3] = drawInfo->UnitYWorld;

            for (int r = 0; r < 4; r++)
            {
                Vector4 currentRowPosition;
                Vector4.Add(ref drawInfo->LeftTopCornerWorld, ref shiftVectorY[r], out currentRowPosition);

                for (int c = 0; c < 4; c++)
                {
                    Vector4 currentPosition;
                    Vector4.Add(ref currentRowPosition, ref shiftVectorX[c], out currentPosition);

                    vertex->Position.X = currentPosition.X;
                    vertex->Position.Y = currentPosition.Y;
                    vertex->Position.Z = currentPosition.Z - (currentPosition.W * drawInfo->DepthBias * DepthBiasShiftOneUnit);
                    vertex->Position.W = currentPosition.W;

                    vertex->Color = drawInfo->Color;

                    vertex->TextureCoordinate.X = uvX[c];
                    vertex->TextureCoordinate.Y = uvY[r];

                    vertex->Swizzle = (int)drawInfo->Swizzle;

                    vertex++;
                }
            }
        }

        private unsafe void CalculateRectangleVertices(UIImageDrawInfo* drawInfo, VertexPositionColorTextureSwizzle* vertex)
        {
            var currentPosition = drawInfo->LeftTopCornerWorld;

            // snap first pixel to prevent possible problems when left/top is in the middle of a pixel
            if (drawInfo->SnapImage)
            {
                var invW = 1.0f / currentPosition.W;
                var backBufferHalfWidth = GraphicsDevice.BackBuffer.ViewWidth / 2;
                var backBufferHalfHeight = GraphicsDevice.BackBuffer.ViewHeight / 2;

                currentPosition.X *= invW;
                currentPosition.Y *= invW;
                currentPosition.X = (float)(Math.Round(currentPosition.X * backBufferHalfWidth) / backBufferHalfWidth);
                currentPosition.Y = (float)(Math.Round(currentPosition.Y * backBufferHalfHeight) / backBufferHalfHeight);
                currentPosition.X *= currentPosition.W;
                currentPosition.Y *= currentPosition.W;
            }

            var textureCoordX = new Vector2(drawInfo->Source.Left, drawInfo->Source.Right);
            var textureCoordY = new Vector2(drawInfo->Source.Top, drawInfo->Source.Bottom);

            // set the two first line of vertices
            for (int r = 0; r < 2; r++)
            {
                for (int c = 0; c < 2; c++)
                {
                    vertex->Color = drawInfo->Color;
                    vertex->Swizzle = (int)drawInfo->Swizzle;
                    vertex->TextureCoordinate.X = textureCoordX[c];
                    vertex->TextureCoordinate.Y = textureCoordY[r];

                    vertex->Position.X = currentPosition.X;
                    vertex->Position.Y = currentPosition.Y;
                    vertex->Position.Z = currentPosition.Z - currentPosition.W * drawInfo->DepthBias * DepthBiasShiftOneUnit;
                    vertex->Position.W = currentPosition.W;

                    if (drawInfo->SnapImage)
                    {
                        var backBufferHalfWidth = GraphicsDevice.BackBuffer.ViewWidth / 2;
                        var backBufferHalfHeight = GraphicsDevice.BackBuffer.ViewHeight / 2;
                        vertex->Position.X = (float)(Math.Round(vertex->Position.X * backBufferHalfWidth) / backBufferHalfWidth);
                        vertex->Position.Y = (float)(Math.Round(vertex->Position.Y * backBufferHalfHeight) / backBufferHalfHeight);
                    }

                    vertex++;

                    if (c == 0)
                        Vector4.Add(ref currentPosition, ref drawInfo->UnitXWorld, out currentPosition);
                    else
                        Vector4.Subtract(ref currentPosition, ref drawInfo->UnitXWorld, out currentPosition);
                }

                Vector4.Add(ref currentPosition, ref drawInfo->UnitYWorld, out currentPosition);
            }
        }

        /// <summary>
        /// The primitive type to draw for an element.
        /// </summary>
        public enum PrimitiveType
        {
            /// <summary>
            /// A simple rectangle composed of 2 triangles
            /// </summary>
            Rectangle,

            /// <summary>
            /// A rectangle with borders tessellated as 3x3 rectangles
            /// </summary>
            BorderRectangle,

            /// <summary>
            /// A simple cube (not necessary square faces)
            /// </summary>
            Cube,

            /// <summary>
            /// A cube with back and front faces inversed.
            /// </summary>
            ReverseCube,
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct UIImageDrawInfo
        {
            public Vector4 LeftTopCornerWorld;
            public Vector4 UnitXWorld;
            public Vector4 UnitYWorld;
            public Vector4 UnitZWorld;
            public RectangleF Source;
            public Vector4 BorderSize;
            public Vector4 VertexShift;
            public Color Color;
            public int DepthBias;
            public SwizzleMode Swizzle;
            public bool SnapImage;
            public PrimitiveType Primitive;

            public float CalculateDepthOrigin()
            {
                return LeftTopCornerWorld.Z / LeftTopCornerWorld.W - DepthBias * DepthBiasShiftOneUnit;
            }
        }
    }
}