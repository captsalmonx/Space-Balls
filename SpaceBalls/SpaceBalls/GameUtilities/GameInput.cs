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

namespace SpaceBalls.GameUtilities
{
    class GameInput
    {
        // Static singleton reference
        private static GameInput m_inst;

        // The previous keyboard state
        private KeyboardState m_ksPrev;

        // The current keyboard state
        private KeyboardState m_ksCurr;

        // The previous game-pad state
        private GamePadState m_gsPrev;

        // The current game-pad state
        private GamePadState m_gsCurr;

        public GameInput()
        {
            // Set-up initial keyboard and game-pad states
            m_ksPrev = Keyboard.GetState();
            m_ksCurr = Keyboard.GetState();
            m_gsPrev = GamePad.GetState(PlayerIndex.One);
            m_gsCurr = GamePad.GetState(PlayerIndex.One);
        }

        // Creates a GameInput instance if necessary and returns a reference
        public static GameInput Get()
        {
            if (m_inst == null)
                m_inst = new GameInput();

            return m_inst;
        }

        // Destroys the static singleton if necessary
        public static void Kill()
        {
            if (m_inst != null)
                m_inst = null;
        }

        public void Update()
        {
            // Save current keyboard state to previous and get the new state
            m_ksPrev = m_ksCurr;
            m_ksCurr = Keyboard.GetState();

            // Save current game-pad state to previous and get the new state
            m_gsPrev = m_gsCurr;
            m_gsCurr = GamePad.GetState(PlayerIndex.One);
        }

        // Returns true if a keyboard key is down, false otherwise
        public bool IsKeyDown(Keys k)
        {
            return m_ksCurr.IsKeyDown(k);
        }

        // Returns true if a keyboard key was down on the last update but is
        // now up, false otherwise
        public bool IsKeyReleased(Keys k)
        {
            return (m_ksPrev.IsKeyDown(k) && m_ksCurr.IsKeyUp(k));
        }

        // Returns true if a game-pad button is down, false otherwise
        public bool IsControllerButtonDown(Buttons b)
        {
            return m_gsCurr.IsButtonDown(b);
        }

        // Returns true if a game-pad button was down on the last update but is
        // now up, false otherwise
        public bool IsControllerButtonReleased(Buttons b)
        {
            return (m_gsPrev.IsButtonDown(b) && m_gsCurr.IsButtonUp(b));
        }

        // Returns the current value (from 0.0 to 1.0) of the game-pad's left
        // trigger
        public float GetControllerTriggerL()
        {
            return m_gsCurr.Triggers.Left;
        }

        // Returns the current value (from 0.0 to 1.0) of the game-pad's right
        // trigger
        public float GetControllerTriggerR()
        {
            return m_gsCurr.Triggers.Right;
        }
    }
}
