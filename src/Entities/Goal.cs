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
    class Goal : Entity
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
