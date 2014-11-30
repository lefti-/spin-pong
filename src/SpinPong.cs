/*
    Spin Pong
    This software uses The MIT License (MIT). See license agreement LICENSE.txt for full details.
*/

using GameStateManager;
using SFML.Graphics;
using SFML.Window;

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