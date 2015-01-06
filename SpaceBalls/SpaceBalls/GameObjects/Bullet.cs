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

namespace SpaceBalls
{
    class Bullet: GameObject
    {
        private bool m_Active; // Boolean to determine if bullet can be used
        private float m_Power; // Float for bullet power

        public Bullet()
            : base()
        {
            Reset();
        }

        public override void Reset()
        {
            base.Reset();
            m_Active = false; // Reset to false to allow being fired
            m_Power = 1.0f;
        }

        public override void Update(Microsoft.Xna.Framework.GameTime gameTime)
        {
            Vector3 position = Position;
            float elapsedTime = ((float)(gameTime.ElapsedGameTime.Milliseconds / 1000.0f));

            Velocity += Acceleration * elapsedTime;
            position += Velocity * elapsedTime * PixelsPerMetre;
            // Same as base update, but only update position, rotation and angle if bullet is active
            if (Active)
            {
                Position = position;
                Rotation += RotationalAcceleration * elapsedTime;
                Angle += Rotation * elapsedTime;
            }
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            if(m_Active)
            base.Draw(spriteBatch); // Only draw bullet if active
        }

        public override bool CheckCollision(GameObject obj2)
        {
            if (m_Active && ((Position - obj2.Position).Length() <= (RadiusScaled + obj2.RadiusScaled)))
            {
                m_Active = false; // If bullet collides with any object then set bullet not active
                m_soundEffects[(int)SFX.BULLETHIT].Play();
                return true;
            }
            else return false;
        }

        public bool Active
        {
            get { return m_Active; }
            set { m_Active = value; }
        }

        public float Power
        {
            get { return m_Power; }
            set { m_Power = value; }
        }
    }
}
