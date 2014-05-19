/*
    Spin Pong
    This software uses The MIT License (MIT). See license agreement LICENSE for full details.
*/

namespace Entities
{
    public class Entity
    {
        // Box2D doesn't use pixels as units, but meters.
        // To work correctly with pixels, convert meters to pixels.
        public const float PixelsToMeter = 32.0f;
    }
}
