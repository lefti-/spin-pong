/*
    Spin Pong
    This software uses The MIT License (MIT). See license agreement LICENSE for full details.
*/

using SFML.Graphics;

namespace GameStates
{
    public abstract class GameState
    {
        public GameState() { }
        public virtual void Initialize(RenderWindow window) { }
        public virtual void BindEvents(RenderWindow window) { }
        public virtual void UnbindEvents(RenderWindow window) { }
        public virtual void Update(RenderWindow window) { }
        public virtual void Draw(RenderWindow window) { }
    }
}
