/*
    Spin Pong
    This software uses The MIT License (MIT). See license agreement LICENSE.txt for full details.
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

using GameStateManager;
using SFML.Graphics;
using SFML.Window;

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
            StateManager gameStateManager = new StateManager(window);
            
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