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
    class Map_Level2 : Map_Default
    {
        private const float GRAVITATIONAL_CONSTANT = 1.0f;
        private const int NUM_PLANETS = 7;

        public Map_Level2()
            : base(NUM_PLANETS)
        {
        }

        public override void Reset()
        {
            base.Reset();
            m_hole.InitialPosition = new Vector3(200.0f, 150.0f, 0.0f);
            m_hole.Mass = 100000.0f;
            m_hole.Radius = 30.0f;
            m_hole.Scale = 1.5f;

            m_ball.InitialPosition = new Vector3(1200.0f, 250.0f, 0.0f);
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

            m_aPlanets[0].InitialPosition = new Vector3(random.Next(590, 610), random.Next(190, 210), 0.0f);
            m_aPlanets[0].Scale = random.Next(5, 7) / 10f;
            m_aPlanets[0].AntiGravity = false;
            m_aPlanets[1].InitialPosition = new Vector3(random.Next(940, 960), random.Next(90, 110), 0.0f);
            m_aPlanets[1].Scale = random.Next(3, 5) / 10f;
            m_aPlanets[1].AntiGravity = true;
            m_aPlanets[2].InitialPosition = new Vector3(random.Next(690, 710), random.Next(690, 710), 0.0f);
            m_aPlanets[2].Scale = random.Next(4, 6) / 10f;
            m_aPlanets[2].AntiGravity = false;
            m_aPlanets[3].InitialPosition = new Vector3(random.Next(940, 960), random.Next(490, 510), 0.0f);
            m_aPlanets[3].Scale = random.Next(5, 7) / 10f;
            m_aPlanets[3].AntiGravity = true;
            m_aPlanets[4].InitialPosition = new Vector3(random.Next(190, 210), random.Next(790, 810), 0.0f);
            m_aPlanets[4].Scale = random.Next(7, 9) / 10f;
            m_aPlanets[4].AntiGravity = false;
            m_aPlanets[5].InitialPosition = new Vector3(random.Next(460, 480), random.Next(620, 640), 0.0f);
            m_aPlanets[5].Scale = random.Next(1, 3) / 10f;
            m_aPlanets[5].AntiGravity = true;
            m_aPlanets[6].InitialPosition = new Vector3(random.Next(120, 140), random.Next(390, 410), 0.0f);
            m_aPlanets[6].Scale = random.Next(4, 6) / 10f;
            m_aPlanets[6].AntiGravity = true;
            
            m_ship.InitialPosition = new Vector3(-100.0f, 0.0f, 0.0f);
            m_ship.Scale = 0.1f;
            m_ship.Static = true;
            
            m_bullet.InitialPosition = m_ship.Position;
            m_bullet.Mass = 1000.0f;
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

                // For input, favour the keyboard, then check the game-pad
                if (GameInput.Get().IsKeyReleased(Keys.Space))
                {
                    if (m_ball.Static) m_ball.Static = false;
                    for (int i = 0; i < m_aPlanets.Length; i++)
                        m_aPlanets[i].AntiGravity = !m_aPlanets[i].AntiGravity;
                    m_soundEffects[(int)SFX.REVERSEGRAVITY].Play();
                }
                else if (GameInput.Get().IsKeyReleased(Keys.R))
                    Reset();
                else
                {
                    // Check the game-pad
                    if (GameInput.Get().IsControllerButtonReleased(Buttons.A))
                    {
                        if (m_ball.Static) m_ball.Static = false;
                        for (int i = 0; i < m_aPlanets.Length; i++)
                            m_aPlanets[i].AntiGravity = !m_aPlanets[i].AntiGravity;
                        m_soundEffects[(int)SFX.REVERSEGRAVITY].Play();
                    }
                    else if (GameInput.Get().IsControllerButtonReleased(Buttons.Back))
                        Reset();
                }

                ApplyGravity(m_ball, m_hole);

                for (int i = 0; i < m_aPlanets.Length; i++)
                {
                    ApplyGravity(m_aPlanets[i], m_ball);
                    m_aPlanets[i].ProcessCollision(m_ball);
                    m_aPlanets[i].Update(gameTime);
                }

                m_ball.Update(gameTime);
                m_hole.Update(gameTime);
            }
        }
    }
}
