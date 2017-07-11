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
using Microsoft.Kinect;
using VideoQuad;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace Silhouette1
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Texture2D textureFade, textureWhite;
		uint[] data;
		int x, y;
		int w, h, sw, sh;
        private KinectSensor kinectSensor = null;
        private CoordinateMapper coordinateMapper = null;
        private BodyFrameReader bodyFrameReader = null;
        private Body[] bodies = null;
        Quad quad;
        VertexDeclaration vertexDeclaration;
        Matrix viewMatrix, projectionMatrix;
        BasicEffect basicEffect;
        Matrix worldMatrix;
		float zoom = 1, rotx = 0, roty = 0, mx, my, lastmx = 0, lastmy = 0;
		bool pressed = false;
		float lastzoomdelta = 0;
		// Texture2D texture;
		RenderTarget2D texture;
		public Form1 form;

		public Game1()
		{
			graphics = new GraphicsDeviceManager(this);
			//this.graphics.PreferredBackBufferWidth = 1920;
			//this.graphics.PreferredBackBufferHeight = 1080;
			//this.graphics.IsFullScreen = true;

			Content.RootDirectory = "Content";
		}

		/// <summary>
		/// Allows the game to perform any initialization it needs to before starting to run.
		/// This is where it can query for any required services and load any non-graphic
		/// related content.  Calling base.Initialize will enumerate through any components
		/// and initialize them as well.
		/// </summary>
		protected override void Initialize()
		{
			this.kinectSensor = KinectSensor.GetDefault();
            this.coordinateMapper = this.kinectSensor.CoordinateMapper;
            FrameDescription frameDescription = this.kinectSensor.DepthFrameSource.FrameDescription;
            this.bodyFrameReader = this.kinectSensor.BodyFrameSource.OpenReader();
            this.kinectSensor.Open();

            quad = new Quad(Vector3.Zero, Vector3.Backward, Vector3.Up, 1, 1);
            viewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 2),
                Vector3.Zero, Vector3.Up);
            projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                MathHelper.PiOver4, 4.0f / 3.0f, 1, 500);

			base.Initialize();

			this.IsFixedTimeStep = false;			
			graphics.SynchronizeWithVerticalRetrace = false;
			this.IsMouseVisible = true;
		}

		/// <summary>
		/// LoadContent will be called once per game and is the place to load
		/// all of your content.
		/// </summary>
		protected override void LoadContent()
		{
			// Create a new SpriteBatch, which can be used to draw textures.
			spriteBatch = new SpriteBatch(GraphicsDevice);

			w = GraphicsDevice.Viewport.Bounds.Width;
			h = GraphicsDevice.Viewport.Bounds.Height;
			data = new uint[w*h];
			sw = GraphicsDevice.Viewport.Bounds.Width;
			sh = GraphicsDevice.Viewport.Bounds.Height;

			// texture = new Texture2D(GraphicsDevice, w, h);
			textureFade = new Texture2D(GraphicsDevice, 1, 1);
			textureFade.SetData(new uint[] { 0x0f000000 });
			textureWhite = new Texture2D(GraphicsDevice, 1, 1);
			textureWhite.SetData(new uint[] { 0xFFFFFFFF });

			texture = new RenderTarget2D(GraphicsDevice, w, h, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
			// texture.RenderTargetUsage = RenderTargetUsage.PreserveContents;

            // Setup our BasicEffect for drawing the quad
            worldMatrix = Matrix.CreateScale(w / (float)h, 1, 1);
            basicEffect = new BasicEffect(graphics.GraphicsDevice);
            basicEffect.View = viewMatrix;
            basicEffect.Projection = projectionMatrix;
            basicEffect.TextureEnabled = true;
			basicEffect.Texture = texture;

            // Create a vertex declaration
            vertexDeclaration = new VertexDeclaration(
                new VertexElement[] {
                    new VertexElement(0, VertexElementFormat.Vector3,VertexElementUsage.Position, 0),
                    new VertexElement(12, VertexElementFormat.Vector3,VertexElementUsage.Normal, 0),
                    new VertexElement(24, VertexElementFormat.Vector2,VertexElementUsage.TextureCoordinate,0) 
                });

			this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
		}

        private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
            using (BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
            {
                if (bodyFrame != null)
                {
                    if (this.bodies == null)
                    {
                        this.bodies = new Body[bodyFrame.BodyCount];
                    }

                    // The first time GetAndRefreshBodyData is called, Kinect will allocate each Body in the array.
                    // As long as those body objects are not disposed and not set to null in the array,
                    // those body objects will be re-used.
                    bodyFrame.GetAndRefreshBodyData(this.bodies);
                }
            }
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
			KeyboardState keyboardState = Keyboard.GetState();
			if(keyboardState.IsKeyDown(Keys.Escape))
				this.Exit();

			// texture.SetData(data);

			var mouse = Mouse.GetState();
			mx = mouse.X;
			my = mouse.Y;
			if(mx >= 0 && mx < sw && my >= 0 && my <= sh) {
				mx -= sw/2; mx /= sw;
				my -= sh/2; my /= sh;
			} else {
				pressed = false; 
				mx = my = 0;
			}
				
			if (mouse.LeftButton == ButtonState.Pressed) {
				if (!pressed) {
					pressed = true;
					lastmx = mx; lastmy = my;
				}
				rotx += (mx - lastmx);
				roty += (my - lastmy);
				lastmx = mx; lastmy = my;
			} else 
			if (mouse.LeftButton == ButtonState.Released) {
				pressed = false;
			}

			worldMatrix = Matrix.CreateScale(w / (float)h * zoom, 1.34f * zoom, 1);
			worldMatrix = worldMatrix * Matrix.CreateRotationY(rotx);
			worldMatrix = worldMatrix * Matrix.CreateRotationX(roty);

			float zoomdelta = mouse.ScrollWheelValue / 10000.0f;
			zoom += zoomdelta - lastzoomdelta;
			lastzoomdelta = zoomdelta; 

			base.Update(gameTime);
		}

		private void DrawMarker(uint[] data, int x, int y)
		{
			if( x < 0 || y < 0 || x > w-10 || y >= h-10) {
				return;
			}
			for( int i = 0; i < 10; i++)
				for( int j = 0; j < 10; j++)
					data[(x+i)+(y+j)*w] = 0xFF00FF00;
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			Rectangle r;

			GraphicsDevice.Textures[0] = null;

			GraphicsDevice.SetRenderTarget(texture);
			r = new Rectangle(0, 0, w, h);
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
			spriteBatch.Draw(textureFade, r, Color.White);
			spriteBatch.End();
			DrawBorders();

			if(bodies != null) {
				for(int i = 0; i < bodies.Length; i++)
					if(bodies[i].IsTracked) {
						foreach( var joint in bodies[i].Joints) {
							float jx = joint.Value.Position.X;
							float jy = joint.Value.Position.Y;
							x = (int)((0.5-jx) * w/2 + w/2);
							y = (int)((0.1-jy) * h/2 + h/2);
							DrawMarker(data, x, y);

							r = new Rectangle( x, y, 10, 10);
							spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
							spriteBatch.Draw(textureWhite, r, Color.White);
							spriteBatch.End();
						}
					}
			}

			GraphicsDevice.SetRenderTarget(null);
			texture.GetData(data);

			if(form.bitmap != null) {
				System.Drawing.Bitmap bitmap = form.bitmap; //new System.Drawing.Bitmap(w, h);
				BitmapData data = bitmap.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, bitmap.PixelFormat);
				int bufferSize = data.Height * data.Stride;
				byte[] bytes = new byte[bufferSize];    
				Marshal.Copy(data.Scan0, bytes, 0, bytes.Length);
				texture.SetData(bytes);
				bitmap.UnlockBits(data);
			}

			GraphicsDevice.Clear(Color.Black);	
			
			basicEffect.World = worldMatrix;

            // Draw the quad
            foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, quad.Vertices, 0, 4, quad.Indexes, 0, 2);
            }

			base.Draw(gameTime);
		}

		private void DrawBorders()
		{
			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
			foreach( var r in new Rectangle[] {
					new Rectangle( 0, 0, w, 10),
					new Rectangle( 0, 0, 10, h),
					new Rectangle( w-10, 0, w, h),
					new Rectangle( 0, h-10, w, h) }) {
				spriteBatch.Draw(textureWhite, r, Color.White);
			}
			spriteBatch.End();
		}
	}
}
