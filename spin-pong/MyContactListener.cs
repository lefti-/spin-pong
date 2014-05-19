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
}
