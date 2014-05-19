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
    public class GameState
    {
        public GameState() { }
        public virtual void Initialize(RenderWindow window) { }
        public virtual void BindEvents(RenderWindow window) { }
        public virtual void UnbindEvents(RenderWindow window) { }
        public virtual void Update(RenderWindow window) { }
        public virtual void Draw(RenderWindow window) { }
    }


    class PlayState : GameState
    {
        private World world;
        private Font arial;
        private Text playerScoreText;
        private Text enemyScoreText;
        private Text spinCounterText;
        private Text ballSpeedText;
        private Text gamePausedText;
        private Text gamePausedContinueText;
        private Sound menuClickBack;
        private Sound pause;
        private Sound unpause;
        private Sound gameOver;
        private Sound gameWon;
        private Player player;
        private Enemy enemy;
        private Ball ball;
        private Wall topWall;
        private Wall bottomWall;
        private Goal playerGoal;
        private Goal enemyGoal;
        private Sprite midlineSprite;
        private MyContactListener contactListener;
        private StopWatch enemyStartTimer;
        private StopWatch ballVelIncreaseTimer;
        private GameStateManager gameStateManager;
        private float velocityX;
        private string difficultySetting;
        public bool gamePaused = false;


        public PlayState(GameStateManager gameStateManager, string difficultySetting)
        {
            this.gameStateManager = gameStateManager;
            this.difficultySetting = difficultySetting;
        }

        public override void Initialize(RenderWindow window)
        {
            // Set gravity value.
            Vec2 gravity = new Vec2(0.0f, 0.0f);

            // Create world bounding box, where simulations occur.
            AABB worldAABB = new AABB();
            worldAABB.LowerBound.Set(0.0f, 0.0f);
            worldAABB.UpperBound.Set(window.Size.X, window.Size.Y);

            // Set up the physics world.
            this.world = new World(worldAABB, gravity, false);

            // Set up a contact listener for collision detection/callbacks.
            contactListener = new MyContactListener();
            this.world.SetContactListener(contactListener);

            this.player = new Player(this.world, 100, (window.Size.Y / 2));

            if (difficultySetting == "Easy")
            {
                this.enemy = new Enemy(this.world, window.Size.X - 100, (window.Size.Y / 2), 5);
            }
            if (difficultySetting == "Medium")
            {
                this.enemy = new Enemy(this.world, window.Size.X - 100, (window.Size.Y / 2), 10);
            }
            if (difficultySetting == "Hard")
            {
                this.enemy = new Enemy(this.world, window.Size.X - 100, (window.Size.Y / 2), 15);
            }
            if (difficultySetting == "Impossible")
            {
                this.enemy = new Enemy(this.world, window.Size.X - 100, (window.Size.Y / 2), 18);
            }

            this.ball = new Ball(this.world, (window.Size.X / 2), (window.Size.Y / 2));
            this.topWall = new Wall(this.world, (window.Size.X / 2), 16);
            this.bottomWall = new Wall(this.world, (window.Size.X / 2), window.Size.Y - 16);
            this.playerGoal = new Goal(this.world, 3, (window.Size.Y / 2));
            this.enemyGoal = new Goal(this.world, window.Size.X - 3, (window.Size.Y / 2));

            // Set font.
            this.arial = new Font("assets/fonts/arial.ttf");

            // Create sound buffer from filename.
            SoundBuffer menuClickBackBuffer = new SoundBuffer("assets/sounds/menu_click_back.wav");
            SoundBuffer pauseBuffer = new SoundBuffer("assets/sounds/pause.wav");
            SoundBuffer unpauseBuffer = new SoundBuffer("assets/sounds/unpause.wav");
            SoundBuffer gameOverBuffer = new SoundBuffer("assets/sounds/game_over.wav");
            SoundBuffer gameWonBuffer = new SoundBuffer("assets/sounds/game_won.wav");

            // Create sound based on sound buffer.
            this.menuClickBack = new Sound(menuClickBackBuffer);
            this.pause = new Sound(pauseBuffer);
            this.unpause = new Sound(unpauseBuffer);
            this.gameOver = new Sound(gameOverBuffer);
            this.gameWon = new Sound(gameWonBuffer);

            // Create mid-line.
            Texture texture = new Texture("assets/images/midline_10x1024.png");
            this.midlineSprite = new Sprite(texture);
            this.midlineSprite.Position = new Vector2f(((window.Size.X / 2) - 5), 0);

            // Create a text object.
            this.gamePausedText = new Text("PAUSED", arial);
            this.gamePausedText.CharacterSize = 120;
            this.gamePausedText.Color = new Color(255, 255, 255);

            this.gamePausedContinueText = new Text("Press 'SPACE' to continue", arial);
            this.gamePausedContinueText.CharacterSize = 50;
            this.gamePausedContinueText.Color = new Color(255, 255, 255);

            // Center text.
            FloatRect pauseTextRect = gamePausedText.GetLocalBounds();
            this.gamePausedText.Origin = new Vector2f(pauseTextRect.Left + pauseTextRect.Width / 2, pauseTextRect.Top + pauseTextRect.Height / 2);
            this.gamePausedText.Position = new Vector2f(window.Size.X / 2, 350);

            FloatRect pauseContinueTextRect = gamePausedContinueText.GetLocalBounds();
            this.gamePausedContinueText.Origin = new Vector2f(pauseContinueTextRect.Left + pauseContinueTextRect.Width / 2, pauseContinueTextRect.Top + pauseContinueTextRect.Height / 2);
            this.gamePausedContinueText.Position = new Vector2f(window.Size.X / 2, 480);

            window.SetMouseCursorVisible(false);

            // Create timers.
            this.enemyStartTimer = new StopWatch();
            this.ballVelIncreaseTimer = new StopWatch();

            BindEvents(window);
        }

        public override void BindEvents(RenderWindow window)
        {
            window.LostFocus += new EventHandler(OnWindowLostFocus);
            window.Closed += new EventHandler(OnWindowClose);
            window.KeyPressed += new EventHandler<KeyEventArgs>(OnKeyPress);
            window.MouseButtonPressed += new EventHandler<MouseButtonEventArgs>(OnMouseButtonPress);
            window.MouseButtonReleased += new EventHandler<MouseButtonEventArgs>(OnMouseButtonRelease);
        }

        public override void UnbindEvents(RenderWindow window)
        {
            window.LostFocus -= new EventHandler(OnWindowLostFocus);
            window.Closed -= new EventHandler(OnWindowClose);
            window.KeyPressed -= new EventHandler<KeyEventArgs>(OnKeyPress);
            window.MouseButtonPressed -= new EventHandler<MouseButtonEventArgs>(OnMouseButtonPress);
            window.MouseButtonReleased -= new EventHandler<MouseButtonEventArgs>(OnMouseButtonRelease);
        }

        public void OnWindowLostFocus(object sender, EventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;

            this.gamePaused = true;
        }

        public void OnWindowClose(object sender, EventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;

            window.Close();
        }

        public void OnMouseButtonPress(object sender, MouseButtonEventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;

            if (e.Button == Mouse.Button.Left)
            {
                this.ball.playerLaunch = true;
            }
        }

        public void OnMouseButtonRelease(object sender, MouseButtonEventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;

            if (e.Button == Mouse.Button.Left)
            {
                this.ball.playerLaunch = false;
            }
        }

        public void OnKeyPress(object sender, KeyEventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;

            if (e.Code == Keyboard.Key.Escape)
            {
                this.menuClickBack.Play();
                gameStateManager.ChangeState(window, new DifficultyState(gameStateManager));
            }
            else if (e.Code == Keyboard.Key.Space)
            {
                if (gamePaused == true)
                {
                    this.unpause.Play();
                    gamePaused = false;

                    // When unpaused, set mouse position to player body position.
                    int playerPosX = (int)this.player.Body.GetPosition().X * 32;
                    int playerPosY = (int)this.player.Body.GetPosition().Y * 32;
                    Mouse.SetPosition(new Vector2i(playerPosX, playerPosY), window);
                }
                else
                {
                    this.pause.Play();
                    gamePaused = true;
                }
            }
        }

        public override void Update(RenderWindow window)
        {
            // Player wins
            if (this.player.score == 3)
            {
                this.gameWon.Play();
                gameStateManager.ChangeState(window, new WinState(gameStateManager));
            }
            // Player loses
            if (this.enemy.score == 3)
            {
                this.gameOver.Play();
                gameStateManager.ChangeState(window, new LoseState(gameStateManager));
            }

            if (gamePaused)
            {
                // Do nothing.
            }
            else
            {
                // Simulate a smaller timestep, to prevent tunneling on high velocities.
                float subSteps = 30;
                for (int i = 0; i < subSteps; i++)
                {
                    this.world.Step(1 / 60.0f / subSteps, 6, 3);

                    // Fast moving physics related stuff here.
                    this.ball.Update(window, this.player, this.enemy, this.enemyStartTimer, this.ballVelIncreaseTimer);
                    this.player.Update(window);
                    this.enemy.Update(this.ball);
                }
                // Slow moving physics related stuff here.
            }
        }

        public void DrawTexts(RenderWindow window)
        {
            // Create text objects.
            string playerScoreFormat = string.Format("{0}", this.player.score);
            this.playerScoreText = new Text(playerScoreFormat, arial);
            this.playerScoreText.Position = new Vector2f(((window.Size.X / 2) - 100), 60);
            this.playerScoreText.CharacterSize = 80;
            this.playerScoreText.Color = new Color(0, 70, 255);

            string enemyScoreFormat = string.Format("{0}", this.enemy.score);
            this.enemyScoreText = new Text(enemyScoreFormat, arial);
            this.enemyScoreText.Position = new Vector2f(((window.Size.X / 2) + 50), 60);
            this.enemyScoreText.CharacterSize = 80;
            this.enemyScoreText.Color = new Color(0, 70, 255);

            string spinCounterFormat = string.Format("spin: {0}", this.ball.spinCounter);
            this.spinCounterText = new Text(spinCounterFormat, arial);
            this.spinCounterText.Position = new Vector2f(120, window.Size.Y / 2 - 50);
            this.spinCounterText.CharacterSize = 40;
            this.spinCounterText.Style = Text.Styles.Italic;
            this.spinCounterText.Color = new Color(110, 86, 0);

            // Display ball speed always as positive float.
            if (this.ball.Body.GetLinearVelocity().X < 0)
            {
                this.velocityX = -this.ball.Body.GetLinearVelocity().X;
            }
            else
            {
                this.velocityX = this.ball.Body.GetLinearVelocity().X;
            }

            string ballSpeedFormat = string.Format("ball:  {0:0.00} km/h", this.velocityX * 2);
            this.ballSpeedText = new Text(ballSpeedFormat, arial);
            this.ballSpeedText.Position = new Vector2f(160, window.Size.Y / 2);
            this.ballSpeedText.CharacterSize = 20;
            this.ballSpeedText.Style = Text.Styles.Italic;
            this.ballSpeedText.Color = new Color(110, 86, 0);

            window.Draw(this.playerScoreText);
            window.Draw(this.enemyScoreText);
            window.Draw(this.spinCounterText);
            window.Draw(this.ballSpeedText);
        }

        public override void Draw(RenderWindow window)
        {
            DrawTexts(window);
            this.player.Draw(window);
            this.enemy.Draw(window);
            window.Draw(this.midlineSprite);
            this.playerGoal.Draw(window);
            this.enemyGoal.Draw(window);
            this.ball.Draw(window);
            this.topWall.Draw(window);
            this.bottomWall.Draw(window);

            if (gamePaused)
            {
                window.Draw(this.gamePausedText);
                window.Draw(this.gamePausedContinueText);
            }
        }
    }


    class MenuState : GameState
    {
        private Font arial;
        private Text titleText;
        private Text playText;
        private Text quitText;
        private Sound menuClickForward;
        private GameStateManager gameStateManager;
        public bool mouseOnPlayButton;
        public bool mouseOnQuitButton;

        public MenuState(GameStateManager gameStateManager)
        {
            this.gameStateManager = gameStateManager;
        }

        public override void Initialize(RenderWindow window)
        {
            // Set font.
            this.arial = new Font("assets/fonts/arial.ttf");

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
        private GameStateManager gameStateManager;
        public bool mouseOnEasyButton;
        public bool mouseOnMediumButton;
        public bool mouseOnHardButton;
        public bool mouseOnImpossibleButton;
        public bool mouseOnBackButton;

        public DifficultyState(GameStateManager gameStateManager)
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


    class WinState : GameState
    {
        private Font arial;
        private Text titleText;
        private Text playText;
        private Text quitText;
        private Sound menuClickForward;
        private GameStateManager gameStateManager;
        public bool mouseOnPlayButton;
        public bool mouseOnQuitButton;

        public WinState(GameStateManager gameStateManager)
        {
            this.gameStateManager = gameStateManager;
        }

        public override void Initialize(RenderWindow window)
        {
            // Set font.
            this.arial = new Font("assets/fonts/arial.ttf");

            // Create sound buffer from filename.
            SoundBuffer menuClickForwardBuffer = new SoundBuffer("assets/sounds/menu_click_forward.wav");

            // Create sound based on sound buffer.
            this.menuClickForward = new Sound(menuClickForwardBuffer);

            // Create menu buttons.
            this.titleText = new Text("You win. Congratulations!", arial);
            this.titleText.CharacterSize = 60;
            this.titleText.Color = new Color(0, 100, 255);

            this.playText = new Text("Play again", arial);
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


    class LoseState : GameState
    {
        private Font arial;
        private Text titleText;
        private Text playText;
        private Text quitText;
        private Sound menuClickForward;
        private GameStateManager gameStateManager;
        public bool mouseOnPlayButton;
        public bool mouseOnQuitButton;

        public LoseState(GameStateManager gameStateManager)
        {
            this.gameStateManager = gameStateManager;
        }

        public override void Initialize(RenderWindow window)
        {
            // Set font.
            this.arial = new Font("assets/fonts/arial.ttf");

            // Create sound buffer from filename.
            SoundBuffer menuClickForwardBuffer = new SoundBuffer("assets/sounds/menu_click_forward.wav");

            // Create sound based on sound buffer.
            this.menuClickForward = new Sound(menuClickForwardBuffer);

            // Create menu buttons.
            this.titleText = new Text("You lose. Try again?", arial);
            this.titleText.CharacterSize = 60;
            this.titleText.Color = new Color(0, 100, 255);

            this.playText = new Text("Play again", arial);
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
