/*
    Spin Pong
    This software uses The MIT License (MIT). See license agreement LICENSE for full details.
*/

using System;
using System.Collections.Generic;
using SFML;
using SFML.Audio;
using SFML.Graphics;
using SFML.Window;
using Color = SFML.Graphics.Color;
using NetEXT.TimeFunctions;
using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;
using Shape = Box2DX.Collision.Shape;

namespace spin_pong
{
    class GameStateManager
    {
        List<GameState> gameStates = new List<GameState>();
        private int currentState;

        public GameStateManager(RenderWindow window)
        {
            gameStates = new List<GameState>();

            this.currentState = 0;
            gameStates.Add(new MenuState(this));
            gameStates[this.currentState].Initialize(window);

            GetCurrentStateInfo();
        }

        public void ChangeState(RenderWindow window, GameState gameState)
        {
            // Unbind state event handlers.
            gameStates[this.currentState].UnbindEvents(window);

            // Remove the current state.
            gameStates.RemoveAt(this.currentState);

            // Add a state.
            this.currentState = 0;
            gameStates.Add(gameState);

            // Initialize the state.
            gameStates[this.currentState].Initialize(window);

            GetCurrentStateInfo();
        }

        public void GetCurrentStateInfo()
        {
            Console.WriteLine("GameState: " + gameStates[this.currentState]);
        }

        public void Update(RenderWindow window)
        {
            gameStates[this.currentState].Update(window);
        }

        public void Draw(RenderWindow window)
        {
            // Clear screen.
            window.Clear();

            // Render graphics.
            gameStates[this.currentState].Draw(window);

            // Update display.
            window.Display();
        }
    }
}
