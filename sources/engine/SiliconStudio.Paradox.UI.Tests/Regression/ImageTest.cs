﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Graphics;
using SiliconStudio.Paradox.UI.Controls;

namespace SiliconStudio.Paradox.UI.Tests.Regression
{
    /// <summary>
    /// Class for rendering tests on the <see cref="ImageElement"/> 
    /// </summary>
    public class ImageTest : UITestGameBase
    {
        private ImageElement imageElement;

        public ImageTest()
        {
            CurrentVersion = 3;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();

            imageElement = new ImageElement { Source = new Sprite(Asset.Load<Texture>("uv"))};
            UIComponent.RootElement = imageElement;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();

            FrameGameSystem.TakeScreenshot();
            FrameGameSystem.Draw(() => ChangeImageColor(Color.Brown)).TakeScreenshot();
            FrameGameSystem.Draw(() => ChangeImageColor(Color.Blue)).TakeScreenshot();
            FrameGameSystem.Draw(() => ChangeImageColor(Color.Red)).TakeScreenshot();
            FrameGameSystem.Draw(() => ChangeImageColor(Color.Lime)).TakeScreenshot();
        }

        private void ChangeImageColor(Color color)
        {
            imageElement.Color = color;
        }

        [Test]
        public void RunImageTest()
        {
            RunGameTest(new ImageTest());
        }

        /// <summary>
        /// Launch the Image test.
        /// </summary>
        public static void Main()
        {
            using (var game = new ImageTest())
                game.Run();
        }
    }
}
