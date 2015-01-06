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
    class Map_Default
    {
        // Values for number of planets to be created/updated
        private int NUM_PLANETS;
        // Constant gravitational value
        private const float GRAVITATIONAL_CONSTANT = 2f;

        protected Random random; // Used for random object attributes

        protected BlackHole m_hole;
        protected Spaceship m_ship;
        protected Bullet m_bullet;
        protected GameObject m_ball;
        protected GameObject[] m_aPlanets; // Each map will have a different number of planets, so create an array for each
        protected Texture2D[] m_textures;
        protected SoundEffect[] m_soundEffects;
        protected SpriteFont m_font;

        public Map_Default(int numberOfPlanets)
        {
            NUM_PLANETS = numberOfPlanets;
            random = new Random();
            // Create an array with the required number of planets for each map
            m_aPlanets = new GameObject[NUM_PLANETS];
            for (int i = 0; i < NUM_PLANETS; i++)
                m_aPlanets[i] = new GameObject();
        }

        // Base initialization 
        // This passes through pre-initialized objects from the main game class for use here
        public virtual void Initialize(BlackHole hole, Spaceship ship, Bullet bullet, GameObject ball, Texture2D[] textures, SoundEffect[] soundEffects, SpriteFont gameFont)
        {
            m_hole = hole;
            m_ball = ball;
            m_ship = ship;
            m_bullet = bullet;
            m_textures = textures;  // As there is a different quantity of planets in each map with random textures
                                    // pass through an array of textures to be used instead
            m_soundEffects = soundEffects; // Pass through array of sound effects to be used
            m_font = gameFont;
        }

        // Generic update function to reset objects
        // Actual resets are located in each map class
        public virtual void Reset()
        {
            m_hole.Reset();
            m_ball.Reset();
            m_ship.Reset();
            m_bullet.Reset();
            for (int i = 0; i < m_aPlanets.Length; i++)
                m_aPlanets[i].Reset();
            m_soundEffects[(int)SFX.RESET].Play();
        }

        // Generic update function to reset forces
        // Game logic is located in each map class
        public virtual void Update(GameTime gameTime, GraphicsDevice graphics)
        {
            m_ship.ResetForces();
            m_ball.ResetForces();
            m_bullet.ResetForces();
            for (int i = 0; i < m_aPlanets.Length; i++)
                m_aPlanets[i].ResetForces();
        }

        // Draw map to screen
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            m_hole.Draw(spriteBatch);
            for (int i = 0; i < m_aPlanets.Length; i++)
                m_aPlanets[i].Draw(spriteBatch);
            m_ball.Draw(spriteBatch);
            m_ship.Draw(spriteBatch);
            m_bullet.Draw(spriteBatch);
            OutOfBoundsValue(m_ball, spriteBatch);
            // If win then display win message
            if (CheckWin())
            {
                spriteBatch.DrawString(m_font, "YOU WIN GOLF", new Vector2(random.Next(505, 510), random.Next(355, 360)), Color.Purple);
                spriteBatch.DrawString(m_font, "PLAY AGAIN?", new Vector2(random.Next(525, 530), random.Next(375, 385)), Color.Purple);
                m_ship.PlaySoundEffect(false); // Stop playing ship sound effects
            }
        }

        // Check for ball/hole win condition
        public virtual bool CheckWin()
        {
            // If ball is in hole then freeze ball in that location
            if (m_hole.CheckCollision(m_ball))
            {
                m_ball.Static = true;
                m_ball.Position = m_hole.Position;
                return true;
            }
            return false;
        }

        // Applies gravity to objects depending on mass and distance
        protected void ApplyGravity(GameObject obj1, GameObject obj2)
        {
            // Calculate the distance between the two objects
            Vector3 vector = obj2.Position - obj1.Position;

            // Check if the object is more than 4 * radius away, and if so don't have any gravitational effect (this is for gameplay purposes)
            // This acts as a gravitional cut off (for gameplay purposes).
            if (vector.Length() < obj2.Radius * 4.0f)
                obj1.ApplyForce((obj2.AntiGravity ? -vector : vector) * (float)(GRAVITATIONAL_CONSTANT * ((obj1.MassScaled * obj2.MassScaled) / (vector.Length() * vector.Length()))), Vector3.Zero);

            if (vector.Length() < obj1.Radius * 4.0f)
                obj2.ApplyForce((obj1.AntiGravity ? vector : -vector) * (float)(GRAVITATIONAL_CONSTANT * ((obj1.MassScaled * obj2.MassScaled) / (vector.Length() * vector.Length()))), Vector3.Zero);
        }

        protected void OutOfBoundsValue(GameObject obj, SpriteBatch spriteBatch)
        {
            // Set up float to measure distance off screen
            float val = 0;
            // Set up vector to use as location for text
            Vector2 vec = Vector2.Zero;
            // If off left or right side, add values to float and position text at object y value
            if (obj.Position.X < 0)
            {
                val += obj.Position.X;
                vec.Y = obj.Position.Y;
            }
            else if (obj.Position.X > 1280)
            {
                val += (obj.Position.X - 1280.0f);
                vec.Y = obj.Position.Y;
                vec.X = 1190.0f;
            }
            // If off top or bottom side, add values to float and position text at object x value
            if (obj.Position.Y < 0)
            {
                val += obj.Position.Y;
                vec.X = obj.Position.X;
            }
            else if (obj.Position.Y > 720)
            {
                val += (obj.Position.Y - 720.0f);
                vec.X = obj.Position.X;
                vec.Y = 660.0f;
            }
            // If in corner position text accordingly
            if (vec.X > 1280.0f) vec.X = 1190.0f;            
            else if (vec.X < 0.0f) vec.X = 0.0f;
            if (vec.Y > 720.0f) vec.Y = 660.0f;
            else if (vec.Y < 0.0f) vec.Y = 0.0f;
            // Draw text with distance to screen
            if (val != 0.0f) spriteBatch.DrawString(m_font, ((int)val).ToString(), vec, Color.Green);
            // If object over 1000 pixels away then reset level
            if (val > 1000.0f || val < -1000.0f) Reset();
        }
    }
}
