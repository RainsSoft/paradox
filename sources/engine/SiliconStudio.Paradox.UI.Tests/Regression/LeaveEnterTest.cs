﻿// Copyright (c) 2014 Silicon Studio Corp. (http://siliconstudio.co.jp)
// This file is distributed under GPL v3. See LICENSE.md for details.
using System.Threading.Tasks;

using NUnit.Framework;

using SiliconStudio.Core.Mathematics;
using SiliconStudio.Paradox.Games;
using SiliconStudio.Paradox.Input;
using SiliconStudio.Paradox.UI.Controls;
using SiliconStudio.Paradox.UI.Panels;

namespace SiliconStudio.Paradox.UI.Tests.Regression
{
    /// <summary>
    /// Class for tests on the <see cref="UIElement.TouchLeave"/> and <see cref="UIElement.TouchEnter"/> events.
    /// </summary>
    public class LeaveEnterTest : UITestGameBase
    {
        private Button buttonLeftTop2;

        private Button buttonLeftTop1;

        private Button buttonLeftTop0;

        private Button bottomButton;

        private Button buttonBottomLeft1;

        private Button buttonBottomLeft0;

        private Button bottonBottomRight1;

        private Button buttomBottonRight0;

        public LeaveEnterTest()
        {
            CurrentVersion = 4;
        }

        protected override async Task LoadContent()
        {
            await base.LoadContent();
            
            buttonLeftTop2 = new Button { Padding = new Thickness(50, 50, 0, 50) };
            buttonLeftTop1 = new Button { Padding = new Thickness(50, 50, 0, 50), Content = buttonLeftTop2 };
            buttonLeftTop0 = new Button { Padding = new Thickness(50, 50, 0, 50), Content = buttonLeftTop1};

            var bottomGrid = new UniformGrid { Rows = 1, Columns = 2 };
            bottomButton = new Button { Content = bottomGrid };
            bottomButton.DependencyProperties.Set(GridBase.RowPropertyKey, 1);
            bottomButton.DependencyProperties.Set(GridBase.ColumnSpanPropertyKey, 2);

            buttonBottomLeft1 = new Button { Margin = new Thickness(50, 50, 0, 50) };
            buttonBottomLeft0 = new Button { Margin = new Thickness(50, 0, 0, 100), Content = buttonBottomLeft1 };

            bottonBottomRight1 = new Button { Margin = new Thickness(0, 0, 50, 100) };
            buttomBottonRight0 = new Button { Margin = new Thickness(0, 0, 50, 50), Content = bottonBottomRight1 };
            buttomBottonRight0.DependencyProperties.Set(GridBase.ColumnPropertyKey, 1);

            bottomGrid.Children.Add(buttonBottomLeft0);
            bottomGrid.Children.Add(buttomBottonRight0);

            var mainGrid = new UniformGrid { Rows = 2, Columns = 2 };
            mainGrid.Children.Add(buttonLeftTop0);
            mainGrid.Children.Add(bottomButton);

            UIComponent.RootElement = mainGrid;
        }

        protected override void RegisterTests()
        {
            base.RegisterTests();
            FrameGameSystem.DrawOrder = -1;
            FrameGameSystem.TakeScreenshot(5); // skip some frames in order to be sure that the picking will work
            FrameGameSystem.Draw(Draw1).TakeScreenshot();
            FrameGameSystem.Draw(Draw2).TakeScreenshot();
            FrameGameSystem.Draw(Draw3).TakeScreenshot();
            FrameGameSystem.Draw(Draw4).TakeScreenshot();
            FrameGameSystem.Draw(Draw5).TakeScreenshot();
            FrameGameSystem.Draw(Draw6).TakeScreenshot();
            FrameGameSystem.Draw(Draw7).TakeScreenshot();
            FrameGameSystem.Draw(Draw8).TakeScreenshot();
            FrameGameSystem.Draw(Draw9).TakeScreenshot();
            FrameGameSystem.Draw(Draw10).TakeScreenshot();
            FrameGameSystem.Draw(Draw11).TakeScreenshot();
        }

        public void Draw1()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Down, new Vector2(0.125f, 0.25f)));

            UI.Update(new GameTime());
        }
        public void Draw2()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Move, new Vector2(0.925f, 0.25f)));
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Up, new Vector2(0.925f, 0.25f)));

            UI.Update(new GameTime());
        }
        public void Draw3()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Down, new Vector2(0.125f, 0.25f)));

            UI.Update(new GameTime());
        }
        public void Draw4()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Move, new Vector2(0.125f, 0.15f)));

            UI.Update(new GameTime());
        }
        public void Draw5()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Move, new Vector2(0.125f, 0.05f)));

            UI.Update(new GameTime());
        }
        public void Draw6()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Move, new Vector2(0.925f, 0.05f)));
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Up, new Vector2(0.925f, 0.05f)));

            UI.Update(new GameTime());
        }
        public void Draw7()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Down, new Vector2(0.4f, 0.65f)));

            UI.Update(new GameTime());
        }

        public void Draw8()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Move, new Vector2(0.6f, 0.65f)));
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Up, new Vector2(0.6f, 0.65f)));

            UI.Update(new GameTime());
        }
        public void Draw9()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Down, new Vector2(0.6f, 0.65f)));

            UI.Update(new GameTime());
        }

        public void Draw10()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Move, new Vector2(0.6f, 0.85f)));

            UI.Update(new GameTime());
        }

        public void Draw11()
        {
            Input.PointerEvents.Clear();
            Input.PointerEvents.Add(CreatePointerEvent(PointerState.Move, new Vector2(0.4f, 0.85f)));

            UI.Update(new GameTime());
        }

        [Test]
        public void RunLeaveEnterTest()
        {
            RunGameTest(new LeaveEnterTest());
        }

        /// <summary>
        /// Launch the Image test.
        /// </summary>
        public static void Main()
        {
            using (var game = new LeaveEnterTest())
                game.Run();
        }
    }
}