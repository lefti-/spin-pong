/*
    Spin Pong
    This software uses The MIT License (MIT). See license agreement LICENSE for full details.
*/

using GameStateManager;
using SFML.Audio;
using SFML.Graphics;
using SFML.Window;
using System;
using Color = SFML.Graphics.Color;

namespace GameStates
{
    class DifficultyState : GameState
    {
        private Font arial;
        private Text titleText;
        private Text easyText;
        private Text mediumText;
        private Text hardText;
        private Text impossibleText;
        private Text backText;
        private Sound menuClickForward;
        private Sound menuClickBack;
        private StateManager gameStateManager;
        public bool mouseOnEasyButton;
        public bool mouseOnMediumButton;
        public bool mouseOnHardButton;
        public bool mouseOnImpossibleButton;
        public bool mouseOnBackButton;

        public DifficultyState(StateManager gameStateManager)
        {
            this.gameStateManager = gameStateManager;
        }

        public override void Initialize(RenderWindow window)
        {
            // Set font.
            this.arial = new Font("assets/fonts/arial.ttf");

            // Create sound buffer from filename.
            SoundBuffer menuClickForwardBuffer = new SoundBuffer("assets/sounds/menu_click_forward.wav");
            SoundBuffer menuClickBackBuffer = new SoundBuffer("assets/sounds/menu_click_back.wav");

            // Create sound based on sound buffer.
            this.menuClickForward = new Sound(menuClickForwardBuffer);
            this.menuClickBack = new Sound(menuClickBackBuffer);

            // Create menu buttons.
            this.titleText = new Text("Choose difficulty:", arial);
            this.titleText.CharacterSize = 70;
            this.titleText.Color = new Color(0, 100, 255);

            this.easyText = new Text("Easy (5 spins)", arial);
            this.easyText.CharacterSize = 50;

            this.mediumText = new Text("Medium (10 spins)", arial);
            this.mediumText.CharacterSize = 50;

            this.hardText = new Text("Hard (15 spins)", arial);
            this.hardText.CharacterSize = 50;

            this.impossibleText = new Text("Impossible (18 spins)", arial);
            this.impossibleText.CharacterSize = 50;

            this.backText = new Text("Back", arial);
            this.backText.CharacterSize = 50;

            // Center texts.
            FloatRect titleTextRect = titleText.GetLocalBounds();
            this.titleText.Origin = new Vector2f(titleTextRect.Left + titleTextRect.Width / 2, titleTextRect.Top + titleTextRect.Height / 2);
            this.titleText.Position = new Vector2f(window.Size.X / 2, 100);

            FloatRect easyTextRect = easyText.GetLocalBounds();
            this.easyText.Origin = new Vector2f(easyTextRect.Left + easyTextRect.Width / 2, easyTextRect.Top + easyTextRect.Height / 2);
            this.easyText.Position = new Vector2f(window.Size.X / 2, 300);

            FloatRect mediumTextRect = mediumText.GetLocalBounds();
            this.mediumText.Origin = new Vector2f(mediumTextRect.Left + mediumTextRect.Width / 2, mediumTextRect.Top + mediumTextRect.Height / 2);
            this.mediumText.Position = new Vector2f(window.Size.X / 2, 360);

            FloatRect hardTextRect = hardText.GetLocalBounds();
            this.hardText.Origin = new Vector2f(hardTextRect.Left + hardTextRect.Width / 2, hardTextRect.Top + hardTextRect.Height / 2);
            this.hardText.Position = new Vector2f(window.Size.X / 2, 420);

            FloatRect impossibleTextRect = impossibleText.GetLocalBounds();
            this.impossibleText.Origin = new Vector2f(impossibleTextRect.Left + impossibleTextRect.Width / 2, impossibleTextRect.Top + impossibleTextRect.Height / 2);
            this.impossibleText.Position = new Vector2f(window.Size.X / 2, 480);

            FloatRect backTextRect = backText.GetLocalBounds();
            this.backText.Origin = new Vector2f(backTextRect.Left + backTextRect.Width / 2, backTextRect.Top + backTextRect.Height / 2);
            this.backText.Position = new Vector2f(window.Size.X / 2, 650);

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
                this.menuClickBack.Play();
                gameStateManager.ChangeState(window, new MenuState(gameStateManager));
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
                FloatRect easyTextBounds = easyText.GetGlobalBounds();
                FloatRect mediumTextBounds = mediumText.GetGlobalBounds();
                FloatRect hardTextBounds = hardText.GetGlobalBounds();
                FloatRect impossibleTextBounds = impossibleText.GetGlobalBounds();
                FloatRect backTextBounds = backText.GetGlobalBounds();

                // Hit tests.
                if (easyTextBounds.Contains(mouse.X, mouse.Y))
                {
                    this.menuClickForward.Play();
                    gameStateManager.ChangeState(window, new PlayState(gameStateManager, "Easy"));
                }
                if (mediumTextBounds.Contains(mouse.X, mouse.Y))
                {
                    this.menuClickForward.Play();
                    gameStateManager.ChangeState(window, new PlayState(gameStateManager, "Medium"));
                }
                if (hardTextBounds.Contains(mouse.X, mouse.Y))
                {
                    this.menuClickForward.Play();
                    gameStateManager.ChangeState(window, new PlayState(gameStateManager, "Hard"));
                }
                if (impossibleTextBounds.Contains(mouse.X, mouse.Y))
                {
                    this.menuClickForward.Play();
                    gameStateManager.ChangeState(window, new PlayState(gameStateManager, "Impossible"));
                }
                if (backTextBounds.Contains(mouse.X, mouse.Y))
                {
                    this.menuClickBack.Play();
                    gameStateManager.ChangeState(window, new MenuState(gameStateManager));
                }
            }
        }

        public override void Update(RenderWindow window)
        {
            // Transform the mouse position from window coordinates to world coordinates.
            Vector2f mouse = window.MapPixelToCoords(Mouse.GetPosition(window));

            // Retrieve the bounding boxes of the text objects.
            FloatRect easyTextBounds = easyText.GetGlobalBounds();
            FloatRect mediumTextBounds = mediumText.GetGlobalBounds();
            FloatRect hardTextBounds = hardText.GetGlobalBounds();
            FloatRect impossibleTextBounds = impossibleText.GetGlobalBounds();
            FloatRect backTextBounds = backText.GetGlobalBounds();

            // Hit tests.
            if (easyTextBounds.Contains(mouse.X, mouse.Y))
            {
                mouseOnEasyButton = true;
            }
            else
            {
                mouseOnEasyButton = false;

            }

            if (mediumTextBounds.Contains(mouse.X, mouse.Y))
            {
                mouseOnMediumButton = true;
            }
            else
            {
                mouseOnMediumButton = false;
            }

            if (hardTextBounds.Contains(mouse.X, mouse.Y))
            {
                mouseOnHardButton = true;
            }
            else
            {
                mouseOnHardButton = false;
            }

            if (impossibleTextBounds.Contains(mouse.X, mouse.Y))
            {
                mouseOnImpossibleButton = true;
            }
            else
            {
                mouseOnImpossibleButton = false;
            }

            if (backTextBounds.Contains(mouse.X, mouse.Y))
            {
                mouseOnBackButton = true;
            }
            else
            {
                mouseOnBackButton = false;
            }
        }

        public override void Draw(RenderWindow window)
        {
            if (!mouseOnEasyButton)
            {
                this.easyText.Color = new Color(0, 70, 255);
            }
            else
            {
                this.easyText.Color = new Color(255, 0, 0);
            }

            if (!mouseOnMediumButton)
            {
                this.mediumText.Color = new Color(0, 70, 255);
            }
            else
            {
                this.mediumText.Color = new Color(255, 0, 0);
            }

            if (!mouseOnHardButton)
            {
                this.hardText.Color = new Color(0, 70, 255);
            }
            else
            {
                this.hardText.Color = new Color(255, 0, 0);
            }

            if (!mouseOnImpossibleButton)
            {
                this.impossibleText.Color = new Color(0, 70, 255);
            }
            else
            {
                this.impossibleText.Color = new Color(255, 0, 0);
            }

            if (!mouseOnBackButton)
            {
                this.backText.Color = new Color(0, 70, 255);
            }
            else
            {
                this.backText.Color = new Color(255, 0, 0);
            }

            window.Draw(this.titleText);
            window.Draw(this.easyText);
            window.Draw(this.mediumText);
            window.Draw(this.hardText);
            window.Draw(this.impossibleText);
            window.Draw(this.backText);
        }
    }
}
