using System;
using System.Collections.Generic;
using System.Linq;
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
    // Game state enumeration for switching between such states
    enum Game_State { MAINSCREEN, LEVEL1, LEVEL2, LEVEL3 };
    // Enumeration for sound effects ease of use
    enum SFX { LEVEL1, LEVEL2, LEVEL3, PAUSE, WIN, SHOOT, RESET, ENGINE, HIT, BULLETHIT, REVERSEGRAVITY };

    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class SpaceBalls : Microsoft.Xna.Framework.Game
    {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        
        // Load graphics and font
        SpriteFont gameFont;
        Texture2D m_texBg;
        Texture2D m_texPause;
        Texture2D m_texMenu;
        Texture2D m_texHelp;
        Texture2D[] m_textures;

        // Load audio
        Song m_menuMusic;
        Song m_levelMusic;
        SoundEffect[] m_soundEffects;
        
        // Load objects
        BlackHole m_hole; 
        Spaceship m_ship;
        Bullet m_bullet;
        GameObject m_ball;

        // Load levels and game state variables
        Game_State m_gameState;
        Map_Level1 m_level1;
        Map_Level2 m_level2;
        Map_Level3 m_level3;
        Map_Menu m_menu;
        bool m_paused;
        
        public SpaceBalls()
        {
            graphics = new GraphicsDeviceManager(this);
            graphics.PreferredBackBufferWidth = 1280;
            graphics.PreferredBackBufferHeight = 720;
            Content.RootDirectory = "Content";

            // Create graphics objects
            m_textures = new Texture2D[4];

            // Create audio objects
            m_soundEffects = new SoundEffect[11];
                                           
            // Create game objects
            m_hole = new BlackHole();
            m_ship = new Spaceship();
            m_bullet = new Bullet();
            m_ball = new GameObject();

            // Create level objects
            m_level1 = new Map_Level1();
            m_level2 = new Map_Level2();
            m_level3 = new Map_Level3();
            m_menu = new Map_Menu();
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // Load planets textures to be passed in map objects. This allow random texture switching on reset.
            m_textures[0] = Content.Load<Texture2D>("Graphics/Planet_Green");
            m_textures[1] = Content.Load<Texture2D>("Graphics/Planet_Purple");
            m_textures[2] = Content.Load<Texture2D>("Graphics/Planet_Blue");
            m_textures[3] = Content.Load<Texture2D>("Graphics/Planet_Grey");

            // Load screen state textures
            m_texBg = Content.Load<Texture2D>("Graphics/Backdrop");
            m_texPause = Content.Load<Texture2D>("Graphics/Pausescreen");
            m_texMenu = Content.Load<Texture2D>("Graphics/Title");
            m_texHelp = Content.Load<Texture2D>("Graphics/Instructions");

            // Load object textures
            m_ball.Texture = Content.Load<Texture2D>("Graphics/Ball");
            m_hole.BackTexture = Content.Load<Texture2D>("Graphics/BlackHoleBack");
            m_hole.Texture = Content.Load<Texture2D>("Graphics/BlackHoleMiddle");
            m_hole.FrontTexture = Content.Load<Texture2D>("Graphics/BlackHoleFront");
            m_bullet.Texture = Content.Load<Texture2D>("Graphics/Bullet");
            m_ship.Texture = Content.Load<Texture2D>("Graphics/SpaceshipAnim");

            // Load game font
            gameFont = Content.Load<SpriteFont>("Fonts/GameFont");

            // Load and initialize audio
            m_menuMusic = Content.Load<Song>("Music/MenuMusic");
            m_levelMusic = Content.Load<Song>("Music/GameMusic");
            MediaPlayer.Play(m_menuMusic); // Menu music plays initially
            MediaPlayer.IsRepeating = true; // Music should loop

            // Load sound effects into array
            m_soundEffects[(int)SFX.LEVEL1] = Content.Load<SoundEffect>("SoundEffects/Level1");
            m_soundEffects[(int)SFX.LEVEL2] = Content.Load<SoundEffect>("SoundEffects/Level2");
            m_soundEffects[(int)SFX.LEVEL3] = Content.Load<SoundEffect>("SoundEffects/Level3");
            m_soundEffects[(int)SFX.PAUSE] = Content.Load<SoundEffect>("SoundEffects/Pause");
            m_soundEffects[(int)SFX.WIN] = Content.Load<SoundEffect>("SoundEffects/Yay");
            m_soundEffects[(int)SFX.SHOOT] = Content.Load<SoundEffect>("SoundEffects/Pew");
            m_soundEffects[(int)SFX.REVERSEGRAVITY] = Content.Load<SoundEffect>("SoundEffects/Woosh");
            m_soundEffects[(int)SFX.RESET] = Content.Load<SoundEffect>("SoundEffects/Pop");
            m_soundEffects[(int)SFX.ENGINE] = Content.Load<SoundEffect>("SoundEffects/Thrust");
            m_soundEffects[(int)SFX.HIT] = Content.Load<SoundEffect>("SoundEffects/Hit");
            m_soundEffects[(int)SFX.BULLETHIT] = Content.Load<SoundEffect>("SoundEffects/BulletHit");

            // Pass these sound effects to relevant classes
            m_ship.SoundEffects = m_soundEffects;
            m_ship.SoundEffectInstance = m_soundEffects[(int)SFX.ENGINE].CreateInstance();
            m_ball.SoundEffects = m_soundEffects;
            m_bullet.SoundEffects = m_soundEffects;
            m_hole.SoundEffects = m_soundEffects;

            // Set up 6 animations and the frame width and height in the ship's sprite sheet
            m_ship.SetupAnims(6, 105, 128);

            // Add all ship state animations
            m_ship.AddAnimation((int)SpaceshipAnim.SSA_IDLE, AnimType.AT_SINGLE, 1, 1);
            m_ship.AddAnimation((int)SpaceshipAnim.SSA_ENGINE_L, AnimType.AT_LOOP, 5, 4);
            m_ship.AddAnimation((int)SpaceshipAnim.SSA_ENGINE_R, AnimType.AT_LOOP, 5, 4);
            m_ship.AddAnimation((int)SpaceshipAnim.SSA_ENGINE_BOTH, AnimType.AT_LOOP, 5, 4);
            m_ship.AddAnimation((int)SpaceshipAnim.SSA_ENGINE_MIDDLE, AnimType.AT_LOOP, 5, 4);
            m_ship.AddAnimation((int)SpaceshipAnim.SSA_ENGINE_BRAKE, AnimType.AT_LOOP, 5, 4);

            // Add frames for SSA_IDLE state
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_IDLE, 0, 0);

            // Add frames for SSA_ENGINE_L state
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_L, 0, 1);
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_L, 1, 1);
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_L, 2, 1);
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_L, 3, 1);

            // Add frames for SSA_ENGINE_R state
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_R, 0, 2);
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_R, 1, 2);
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_R, 2, 2);
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_R, 3, 2);

            // Add frames for SSA_ENGINE_BOTH state
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_BOTH, 0, 3);
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_BOTH, 1, 3);
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_BOTH, 2, 3);
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_BOTH, 3, 3);

            // Add frames for SSA_ENGINE_MIDDLE state
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_MIDDLE, 0, 4);
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_MIDDLE, 1, 4);
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_MIDDLE, 2, 4);
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_MIDDLE, 3, 4);

            // Add frames for SSA_ENGINE_BRAKE state
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_BRAKE, 0, 5);
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_BRAKE, 1, 5);
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_BRAKE, 2, 5);
            m_ship.AddAnimFrame((int)SpaceshipAnim.SSA_ENGINE_BRAKE, 3, 5);

            // Set the ships' animation SSA_IDLE
            m_ship.CurrentAnimationID = (int)SpaceshipAnim.SSA_IDLE;
            
            // Set up motion speed for objects
            m_ball.PixelsPerMetre = 2.0f;
            m_bullet.PixelsPerMetre = 1.0f;
            m_ship.PixelsPerMetre = 2.0f;
             
            // Set up ship engines attributes
            // Offset is tweaked to match texture co-ordinates
            m_ship.LeftEngineForce = new Vector3(0.0f, -1.0f, 0.0f);
            m_ship.LeftEngineOffset = new Vector3(-40.0f, 85.0f, 0.0f);
            m_ship.LeftEnginePower = 250000.0f;

            m_ship.RightEngineForce = new Vector3(0.0f, -1.0f, 0.0f);
            m_ship.RightEngineOffset = new Vector3(40.0f, 85.0f, 0.0f);
            m_ship.RightEnginePower = 250000.0f;
            
            m_ship.BrakeEngineForce = new Vector3(0.0f, 1.0f, 0.0f);
            m_ship.BrakeEngineOffset = new Vector3(0.0f, -85.0f, 0.0f);
            m_ship.BrakeEnginePower = 250000.0f;

            m_ship.ThrustEngineForce = new Vector3(0.0f, -1.0f, 0.0f);
            m_ship.ThrustEngineOffset = new Vector3(0.0f, 85.0f, 0.0f);
            m_ship.ThrustEnginePower = 250000.0f;
                
            // Initialize levels with game objects
            m_menu.Initialize(m_hole, m_ship, m_bullet, m_ball, m_textures, m_soundEffects, gameFont);
            m_level1.Initialize(m_hole, m_ship, m_bullet, m_ball, m_textures, m_soundEffects, gameFont);
            m_level2.Initialize(m_hole, m_ship, m_bullet, m_ball, m_textures, m_soundEffects, gameFont);
            m_level3.Initialize(m_hole, m_ship, m_bullet, m_ball, m_textures, m_soundEffects, gameFont);
            m_menu.Reset(); // Reset menu level for game start

            base.Initialize();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Update the game input state for this frame
            GameInput.Get().Update();
            // If not paused update screen
            if (!m_paused)
            {
                switch (m_gameState) // Switch between level code via game state identifier
                {
                    case Game_State.MAINSCREEN:
                        m_menu.Update(gameTime, GraphicsDevice);

                        if (GameInput.Get().IsKeyReleased(Keys.Escape) || GameInput.Get().IsControllerButtonReleased(Buttons.Back))
                            this.Exit(); // Exit game if on main screen

                        if (GameInput.Get().IsKeyReleased(Keys.D1) || GameInput.Get().IsControllerButtonReleased(Buttons.X))
                        { // Player has selected map no. 1
                            m_gameState = Game_State.LEVEL1; // Change game state
                            m_level1.Reset();  // Reset level once for start
                            MediaPlayer.Pause(); // Pause menu music
                            MediaPlayer.Play(m_levelMusic); // Play level music
                            m_soundEffects[(int)SFX.LEVEL1].Play(); // Play level load sound
                        }
                        else if (GameInput.Get().IsKeyReleased(Keys.D2) || GameInput.Get().IsControllerButtonReleased(Buttons.A))
                        { // Player has selected map no. 2
                            m_gameState = Game_State.LEVEL2;
                            m_level2.Reset();
                            MediaPlayer.Pause();
                            MediaPlayer.Play(m_levelMusic);
                            m_soundEffects[(int)SFX.LEVEL2].Play();
                        }
                        else if (GameInput.Get().IsKeyReleased(Keys.D3) || GameInput.Get().IsControllerButtonReleased(Buttons.B))
                        { // Player has selected map no. 3
                            m_gameState = Game_State.LEVEL3;
                            m_level3.Reset();
                            MediaPlayer.Pause();
                            MediaPlayer.Play(m_levelMusic);
                            m_soundEffects[(int)SFX.LEVEL3].Play();
                        }
                        break;

                    // Update levels accordingly
                    case Game_State.LEVEL1:
                        m_level1.Update(gameTime, GraphicsDevice);
                        break;

                    case Game_State.LEVEL2:
                        m_level2.Update(gameTime, GraphicsDevice);
                        break;

                    case Game_State.LEVEL3:
                        m_level3.Update(gameTime, GraphicsDevice);
                        break;
                }
            }
            if (m_gameState != Game_State.MAINSCREEN) // Always check for this input when not on mainscreen
            {
                if (GameInput.Get().IsKeyReleased(Keys.Escape) || GameInput.Get().IsControllerButtonReleased(Buttons.Back))
                { // Player has returned to main screen
                    m_paused = false; // Unpause always
                    m_menu.Reset(); // Reset menu
                    m_gameState = Game_State.MAINSCREEN; // Change game state
                    MediaPlayer.Pause(); // Pause level music
                    MediaPlayer.Play(m_menuMusic); // Play menu music
                }
                if (GameInput.Get().IsKeyReleased(Keys.P) || GameInput.Get().IsControllerButtonReleased(Buttons.Start))
                { // Check for pause & unpause
                    m_paused = !m_paused; // Switch between pause states
                    if (m_paused) m_soundEffects[(int)SFX.PAUSE].Play(); // If to Play pause sound effect
                    if (MediaPlayer.State == MediaState.Playing) MediaPlayer.Pause(); // Pause playing music
                    else MediaPlayer.Resume();
                }
            }
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            spriteBatch.Draw(m_texBg, Vector2.Zero, Color.White); // Draw generic background

            switch (m_gameState)
            {
                case Game_State.MAINSCREEN:
                    m_menu.Draw(spriteBatch); // Draw main screen objects
                    spriteBatch.Draw(m_texMenu, Vector2.Zero, Color.White); // Draw main screen overlay
                    break;

                    // Draw levels accordingly
                case Game_State.LEVEL1:
                    m_level1.Draw(spriteBatch);
                    break;

                case Game_State.LEVEL2:
                    m_level2.Draw(spriteBatch);
                    break;

                case Game_State.LEVEL3:
                    m_level3.Draw(spriteBatch);
                    break;
            }

            if(m_paused)
                spriteBatch.Draw(m_texPause, Vector2.Zero, Color.White); // Draw pause screen overlay if paused

            if ((GameInput.Get().IsKeyDown(Keys.I) || GameInput.Get().IsControllerButtonDown(Buttons.Y)) && (m_paused || m_gameState == Game_State.MAINSCREEN))
                spriteBatch.Draw(m_texHelp, Vector2.Zero, Color.White);

            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
