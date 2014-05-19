/*
    Spin Pong
    This software uses The MIT License (MIT). See license agreement LICENSE.txt for full details.
*/

using GameStateManager;
using SFML.Audio;
using SFML.Graphics;
using SFML.Window;
using System;
using Color = SFML.Graphics.Color;

namespace GameStates
{
    class MenuState : GameState
    {
        private Font arial;
        private Text titleText;
        private Text playText;
        private Text quitText;
        private Sound menuClickForward;
        private StateManager gameStateManager;
        public bool mouseOnPlayButton;
        public bool mouseOnQuitButton;

        public MenuState(StateManager gameStateManager)
        {
            this.gameStateManager = gameStateManager;
        }

        public override void Initialize(RenderWindow window)
        {
            // Set font.
            this.arial = new Font("assets/fonts/centurygothic.ttf");

            // Create sound buffer from filename.
            SoundBuffer menuClickForwardBuffer = new SoundBuffer("assets/sounds/menu_click_forward.wav");

            // Create sound based on sound buffer.
            this.menuClickForward = new Sound(menuClickForwardBuffer);

            // Create menu buttons.
            this.titleText = new Text("Spin Pong", arial);
            this.titleText.CharacterSize = 80;
            this.titleText.Color = new Color(0, 100, 255);

            this.playText = new Text("Play", arial);
            this.playText.CharacterSize = 60;

            this.quitText = new Text("Quit", arial);
            this.quitText.CharacterSize = 60;

            // Center texts.
            FloatRect titleTextRect = titleText.GetLocalBounds();
            this.titleText.Origin = new Vector2f(titleTextRect.Left + titleTextRect.Width / 2, titleTextRect.Top + titleTextRect.Height / 2);
            this.titleText.Position = new Vector2f(window.Size.X / 2, 100);

            FloatRect playTextRect = playText.GetLocalBounds();
            this.playText.Origin = new Vector2f(playTextRect.Left + playTextRect.Width / 2, playTextRect.Top + playTextRect.Height / 2);
            this.playText.Position = new Vector2f(window.Size.X / 2, 400);

            FloatRect quitTextRect = quitText.GetLocalBounds();
            this.quitText.Origin = new Vector2f(quitTextRect.Left + quitTextRect.Width / 2, quitTextRect.Top + quitTextRect.Height / 2);
            this.quitText.Position = new Vector2f(window.Size.X / 2, 500);

            window.SetMouseCursorVisible(true);

            BindEvents(window);
        }

        public override void BindEvents(RenderWindow window)
        {
            window.Closed += new EventHandler(OnWindowClose);
            window.KeyPressed += new EventHandler<KeyEventArgs>(OnKeyPress);
            window.MouseButtonPressed += new EventHandler<MouseButtonEventArgs>(OnMouseButtonPress);
        }

        public override void UnbindEvents(RenderWindow window)
        {
            window.Closed -= new EventHandler(OnWindowClose);
            window.KeyPressed -= new EventHandler<KeyEventArgs>(OnKeyPress);
            window.MouseButtonPressed -= new EventHandler<MouseButtonEventArgs>(OnMouseButtonPress);
        }

        public void OnWindowClose(object sender, EventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;

            window.Close();
        }

        public void OnKeyPress(object sender, KeyEventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;

            if (e.Code == Keyboard.Key.Escape)
            {
                window.Close();
            }
            if (e.Code == Keyboard.Key.W)
            {
                gameStateManager.ChangeState(window, new WinState(gameStateManager));
            }
            if (e.Code == Keyboard.Key.K)
            {
                gameStateManager.ChangeState(window, new LoseState(gameStateManager));
            }
        }

        public void OnMouseButtonPress(object sender, MouseButtonEventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;

            if (e.Button == Mouse.Button.Left)
            {
                // Transform the mouse position from window coordinates to world coordinates.
                Vector2f mouse = window.MapPixelToCoords(Mouse.GetPosition(window));

                // Retrieve the bounding boxes of the text objects.
                FloatRect playTextBounds = playText.GetGlobalBounds();
                FloatRect quitTextBounds = quitText.GetGlobalBounds();

                // Hit tests.
                if (playTextBounds.Contains(mouse.X, mouse.Y))
                {
                    this.menuClickForward.Play();
                    gameStateManager.ChangeState(window, new DifficultyState(gameStateManager));
                }
                if (quitTextBounds.Contains(mouse.X, mouse.Y))
                {
                    window.Close();
                }
            }
        }

        public override void Update(RenderWindow window)
        {
            // Transform the mouse position from window coordinates to world coordinates.
            Vector2f mouse = window.MapPixelToCoords(Mouse.GetPosition(window));

            // Retrieve the bounding boxes of the text objects.
            FloatRect playTextBounds = playText.GetGlobalBounds();
            FloatRect quitTextBounds = quitText.GetGlobalBounds();

            // Hit tests.
            if (playTextBounds.Contains(mouse.X, mouse.Y))
            {
                mouseOnPlayButton = true;
            }
            else
            {
                mouseOnPlayButton = false;
            }
            if (quitTextBounds.Contains(mouse.X, mouse.Y))
            {
                mouseOnQuitButton = true;
            }
            else
            {
                mouseOnQuitButton = false;
            }
        }

        public override void Draw(RenderWindow window)
        {
            if (!mouseOnPlayButton)
            {
                this.playText.Color = new Color(0, 70, 255);
            }
            else
            {
                this.playText.Color = new Color(255, 0, 0);
            }

            if (!mouseOnQuitButton)
            {
                this.quitText.Color = new Color(0, 70, 255);
            }
            else
            {
                this.quitText.Color = new Color(255, 0, 0);
            }

            window.Draw(this.titleText);
            window.Draw(this.playText);
            window.Draw(this.quitText);
        }
    }
}
