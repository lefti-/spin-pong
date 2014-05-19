/*
    Spin Pong
    This software uses The MIT License (MIT). See license agreement LICENSE for full details.
*/

using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;
using Entities;
using GameStateManager;
using NetEXT.TimeFunctions;
using SFML.Audio;
using SFML.Graphics;
using SFML.Window;
using System;
using Color = SFML.Graphics.Color;
using MyCollisionListener;

namespace GameStates
{
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
        private CollisionListener collisionListener;
        private StopWatch enemyStartTimer;
        private StopWatch ballVelIncreaseTimer;
        private StateManager gameStateManager;
        private float velocityX;
        private string difficultySetting;
        public bool gamePaused = false;


        public PlayState(StateManager gameStateManager, string difficultySetting)
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
            collisionListener = new CollisionListener();
            this.world.SetContactListener(collisionListener);

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
}
