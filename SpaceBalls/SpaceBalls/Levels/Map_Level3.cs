using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using SpaceBalls.GameUtilities;

namespace SpaceBalls
{
    class Map_Level3 : Map_Default
    {
        private const float GRAVITATIONAL_CONSTANT = 1.0f;
        private const int NUM_PLANETS = 50;

        public Map_Level3()
            : base(NUM_PLANETS)
        {
        }

        public override void Reset()
        {
            base.Reset();
            m_hole.InitialPosition = new Vector3(1000.0f, 360.0f, 0.0f);
            m_hole.Mass = 100000.0f;
            m_hole.Radius = 20.0f;
            m_hole.Scale = 1.5f;

            m_ball.InitialPosition = new Vector3(200.0f, 360.0f, 0.0f);
            m_ball.Mass = 2500.0f;
            m_ball.Static = true;

            for (int i = 0; i < m_aPlanets.Length; i++)
            {
                m_aPlanets[i].Mass = 50000.0f;
                m_aPlanets[i].Texture = m_textures[random.Next(0, 4)]; // Set a random texture
                m_aPlanets[i].Position = new Vector3(random.Next(400, 1200), random.Next(100, 650), 0.0f); // Set to random position on right side of game area
                m_aPlanets[i].Scale = random.Next(3, 10) / 50.0f; // Set a random scale
                m_aPlanets[i].Velocity = new Vector3((random.Next(0, 2) == 0 ? (float)random.Next(1, 10) : (float)random.Next(-10, -1)), // Set a random small velocity
                    random.Next(0, 2) == 0 ? (float)random.Next(1, 10) : (float)random.Next(-10, -1), 0.0f);
                m_aPlanets[i].SoundEffects = m_soundEffects;
                m_aPlanets[i].Sound = false;
            }

            m_ship.InitialPosition = new Vector3(50, 360.0f, 0.0f);
            m_ship.Mass = 5000.0f;
            m_ship.Radius = 64.0f;
            m_ship.Scale = 0.5f;
            m_ship.Angle = 2.0f;
            m_ship.Static = false;

            m_bullet.InitialPosition = m_ship.Position;
            m_bullet.Mass = 1000000.0f;
            m_bullet.Radius = 2.5f;
            m_bullet.Power = 500.0f;
        }

        public override void Update(GameTime gameTime, GraphicsDevice graphics)
        {
            if (GameInput.Get().IsKeyReleased(Keys.R) || GameInput.Get().IsControllerButtonReleased(Buttons.X))
                Reset();
            if (!CheckWin())
            {
                base.Update(gameTime, graphics);

                m_ship.Input(m_bullet);

                ApplyGravity(m_hole, m_ball);

                for (int i = 0; i < m_aPlanets.Length; i++)
                {
                    ApplyGravity(m_bullet, m_aPlanets[i]);
                    m_ball.ProcessCollision(m_aPlanets[i]);
                    m_ship.ProcessCollision(m_aPlanets[i]);
                    m_bullet.ProcessCollision(m_aPlanets[i]);
                    for (int j = 0; j < m_aPlanets.Length; j++)
                        if (j != i)
                        {
                            m_aPlanets[j].ProcessCollision(m_aPlanets[i]); // Process collision between planets
                        }
                    m_aPlanets[i].ProcessCollision_Boundary(graphics);
                    if (random.Next(0, 500) == 0)
                        m_aPlanets[i].AntiGravity = !m_aPlanets[i].AntiGravity; // Randomly switch planets between gravity states
                    m_aPlanets[i].Update(gameTime);
                }

                if (m_ball.CheckCollision(m_bullet) || m_ball.CheckCollision(m_ship))
                    m_ball.Static = false; // Ball needs to be 'tee-ed off' by either colliding with ship or being shot

                m_ball.ProcessCollision_Boundary(graphics);
                m_bullet.ProcessCollision_Boundary(graphics);
                m_ship.ProcessCollision_Boundary(graphics);

                m_bullet.ProcessCollision(m_ball);
                m_ship.ProcessCollision(m_ball);

                m_ship.Update(gameTime);
                m_hole.Update(gameTime);
                m_ball.Update(gameTime);
                m_bullet.Update(gameTime);
            }
        }
    }
}
