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
    public class Entity
    {
        // Box2D doesn't use pixels as units, but meters.
        // To work correctly with pixels, convert meters to pixels.
        public const float PixelsToMeter = 32.0f;
    }


    public class Ball : Entity
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


    public class Player : Entity
    {
        private Body playerBody;
        private PolygonDef rightShape;
        private PolygonDef topShape;
        private PolygonDef bottomShape;
        private Sprite playerSprite;
        public Body Body { get { return this.playerBody; } }
        public Sprite Sprite { get { return this.playerSprite; } }
        private const int HalfWidth = 12;
        private const int HalfHeight = 64;
        public int contactCounter = 0;
        public int score = 0;
        public bool collisionWithWall = false;

        public Player(World world, float positionX, float positionY)
        {
            // Define a body with a position.
            BodyDef playerBodyDef = new BodyDef();
            playerBodyDef.Position.Set(positionX / PixelsToMeter, positionY / PixelsToMeter);

            // Create the physics body.
            this.playerBody = world.CreateBody(playerBodyDef);

            // Set player body as a bullet (for continuous collision detection on high velocities).
            this.playerBody.SetBullet(true);

            // Set user data in the body.
            this.playerBody.SetUserData(this);

            // Define new shape definitions.
            this.rightShape = new PolygonDef() { UserData = "side", IsSensor = true, Density = 1.0f };
            // (4px, 124px) fixture at (player.body: 22, 2)
            this.rightShape.SetAsBox(2f / PixelsToMeter, (HalfHeight - 2) / PixelsToMeter, new Vec2(10 / PixelsToMeter, 2 / PixelsToMeter), 0);

            this.topShape = new PolygonDef() { UserData = "top", IsSensor = true, Density = 1.0f };
            // (24px, 4px) fixture at (player.body: 0, 0)
            this.topShape.SetAsBox(HalfWidth / PixelsToMeter, 2 / PixelsToMeter, new Vec2(-12 / PixelsToMeter, -HalfHeight / PixelsToMeter), 0);

            this.bottomShape = new PolygonDef() { UserData = "bottom", IsSensor = true, Density = 1.0f };
            // (24px, 4px) fixture at (player.body: 0, 128)
            this.bottomShape.SetAsBox(HalfWidth / PixelsToMeter, 2f / PixelsToMeter, new Vec2(-12 / PixelsToMeter, HalfHeight / PixelsToMeter), 0);

            // Create a texture from filename.
            Texture texture = new Texture("assets/images/paddle_24x128.png");

            // Create a sprite based on texture.
            this.playerSprite = new Sprite(texture) { Origin = new Vector2f(HalfWidth, HalfHeight) };
            this.playerSprite.Position = new Vector2f(PixelsToMeter * playerBody.GetPosition().X, PixelsToMeter * playerBody.GetPosition().Y);

            // Finalize the shape and body.
            this.playerBody.CreateShape(rightShape);
            this.playerBody.CreateShape(topShape);
            this.playerBody.CreateShape(bottomShape);
            this.playerBody.SetMassFromShapes();
        }

        public void Update(RenderWindow window)
        {
            Vec2 playerVelocity = new Vec2();

            // Transform the mouse position from window coordinates to world coordinates.
            Vector2f mousePos = window.MapPixelToCoords(Mouse.GetPosition(window)) / PixelsToMeter;
            float playerPosY = this.Body.GetPosition().Y;

            // Player collides with wall.
            if (this.collisionWithWall)
            {
                playerVelocity.Y = 0;

                // Collision with top wall.
                if (playerPosY < ((window.Size.Y) / 2) / PixelsToMeter)
                {
                    // End collision.
                    if (this.collisionWithWall && mousePos.Y > (Wall.HalfHeight + Wall.HalfHeight + HalfHeight) / PixelsToMeter)
                    {
                        this.collisionWithWall = false;
                    }
                }
                // Collision with bottom wall.
                else if (playerPosY > ((window.Size.Y) / 2) / PixelsToMeter)
                {
                    // End collision.
                    if (this.collisionWithWall && mousePos.Y < (window.Size.Y - (Wall.HalfHeight + Wall.HalfHeight + HalfHeight)) / PixelsToMeter)
                    {
                        this.collisionWithWall = false;
                    }
                }
            }
            // No collisions.
            else
            {
                // Moving mouse cursor up or down from player's center.
                if (mousePos.Y - playerPosY > 0.01f || mousePos.Y - playerPosY < -0.01f)
                {
                    playerVelocity.Y = (mousePos.Y - playerPosY) * 60;
                }
            }

            this.Body.SetLinearVelocity(playerVelocity);
        }

        public void Draw(RenderWindow window)
        {
            this.Sprite.Position = new Vector2f(PixelsToMeter * this.Body.GetPosition().X, PixelsToMeter * this.Body.GetPosition().Y);
            window.Draw(this.Sprite);
        }
    }


    public class Enemy : Entity
    {
        public Body enemyBody;
        public Sprite enemySprite;
        private PolygonDef sideShape;
        private PolygonDef topShape;
        private PolygonDef bottomShape;
        public Body Body { get { return this.enemyBody; } }
        public Sprite Sprite { get { return this.enemySprite; } }
        private const int HalfWidth = 12;
        private const int HalfHeight = 64;
        private int spinsNeeded;
        public int contactCounter = 0;
        public int score = 0;
        public bool collisionWithWall = false;

        public Enemy(World world, float positionX, float positionY, int spinsNeeded)
        {
            // Enemy difficulty setting.
            this.spinsNeeded = spinsNeeded;

            // Define a body with a position.
            BodyDef enemyBodyDef = new BodyDef();
            enemyBodyDef.Position.Set(positionX / PixelsToMeter, positionY / PixelsToMeter);

            // Create the physics body.
            this.enemyBody = world.CreateBody(enemyBodyDef);

            // Set user data in the body.
            this.enemyBody.SetUserData(this);

            // Define new shape definitions.
            this.sideShape = new PolygonDef();
            this.sideShape.IsSensor = true;
            this.sideShape.Density = 1.0f;

            // (4px, 124px) fixture at (player.body: 2, 2)
            this.sideShape.SetAsBox(2f / PixelsToMeter, (HalfHeight - 2) / PixelsToMeter, new Vec2(-10 / PixelsToMeter, 2 / PixelsToMeter), 0);
            this.sideShape.UserData = "side";

            this.topShape = new PolygonDef();
            this.topShape.IsSensor = true;
            this.topShape.Density = 1.0f;

            // (24px, 4px) fixture at (enemy.body: 15?? (why 0 doesn't work?), 0)
            this.topShape.SetAsBox(HalfWidth / PixelsToMeter, 2f / PixelsToMeter, new Vec2(3 / PixelsToMeter, -HalfHeight / PixelsToMeter), 0);
            this.topShape.UserData = "top";

            this.bottomShape = new PolygonDef();
            this.bottomShape.IsSensor = true;
            this.bottomShape.Density = 1.0f;

            // (24px, 4px) fixture at (enemy.body: 15??, 128)
            this.bottomShape.SetAsBox(HalfWidth / PixelsToMeter, 2f / PixelsToMeter, new Vec2(3 / PixelsToMeter, HalfHeight / PixelsToMeter), 0);
            this.bottomShape.UserData = "bottom";

            // Create a texture from filename.
            Texture texture = new Texture("assets/images/paddle_24x128.png");

            // Create a sprite based on texture.
            this.enemySprite = new Sprite(texture) { Origin = new Vector2f(HalfWidth, HalfHeight) };
            this.enemySprite.Position = new Vector2f(PixelsToMeter * enemyBody.GetPosition().X, PixelsToMeter * enemyBody.GetPosition().Y);

            // Finalize the shape and body.
            this.enemyBody.CreateShape(sideShape);
            this.enemyBody.CreateShape(topShape);
            this.enemyBody.CreateShape(bottomShape);
            this.enemyBody.SetMassFromShapes();
        }

        public void Update(Ball ball)
        {

            Vec2 enemyVelocity = this.Body.GetLinearVelocity();
            Vec2 ballVelocity = ball.Body.GetLinearVelocity();

            // Ball moves to the right.
            if (ballVelocity.X > 0)
            {
                if (this.collisionWithWall)
                {
                    enemyVelocity.Y = 0;

                    // Collision with top wall.
                    if (this.Body.GetPosition().Y < 500 / PixelsToMeter)
                    {
                        //  End collision with wall, when ball.pos.Y > enemy.pos.Y.
                        if (this.collisionWithWall && ball.Body.GetPosition().Y > this.Body.GetPosition().Y)
                        {
                            this.collisionWithWall = false;
                        }
                    }
                    // Collision with bottom wall.
                    else
                    {
                        //  End collision with wall, when ball.pos.Y < enemy.pos.Y.
                        if (this.collisionWithWall && ball.Body.GetPosition().Y < this.Body.GetPosition().Y)
                        {
                            this.collisionWithWall = false;
                        }
                    }
                }
                // No collision.
                else
                {
                    // Move down if the ball is down from enemy.
                    if (ball.Body.GetPosition().Y > this.Body.GetPosition().Y + 5 / PixelsToMeter)
                    {
                        // Ball moving down.
                        if (ballVelocity.Y > 0)
                        {
                            // Enemy catches up to ball.
                            if (ball.spinCounter < spinsNeeded)
                            {
                                enemyVelocity.Y = ballVelocity.X + ballVelocity.Y; // enemyVelocity.Y = ballVelocity.Y - 0.5f;
                            }

                            // Enemy becomes much slower than ball.
                            else if (ball.spinCounter >= spinsNeeded)
                            {
                                enemyVelocity.Y = ballVelocity.X - (ballVelocity.X * 0.50f);
                            }
                        }
                        // Ball moving up.
                        else if (ballVelocity.Y < 0)
                        {
                            // Enemy catches up to ball.
                            if (ball.spinCounter < spinsNeeded)
                            {
                                enemyVelocity.Y = ballVelocity.X - ballVelocity.Y; // enemyVelocity.Y = -ballVelocity.Y - 0.5f;
                            }

                            // Enemy becomes much slower than ball.
                            else if (ball.spinCounter >= spinsNeeded)
                            {
                                enemyVelocity.Y = ballVelocity.X - (ballVelocity.X * 0.50f);
                            }
                        }
                        else
                        {
                            enemyVelocity.Y = 0;
                        }
                    }
                    // Move up if the ball is up from enemy.
                    else if (ball.Body.GetPosition().Y < this.Body.GetPosition().Y - 5 / PixelsToMeter)
                    {
                        // Ball moving down.
                        if (ballVelocity.Y > 0)
                        {
                            // Enemy catches up to ball.
                            if (ball.spinCounter < spinsNeeded)
                            {
                                enemyVelocity.Y = -ballVelocity.X - ballVelocity.Y; // enemyVelocity.Y = -ballVelocity.Y + 0.5f;
                            }

                            // Enemy becomes much slower than ball.
                            else if (ball.spinCounter >= spinsNeeded)
                            {
                                enemyVelocity.Y = -ballVelocity.X + (ballVelocity.X * 0.50f);
                            }
                        }
                        // Ball moving up.
                        else if (ballVelocity.Y < 0)
                        {
                            // Enemy catches up to ball.
                            if (ball.spinCounter < spinsNeeded)
                            {
                                enemyVelocity.Y = -ballVelocity.X + ballVelocity.Y; // enemyVelocity.Y = ballVelocity.Y + 0.5f;
                            }

                            // Enemy becomes much slower than ball.
                            else if (ball.spinCounter >= spinsNeeded)
                            {
                                enemyVelocity.Y = -ballVelocity.X + (ballVelocity.X * 0.50f);
                            }
                        }
                        else
                        {
                            enemyVelocity.Y = 0;
                        }
                    }
                    // Without this else, enemy velocity goes from negative to positive in an instant,
                    // introducing bugs when enemy is colliding.
                    else
                    {
                        enemyVelocity.Y = 0;
                    }
                }
            }
            // Ball moves to the left.
            else
            {
                enemyVelocity.Y = 0;
            }

            this.Body.SetLinearVelocity(enemyVelocity);
        }

        public void Draw(RenderWindow window)
        {
            this.Sprite.Position = new Vector2f(PixelsToMeter * this.Body.GetPosition().X, PixelsToMeter * this.Body.GetPosition().Y);
            window.Draw(this.Sprite);
        }
    }


    public class Wall : Entity
    {
        private Body wallBody;
        private PolygonDef wallPoly;
        private Sprite wallSprite;
        public Body Body { get { return this.wallBody; } }
        public Sprite Sprite { get { return this.wallSprite; } }
        public const int HalfWidth = 512;
        public const int HalfHeight = 16;

        public Wall(World world, float positionX, float positionY)
        {
            // Define a body with a position.
            BodyDef wallBodyDef = new BodyDef();
            wallBodyDef.Position.Set(positionX / PixelsToMeter, positionY / PixelsToMeter);

            // Create the physics body.
            this.wallBody = world.CreateBody(wallBodyDef);

            // Set user data in the body.
            this.wallBody.SetUserData(this);

            // Define a shape definition.
            this.wallPoly = new PolygonDef();
            this.wallPoly.IsSensor = true;
            this.wallPoly.SetAsBox(HalfWidth / PixelsToMeter, HalfHeight / PixelsToMeter);

            // Create a texture from filename.
            Texture texture = new Texture("assets/images/border_1024x32.png");

            // Create a sprite based on texture.
            this.wallSprite = new Sprite(texture) { Origin = new Vector2f(HalfWidth, HalfHeight) };
            this.wallSprite.Position = new Vector2f(PixelsToMeter * wallBody.GetPosition().X, PixelsToMeter * wallBody.GetPosition().Y);

            // Finalize the shape and body.
            this.wallBody.CreateShape(wallPoly);
            this.wallBody.SetMassFromShapes();
        }

        public void Update() { }

        public void Draw(RenderWindow window)
        {
            this.Sprite.Position = new Vector2f(PixelsToMeter * this.Body.GetPosition().X, PixelsToMeter * this.Body.GetPosition().Y);
            window.Draw(this.Sprite);
        }
    }


    public class Goal : Entity
    {
        private Body goalBody;
        private PolygonDef goalPoly;
        private Sprite goalSprite;
        public Body Body { get { return this.goalBody; } }
        public Sprite Sprite { get { return this.goalSprite; } }
        public const int HalfWidth = 3;
        public const int HalfHeight = 512;

        public Goal(World world, float positionX, float positionY)
        {
            // Define a body with a position.
            BodyDef goalBodyDef = new BodyDef();
            goalBodyDef.Position.Set(positionX / PixelsToMeter, positionY / PixelsToMeter);

            // Create the physics body.
            this.goalBody = world.CreateBody(goalBodyDef);

            // Set user data in the body.
            this.goalBody.SetUserData(this);

            // Define a shape definition.
            this.goalPoly = new PolygonDef();
            this.goalPoly.IsSensor = true;
            this.goalPoly.SetAsBox(HalfWidth / PixelsToMeter, HalfHeight / PixelsToMeter);

            // Create a texture from filename.
            Texture texture = new Texture("assets/images/goal_6x1024.png");

            // Create a sprite based on texture.
            this.goalSprite = new Sprite(texture) { Origin = new Vector2f(HalfWidth, HalfHeight) };
            this.goalSprite.Position = new Vector2f(PixelsToMeter * goalBody.GetPosition().X, PixelsToMeter * goalBody.GetPosition().Y);

            // Finalize the shape and body.
            this.goalBody.CreateShape(goalPoly);
            this.goalBody.SetMassFromShapes();
        }

        public void Update() { }

        public void Draw(RenderWindow window)
        {
            this.Sprite.Position = new Vector2f(PixelsToMeter * this.Body.GetPosition().X, PixelsToMeter * this.Body.GetPosition().Y);
            window.Draw(this.Sprite);
        }
    }
}
