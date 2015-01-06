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
    // Animation types: -
    //  - AT_SINGLE: animation plays once and stops at the last frame
    //  - AT_LOOP: when animation gets to the last frame, the frame is returned
    //             to frame 0.
    enum AnimType { AT_SINGLE, AT_LOOP };

    // struct to store a single animation frame reference
    struct AnimFrame
    {
        public int frameX; // Frame row in sprite sheet
        public int frameY; // Frame column in sprite sheet
    }

    // struct to store a single animation
    struct Anim
    {
        // Animation's unique ID
        public int animID;

        // Current animation frame number
        public int currFrame;

        // Number of game frames to wait before stepping the animation
        public int frameLimiter;

        // Game frame counter used for limiter above
        public int frameLimitCounter;

        // Animation type (see above)
        public AnimType animType;

        // Array of animation frame references
        public AnimFrame[] frames;
    }

    class GameObject
    {
        // Object's texture (single frame or tile sheet)
        private Texture2D m_Texture;

        // Object's sound effects
        protected SoundEffect[] m_soundEffects;

        // Bool to detect if object can make sound
        private bool m_bSound;

        // Object's animations
        private Anim[] m_anims;

        // Currently playing animation ID (or -1 for none)
        private int m_iCurrAnimID;

        // The array index in m_anims of the currently playing animation
        private int m_iCurrAnimIdx;

        // Source rectangle used when drawing, modified to cause correct
        // animation frame to be drawn
        private Rectangle m_srcRectangle;

        // If true, the object will not respond to applied forces
        private bool m_bStatic;

        // If true, the calculated gravity force vector will be reversed
        private bool m_bAntiGravity;

        // Object's mass in Kg and scaled version
        private float m_fMass;
        private float m_fMassScaled;

        // Object's radius in pixels and scaled version
        private float m_fRadius;
        private float m_fRadiusScaled;

        // Object's current rotation rate
        private float m_fRotation;

        // Object's current rotational acceleration due to torque
        private float m_fRotationalAcceleration;

        // Object's current net torque
        private float m_fCurrentRotationalForce;

        // Object's current rotation angle
        protected float m_fAngle;

        // Object's scale from the base dimensions of the texture
        private float m_fScale;

        // Ratio of pixels to metres used for physical calculations
        private float m_fPixelsPerMetre;

        // Object's origin, usually the centre. All offsets are from this
        // point.
        private Vector2 m_vOrigin;

        // Current screen co-ordinates
        private Vector2 m_vDrawPosition;

        // Object's starting position in the world (used when resetting)
        private Vector3 m_vInitialPosition;

        // Current world co-ordinates
        private Vector3 m_vPosition;

        // Current acceleration vector
        private Vector3 m_vAcceleration;

        // Current velocity vector
        private Vector3 m_vVelocity;

        // Current net force vector
        private Vector3 m_vCurrentForce;

        public GameObject()
        {
            // Reset to ensure valid values
            Reset();
        }

        ~GameObject()
        {
        }

        // Sets the number of animations, frame width and height
        public void SetupAnims(int numberOfAnims, int frameWidth, int frameHeight)
        {
            // Create and initialise animation array
            m_anims = new Anim[numberOfAnims];
            for (int i = 0; i < numberOfAnims; i++)
            {
                m_anims[i].animID = -1;
                m_anims[i].currFrame = 0;
                m_anims[i].frameLimiter = 0;
                m_anims[i].frameLimitCounter = 0;
                m_anims[i].animType = AnimType.AT_SINGLE;
            }

            // Set frame width and height
            FrameWidth = frameWidth;
            FrameHeight = frameHeight;
        }

        // Adds an animation to the object's animation array
        public bool AddAnimation(int animID, AnimType type, int frameLimiter, int numberOfFrames)
        {
            bool bFound = false;

            if (m_anims != null)
            {
                for (int a = 0; a < m_anims.Length; a++)
                {
                    if (m_anims[a].animID == -1)
                    {
                        // A free animation has been found, use this one
                        m_anims[a].animID = animID;
                        m_anims[a].animType = type;
                        m_anims[a].frameLimiter = frameLimiter;
                        m_anims[a].currFrame = 0;

                        // Create and initialise the frames array
                        m_anims[a].frames = new AnimFrame[numberOfFrames];
                        for (int f = 0; f < numberOfFrames; f++)
                        {
                            m_anims[a].frames[f].frameX = -1;
                            m_anims[a].frames[f].frameY = -1;
                        }

                        bFound = true;
                        break;
                    }
                }
            }
            return bFound;
        }

        // Adds a single frame to a specific animation
        public bool AddAnimFrame(int ID, int frameX, int frameY)
        {
            bool bFound = false;

            if (m_anims != null)
            {
                // Search through the animations array
                for (int a = 0; a < m_anims.Length; a++)
                {
                    if (m_anims[a].animID == ID)
                    {
                        for (int f = 0; f < m_anims[a].frames.Length; f++)
                        {
                            if (m_anims[a].frames[f].frameX == -1 && m_anims[a].frames[f].frameY == -1)
                            {
                                // If the animation ID is already in the array, add the frame
                                m_anims[a].frames[f].frameX = frameX;
                                m_anims[a].frames[f].frameY = frameY;
                                bFound = true;
                                break;
                            }
                        }
                    }
                    if (bFound)
                        break;
                }
            }
            return bFound;
        }

        // Adds a force vector at position offset to the object. This will
        // accumulate into CurrentForce and CurrentRotationalForce since
        // ResetForces() was last called.
        public virtual void ApplyForce(Vector3 force, Vector3 offsetPos)
        {
            // Only apply forces to non-static objects
            if (!Static)
            {
                // Calculate the torque vector (cross product of the force
                // vector and its offset from the rotational axis)
                Vector3 vTorque = Vector3.Cross(offsetPos, force);

                // Moment of inertia for a solid sphere
                float I = (2.0f * (MassScaled * (RadiusScaled * RadiusScaled))) / 5.0f;

                // Accumulate net forces on the object
                CurrentForce += force;
                CurrentRotationalForce += vTorque.Z;

                // Calculate new acceleration (Fnet = m * a, a = Fnet / m)
                Acceleration = CurrentForce / MassScaled;

                // Calculate new rotational acceleration (ra = Frnet / I)
                if (I != 0.0f)
                    RotationalAcceleration = CurrentRotationalForce / I;
            }
        }

        // Resets all net forces to zero
        public virtual void ResetForces()
        {
            CurrentForce = Vector3.Zero;
            CurrentRotationalForce = 0.0f;
            Acceleration = Vector3.Zero;
            RotationalAcceleration = 0.0f;
        }

        // Resets the state of the object to default values
        public virtual void Reset()
        {
            Mass = 1.0f;
            Rotation = 0.0f;
            PixelsPerMetre = 1.0f;
            RotationalAcceleration = 0.0f;
            Angle = 0.0f;
            Scale = 1.0f;
            AntiGravity = false;
            Sound = false;
            m_fRadiusScaled = Radius * Scale;
            m_fMassScaled = Mass * Scale;
            CurrentForce = Vector3.Zero;
            CurrentRotationalForce = 0.0f;
            InitialPosition = Vector3.Zero;
            Position = InitialPosition;
            Acceleration = Vector3.Zero;
            Velocity = Vector3.Zero;
            m_vDrawPosition = Vector2.Zero;
            m_iCurrAnimID = -1;
            m_iCurrAnimIdx = 0;
        }

        // Performs a circular collision check between this object and another.
        // Returns true if there was a collision, otherwise false.
        public virtual bool CheckCollision(GameObject obj2)
        {
            // If the length of the vector between the two centre points is
            // less than or equal to the sum of the radii, there is a collision
            if ((Position - obj2.Position).Length() <= (RadiusScaled + obj2.RadiusScaled))
            {
                // Check to see if a sound effect should be played and play it if necessary
                if (m_soundEffects != null && Sound && obj2.Sound)
                    m_soundEffects[(int)SFX.HIT].Play();
                return true;
            }
            else
                return false;
        }


        // If two objects have collided, this method calculates and sets the
        // resultant velocity vectors of each due to the collision.
        public virtual void ProcessCollision(GameObject obj2)
        {
            float fImp1;
            float fImp2;
            float fV1n = 0.0f;
            float fV1t = 0.0f;
            float fV2n = 0.0f;
            float fV2t = 0.0f;
            float fVf1n = 0.0f;
            float fVf2n = 0.0f;

            Vector3 vCollisionNormal;
            Vector3 vCollisionTangent = new Vector3();
            Vector3 vVf1n;
            Vector3 vVf1t;
            Vector3 vVf2n;
            Vector3 vVf2t;
            Vector3 vF1;
            Vector3 vF2;

            if (CheckCollision(obj2))
            {
                // Calculate the impulse force magnitudes (assuming 0.1s time
                // delta)
                fImp1 = (MassScaled * -Velocity.Length()) / 0.1f;
                fImp2 = (obj2.MassScaled * -obj2.Velocity.Length()) / 0.1f;

                // Calculate the vector that's normal to the collision surface
                vCollisionNormal = obj2.Position - Position;

                // Make the collision normal vector a unit vector
                vCollisionNormal.Normalize();

                // Calculate the tangent vector to the collision surface. This
                // vector is at a right angle to vCollisionNormal, so it can be
                // created by setting the X component to the normal's -Y
                // component and the Y component to the normal's -X
                vCollisionTangent.X = -vCollisionNormal.Y;
                vCollisionTangent.Y = vCollisionNormal.X;

                // Calculate the projection of each object's velocity vector
                // onto the normal and tangent vectors by taking the dot
                // products
                fV1n = Vector3.Dot(vCollisionNormal, Velocity);
                fV1t = Vector3.Dot(vCollisionTangent, Velocity);
                fV2n = Vector3.Dot(vCollisionNormal, obj2.Velocity);
                fV2t = Vector3.Dot(vCollisionTangent, obj2.Velocity);

                // Calculate the 1-dimensional elastic collision amounts along
                // the normal vector (tangential components remain unchanged)
                fVf1n = ((fV1n * (MassScaled - obj2.MassScaled)) + (2 * obj2.MassScaled * fV2n)) / (MassScaled + obj2.MassScaled);
                fVf2n = ((fV2n * (obj2.MassScaled - MassScaled)) + (2 * MassScaled * fV1n)) / (MassScaled + obj2.MassScaled);

                // Multiply the final scalars by the relevant vectors to obtain
                // final velocities along each vector
                vVf1n = vCollisionNormal * fVf1n;
                vVf1t = vCollisionTangent * fV1t;
                vVf2n = vCollisionNormal * fVf2n;
                vVf2t = vCollisionTangent * fV2t;

                // The final velocities for each object are now the sum of both
                // normal and tangent velocity vectors
                Velocity = vVf1n + vVf1t;
                obj2.Velocity = vVf2n + vVf2t;

                // Apply a small amount of dampening. This means that the
                // collisions are no longer purely elastic and the system will
                // eventually lose all energy.
                Velocity = Velocity * 0.8f;
                obj2.Velocity = obj2.Velocity * 0.8f;

                // Calculate force unit vectors in the direction of the new
                // velocity vectors
                vF1 = (vVf1n + vVf1t);
                vF2 = (vVf2n + vVf2t);
                vF1.Normalize();
                vF2.Normalize();

                // Apply an impulse force to both objects along the calculated
                // force vectors
                ApplyForce(-vF1 * fImp1, vCollisionNormal * RadiusScaled);
                obj2.ApplyForce(-vF2 * fImp2, vCollisionNormal * obj2.RadiusScaled);

                // Make sure the objects no longer collide
                if (!obj2.Static)
                    obj2.Position = Position + (vCollisionNormal * (RadiusScaled + obj2.RadiusScaled + 1.0f));
                else
                    Position = obj2.Position + (-vCollisionNormal * (RadiusScaled + obj2.RadiusScaled + 1.0f));
            }
        }

        // If an object is at or past the boundary, this method performs a
        // collision and sets the object's new velocity vector and position
        public virtual void ProcessCollision_Boundary(GraphicsDevice gd)
        {
            Vector3 pos = Position, vel = Velocity, force = Vector3.Zero;
            float rad = RadiusScaled, len = 0.0f, dampening = 0.9f, friction_cof = 0.5f;
            /* 
              If an object collides with the boundary: -
              1. If it hits the top or bottom, reverse the Y velocity
                 component. If it hits the left or right, reverse X.
              2. Calculate the normal force of the collision (velocity vector
                 multiplied by the cosine of the angle (dot product)). From
                 this, find the frictional force by multiplying the negative
                 co-efficient of friction by the normal vector.
              3. Apply the frictional force to the object at the point of
                 collision.
              4. Find the velocity magnitude and increase the opposite
                 component to the component that was reversed by a factor
                 based on the current rotation rate. The rotation rate is the
                 number of radians by which this object will rotate per
                 update, so the length of the edge that will slide over the
                 contact surface for this frame is
                 ((Rotation/2*PI) * 2*PI*rad) which can be simplified to
                 (Rotation*rad). Finally, this is multiplied by the
                 co-efficient of friction.
              5. Normalize the velocity then multiply by the length
                 calculated in 4 to obtain the new velocity in the new
                 direction.
              6. Dampen the new velocity slightly.
              7. Set the object's position so that it's within the boundary.
            */
            if (pos.X - rad <= 0.0f)
            {
                force = (-friction_cof * (vel * (float)((double)(Vector3.Dot(vel, vel.Y < 0 ? Vector3.Down : Vector3.Up) / vel.Length())))) * Vector3.Up;
                ApplyForce(force * Mass * 2.0f, new Vector3(-rad, 0.0f, 0.0f));
                vel.X = -vel.X;
                len = vel.Length();
                vel.Y += Rotation * rad * friction_cof;
                vel.Normalize();
                vel *= len * dampening;
                pos.X = rad;
            }
            if (pos.X + rad >= gd.Viewport.Width)
            {
                force = (-friction_cof * (vel * (float)((double)(Vector3.Dot(vel, vel.Y < 0 ? Vector3.Down : Vector3.Up) / vel.Length())))) * Vector3.Up;
                ApplyForce(force * Mass * 2.0f, new Vector3(rad, 0.0f, 0.0f));
                vel.X = -vel.X;
                len = vel.Length();
                vel.Y -= Rotation * rad * friction_cof;
                vel.Normalize();
                vel *= len * dampening;
                pos.X = gd.Viewport.Width - rad;
            }
            if (pos.Y - rad <= 0.0f)
            {
                force = (-friction_cof * (vel * (float)((double)(Vector3.Dot(vel, vel.X < 0 ? Vector3.Left : Vector3.Right) / vel.Length())))) * Vector3.Right;
                ApplyForce(force * Mass * 2.0f, new Vector3(0.0f, -rad, 0.0f));
                vel.Y = -vel.Y;
                len = vel.Length();
                vel.X -= Rotation * rad * friction_cof;
                vel.Normalize();
                vel *= len * dampening;
                pos.Y = rad;
            }
            if (pos.Y + rad >= gd.Viewport.Height)
            {
                force = (-friction_cof * (vel * (float)((double)(Vector3.Dot(vel, vel.X < 0 ? Vector3.Left : Vector3.Right) / vel.Length())))) * Vector3.Right;
                ApplyForce(force * Mass * 2.0f, new Vector3(0.0f, rad, 0.0f));

                vel.Y = -vel.Y;
                len = vel.Length();
                vel.X += Rotation * rad * friction_cof;
                vel.Normalize();
                vel *= len * dampening;
                pos.Y = gd.Viewport.Height - rad;
            }

            Position = pos;
            Velocity = vel;
        }

        // Draws the object at the draw position and to scale. Colors object yellow to indicate anti-gravity effect
        public virtual void Draw(SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(Texture, m_vDrawPosition, m_srcRectangle, AntiGravity ? Color.Yellow : Color.White, Angle, Origin, Scale, SpriteEffects.None, 0.0f);
        }

        // Updates an object and calculates physical changes based on gameTime
        public virtual void Update(GameTime gameTime)
        {
            Vector3 position = Position;

            // Find the elapsed game time in ms
            float elapsedTime = ((float)(gameTime.ElapsedGameTime.Milliseconds / 1000.0f));

            // Only update physical properties if the object is not static
            if (!Static)
            {
                // Calculate velocity based on acceleration and time
                Velocity += Acceleration * elapsedTime;

                // Calculate the new position based on velocity, time and the
                // pixels-per-metre ratio
                position += Velocity * elapsedTime * PixelsPerMetre;

                // Set the final position
                Position = position;

                // Calculate the rotation rate based on rotational acceleration
                // and time
                Rotation += RotationalAcceleration * elapsedTime;
            }

            // Limit the maximum rotation rate
            if (Rotation > 10.0f)
                Rotation = 10.0f;
            else if (Rotation < -10.0f)
                Rotation = -10.0f;

            // Calculate the final angle based on rotation rate and time
            Angle += Rotation * elapsedTime;

            // Update the sprite animations
            UpdateAnims();
        }

        // Wraps the object to the other side of the screen if the object is
        // outside the boundary. Also used for checking boundary
        public virtual bool WrapBoundary(GraphicsDevice gd)
        {
            if (Position.X + (((float)m_srcRectangle.Width / 2.0f) * Scale) < 0)
            {
                m_vPosition.X += gd.Viewport.Width + ((float)m_srcRectangle.Width * Scale);
                return true;
            }
            else if (Position.X - (((float)m_srcRectangle.Width / 2.0f) * Scale) > gd.Viewport.Width)
            {
                m_vPosition.X -= gd.Viewport.Width + ((float)m_srcRectangle.Width * Scale);
                return true;
            }
            else if (Position.Y + (((float)m_srcRectangle.Height / 2.0f) * Scale) < 0)
            {
                m_vPosition.Y += gd.Viewport.Height + ((float)m_srcRectangle.Height * Scale);
                return true;
            }
            else if (Position.Y - (((float)m_srcRectangle.Height / 2.0f) * Scale) > gd.Viewport.Height)
            {
                m_vPosition.Y -= gd.Viewport.Height + ((float)m_srcRectangle.Height * Scale);
                return true;
            }
            else return false;
        }

        // Updates the animation if an animation is playing
        private void UpdateAnims()
        {
            if (CurrentAnimationID != -1)
            {
                // Only update the frame if the decremented frame limit counter is <= 0
                if (--m_anims[m_iCurrAnimIdx].frameLimitCounter <= 0)
                {
                    // Recalculate source rectangle position in texture based on current frame
                    m_srcRectangle.X = m_anims[m_iCurrAnimIdx].frames[m_anims[m_iCurrAnimIdx].currFrame].frameX * m_srcRectangle.Width;
                    m_srcRectangle.Y = m_anims[m_iCurrAnimIdx].frames[m_anims[m_iCurrAnimIdx].currFrame].frameY * m_srcRectangle.Height;

                    if (++m_anims[m_iCurrAnimIdx].currFrame >= m_anims[m_iCurrAnimIdx].frames.Length)
                    {
                        // If the current frame is at the end of the set
                        switch (m_anims[m_iCurrAnimIdx].animType)
                        {
                            case AnimType.AT_SINGLE: // Make sure frame stays at final frame
                                m_anims[m_iCurrAnimIdx].currFrame = m_anims[m_iCurrAnimIdx].frames.Length - 1;
                                break;
                            case AnimType.AT_LOOP: // Set frame back to start
                                m_anims[m_iCurrAnimIdx].currFrame = 0;
                                break;
                        }
                    }

                    // Reset the frame limit counter
                    m_anims[m_iCurrAnimIdx].frameLimitCounter = m_anims[m_iCurrAnimIdx].frameLimiter;
                }
            }
        }

        public Texture2D Texture
        {
            get { return m_Texture; }
            set
            {
                m_Texture = value;

                // Calculate the radius based on the texture width
                if (Radius <= 0.0f)
                    Radius = m_Texture.Width / 2.0f;

                // Calculate the object's origin point
                m_vOrigin.X = m_Texture.Width / 2.0f;
                m_vOrigin.Y = m_Texture.Height / 2.0f;

                // Set up the source rectangle, animations will override this
                m_srcRectangle.X = 0;
                m_srcRectangle.Y = 0;
                m_srcRectangle.Width = m_Texture.Width;
                m_srcRectangle.Height = m_Texture.Height;
            }
        }

        public SoundEffect[] SoundEffects
        {
            set { m_soundEffects = value; }
        }

        public int FrameWidth
        {
            get { return m_srcRectangle.Width; }
            set
            {
                m_srcRectangle.Width = value;
                m_vOrigin.X = (float)m_srcRectangle.Width / 2.0f;
            }
        }

        public int FrameHeight
        {
            get { return m_srcRectangle.Height; }
            set
            {
                m_srcRectangle.Height = value;
                m_vOrigin.Y = (float)m_srcRectangle.Height / 2.0f;
            }
        }

        public int CurrentAnimationID
        {
            get { return m_iCurrAnimID; }
            set
            {
                // Initially set to invalid ID
                m_iCurrAnimID = -1;

                if (m_anims != null)
                {
                    // Loop through all loaded animations
                    for (int i = 0; i < m_anims.Length; i++)
                    {
                        if (m_anims[i].animID == value)
                        {
                            // If the ID is found, assign
                            // the current animation ID
                            m_iCurrAnimID = value;
                            m_iCurrAnimIdx = i;
                            break;
                        }
                    }
                }
            }
        }

        public bool Static
        {
            get { return m_bStatic; }
            set { m_bStatic = value; }
        }

        public bool AntiGravity
        {
            get { return m_bAntiGravity; }
            set { m_bAntiGravity = value; }
        }

        public float Mass
        {
            get { return m_fMass; }
            set
            {
                if (value < 0.0f)
                    m_fMass = 1.0f;
                else
                    m_fMass = value;
                m_fMassScaled = m_fMass * Scale; // Adjust mass to scale
            }
        }

        public float MassScaled
        {
            get { return m_fMassScaled; }
        }

        public float Radius
        {
            get { return m_fRadius; }
            set { 
                m_fRadius = value;
                m_fRadiusScaled = m_fRadius * Scale; // Adjust radius to scale
            }
        }

        public float RadiusScaled
        {
            get { return m_fRadiusScaled; }
        }

        public float Rotation
        {
            get { return m_fRotation; }
            set { m_fRotation = value; }
        }

        public float RotationalAcceleration
        {
            get { return m_fRotationalAcceleration; }
            set { m_fRotationalAcceleration = value; }
        }

        public float CurrentRotationalForce
        {
            get { return m_fCurrentRotationalForce; }
            set { m_fCurrentRotationalForce = value; }
        }

        public float Angle
        {
            get { return m_fAngle; }
            set { m_fAngle = value; }
        }

        public float Scale
        {
            get { return m_fScale; }
            set
            {
                m_fScale = value;
                m_fRadiusScaled = Radius * value;
                m_fMassScaled = Mass * value;
            }
        }

        public float PixelsPerMetre
        {
            get { return m_fPixelsPerMetre; }
            set { m_fPixelsPerMetre = value; }
        }

        public Vector2 Origin
        {
            get { return m_vOrigin; }
            set { m_vOrigin = value; }
        }

        public Vector3 InitialPosition
        {
            get { return m_vInitialPosition; }
            set
            {
                m_vInitialPosition = value;
                Position = m_vInitialPosition;
            }
        }

        public Vector3 Position
        {
            get { return m_vPosition; }
            set
            {
                m_vPosition = value;
                m_vDrawPosition.X = value.X; 
                m_vDrawPosition.Y = value.Y;
            }
        }

        public Vector3 Acceleration
        {
            get { return m_vAcceleration; }
            set { m_vAcceleration = value; }
        }

        public Vector3 Velocity
        {
            get { return m_vVelocity; }
            set { m_vVelocity = value; }
        }

        public Vector3 CurrentForce
        {
            get { return m_vCurrentForce; }
            set { m_vCurrentForce = value; }
        }

        public bool Sound
        {
            get { return m_bSound; }
            set { m_bSound = value; }
        }
    }
}
