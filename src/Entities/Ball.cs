/*
    Spin Pong
    This software uses The MIT License (MIT). See license agreement LICENSE.txt for full details.
*/

using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;
using NetEXT.TimeFunctions;
using SFML.Audio;
using SFML.Graphics;
using SFML.Window;
using System;

namespace Entities
{
    class Ball : Entity
    {
        private Body ballBody;
        private PolygonDef ballPoly;
        private Sprite ballSprite;
        private Random random;
        private Sound ballHitsPlayer;
        private Sound ballHitsEnemy;
        private Sound ballHitsGoal;
        public Body Body { get { return this.ballBody; } }
        public Sprite Sprite { get { return this.ballSprite; } }
        private const int HalfWidth = 12;
        private const int HalfHeight = 12;
        public int contactCounter = 0;
        public float spinCounter = 0;
        public bool collisionWithWall = false;
        public bool collisionWithPaddleSidePlayer = false;
        public bool collisionWithPaddleSideEnemy = false;
        public bool collisionWithPaddleTop = false;
        public bool collisionWithPaddleBottom = false;
        public bool collisionWithEnemyGoal = false;
        public bool collisionWithPlayerGoal = false;
        public bool playerStart = false;
        public bool playerLaunch = false;
        public bool enemyStart = false;

        public Ball(World world, float positionX, float positionY)
        {
            // Define a body with a position.
            BodyDef ballBodyDef = new BodyDef();
            ballBodyDef.Position.Set(positionX / PixelsToMeter, positionY / PixelsToMeter);

            // Create the physics body.
            this.ballBody = world.CreateBody(ballBodyDef);

            // Set user data in the body.
            this.ballBody.SetUserData(this);

            // Define a shape definition.
            this.ballPoly = new PolygonDef();
            this.ballPoly.IsSensor = true;
            this.ballPoly.SetAsBox(HalfWidth / PixelsToMeter, HalfHeight / PixelsToMeter);
            this.ballPoly.Density = 1.0f;

            // Create a texture from filename.
            Texture texture = new Texture("assets/images/ball_24x24.png");

            // Create a sprite based on texture.
            this.ballSprite = new Sprite(texture) { Origin = new Vector2f(HalfWidth, HalfHeight) };
            this.ballSprite.Position = new Vector2f(PixelsToMeter * ballBody.GetPosition().X, PixelsToMeter * ballBody.GetPosition().Y);

            // Finalize the shape and body.
            this.ballBody.CreateShape(ballPoly);
            this.ballBody.SetMassFromShapes();

            // Create sound buffer from filename.
            SoundBuffer ballHitsPlayerBuffer = new SoundBuffer("assets/sounds/ball_hits_player.wav");
            SoundBuffer ballHitsEnemyBuffer = new SoundBuffer("assets/sounds/ball_hits_enemy.wav");
            SoundBuffer ballHitsGoalBuffer = new SoundBuffer("assets/sounds/scoring_point.wav");

            // Create sound based on sound buffer.
            this.ballHitsPlayer = new Sound(ballHitsPlayerBuffer);
            this.ballHitsEnemy = new Sound(ballHitsEnemyBuffer);
            this.ballHitsGoal = new Sound(ballHitsGoalBuffer);

            this.random = new Random();
            RandomizeBallDirection();
        }

        // Four possible directions.
        public void RandomizeBallDirection()
        {
            if (this.random.Next(0, 3) == 0)
            {
                this.Body.SetLinearVelocity(new Vec2(-4, -6));
            }
            else if (this.random.Next(0, 3) == 1)
            {
                this.Body.SetLinearVelocity(new Vec2(4, -6));
            }
            else if (this.random.Next(0, 3) == 2)
            {
                this.Body.SetLinearVelocity(new Vec2(4, 6));
            }
            else
            {
                this.Body.SetLinearVelocity(new Vec2(-4, 6));
            }
        }

        public void Respawn(RenderWindow window)
        {
            this.Body.SetXForm(new Vec2((window.Size.X / 2) / PixelsToMeter, (window.Size.Y / 2) / PixelsToMeter), 0);
            this.RandomizeBallDirection();
        }

        public void Update(RenderWindow window, Player player, Enemy enemy, StopWatch enemyStartTimer, StopWatch ballVelIncreaseTimer)
        {
            double enemyStartElapsed = enemyStartTimer.ElapsedTime.Seconds;
            double ballVelIncreaseElapsed = ballVelIncreaseTimer.ElapsedTime.Seconds;

            Vec2 ballVelocity = this.Body.GetLinearVelocity();

            // Respawn the ball, if it flies off-screen for some reason.
            if (this.Body.GetPosition().X < 0 || this.Body.GetPosition().Y > window.Size.X / PixelsToMeter ||
                this.Body.GetPosition().Y < 0 || this.Body.GetPosition().Y > window.Size.Y / PixelsToMeter)
            {
                this.Respawn(window);
            }

            if (this.collisionWithWall)
            {
                ballVelocity.Y *= -1;
                this.collisionWithWall = false;
            }

            if (this.collisionWithPaddleSidePlayer)
            {
                this.ballHitsPlayer.Play();
                ballVelocity.X *= -1;

                // Player moving up.
                if (player.Body.GetLinearVelocity().Y < 0)
                {
                    ballVelocity.Y -= 4;

                    if (ballVelocity.Y > 2)
                    {
                        spinCounter -= 1;
                    }
                    else if (ballVelocity.Y < -2)
                    {
                        spinCounter += 1;
                    }
                    else
                    {
                        spinCounter = 0;
                    }
                }
                // Player moving down.
                else if (player.Body.GetLinearVelocity().Y > 0)
                {
                    ballVelocity.Y += 4;
                    if (ballVelocity.Y > 2)
                    {
                        spinCounter += 1;
                    }
                    else if (ballVelocity.Y < -2)
                    {
                        spinCounter -= 1;
                    }
                    else
                    {
                        spinCounter = 0;
                    }
                }

                this.collisionWithPaddleSidePlayer = false;
            }

            if (this.collisionWithPaddleSideEnemy)
            {
                this.ballHitsEnemy.Play();
                ballVelocity.X *= -1;
                this.collisionWithPaddleSideEnemy = false;
            }

            if (this.collisionWithPaddleTop)
            {
                this.ballHitsEnemy.Play();
                if (ballVelocity.Y > 0)
                {
                    ballVelocity.Y *= -1;
                }

                this.collisionWithPaddleTop = false;
            }

            if (this.collisionWithPaddleBottom)
            {
                this.ballHitsPlayer.Play();
                if (ballVelocity.Y < 0)
                {
                    ballVelocity.Y *= -1;
                }

                this.collisionWithPaddleBottom = false;
            }

            // Enemy gets a score.
            if (this.collisionWithPlayerGoal)
            {
                enemy.score++;

                if (enemy.score != 3)
                {
                    this.ballHitsGoal.Play();
                }

                this.collisionWithPlayerGoal = false;
                this.playerStart = true;
            }

            // Player gets a score.
            if (this.collisionWithEnemyGoal)
            {
                player.score++;

                if (player.score != 3)
                {
                    this.ballHitsGoal.Play();
                }

                this.Body.SetXForm(new Vec2(enemy.Body.GetPosition().X - (25 / PixelsToMeter), enemy.Body.GetPosition().Y), 0);
                ballVelocity.Y = 0;
                ballVelocity.X = 0;
                this.collisionWithEnemyGoal = false;
                this.enemyStart = true;
            }

            // After ball collides with player goal.
            if (this.playerStart)
            {
                this.Body.SetXForm(new Vec2((player.Body.GetPosition().X + 25 / PixelsToMeter), player.Body.GetPosition().Y), 0);
                ballVelocity.X = 0;
                spinCounter = 0;

                // Player launches ball.
                if (this.playerLaunch)
                {
                    this.ballHitsPlayer.Play();

                    // Choose ball direction based on player velocity.
                    if (player.Body.GetLinearVelocity().Y < 0)
                    {
                        ballVelocity.Y = -6;
                        ballVelocity.X = 6;
                        this.playerStart = false;
                        this.playerLaunch = false;
                    }
                    else if (player.Body.GetLinearVelocity().Y > 0)
                    {
                        ballVelocity.Y = 6;
                        ballVelocity.X = 6;
                        this.playerStart = false;
                        this.playerLaunch = false;
                    }
                    // Player not moving, randomize ball direction.
                    else
                    {
                        if (this.random.Next(0, 2) == 0)
                        {
                            ballVelocity.Y = -6;
                            ballVelocity.X = 6;
                            this.playerStart = false;
                            this.playerLaunch = false;
                        }
                        else
                        {
                            ballVelocity.Y = 6;
                            ballVelocity.X = 6;
                            this.playerStart = false;
                            this.playerLaunch = false;
                        }
                    }
                }
            }

            // After ball collides with enemy goal.
            if (this.enemyStart)
            {
                enemyStartTimer.Start();
                spinCounter = 0;

                // Enemy launches the ball in random direction.
                if (this.random.Next(0, 2) == 0)
                {
                    ballVelocity.X = 0;

                    if (enemyStartElapsed >= 1.2f)
                    {
                        this.ballHitsEnemy.Play();
                        this.enemyStart = false;
                        ballVelocity.Y = -6;
                        ballVelocity.X = -6;
                        enemyStartTimer.Reset();
                    }
                }
                else
                {
                    ballVelocity.X = 0;

                    if (enemyStartElapsed >= 1.2f)
                    {
                        this.ballHitsEnemy.Play();
                        this.enemyStart = false;
                        ballVelocity.Y = 6;
                        ballVelocity.X = -6;
                        enemyStartTimer.Reset();
                    }
                }
            }
            // Gradually increase ball x-velocity, as time passes.
            else
            {
                ballVelIncreaseTimer.Start();

                if (ballVelIncreaseElapsed >= 0.05f)
                {
                    if (this.Body.GetLinearVelocity().X > 0)
                    {
                        ballVelocity.X += 0.025f;
                        ballVelIncreaseTimer.Reset();
                    }
                    else
                    {
                        ballVelocity.X -= 0.025f;
                        ballVelIncreaseTimer.Reset();
                    }
                }
            }

            this.Body.SetLinearVelocity(ballVelocity);
        }

        public void Draw(RenderWindow window)
        {
            this.Sprite.Position = new Vector2f(PixelsToMeter * this.Body.GetPosition().X, PixelsToMeter * this.Body.GetPosition().Y);
            window.Draw(this.Sprite);
        }
    }
}
