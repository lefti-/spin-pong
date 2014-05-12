/*
    Spin Pong
    This software uses The MIT License (MIT). See license agreement LICENSE.txt for full details.

    YOUR GOAL:
    Apply spin to the ball. After certain amount of spin, your chances of winning are much bigger.
    Be quick, the ball becomes faster and faster as time passes!

    SFML is used for windowing, handling events and input, rendering and audio.
    Box2D is used for collision detection.
*/

/////////////////////////////////////////////////////////////////////////////////////////////////
// CHECKLIST
/////////////////////////////////////////////////////////////////////////////////////////////////
// Difficulty settings:
//      * Easy: 5 spins.
//      * Medium: 10 spins.
//      * Hard: 15 spins.
//      * Impossible: 18-20 spins.
//
// TO-DO: Pressing Right Mouse button or Space, "respawns" the ball if the ball glitches
// TO-DO: Menus
// TO-DO: Sounds
// TO-DO: Pause
//
// BUG: Sometimes when the ball gets between enemy and wall, the ball goes through the wall and the game essentially freezes
// WHY?: *Ball going through the wall: Due to the collisions getting glitched, I haven't defined what happens when the ball gets
//                            stuck between the paddle and the wall.
//       *The game freezes: Because the ball flies off into the distance until infinity
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


namespace sfml_pong
{   // Box2D's ContactListener for collision detection
    class MyContactListener : ContactListener
    {
        public override void Add(ContactPoint point)
        {
            String shapeA = (String)point.Shape1.UserData;
            String shapeB = (String)point.Shape2.UserData;

            Entity typeA = (Entity)point.Shape1.GetBody().GetUserData();
            Entity typeB = (Entity)point.Shape2.GetBody().GetUserData();

            // Ball collision with Enemy
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

            // Ball collision with Player            
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

            // Ball collision with Wall
            else if ((typeA is Ball && typeB is Wall) || (typeB is Wall && typeA is Ball))
            {
                Ball theBall = (Ball)typeA;

                if (theBall.contactCounter == 0)
                {
                    theBall.contactCounter++;
                    theBall.collisionWithWall = true;
                }
            }

            // Ball collision with Goal
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

            // Player collision with Wall
            else if ((typeA is Player && typeB is Wall) || (typeB is Wall && typeA is Player))
            {
                Player thePlayer = (Player)typeA;

                if (thePlayer.contactCounter == 0)
                {
                }

                thePlayer.collisionWithWall = true;

            }

            // Enemy collision with Wall
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

            // Ball collision with Enemy
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

            // Ball collision with Player
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

            // Ball collision with Wall
            else if ((typeA is Ball && typeB is Wall) || (typeB is Ball && typeA is Wall))
            {
                Ball theBall = (Ball)typeA;

                if (theBall.contactCounter == 1)
                {
                    theBall.contactCounter--;
                    theBall.collisionWithWall = false;
                }
            }

            // Ball collision with Goal
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

            // Player collision with Wall
            else if ((typeA is Player && typeB is Wall) || (typeB is Wall && typeA is Player))
            {
                Player thePlayer = (Player)typeA;

                if (thePlayer.contactCounter == 1)
                {
                    thePlayer.contactCounter--;
                }

                thePlayer.collisionWithWall = false;
            }

            // Enemy collision with Wall
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
        // Box2D doesn't use pixels as units, but meters
        // To work correctly with pixels, convert meters to pixels
        public const float PixelsToMeter = 32.0f;
    }


    public class Ball : Entity
    {
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
        private Body ballBody;
        private PolygonDef ballPoly;
        private Sprite ballSprite;
        private Random random;
        public Body Body { get { return this.ballBody; } }
        public Sprite Sprite { get { return this.ballSprite; } }

        public Ball(World world, float positionX, float positionY)
        {
            // We need to define a body with the position
            BodyDef ballBodyDef = new BodyDef();
            ballBodyDef.Position.Set(positionX / PixelsToMeter, positionY / PixelsToMeter);

            // Create the physics body
            this.ballBody = world.CreateBody(ballBodyDef);

            // We then set the user data in the body
            // so we can access it later on
            this.ballBody.SetUserData(this);

            // Define a new shape def
            this.ballPoly = new PolygonDef();
            this.ballPoly.IsSensor = true;
            this.ballPoly.SetAsBox(HalfWidth / PixelsToMeter, HalfHeight / PixelsToMeter);
            this.ballPoly.Density = 1.0f;

            // Create a texture from filename
            Texture texture = new Texture("ball2_24x24.png");

            // Create a sprite based on texture
            this.ballSprite = new Sprite(texture) { Origin = new Vector2f(HalfWidth, HalfHeight) };
            this.ballSprite.Position = new Vector2f(PixelsToMeter * ballBody.GetPosition().X, PixelsToMeter * ballBody.GetPosition().Y);

            // Finalize the shape and body
            this.ballBody.CreateShape(ballPoly);
            this.ballBody.SetMassFromShapes();

            this.random = new Random();
            RandomizeBallDirection();
        }

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

        public void Update(Player player, Enemy enemy, StopWatch enemyStartTimer, StopWatch ballVelIncreaseTimer)
        {
            double enemyStartElapsed = enemyStartTimer.ElapsedTime.Seconds;
            double ballVelIncreaseElapsed = ballVelIncreaseTimer.ElapsedTime.Seconds;

            Vec2 ballVelocity = this.Body.GetLinearVelocity();

            if (this.collisionWithWall)
            {
                ballVelocity.Y *= -1;
                this.collisionWithWall = false;
            }

            if (this.collisionWithPaddleSidePlayer)
            {
                ballVelocity.X *= -1;

                // Player moving up
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
                // Player moving down
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
                ballVelocity.X *= -1;
                this.collisionWithPaddleSideEnemy = false;
            }

            if (this.collisionWithPaddleTop)
            {
                if (ballVelocity.Y > 0)
                {
                    ballVelocity.Y *= -1;
                }

                this.collisionWithPaddleTop = false;
            }

            if (this.collisionWithPaddleBottom)
            {
                if (ballVelocity.Y < 0)
                {
                    ballVelocity.Y *= -1;
                }

                this.collisionWithPaddleBottom = false;
            }

            // Enemy gets a score
            if (this.collisionWithPlayerGoal)
            {
                this.Body.SetXForm(new Vec2(player.Body.GetPosition().X + (25 / PixelsToMeter), player.Body.GetPosition().Y), 0);
                ballVelocity.Y = 0;
                ballVelocity.X = 0;
                enemy.score++;
                this.collisionWithPlayerGoal = false;
                this.playerStart = true;
            }

            // Player gets a score
            if (this.collisionWithEnemyGoal)
            {
                this.Body.SetXForm(new Vec2(enemy.Body.GetPosition().X - (24 / PixelsToMeter), enemy.Body.GetPosition().Y), 0);
                ballVelocity.Y = 0;
                ballVelocity.X = 0;
                player.score++;
                this.collisionWithEnemyGoal = false;
                this.enemyStart = true;
            }

            // After ball collides with player goal...
            if (this.playerStart)
            {
                this.Body.SetXForm(new Vec2((player.Body.GetPosition().X + 24 / PixelsToMeter), player.Body.GetPosition().Y), 0);
                ballVelocity.X = 0;
                spinCounter = 0;

                // Player launches ball
                if (this.playerLaunch)
                {

                    // Choose ball direction based on player velocity
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
                    // Player not moving, randomize ball direction
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

            // After ball collides with enemy goal...
            if (this.enemyStart)
            {
                enemyStartTimer.Start();
                spinCounter = 0;

                // Enemy launches the ball in random direction
                if (this.random.Next(0, 2) == 0)
                {
                    ballVelocity.X = 0;

                    if (enemyStartElapsed >= 1.2f)
                    {
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
                        this.enemyStart = false;
                        ballVelocity.Y = 6;
                        ballVelocity.X = -6;
                        enemyStartTimer.Reset();
                    }
                }
            }
            // Gradually increase ball x-velocity, as time passes
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
        private const int HalfWidth = 12;
        private const int HalfHeight = 64;
        public int contactCounter = 0;
        public int score = 0;
        public bool up = false;
        public bool down = false;
        public bool collisionWithWall = false;
        private Body playerBody;
        private PolygonDef rightShape;
        private PolygonDef topShape;
        private PolygonDef bottomShape;
        private Sprite playerSprite;
        public Body Body { get { return this.playerBody; } }
        public Sprite Sprite { get { return this.playerSprite; } }

        public Player(World world, float positionX, float positionY)
        {
            // We need to define a body with the position
            BodyDef playerBodyDef = new BodyDef();
            playerBodyDef.Position.Set(positionX / PixelsToMeter, positionY / PixelsToMeter);

            // Create the physics body
            this.playerBody = world.CreateBody(playerBodyDef);
            // Set player body as a bullet (for continuous collision detection on high velocities)
            this.playerBody.SetBullet(true);

            // We then set the user data in the body
            // so we can access it later on
            this.playerBody.SetUserData(this);

            // Define a new shape definitions
            this.rightShape = new PolygonDef() { UserData = "side", IsSensor = true, Density = 1.0f };
            // (4px, 124px) fixture at (player.body: 22, 2)
            this.rightShape.SetAsBox(2f / PixelsToMeter, (HalfHeight - 2) / PixelsToMeter, new Vec2(10 / PixelsToMeter, 2 / PixelsToMeter), 0);

            this.topShape = new PolygonDef() { UserData = "top", IsSensor = true, Density = 1.0f };
            // (24px, 4px) fixture at (player.body: 0, 0)
            this.topShape.SetAsBox(HalfWidth / PixelsToMeter, 2 / PixelsToMeter, new Vec2(-12 / PixelsToMeter, -HalfHeight / PixelsToMeter), 0);

            this.bottomShape = new PolygonDef() { UserData = "bottom", IsSensor = true, Density = 1.0f };
            // (24px, 4px) fixture at (player.body: 0, 128)
            this.bottomShape.SetAsBox(HalfWidth / PixelsToMeter, 2f / PixelsToMeter, new Vec2(-12 / PixelsToMeter, HalfHeight / PixelsToMeter), 0);

            // Create a texture from filename
            Texture texture = new Texture("paddle2_24x128.png");

            // Create a sprite based on texture
            this.playerSprite = new Sprite(texture) { Origin = new Vector2f(HalfWidth, HalfHeight) };
            this.playerSprite.Position = new Vector2f(PixelsToMeter * playerBody.GetPosition().X, PixelsToMeter * playerBody.GetPosition().Y);

            // Finalize the shape and body
            this.playerBody.CreateShape(rightShape);
            this.playerBody.CreateShape(topShape);
            this.playerBody.CreateShape(bottomShape);
            this.playerBody.SetMassFromShapes();
        }

        public void Update(RenderWindow window)
        {
            Vec2 playerVelocity = new Vec2();
            float mousePosY = Mouse.GetPosition(window).Y / PixelsToMeter;
            float playerPosY = this.Body.GetPosition().Y;

            // Player collides with wall
            if (this.collisionWithWall)
            {
                playerVelocity.Y = 0;

                // Collision with top wall
                if (playerPosY < ((window.Size.Y) / 2) / PixelsToMeter)
                {
                    // End collision
                    if (this.collisionWithWall && mousePosY > (Wall.HalfHeight + Wall.HalfHeight + HalfHeight) / PixelsToMeter)
                    {
                        this.collisionWithWall = false;
                    }
                }
                // Collision with bottom wall
                else if (playerPosY > ((window.Size.Y) / 2) / PixelsToMeter)
                {
                    // End collision
                    if (this.collisionWithWall && mousePosY < (window.Size.Y - (Wall.HalfHeight + Wall.HalfHeight + HalfHeight)) / PixelsToMeter)
                    {
                        this.collisionWithWall = false;
                    }
                }
            }
            // No collisions
            else
            {
                // Mouse cursor up or down from player's center
                if (mousePosY - playerPosY > 0.01f || mousePosY - playerPosY < -0.01f)
                {
                    playerVelocity.Y = (mousePosY - playerPosY) * 60;
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
        private const int HalfWidth = 12;
        private const int HalfHeight = 64;
        public int contactCounter = 0;
        public int score = 0;
        public bool collisionWithWall = false;
        public Body enemyBody;
        public Sprite enemySprite;
        private PolygonDef sideShape;
        private PolygonDef topShape;
        private PolygonDef bottomShape;
        public Body Body { get { return this.enemyBody; } }
        public Sprite Sprite { get { return this.enemySprite; } }

        public Enemy(World world, float positionX, float positionY)
        {
            // We need to define a body with the position
            BodyDef enemyBodyDef = new BodyDef();
            enemyBodyDef.Position.Set(positionX / PixelsToMeter, positionY / PixelsToMeter);

            // Create the physics body
            this.enemyBody = world.CreateBody(enemyBodyDef);

            // We then set the user data in the body
            // so we can access it later on
            this.enemyBody.SetUserData(this);

            // Define a new shape definition
            this.sideShape = new PolygonDef();
            this.sideShape.IsSensor = true;
            this.sideShape.Density = 1.0f;
            // (4px, 124px) fixture at (player.body: 2, 2)
            this.sideShape.SetAsBox(2f / PixelsToMeter, (HalfHeight - 2) / PixelsToMeter, new Vec2(-10 / PixelsToMeter, 2 / PixelsToMeter), 0);
            this.sideShape.UserData = "side";

            // Define a new shape definition
            this.topShape = new PolygonDef();
            this.topShape.IsSensor = true;
            this.topShape.Density = 1.0f;
            // (24px, 4px) fixture at (enemy.body: 15?? (why 0 doesn't work?), 0)
            this.topShape.SetAsBox(HalfWidth / PixelsToMeter, 2f / PixelsToMeter, new Vec2(3 / PixelsToMeter, -HalfHeight / PixelsToMeter), 0);
            this.topShape.UserData = "top";

            // Define a new shape definition
            this.bottomShape = new PolygonDef();
            this.bottomShape.IsSensor = true;
            this.bottomShape.Density = 1.0f;
            // (24px, 4px) fixture at (enemy.body: 15??, 128)
            this.bottomShape.SetAsBox(HalfWidth / PixelsToMeter, 2f / PixelsToMeter, new Vec2(3 / PixelsToMeter, HalfHeight / PixelsToMeter), 0);
            this.bottomShape.UserData = "bottom";

            // Create a texture from filename
            Texture texture = new Texture("paddle2_24x128.png");

            // Create a sprite based on texture
            this.enemySprite = new Sprite(texture) { Origin = new Vector2f(HalfWidth, HalfHeight) };
            this.enemySprite.Position = new Vector2f(PixelsToMeter * enemyBody.GetPosition().X, PixelsToMeter * enemyBody.GetPosition().Y);

            // Finalize the shape and body
            this.enemyBody.CreateShape(sideShape);
            this.enemyBody.CreateShape(topShape);
            this.enemyBody.CreateShape(bottomShape);
            this.enemyBody.SetMassFromShapes();
        }

        public void Update(Ball ball)
        {

            Vec2 enemyVelocity = this.Body.GetLinearVelocity();
            Vec2 ballVelocity = ball.Body.GetLinearVelocity();

            // Ball moves to the right
            if (ballVelocity.X > 0)
            {
                if (this.collisionWithWall)
                {
                    enemyVelocity.Y = 0;

                    // Collision with top wall
                    if (this.Body.GetPosition().Y < 500 / PixelsToMeter)
                    {
                        //  End collision with wall, when ball.pos.Y > enemy.pos.Y
                        if (this.collisionWithWall && ball.Body.GetPosition().Y > this.Body.GetPosition().Y)
                        {
                            this.collisionWithWall = false;
                        }
                    }
                    // Collision with bottom wall
                    else
                    {
                        //  End collision with wall, when ball.pos.Y < enemy.pos.Y
                        if (this.collisionWithWall && ball.Body.GetPosition().Y < this.Body.GetPosition().Y)
                        {
                            this.collisionWithWall = false;
                        }
                    }
                }
                // No collision
                else
                {
                    // Move down if the ball is down from enemy
                    if (ball.Body.GetPosition().Y > this.Body.GetPosition().Y + 5 / PixelsToMeter)
                    {
                        // Ball going down
                        if (ballVelocity.Y > 0)
                        {
                            if (ball.spinCounter < 15)
                            {
                                enemyVelocity.Y = ballVelocity.X + ballVelocity.Y; // enemyVelocity.Y = ballVelocity.Y - 0.5f;
                            }
                            else if (ball.spinCounter >= 15)
                            {
                                enemyVelocity.Y = ballVelocity.X - (ballVelocity.X * 0.50f);
                                Console.WriteLine("EnemyVelY: " + enemyVelocity.Y);
                            }
                        }
                        // Ball going up
                        else if (ballVelocity.Y < 0)
                        {
                            if (ball.spinCounter < 15)
                            {
                                enemyVelocity.Y = ballVelocity.X - ballVelocity.Y; // enemyVelocity.Y = -ballVelocity.Y - 0.5f;
                            }
                            else if (ball.spinCounter >= 15)
                            {
                                enemyVelocity.Y = ballVelocity.X - (ballVelocity.X * 0.50f);
                                Console.WriteLine("EnemyVelY: " + enemyVelocity.Y);
                            }
                        }
                        else
                        {
                            enemyVelocity.Y = 0;
                        }
                    }
                    // Move up if the ball is up from enemy
                    else if (ball.Body.GetPosition().Y < this.Body.GetPosition().Y - 5 / PixelsToMeter)
                    {
                        // Ball going down
                        if (ballVelocity.Y > 0)
                        {
                            if (ball.spinCounter < 15)
                            {
                                enemyVelocity.Y = -ballVelocity.X - ballVelocity.Y; // enemyVelocity.Y = -ballVelocity.Y + 0.5f;
                            }
                            else if (ball.spinCounter >= 15)
                            {
                                enemyVelocity.Y = -ballVelocity.X + (ballVelocity.X * 0.50f);
                                Console.WriteLine("EnemyVelY: " + enemyVelocity.Y);
                            }
                        }
                        // Ball going up
                        else if (ballVelocity.Y < 0)
                        {
                            if (ball.spinCounter < 15)
                            {
                                enemyVelocity.Y = -ballVelocity.X + ballVelocity.Y; // enemyVelocity.Y = ballVelocity.Y + 0.5f;
                            }
                            else if (ball.spinCounter >= 15)
                            {
                                enemyVelocity.Y = -ballVelocity.X + (ballVelocity.X * 0.50f);
                                Console.WriteLine("EnemyVelY: " + enemyVelocity.Y);
                            }
                        }
                        else
                        {
                            enemyVelocity.Y = 0;
                        }
                    }
                    // Without this else, Enemy velocity goes from negative to positive in an instant,
                    // introducing bugs when enemy is colliding
                    else
                    {
                        enemyVelocity.Y = 0;
                    }
                }
            }
            // Ball moves to the left
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
        public const int HalfWidth = 512;
        public const int HalfHeight = 16;
        private Body wallBody;
        private PolygonDef wallPoly;
        private Sprite wallSprite;
        public Body Body { get { return this.wallBody; } }
        public Sprite Sprite { get { return this.wallSprite; } }

        public Wall(World world, float positionX, float positionY)
        {
            // We need to define a body with the position
            BodyDef wallBodyDef = new BodyDef();
            wallBodyDef.Position.Set(positionX / PixelsToMeter, positionY / PixelsToMeter);

            // Create the physics body
            this.wallBody = world.CreateBody(wallBodyDef);

            // We then set the user data in the body
            // so we can access it later on
            this.wallBody.SetUserData(this);

            // Define a new shape def
            this.wallPoly = new PolygonDef();
            // EdgeShapes
            this.wallPoly.IsSensor = true;
            this.wallPoly.SetAsBox(HalfWidth / PixelsToMeter, HalfHeight / PixelsToMeter);

            // Create a texture from filename
            Texture texture = new Texture("border2_1024x32.png");

            // Create a sprite based on texture
            this.wallSprite = new Sprite(texture) { Origin = new Vector2f(HalfWidth, HalfHeight) };
            this.wallSprite.Position = new Vector2f(PixelsToMeter * wallBody.GetPosition().X, PixelsToMeter * wallBody.GetPosition().Y);

            // Finalize the shape and body
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

        public const int HalfWidth = 3;
        public const int HalfHeight = 512;

        public Body Body { get { return this.goalBody; } }
        public Sprite Sprite { get { return this.goalSprite; } }

        public Goal(World world, float positionX, float positionY)
        {
            // We need to define a body with the position
            BodyDef goalBodyDef = new BodyDef();
            goalBodyDef.Position.Set(positionX / PixelsToMeter, positionY / PixelsToMeter);

            // Create the physics body
            this.goalBody = world.CreateBody(goalBodyDef);

            // We then set the user data in the body
            // so we can access it later on
            this.goalBody.SetUserData(this);

            // Define a new shape def
            this.goalPoly = new PolygonDef();
            this.goalPoly.IsSensor = true;
            this.goalPoly.SetAsBox(HalfWidth / PixelsToMeter, HalfHeight / PixelsToMeter);

            // Create a texture from filename
            Texture texture = new Texture("goal_6x1024.png");

            // Create a sprite based on texture
            this.goalSprite = new Sprite(texture) { Origin = new Vector2f(HalfWidth, HalfHeight) };
            this.goalSprite.Position = new Vector2f(PixelsToMeter * goalBody.GetPosition().X, PixelsToMeter * goalBody.GetPosition().Y);

            // Finalize the shape and body
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


    abstract class Scene
    {
        public Scene() { }
        public virtual void Draw() { }
        public virtual void Update() { }
    }


    class GameScene : Scene
    {
        private World world;
        private Font arial;
        private Text playerScoreText;
        private Text enemyScoreText;
        private Text spinCounterText;
        private Text ballSpeedText;
        private Player player;
        private Enemy enemy;
        private Ball ball;
        private Wall topWall;
        private Wall bottomWall;
        private Goal playerGoal;
        private Goal enemyGoal;
        private Sprite midlineSprite;
        private MyContactListener contactListener;
        private float velocityX;
        public float dt = 1 / 60.0f / 30; // Physics time-step with a smaller simulation of sub-steps = 30

        public GameScene(RenderWindow window)
        {
            // Set gravity value
            Vec2 gravity = new Vec2(0.0f, 0.0f);

            // Create world bounding box, where simulations occur
            AABB worldAABB = new AABB();
            worldAABB.LowerBound.Set(0.0f, 0.0f);
            worldAABB.UpperBound.Set(window.Size.X, window.Size.Y);

            // Set up the physics world
            this.world = new World(worldAABB, gravity, false);

            // Set up a contact listener for collision detection/callbacks
            contactListener = new MyContactListener();
            this.world.SetContactListener(contactListener);

            // Create objects
            this.player = new Player(this.world, 100, (window.Size.Y / 2));
            this.enemy = new Enemy(this.world, window.Size.X - 100, (window.Size.Y / 2));
            this.ball = new Ball(this.world, (window.Size.X / 2), (window.Size.Y / 2));
            this.topWall = new Wall(this.world, (window.Size.X / 2), 16);
            this.bottomWall = new Wall(this.world, (window.Size.X / 2), window.Size.Y - 16);
            this.playerGoal = new Goal(this.world, 3, (window.Size.Y / 2));
            this.enemyGoal = new Goal(this.world, window.Size.X - 3, (window.Size.Y / 2));

            // Set font
            this.arial = new Font("arial.ttf");

            // Create mid-line
            Texture texture = new Texture("midline_10x1024.png");
            this.midlineSprite = new Sprite(texture);
            this.midlineSprite.Position = new Vector2f(((window.Size.X / 2) - 5), 0);

            // Setup event handlers
            window.Closed += new EventHandler(OnWindowClose);
            window.KeyPressed += new EventHandler<KeyEventArgs>(OnKeyPress);
            window.KeyReleased += new EventHandler<KeyEventArgs>(OnKeyRelease);
            window.MouseButtonPressed += new EventHandler<MouseButtonEventArgs>(OnMouseButtonPress);
            window.MouseButtonReleased += new EventHandler<MouseButtonEventArgs>(OnMouseButtonRelease);
        }

        public void DrawTexts(RenderWindow window)
        {
            // Create text objects
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

            // Display ball speed always as positive float
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
                window.Close();
            }
            else if (e.Code == Keyboard.Key.Up)
            {
                this.player.up = true;
            }
            else if (e.Code == Keyboard.Key.Down)
            {
                this.player.down = true;
            }
            else if (e.Code == Keyboard.Key.Space)
            {
                this.ball.playerLaunch = true;
            }
        }

        public void OnKeyRelease(object sender, KeyEventArgs e)
        {
            RenderWindow window = (RenderWindow)sender;

            if (e.Code == Keyboard.Key.Up)
            {
                this.player.up = false;
            }
            else if (e.Code == Keyboard.Key.Down)
            {
                this.player.down = false;
            }
            else if (e.Code == Keyboard.Key.Space)
            {
                this.ball.playerLaunch = false;
            }
        }

        public void Update(StopWatch enemyStartTimer, StopWatch ballVelIncreaseTimer, RenderWindow window)
        {
            // Simulate a smaller timestep, to prevent tunneling on high velocity
            for (int i = 0; i < 30; i++)
            {
                this.world.Step(dt, 6, 3);

                // Fast moving physics related stuff here
                this.ball.Update(this.player, this.enemy, enemyStartTimer, ballVelIncreaseTimer);
                this.player.Update(window);
                this.enemy.Update(this.ball);
            }
            // Slow moving physics related stuff here
        }

        public void Draw(RenderWindow window)
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
        }
    }


    class Pong
    {
        public const int WindowWidth = 1024;
        public const int WindowHeight = 768;
        public const string Title = "Pong";

        static void Main()
        {
            // Create window
            RenderWindow window = new RenderWindow(new VideoMode(WindowWidth, WindowHeight), Title);
            window.SetMouseCursorVisible(false);
            window.SetKeyRepeatEnabled(false);
            //window.SetVerticalSyncEnabled(true);
            window.SetFramerateLimit(60);

            GameScene firstScene = new GameScene(window);

            StopWatch enemyStartTimer = new StopWatch();
            StopWatch ballVelIncreaseTimer = new StopWatch();

            // Main loop
            while (window.IsOpen())
            {
                // Handle events
                window.DispatchEvents();

                // Update objects
                firstScene.Update(enemyStartTimer, ballVelIncreaseTimer, window);

                // Clear screen
                window.Clear();

                // Render everything
                firstScene.Draw(window);

                // Update the display
                window.Display();
            }
        }
    }
}