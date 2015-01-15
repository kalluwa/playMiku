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
using PmdModelLib;

namespace PlayMiku
{
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        
        #region Variables


        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        //staic information for window
        public static int Width = 640;
        public static int Height = 480;
        //camera pos
        float height = 15;
        float g_fDistanceToMiku = 20;
        bool g_bRotateCamera = false;
        public static Vector3 CameraPos = new Vector3(20, 30, -20);
        public static Vector3 CameraTarget = new Vector3(0, 5, 0);
        public static Vector3 CameraUp = new Vector3(0, 1, 0);
        public static Vector3 CameraRight = new Vector3(1, 0, 0);
        float g_time = 0.0f;
        //matrix
        public static Matrix World = Matrix.Identity;
        public static Matrix View = Matrix.CreateLookAt(CameraPos, new Vector3(0, 10, 0), new Vector3(0, 1, 0));
        public static Matrix Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, 1.3333f, 1.0f, 500.0f);
        //outTest Model
        PmdModel miku;
        //miku animation
        VmdAnimation anim;
        // Helpers
#if WINDOWS
        BoneHelper helper;
#endif
        //ik 
        bool bShowAllBone=false;
        bool bShowIkBone = false;
        bool bShowMiku = true;
        int showBoneIndex = 75;
        bool bShowSingleIkChain = false;
        int showSingleChainIndex = 0;
        //control
        KeyboardState lastKeyState;
        MouseState lastMouseState;

        #endregion
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            //basicSetting
            IsMouseVisible = true;

            graphics.PreferredBackBufferWidth = Width;
            graphics.PreferredBackBufferHeight = Height;
            //GraphicsDevice.PresentationParameters.BackBufferWidth = Width;
            //GraphicsDevice.PresentationParameters.BackBufferHeight = Height;

        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

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
            Font.Initialze(GraphicsDevice, Content);

            miku = Content.Load<PmdModel>("Models/miku");
            miku.Initialize(GraphicsDevice, Content);
            anim = Content.Load<VmdAnimation>("Anims/Lamb");
            miku.SetAnim(anim);
            anim.AnimationSpeed =0.1f;
            //bone helper
#if WINDOWS
            helper = new BoneHelper(GraphicsDevice, Content);
#endif
            
            // TODO: use this.Content to load your game content here
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            // Allows the game to exit
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                this.Exit();
            
            // TODO: Add your update logic here
            //update view
            if (g_bRotateCamera)
            {
                g_time += gameTime.ElapsedGameTime.Milliseconds * 0.001f;
                CameraPos = new Vector3(g_fDistanceToMiku * (float)Math.Cos(g_time), height, g_fDistanceToMiku * (float)Math.Sin(g_time));
                View = Matrix.CreateLookAt(CameraPos, new Vector3(0, height-5, 0), new Vector3(0, 1, 0));

            }
            
            miku.Update(gameTime);


            #region Control 
            //control region
            KeyboardState currentState = Keyboard.GetState();
            MouseState currentMouseState = Mouse.GetState();

            if (currentState.IsKeyDown(Keys.C) && lastKeyState.IsKeyUp(Keys.C))
            {
                miku = Content.Load<PmdModel>("Models/model_1");
                miku.Initialize(GraphicsDevice, Content);
                anim = Content.Load<VmdAnimation>("Anims/Lamb");
                miku.SetAnim(anim);
                anim.AnimationSpeed = 0.1f;
            }

            if (currentState.IsKeyDown(Keys.V) && lastKeyState.IsKeyUp(Keys.V))
            {
                miku = Content.Load<PmdModel>("Models/miku");
                miku.Initialize(GraphicsDevice, Content);
                anim = Content.Load<VmdAnimation>("Anims/Lamb");
                miku.SetAnim(anim);
                anim.AnimationSpeed = 0.1f;
            }
            if (currentState.IsKeyDown(Keys.Space)&&lastKeyState.IsKeyUp(Keys.Space))
                bShowAllBone = !bShowAllBone;
            if (currentState.IsKeyDown(Keys.Enter)&&lastKeyState.IsKeyUp(Keys.Enter))
                g_bRotateCamera = !g_bRotateCamera;
            if (currentState.IsKeyDown(Keys.P)&&lastKeyState.IsKeyUp(Keys.P))
                miku.Pause = !miku.Pause;
            if (currentState.IsKeyDown(Keys.I) && lastKeyState.IsKeyUp(Keys.I))
                miku.PauseBoneUpdate = !miku.PauseBoneUpdate;
            //show ik bone
            if (currentState.IsKeyDown(Keys.K) && lastKeyState.IsKeyUp(Keys.K))
                bShowIkBone = !bShowIkBone;
            if (currentState.IsKeyDown(Keys.Up) && lastKeyState.IsKeyUp(Keys.Up))
                showBoneIndex = (showBoneIndex + 1) % miku.BoneCount;
            else if (currentState.IsKeyDown(Keys.Down) && lastKeyState.IsKeyUp(Keys.Down))
                showBoneIndex = (showBoneIndex - 1 + miku.BoneCount) % miku.BoneCount;
            if (currentState.IsKeyDown(Keys.Left))
                showBoneIndex = (showBoneIndex + 1) % miku.BoneCount;
            else if (currentState.IsKeyDown(Keys.Right))
                showBoneIndex = (showBoneIndex - 1 + miku.BoneCount) % miku.BoneCount;

            Vector3 forward = Vector3.Cross(CameraUp, CameraRight);
            forward.Normalize();
            
            if (currentState.IsKeyDown(Keys.W) || currentState.IsKeyDown(Keys.A) ||
                currentState.IsKeyDown(Keys.D) || currentState.IsKeyDown(Keys.S))
            {
                Vector3 moveMent=Vector3.Zero;
                if (currentState.IsKeyDown(Keys.W))
                {
                    moveMent= forward;
                }
                else if (currentState.IsKeyDown(Keys.S))
                {
                    moveMent= - forward;
                }
                if (currentState.IsKeyDown(Keys.A))
                {
                    moveMent=  -CameraRight;
                }
                else if (currentState.IsKeyDown(Keys.D))
                {
                    moveMent=   CameraRight;
                }
                moveMent *= 0.05f;
                CameraPos+=moveMent;
                CameraTarget+=moveMent;
                View = Matrix.CreateLookAt(CameraPos, CameraTarget, CameraUp);
            }

            if ((lastMouseState.LeftButton==ButtonState.Pressed)&&(lastMouseState.X != currentMouseState.X ||
                lastMouseState.Y != currentMouseState.Y))
            {
                float xMove = lastMouseState.X - currentMouseState.X;
                float yMove = lastMouseState.Y - currentMouseState.Y;

                CameraTarget += 0.05f*(-xMove * CameraRight + yMove * CameraUp);
                forward = CameraTarget - CameraPos;
                forward.Normalize();

                CameraRight = Vector3.Cross(forward,CameraUp);
                CameraRight.Normalize();
                CameraUp = Vector3.Cross(CameraRight, forward);
                CameraUp.Normalize();

                //CameraUp = new Vector3(0, 1, 0);
                View = Matrix.CreateLookAt(CameraPos, CameraTarget, CameraUp);
            
            }
            if (currentState.IsKeyDown(Keys.Escape) && lastKeyState.IsKeyUp(Keys.Escape))
            bShowMiku = !bShowMiku;
            //show single chain
            if (currentState.IsKeyDown(Keys.F1) && lastKeyState.IsKeyUp(Keys.F1))
                bShowSingleIkChain = !bShowSingleIkChain;
            if (currentState.IsKeyDown(Keys.PageDown) && lastKeyState.IsKeyUp(Keys.PageDown))
                showSingleChainIndex = (showSingleChainIndex - 1 + miku.IK_Chains.IKChainCount) % miku.IK_Chains.IKChainCount;
            if (currentState.IsKeyDown(Keys.PageUp) && lastKeyState.IsKeyUp(Keys.PageUp))
                showSingleChainIndex = (showSingleChainIndex + 1) % miku.IK_Chains.IKChainCount;
            
            lastKeyState = currentState;
            lastMouseState = currentMouseState;
            Font.DrawMessage(showBoneIndex.ToString(), 20, 20);
            #endregion
            base.Update(gameTime);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);
            
            // TODO: Add your drawing code here
            //if(bShowMiku)
            miku.Render(World, View, Projection);
#if WINDOWS
            if (miku != null)
            {
                if(bShowAllBone)
                    helper.DrawBones(miku.BoneFinalPos, miku.BoneParent);
                if (bShowIkBone)
                {
                //    //ik nodes
                    helper.DrawIK(miku.BoneFinalPos, miku.BoneList);
                //    //ik target
                    helper.DrawIK(miku.BoneFinalPos, miku.BoneList,2,Color.Blue);
                //    //single bone
                    helper.DrawSingleIK(miku.BoneFinalPos, miku.BoneList, showBoneIndex, Color.Yellow);
                }
                if (bShowSingleIkChain)
                {
                    GraphicsDevice.Clear(Color.White);
                    helper.DrawIKChain(miku.BoneFinalPos, miku.BoneLocalPose, miku.IK_Chains.IkChains[showSingleChainIndex], Color.Blue);
                }
            }

            Font.Draw();
#endif
            base.Draw(gameTime);
        }
    }
}
