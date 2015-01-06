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
    class BlackHole: GameObject
    {
        // Two additional textures for the black hole layered effect
        private Texture2D m_TextureFront;
        private Texture2D m_TextureBack;

        public BlackHole()
            : base()
        {
            Reset();
        }

        public override void Reset()
        {
            base.Reset();
            base.Rotation = 1.0f; // Set rotation for black hole sprites
            base.Static = true; // The black hole object never moves around the screen
        }

        // Draw three textures on the same position, but at opposing angles
        public override void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(m_TextureBack, new Vector2(Position.X, Position.Y), null, AntiGravity ? Color.Yellow : Color.White, -Angle * 0.5f, Origin, Scale, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(Texture, new Vector2(Position.X, Position.Y), null, AntiGravity ? Color.Yellow : Color.White, Angle, Origin, Scale, SpriteEffects.None, 0.0f);
            spriteBatch.Draw(m_TextureFront, new Vector2(Position.X, Position.Y), null, AntiGravity ? Color.Yellow : Color.White, -Angle, Origin, Scale, SpriteEffects.None, 0.0f);
        }

        public override bool CheckCollision(GameObject obj2)
        {
            // If the length of the vector between the two centre points is
            // less than or equal to the sum of the radii, there is a collision
            
            if ((Position - obj2.Position).Length() <= (RadiusScaled - (obj2.RadiusScaled / 2)))
            {
                if (!obj2.Static) m_soundEffects[(int)SFX.WIN].Play();
                return true;
            }
            else return false;

        }

        public Texture2D BackTexture
        {
            get { return m_TextureBack; }
            set { m_TextureBack = value; }
        }

        public Texture2D FrontTexture
        {
            get { return m_TextureFront; }
            set { m_TextureFront = value; }
        }
    }
}
