/*
    Spin Pong
    This software uses The MIT License (MIT). See license agreement LICENSE for full details.
*/

using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;
using SFML.Graphics;
using SFML.Window;

namespace Entities
{
    class Enemy : Entity
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
}
