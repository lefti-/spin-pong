/*
    Spin Pong
    This software uses The MIT License (MIT). See license agreement LICENSE for full details.
*/

/////////////////////////////////////////////////////////////////////////////////////////////////
// CHECKLIST
/////////////////////////////////////////////////////////////////////////////////////////////////
//
// TO-DO: Find a better font.
// TO-DO: Create README.
//
// SOUND ATTRIBUTIONS:
//              - "menu_click_forward.wav" - https://www.freesound.org/people/NenadSimic/sounds/171697/
//              - "menu_click_back.wav" - https://www.freesound.org/people/Callum_Sharp279/sounds/198778/
//              - "ballhitspaddle.wav" - https://www.freesound.org/people/mickdow/sounds/177409/
//                  * Edited to two .wav files: "ball_hits_player.wav", "ball_hits_enemy.wav".
//              - "scoring_point.wav" - https://www.freesound.org/people/timgormly/sounds/170155/
//              - "pause.wav" - https://www.freesound.org/people/crisstanza/sounds/167127/
//              - "unpause.wav" - https://www.freesound.org/people/crisstanza/sounds/167126/
//              - "game_over.wav" - https://www.freesound.org/people/notchfilter/sounds/43698/
//              - "game_won.wav" - https://www.freesound.org/people/jobro/sounds/60445/
//
// TO-DO (maybe): Mouse cursor clipped/grabbed/captured to the window.
// TO-DO (maybe): Change to fixed timestep, tie game logic to game time elapsed, not framerate.
//
/////////////////////////////////////////////////////////////////////////////////////////////////

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
    class SpinPong
    {
        public const int WindowWidth = 1024;
        public const int WindowHeight = 768;
        public const string Title = "Spin Pong";

        static void Main()
        {
            // Create game window.
            RenderWindow window = new RenderWindow(new VideoMode(WindowWidth, WindowHeight), Title);

            window.SetKeyRepeatEnabled(false);
            window.SetFramerateLimit(60);
            //window.SetVerticalSyncEnabled(true);

            // Create a manager for game states.
            GameStateManager gameStateManager = new GameStateManager(window);
            
            // Main loop.
            while (window.IsOpen())
            {
                // Handle events.
                window.DispatchEvents();

                // Update objects.
                gameStateManager.Update(window);

                // Render graphics.
                gameStateManager.Draw(window);
            }
        }
    }
}