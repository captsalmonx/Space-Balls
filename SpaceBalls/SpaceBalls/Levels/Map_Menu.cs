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
    class Map_Menu: Map_Default
    {
        private const float GRAVITATIONAL_CONSTANT = 1.0f;
        private const int NUM_PLANETS = 50;

        public Map_Menu()
            : base(NUM_PLANETS)
        {
        }
        
        public override void Reset()
        {
            base.Reset();
            m_hole.InitialPosition = new Vector3((float)random.Next(0, 1280), (float)random.Next(0, 720), 0.0f);
            m_hole.Mass = 10000.0f;
            m_hole.Radius = 300.0f;
            m_hole.Scale = 1.0f;

            m_ball.InitialPosition = new Vector3((float)random.Next(0, 1280), (float)random.Next(0, 720), 0.0f);
            m_ball.Mass = 2500.0f;
            m_ball.Static = false;
            m_ball.Sound = false;

            for (int i = 0; i < m_aPlanets.Length; i++)
            {
                m_aPlanets[i].PixelsPerMetre = 2.0f;
                m_aPlanets[i].Position = new Vector3((float)random.Next(0, 1280), (float)random.Next(0, 720), 0.0f);
                m_aPlanets[i].Scale = ((float)random.Next(3, 10)) / 50.0f;
                m_aPlanets[i].Mass = 1000.0f;
                m_aPlanets[i].Velocity = new Vector3((random.Next(0, 2) == 0 ? (float)random.Next(1, 10) : (float)random.Next(-10, -1)),
                    random.Next(0, 2) == 0 ? (float)random.Next(1, 10) : (float)random.Next(-10, -1), 0.0f);
                m_aPlanets[i].Texture = m_textures[random.Next(0, 4)];
                m_aPlanets[i].Sound = false;
            }

            m_ship.InitialPosition =  new Vector3((float)random.Next(0, 1280), (float)random.Next(0, 720), 0.0f);
            m_ship.Mass = 5000.0f;
            m_ship.Radius = 64.0f;
            m_ship.Scale = 0.8f;
            m_ship.Static = false;
            m_ship.Sound = false;
        }

        public override void Update(GameTime gameTime, GraphicsDevice graphics)
        {
            base.Update(gameTime, graphics);
            for (int i = 0; i < m_aPlanets.Length; i++)
            {
                m_ship.ProcessCollision(m_aPlanets[i]);
                m_ball.ProcessCollision(m_aPlanets[i]);
                for (int j = 0; j < m_aPlanets.Length; j++)
                    if (j != i)
                        m_aPlanets[j].ProcessCollision(m_aPlanets[i]);
                ApplyGravity(m_hole, m_aPlanets[i]);
                m_aPlanets[i].WrapBoundary(graphics);
                m_aPlanets[i].Update(gameTime);
            }
            if (random.Next(0, 500) == 0)
                m_hole.AntiGravity = !m_hole.AntiGravity;
            m_ship.WrapBoundary(graphics);
            m_ship.ProcessCollision(m_ball);
            ApplyGravity(m_hole, m_ship);
            ApplyGravity(m_hole, m_ball);
            m_ship.Update(gameTime);
            m_hole.Update(gameTime);
            m_ball.WrapBoundary(graphics);
            m_ball.Update(gameTime);
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            m_hole.Draw(spriteBatch);
            for (int i = 0; i < m_aPlanets.Length; i++)
                m_aPlanets[i].Draw(spriteBatch);
            m_ball.Draw(spriteBatch);
            m_ship.Draw(spriteBatch);
            m_bullet.Draw(spriteBatch);
        }
    }
}
