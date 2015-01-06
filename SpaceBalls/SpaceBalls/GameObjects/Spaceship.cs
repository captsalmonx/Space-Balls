using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using SpaceBalls.GameUtilities;

namespace SpaceBalls
{
    enum SpaceshipAnim { SSA_IDLE, SSA_ENGINE_L, SSA_ENGINE_R, SSA_ENGINE_BOTH, SSA_ENGINE_MIDDLE, SSA_ENGINE_BRAKE };

    struct Spaceship_Engine
    {
        // The force unit vector that the engine will apply and the transformed
        // vector (after rotation and scaling)
        public Vector3 vForce;
        public Vector3 vForceTrans;

        // The vector offset of the engine from the ship's axis of rotation and
        // the transformed vector (after rotation and scaling)
        public Vector3 vPosOffset;
        public Vector3 vPosOffsetTrans;

        // The engine's force magnitude in Newtons
        public float fPower;
    }

    class Spaceship : GameObject
    {
        private Spaceship_Engine m_EngineL;
        private Spaceship_Engine m_EngineR;
        private Spaceship_Engine m_EngineB;
        private Spaceship_Engine m_EngineF;

        private SoundEffectInstance m_sound; // Ship thrust is looped, so a SoundEffectInstance must be created to control this

        public Spaceship()
            : base()
        {
            m_EngineL.vForce = Vector3.Zero;
            m_EngineL.vForceTrans = m_EngineL.vForce;
            m_EngineL.vPosOffset = Vector3.Zero;
            m_EngineL.vPosOffsetTrans = m_EngineL.vPosOffset;
            m_EngineL.fPower = 1.0f;

            m_EngineR.vForce = Vector3.Zero;
            m_EngineR.vForceTrans = m_EngineR.vForce;
            m_EngineR.vPosOffset = Vector3.Zero;
            m_EngineR.vPosOffsetTrans = m_EngineR.vPosOffset;
            m_EngineR.fPower = 1.0f;

            m_EngineB.vForce = Vector3.Zero;
            m_EngineB.vForceTrans = m_EngineB.vForce;
            m_EngineB.vPosOffset = Vector3.Zero;
            m_EngineB.vPosOffsetTrans = m_EngineB.vPosOffset;
            m_EngineB.fPower = 1.0f;

            m_EngineF.vForce = Vector3.Zero;
            m_EngineF.vForceTrans = m_EngineF.vForce;
            m_EngineF.vPosOffset = Vector3.Zero;
            m_EngineF.vPosOffsetTrans = m_EngineF.vPosOffset;
            m_EngineF.fPower = 1.0f;
        }

        ~Spaceship()
        {
        }

        // Internal method that calculates the transformed engine force and
        // offset vectors based on the current rotation angle and scale of the
        // ship
        private void _CalcEngineTrans()
        {
            // Calculate the transform matrix (rotation(Angle) * scale(Scale))
            Matrix matTransform = Matrix.CreateRotationZ(Angle) * Matrix.CreateScale(Scale);

            // For each engine: multiply vForce and vPosOffset by matTransform
            // and save the result in vForceTrans and vPosOffsetTrans
            m_EngineL.vForceTrans = Vector3.Transform(m_EngineL.vForce, matTransform);
            m_EngineL.vPosOffsetTrans = Vector3.Transform(m_EngineL.vPosOffset, matTransform);

            m_EngineR.vForceTrans = Vector3.Transform(m_EngineR.vForce, matTransform);
            m_EngineR.vPosOffsetTrans = Vector3.Transform(m_EngineR.vPosOffset, matTransform);

            m_EngineB.vForceTrans = Vector3.Transform(m_EngineB.vForce, matTransform);
            m_EngineB.vPosOffsetTrans = Vector3.Transform(m_EngineB.vPosOffset, matTransform);

            m_EngineF.vForceTrans = Vector3.Transform(m_EngineF.vForce, matTransform);
            m_EngineF.vPosOffsetTrans = Vector3.Transform(m_EngineF.vPosOffset, matTransform);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            // Make sure engines are transformed after every update
            _CalcEngineTrans();
        }

        // Switch between sound states of play
        public void PlaySoundEffect(bool play)
        {
            if (play && m_sound.State == SoundState.Stopped) // As sound is looped, there is no need to play it again if still playing
                m_sound.Play();
            else if (!play)
                m_sound.Stop();
        }

        // Applies a force from the left engine multiplied by power
        public void ApplyLeftEngine(float power)
        {
            ApplyForce(m_EngineL.vForceTrans * (m_EngineL.fPower * power), m_EngineL.vPosOffsetTrans);
            PlaySoundEffect(true);
        }

        // Applies a force from the right engine multiplied by power
        public void ApplyRightEngine(float power)
        {
            ApplyForce(m_EngineR.vForceTrans * (m_EngineR.fPower * power), m_EngineR.vPosOffsetTrans);
            PlaySoundEffect(true);
        }

        // Applies a force from the brake engine multiplied by power
        public void ApplyBrakeEngine(float power)
        {
            ApplyForce(m_EngineB.vForceTrans * (m_EngineB.fPower * power), m_EngineB.vPosOffsetTrans);
            PlaySoundEffect(true);
        }

        // Applies a force from the thrust engine multiplied by power
        public void ApplyThrustEngine(float power)
        {
            ApplyForce(m_EngineF.vForceTrans * (m_EngineF.fPower * power), m_EngineF.vPosOffsetTrans);
            PlaySoundEffect(true);
        }

        // Check for input and control ship accordingly. Also shoots bullet.
        public void Input(Bullet bullet)
        {
            // For input, favour the keyboard, then check the game-pad
            if (GameInput.Get().IsKeyDown(Keys.Up))
            {
                CurrentAnimationID = (int)SpaceshipAnim.SSA_ENGINE_BOTH;
                ApplyThrustEngine(1.5f);
            }
            else if (GameInput.Get().IsKeyDown(Keys.Right))
            {
                CurrentAnimationID = (int)SpaceshipAnim.SSA_ENGINE_L;
                ApplyLeftEngine(1.5f);
            }
            else if (GameInput.Get().IsKeyDown(Keys.Left))
            {
                CurrentAnimationID = (int)SpaceshipAnim.SSA_ENGINE_R;
                ApplyRightEngine(1.5f);
            }
            else if (GameInput.Get().IsKeyDown(Keys.Down))
            {
                CurrentAnimationID = (int)SpaceshipAnim.SSA_ENGINE_BRAKE;
                ApplyBrakeEngine(1.5f);
            }
            else
            {
                // Check the game-pad
                if (GameInput.Get().GetControllerTriggerL() > 0.0f && GameInput.Get().GetControllerTriggerR() > 0.0f)
                {
                    CurrentAnimationID = (int)SpaceshipAnim.SSA_ENGINE_BOTH;
                    ApplyLeftEngine(GameInput.Get().GetControllerTriggerL());
                    ApplyRightEngine(GameInput.Get().GetControllerTriggerR());
                }
                else if (GameInput.Get().GetControllerTriggerL() > 0.0f)
                {
                    CurrentAnimationID = (int)SpaceshipAnim.SSA_ENGINE_L;
                    ApplyRightEngine(GameInput.Get().GetControllerTriggerL() * 1.5f);
                }
                else if (GameInput.Get().GetControllerTriggerR() > 0.0f)
                {
                    CurrentAnimationID = (int)SpaceshipAnim.SSA_ENGINE_R;
                    ApplyLeftEngine(GameInput.Get().GetControllerTriggerR() * 1.5f);
                }
                else if (GameInput.Get().IsControllerButtonDown(Buttons.B))
                {
                    CurrentAnimationID = (int)SpaceshipAnim.SSA_ENGINE_BRAKE;
                    ApplyBrakeEngine(1.0f);
                }
                else
                {
                    CurrentAnimationID = (int)SpaceshipAnim.SSA_IDLE;
                    PlaySoundEffect(false);
                }
                if ((GameInput.Get().IsKeyDown(Keys.Space) || GameInput.Get().IsControllerButtonDown(Buttons.A)) && !bullet.Active)
                {
                    bullet.Active = true;
                    bullet.Position = Position;
                    bullet.Angle = Angle;
                    bullet.Velocity = Vector3.Transform(-Vector3.UnitY, Matrix.CreateRotationZ(bullet.Angle));
                    bullet.Velocity *= bullet.Power;
                    m_soundEffects[(int)SFX.SHOOT].Play();
                }
            }
        }

        public Vector3 LeftEngineForce
        {
            get { return m_EngineL.vForce; }
            set { m_EngineL.vForce = value; }
        }

        public Vector3 RightEngineForce
        {
            get { return m_EngineR.vForce; }
            set { m_EngineR.vForce = value; }
        }

        public Vector3 BrakeEngineForce
        {
            get { return m_EngineB.vForce; }
            set { m_EngineB.vForce = value; }
        }

        public Vector3 ThrustEngineForce
        {
            get { return m_EngineF.vForce; }
            set { m_EngineF.vForce = value; }
        }

        public Vector3 LeftEngineOffset
        {
            get { return m_EngineL.vPosOffset; }
            set { m_EngineL.vPosOffset = value; }
        }

        public Vector3 RightEngineOffset
        {
            get { return m_EngineR.vPosOffset; }
            set { m_EngineR.vPosOffset = value; }
        }

        public Vector3 BrakeEngineOffset
        {
            get { return m_EngineB.vPosOffset; }
            set { m_EngineB.vPosOffset = value; }
        }

        public Vector3 ThrustEngineOffset
        {
            get { return m_EngineF.vPosOffset; }
            set { m_EngineF.vPosOffset = value; }
        }

        public float LeftEnginePower
        {
            get { return m_EngineL.fPower; }
            set { m_EngineL.fPower = value; }
        }

        public float RightEnginePower
        {
            get { return m_EngineR.fPower; }
            set { m_EngineR.fPower = value; }
        }

        public float BrakeEnginePower
        {
            get { return m_EngineB.fPower; }
            set { m_EngineB.fPower = value; }
        }

        public float ThrustEnginePower
        {
            get { return m_EngineF.fPower; }
            set { m_EngineF.fPower = value; }
        }

        public SoundEffectInstance SoundEffectInstance
        {
            set
            {
                m_sound = value;
                m_sound.IsLooped = true; // SoundEffectInstance for spaceship is looped by default
            }
        }
    }
}
