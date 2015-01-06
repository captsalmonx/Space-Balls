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
    class Map_Level1 : Map_Default
    {
        private const float GRAVITATIONAL_CONSTANT = 1.0f;
        private const int NUM_PLANETS = 5;

        public Map_Level1()
            : base(NUM_PLANETS)
        {
        }

        public override void Reset()
        {
            base.Reset();
            m_hole.InitialPosition = new Vector3(1000.0f, 400.0f, 0.0f);
            m_hole.Mass = 100000.0f;
            m_hole.Radius = 30.0f;
            m_hole.Scale = 1.5f;

            m_ball.InitialPosition = new Vector3(300.0f, 200.0f, 0.0f);
            m_ball.Mass = 2500.0f;
            m_ball.Static = true;
            m_ball.Sound = true;

            for (int i = 0; i < m_aPlanets.Length; i++)
            {
                m_aPlanets[i].Mass = 50000.0f;
                m_aPlanets[i].Static = true;
                m_aPlanets[i].Angle = (float)random.Next(0, 360);
                m_aPlanets[i].Rotation = ((float)random.Next(-3, 3) / 10.0f);
                m_aPlanets[i].Texture = m_textures[random.Next(0, 4)];
                m_aPlanets[i].SoundEffects = m_soundEffects;
                m_aPlanets[i].Sound = true;
            }

            m_aPlanets[0].InitialPosition = new Vector3(random.Next(40, 60), random.Next(90, 110), 0.0f);
            m_aPlanets[0].Scale = random.Next(4,6) / 10.0f;
            m_aPlanets[1].InitialPosition = new Vector3(random.Next(1190, 1210), random.Next(90, 110), 0.0f);
            m_aPlanets[1].Scale = random.Next(6,8) / 10.0f;
            m_aPlanets[2].InitialPosition = new Vector3(random.Next(1090, 1110), random.Next(690, 710), 0.0f);
            m_aPlanets[2].Scale = random.Next(8,10) / 10.0f;
            m_aPlanets[3].InitialPosition = new Vector3(random.Next(690, 710), random.Next(240, 260), 0.0f);
            m_aPlanets[3].Scale = random.Next(4,6) / 10.0f;
            m_aPlanets[4].InitialPosition = new Vector3(random.Next(190, 210), random.Next(490, 510), 0.0f);
            m_aPlanets[4].Scale = random.Next(7,9) / 10.0f;

            m_ship.InitialPosition = new Vector3(300.0f, 50.0f, 0.0f);
            m_ship.Mass = 5000.0f;
            m_ship.Radius = 64.0f;
            m_ship.Scale = 0.5f;
            m_ship.Angle = 3.0f;
            m_ship.Static = false;
            m_ship.Sound = true;

            m_bullet.InitialPosition = m_ship.Position;
            m_bullet.Mass = 1000.0f;
            m_bullet.Radius = 2.5f;
            m_bullet.Power = 500.0f;
            m_bullet.Sound = true;
        }

        public override void Update(GameTime gameTime, GraphicsDevice graphics)
        {
            if (GameInput.Get().IsKeyReleased(Keys.R) || GameInput.Get().IsControllerButtonReleased(Buttons.X))
                Reset();
            if (!CheckWin())
            {
                base.Update(gameTime, graphics);

                m_ship.Input(m_bullet);

                ApplyGravity(m_ball, m_hole);

                for (int i = 0; i < m_aPlanets.Length; i++)
                {
                    ApplyGravity(m_ball, m_aPlanets[i]);
                    ApplyGravity(m_bullet, m_aPlanets[i]);
                    m_aPlanets[i].ProcessCollision(m_ball);
                    m_aPlanets[i].ProcessCollision(m_ship);
                    m_bullet.ProcessCollision(m_aPlanets[i]);
                    m_aPlanets[i].Update(gameTime);
                }

                if (!m_ball.Static)
                {
                    m_ship.ProcessCollision(m_ball);
                    if (m_bullet.Active)
                        m_bullet.ProcessCollision(m_ball);
                }

                if (m_ball.CheckCollision(m_bullet) || m_ball.CheckCollision(m_ship))
                    m_ball.Static = false;

                m_ship.Update(gameTime);
                m_ball.Update(gameTime);
                m_hole.Update(gameTime);
                if (m_bullet.Active)
                    m_bullet.Update(gameTime);
                if (m_bullet.WrapBoundary(graphics))
                    m_bullet.Active = false;
            }
        }
    }
}
