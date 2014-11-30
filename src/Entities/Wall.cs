/*
    Spin Pong
    This software uses The MIT License (MIT). See license agreement LICENSE.txt for full details.
*/

using Box2DX.Collision;
using Box2DX.Dynamics;
using SFML.Graphics;
using SFML.Window;

namespace Entities
{
    class Wall : Entity
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
}
