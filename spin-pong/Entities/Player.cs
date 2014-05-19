/*
    Spin Pong
    This software uses The MIT License (MIT). See license agreement LICENSE.txt for full details.
*/

using Box2DX.Collision;
using Box2DX.Common;
using Box2DX.Dynamics;
using SFML.Graphics;
using SFML.Window;

namespace Entities
{
    class Player : Entity
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
}
