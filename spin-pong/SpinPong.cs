/*
    Spin Pong
    This software uses The MIT License (MIT). See license agreement LICENSE for full details.

    YOUR GOAL:
        - Apply spin to the ball. After certain amount of spin, your chances of scoring are MUCH higher.
        - To apply spin: when hitting the ball, move paddle to the same direction where the ball moves.
        - Score 3 points to win.
        - Be quick, the ball becomes faster and faster as time passes!

    FRAMEWORKS/LIBRARIES USED:
        - SFML is used for windowing, handling events and input, rendering graphics and audio.
        - Box2D is used for collision detection.
        - NetEXT is used for more accurate time-related functions.
*/

/////////////////////////////////////////////////////////////////////////////////////////////////
// CHECKLIST
/////////////////////////////////////////////////////////////////////////////////////////////////
//
//  SOUND ATTRIBUTIONS:
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
    // Box2D's ContactListener is implemented for collision detection.
    class MyContactListener : ContactListener
    {
        public override void Add(ContactPoint point)
        {
            String shapeA = (String)point.Shape1.UserData;
            String shapeB = (String)point.Shape2.UserData;

            Entity typeA = (Entity)point.Shape1.GetBody().GetUserData();
            Entity typeB = (Entity)point.Shape2.GetBody().GetUserData();

            // Ball collision with Enemy.
            if ((typeA is Enemy && typeB is Ball) || (typeB is Ball && typeA is Enemy))
            {
                Ball theBall = (Ball)typeB;

                if (theBall.contactCounter == 0)
                {
                    theBall.contactCounter++;
                }

                if (shapeA == "side")
                {
                    theBall.collisionWithPaddleSideEnemy = true;
                }
                if (shapeA == "top")
                {
                    theBall.collisionWithPaddleTop = true;
                }
                if (shapeA == "bottom")
                {
                    theBall.collisionWithPaddleBottom = true;
                }
            }

            // Ball collision with Player.
            else if ((typeA is Player && typeB is Ball) || (typeB is Ball && typeA is Player))
            {
                Ball theBall = (Ball)typeB;

                if (theBall.contactCounter == 0)
                {
                    theBall.contactCounter++;
                }

                if (shapeA == "side")
                {
                    theBall.collisionWithPaddleSidePlayer = true;
                }
                if (shapeA == "top")
                {
                    theBall.collisionWithPaddleTop = true;
                }
                if (shapeA == "bottom")
                {
                    theBall.collisionWithPaddleBottom = true;
                }
            }

            // Ball collision with Wall.
            else if ((typeA is Ball && typeB is Wall) || (typeB is Wall && typeA is Ball))
            {
                Ball theBall = (Ball)typeA;

                if (theBall.contactCounter == 0)
                {
                    theBall.contactCounter++;
                    theBall.collisionWithWall = true;
                }
            }

            // Ball collision with Goal.
            else if ((typeA is Ball && typeB is Goal) || (typeB is Goal && typeA is Ball))
            {
                Ball theBall = (Ball)typeA;

                Vec2 velocity = theBall.Body.GetLinearVelocity();

                if (theBall.contactCounter == 0)
                {
                    theBall.contactCounter++;
                }

                if (velocity.X > 0)
                {
                    theBall.collisionWithEnemyGoal = true;
                }
                else
                {
                    theBall.collisionWithPlayerGoal = true;
                }
            }

            // Player collision with Wall.
            else if ((typeA is Player && typeB is Wall) || (typeB is Wall && typeA is Player))
            {
                Player thePlayer = (Player)typeA;

                if (thePlayer.contactCounter == 0)
                {
                }

                thePlayer.collisionWithWall = true;

            }

            // Enemy collision with Wall.
            else if ((typeA is Enemy && typeB is Wall) || (typeB is Wall && typeA is Enemy))
            {
                Enemy theEnemy = (Enemy)typeA;

                if (theEnemy.contactCounter == 0)
                {
                    theEnemy.contactCounter++;
                    theEnemy.collisionWithWall = true;
                }
            }
        }
        public override void Remove(ContactPoint point)
        {
            String shapeA = (String)point.Shape1.UserData;
            String shapeB = (String)point.Shape2.UserData;

            Entity typeA = (Entity)point.Shape1.GetBody().GetUserData();
            Entity typeB = (Entity)point.Shape2.GetBody().GetUserData();

            // Ball collision with Enemy.
            if ((typeA is Enemy && typeB is Ball) || (typeB is Ball && typeA is Enemy))
            {
                Ball theBall = (Ball)typeB;

                if (theBall.contactCounter == 1)
                {
                    theBall.contactCounter--;
                }

                if (shapeA == "side")
                {
                    theBall.collisionWithPaddleSideEnemy = false;
                }
                if (shapeA == "top")
                {
                    theBall.collisionWithPaddleTop = false;
                }
                if (shapeA == "bottom")
                {
                    theBall.collisionWithPaddleBottom = false;
                }
            }

            // Ball collision with Player.
            else if ((typeA is Player && typeB is Ball) || (typeB is Ball && typeA is Player))
            {
                Ball theBall = (Ball)typeB;

                if (theBall.contactCounter == 1)
                {
                    theBall.contactCounter--;
                }

                if (shapeA == "side")
                {
                    theBall.collisionWithPaddleSidePlayer = false;
                }
                if (shapeA == "top")
                {
                    theBall.collisionWithPaddleTop = false;
                }
                if (shapeA == "bottom")
                {
                    theBall.collisionWithPaddleBottom = false;
                }
            }

            // Ball collision with Wall.
            else if ((typeA is Ball && typeB is Wall) || (typeB is Ball && typeA is Wall))
            {
                Ball theBall = (Ball)typeA;

                if (theBall.contactCounter == 1)
                {
                    theBall.contactCounter--;
                    theBall.collisionWithWall = false;
                }
            }

            // Ball collision with Goal.
            else if ((typeA is Ball && typeB is Goal) || (typeB is Goal && typeA is Ball))
            {
                Ball theBall = (Ball)typeA;

                if (theBall.contactCounter == 1)
                {
                    theBall.contactCounter--;
                    theBall.collisionWithEnemyGoal = false;
                    theBall.collisionWithPlayerGoal = false;
                }
            }

            // Player collision with Wall.
            else if ((typeA is Player && typeB is Wall) || (typeB is Wall && typeA is Player))
            {
                Player thePlayer = (Player)typeA;

                if (thePlayer.contactCounter == 1)
                {
                    thePlayer.contactCounter--;
                }

                thePlayer.collisionWithWall = false;
            }

            // Enemy collision with Wall.
            else if ((typeA is Enemy && typeB is Wall) || (typeB is Wall && typeA is Enemy))
            {
                Enemy theEnemy = (Enemy)typeA;

                if (theEnemy.contactCounter == 1)
                {
                    theEnemy.contactCounter--;
                    theEnemy.collisionWithWall = true;
                }
            }
        }
        public override void Persist(ContactPoint point) { }
        public override void Result(ContactResult point) { }
    }


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
            Texture texture = new Texture("ball2_24x24.png");

            // Create a sprite based on texture.
            this.ballSprite = new Sprite(texture) { Origin = new Vector2f(HalfWidth, HalfHeight) };
            this.ballSprite.Position = new Vector2f(PixelsToMeter * ballBody.GetPosition().X, PixelsToMeter * ballBody.GetPosition().Y);

            // Finalize the shape and body.
            this.ballBody.CreateShape(ballPoly);
            this.ballBody.SetMassFromShapes();

            // Set sound buffer from filename.
            SoundBuffer ballHitsPlayerBuffer = new SoundBuffer("ball_hits_player.wav");
            SoundBuffer ballHitsEnemyBuffer = new SoundBuffer("ball_hits_enemy.wav");
            SoundBuffer ballHitsGoalBuffer = new SoundBuffer("scoring_point.wav");

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
            Texture texture = new Texture("paddle2_24x128.png");

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
            Texture texture = new Texture("paddle2_24x128.png");

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
            Texture texture = new Texture("border2_1024x32.png");

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
            Texture texture = new Texture("goal_6x1024.png");

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
            this.arial = new Font("arial.ttf");

            // Set sound buffer from filename.
            SoundBuffer menuClickBackBuffer = new SoundBuffer("menu_click_back.wav");
            SoundBuffer pauseBuffer = new SoundBuffer("pause.wav");
            SoundBuffer unpauseBuffer = new SoundBuffer("unpause.wav");
            SoundBuffer gameOverBuffer = new SoundBuffer("game_over.wav");
            SoundBuffer gameWonBuffer = new SoundBuffer("game_won.wav");

            // Create sound based on sound buffer.
            this.menuClickBack = new Sound(menuClickBackBuffer);
            this.pause = new Sound(pauseBuffer);
            this.unpause = new Sound(unpauseBuffer);
            this.gameOver = new Sound(gameOverBuffer);
            this.gameWon = new Sound(gameWonBuffer);

            // Create mid-line.
            Texture texture = new Texture("midline_10x1024.png");
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
            this.playerScoreText.Color = new Color(0, 38, 255);

            string enemyScoreFormat = string.Format("{0}", this.enemy.score);
            this.enemyScoreText = new Text(enemyScoreFormat, arial);
            this.enemyScoreText.Position = new Vector2f(((window.Size.X / 2) + 50), 60);
            this.enemyScoreText.CharacterSize = 80;
            this.enemyScoreText.Color = new Color(0, 38, 255);

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
            this.arial = new Font("arial.ttf");

            // Set sound buffer from filename.
            SoundBuffer menuClickForwardBuffer = new SoundBuffer("menu_click_forward.wav");

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
            this.arial = new Font("arial.ttf");

            // Set sound buffer from filename.
            SoundBuffer menuClickForwardBuffer = new SoundBuffer("menu_click_forward.wav");
            SoundBuffer menuClickBackBuffer = new SoundBuffer("menu_click_back.wav");

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
            this.arial = new Font("arial.ttf");

            // Set sound buffer from filename.
            SoundBuffer menuClickForwardBuffer = new SoundBuffer("menu_click_forward.wav");

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
            this.arial = new Font("arial.ttf");

            // Set sound buffer from filename.
            SoundBuffer menuClickForwardBuffer = new SoundBuffer("menu_click_forward.wav");

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


    class Pong
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