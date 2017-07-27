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
using Silhouette1.Ops;

namespace Silhouette1
{
	/// <summary>
	/// This is the main type for your game
	/// </summary>
	public class Game1 : Microsoft.Xna.Framework.Game
	{
		GraphicsDeviceManager graphics;
		SpriteBatch spriteBatch;
		Texture2D textureFade;
		public static Texture2D textureWhite;
		uint[] data;
		int x, y;
		int w, h, sw, sh;
        private KinectSensor kinectSensor = null;
        private CoordinateMapper coordinateMapper = null;
        private BodyFrameReader bodyFrameReader = null;
        private DepthFrameReader depthFrameReader = null;
        private Body[] bodies = null;
		private Body[] lastBodies = null;
        Quad quad;
        VertexDeclaration vertexDeclaration;
        Matrix viewMatrix, projectionMatrix;
		Effect display, depthEffect;
        Matrix worldMatrix;
		float zoom = 1, rotx = 0, roty = 0, mx, my, lastmx = 0, lastmy = 0;
		bool pressed = false;
		float lastzoomdelta = 0;
		public Form1 form;
		ushort[] depth;
		List<Texture2D> depths = new List<Texture2D>();
		int depthcount = 2, depthframecount = 0;

		float timestep = 1.0f;
		RenderTargetDouble velocity, density, velocityDivergence, velocityVorticity, pressure;
		Advect advect;
		Boundary boundary;
		Jacobi diffuse;
		Divergence divergence;
		Jacobi poissonPressureEq;
		Gradient gradient;
		Splat splat;
		Vorticity vorticity;
		VorticityConfinement vorticityConfinement;
		Vector3 source = new Vector3(0.0f, 0.0f, 0.8f);
        Vector3 ink = new Vector3(0.0f, 0.06f, 0.19f);
		float hue;
		float viscosity = 0.8f;

		public Game1()
		{
			graphics = new CustomGraphicsDeviceManager(this);
			this.graphics.PreferredBackBufferWidth = 1280;
			this.graphics.PreferredBackBufferHeight = 720;
			this.graphics.IsFullScreen = true;

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
			// this.depthFrameReader = this.kinectSensor.DepthFrameSource.OpenReader();
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

            // Setup our BasicEffect for drawing the quad
            worldMatrix = Matrix.CreateScale(w / (float)h, 1, 1);
			display = Content.Load<Effect>("basic");
			depthEffect = Content.Load<Effect>("depth");

			Matrix projection = Matrix.CreateOrthographicOffCenter(0, 
					GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 0, 1);
			Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);
			display.Parameters["MatrixTransform"].SetValue(halfPixelOffset * projection);
			depthEffect.Parameters["MatrixTransform"].SetValue(halfPixelOffset * projection);

            // Create a vertex declaration
            vertexDeclaration = new VertexDeclaration(
                new VertexElement[] {
                    new VertexElement(0, VertexElementFormat.Vector3,VertexElementUsage.Position, 0),
                    new VertexElement(12, VertexElementFormat.Vector3,VertexElementUsage.Normal, 0),
                    new VertexElement(24, VertexElementFormat.Vector2,VertexElementUsage.TextureCoordinate,0) 
                });

			velocity = new RenderTargetDouble(GraphicsDevice, w, h);
			density = new RenderTargetDouble(GraphicsDevice, w, h);
			velocityDivergence = new RenderTargetDouble(GraphicsDevice, w, h);
			velocityVorticity = new RenderTargetDouble(GraphicsDevice, w, h);
			pressure = new RenderTargetDouble(GraphicsDevice, w, h);

			advect = new Advect(w, h, timestep, Content);
			boundary = new Boundary(w, h, Content);
			diffuse = new Jacobi(Content.Load<Effect>("jacobivector"), w, h);
			divergence = new Divergence(w, h, Content);
			poissonPressureEq = new Jacobi(Content.Load<Effect>("jacobiscalar"), w, h);
			gradient = new Gradient(w, h, Content);
			splat = new Splat(w, h, Content);
			vorticity = new Vorticity(w, h, Content);
			vorticityConfinement = new VorticityConfinement(Content.Load<Effect>("vorticityforce"), w, h, timestep);

			this.bodyFrameReader.FrameArrived += this.Reader_FrameArrived;
			// this.depthFrameReader.FrameArrived += DepthFrameReader_FrameArrived;
		}

		private void DepthFrameReader_FrameArrived(object sender, DepthFrameArrivedEventArgs e)
		{
			using(DepthFrame frame = e.FrameReference.AcquireFrame()) {
				if(frame != null)
				{
					GraphicsDevice.Textures[2] = null;
					GraphicsDevice.Textures[3] = null;

					if(depth == null)
					{
						depth = new ushort[frame.FrameDescription.LengthInPixels];
						for(int i = 0; i < depthcount; i++)
							depths.Add(new Texture2D(GraphicsDevice, frame.FrameDescription.Width, frame.FrameDescription.Height, false, SurfaceFormat.Single));
					}
					frame.CopyFrameDataToArray(depth);
					depths.Insert(0, depths.Last());
					depths.RemoveAt(depths.Count - 1);
					float[] fdepth = new float[depth.Length];
					for(int i = 0; i < depth.Length; i++)
						fdepth[i] = ((float)depth[i] / frame.DepthMaxReliableDistance);
					depths[0].SetData(fdepth);
					depthframecount++;
				}
			}
		}

		private void AddForcesFromDepthFrame()
		{
			if(depthframecount > depthcount)
			{
				depthEffect.Parameters["SourceTexture"].SetValue(velocity.Read);
				depthEffect.Parameters["DepthTexture"].SetValue(depths[0]);
				depthEffect.Parameters["DepthTextureOld"].SetValue(depths[depths.Count - 1]);
				depthEffect.Parameters["DepthSize"].SetValue(new Vector2(depths[0].Width, depths[0].Height));

				SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
				Rectangle r = new Rectangle(0, 0, w, h);

				// update velocity with depth info
				GraphicsDevice.SetRenderTarget(velocity.Write);
				spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, depthEffect);
				spriteBatch.Draw(Game1.textureWhite, r, Color.White);
				spriteBatch.End();
				GraphicsDevice.SetRenderTarget(null);
				velocity.Swap();
			}
		}

		private void Reader_FrameArrived(object sender, BodyFrameArrivedEventArgs e)
        {
			using(BodyFrame bodyFrame = e.FrameReference.AcquireFrame())
			{
				if(bodyFrame != null)
				{
					this.lastBodies = this.bodies;
					this.bodies = new Body[bodyFrame.BodyCount];
					bodyFrame.GetAndRefreshBodyData(this.bodies);
					// AddForcesFromBodies(this.lastBodies, this.bodies);
				}
			}
		}

		private void AddForcesFromBodies()
		{
			if(lastBodies == null || bodies == null)
				return;

			for(int i = 0; i < bodies.Length; i++)
				if(bodies[i].IsTracked && lastBodies[i] != null && lastBodies[i].IsTracked)
				{
					foreach(var joint in bodies[i].Joints)
						// if(joint.Key == JointType.HandLeft)
						{
							Vector2 c = ConvertJoint(joint.Value.Position);
							Joint lastJoint = lastBodies[i].Joints[joint.Key];
							Vector2 o = ConvertJoint(lastJoint.Position);

							float f = 10.0f;
							Vector3 force = new Vector3( (c-o)*f, 0.0f);
							Vector2 point = o;

							this.splat.Render(GraphicsDevice, this.velocity, force, point, 4f, this.velocity);
							this.boundary.Render(GraphicsDevice, this.velocity, -1, this.velocity);
							this.splat.Render(GraphicsDevice, this.density, this.source, point, 4f, this.density);
						}
				}
		}  

		Vector2 ConvertJoint(CameraSpacePoint j) {
			float x = ((0.1f - j.X) * w / 2 + w / 2);
			float y = ((0.1f - j.Y) * h / 2 + h / 2);
			return new Vector2(x,y);
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
			KeyboardState keyboardState = Keyboard.GetState();
			if(keyboardState.IsKeyDown(Keys.Escape))
				this.Exit();

			//var mouse = Mouse.GetState();
			//mx = mouse.X;
			//my = mouse.Y;
			//if(mx >= 0 && mx < sw && my >= 0 && my <= sh) {
			//	mx -= sw/2; mx /= sw;
			//	my -= sh/2; my /= sh;
			//} else {
			//	pressed = false; 
			//	mx = my = 0;
			//}
				
			//if (mouse.LeftButton == ButtonState.Pressed) {
			//	if (!pressed) {
			//		pressed = true;
			//		lastmx = mx; lastmy = my;
			//	}
			//	rotx += (mx - lastmx);
			//	roty += (my - lastmy);
			//	lastmx = mx; lastmy = my;
			//} else 
			//if (mouse.LeftButton == ButtonState.Released) {
			//	pressed = false;
			//}

			//worldMatrix = Matrix.CreateScale(w / (float)h * zoom, 1.34f * zoom, 1);
			//worldMatrix = worldMatrix * Matrix.CreateRotationY(rotx);
			//worldMatrix = worldMatrix * Matrix.CreateRotationX(roty);

			//float zoomdelta = mouse.ScrollWheelValue / 10000.0f;
			//zoom += zoomdelta - lastzoomdelta;
			//lastzoomdelta = zoomdelta; 

			base.Update(gameTime);
		}

		/// <summary>
		/// This is called when the game should draw itself.
		/// </summary>
		/// <param name="gameTime">Provides a snapshot of timing values.</param>
		protected override void Draw(GameTime gameTime)
		{
			var temp = this.advect.dissipation;
			this.advect.dissipation = 1;
			this.advect.Render(GraphicsDevice, this.velocity, this.velocity, this.velocity);
			this.boundary.Render(GraphicsDevice, this.velocity, -1, this.velocity);

			this.advect.dissipation = temp;
			this.advect.Render(GraphicsDevice, this.velocity, this.density, this.density);

			// AddForcesFromDepthFrame();
			AddForcesFromMouse();
			AddForcesFromBodies();

			// vorticity
			this.vorticity.Render(GraphicsDevice, this.velocity, this.velocityVorticity);
			this.vorticityConfinement.Render(GraphicsDevice, this.velocity, this.velocityVorticity, this.velocity);
			this.boundary.Render(GraphicsDevice, this.velocity, -1, this.velocity);

			//// viscosity
			this.diffuse.alpha = 1.0f / (this.viscosity * this.timestep);
			this.diffuse.beta = 4 + this.diffuse.alpha;
			this.diffuse.Render(GraphicsDevice, this.velocity, this.velocity, this.velocity, this.boundary, -1);

			Project();

			GraphicsDevice.Clear(Color.Black);

			//basicEffect.World = worldMatrix;
			//basicEffect.Texture = velocity.Read;

			// Draw the quad
			//foreach(EffectPass pass in basicEffect.CurrentTechnique.Passes)
			//{
			//	pass.Apply();
			//	GraphicsDevice.DrawUserIndexedPrimitives<VertexPositionNormalTexture>(PrimitiveType.TriangleList, quad.Vertices, 0, 4, quad.Indexes, 0, 2);
			//}

			display.Parameters["ScreenTexture"].SetValue(density.Read);

			SpriteBatch spriteBatch = new SpriteBatch(GraphicsDevice);
			Rectangle r = new Rectangle(0, 0, w, h);

			spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, display);
			spriteBatch.Draw(textureWhite, r, Color.White);
			spriteBatch.End();

			GraphicsDevice.Textures[2] = null;
			GraphicsDevice.Textures[3] = null;

			ChangeColor();

			base.Draw(gameTime);
		}

		private void ChangeColor()
		{
			System.Drawing.Color color = (System.Drawing.Color)new HSLColor(hue,1.0,0.5); 
			this.source = new Vector3( color.R / 255.0f, color.G / 255.0f, color.B / 255.0f);
			hue += 0.01f;
			if(hue > 1.0) {
				hue = 0.0f;
			}
		}

		private void Project()
		{
            this.divergence.Render(GraphicsDevice,this.velocity,this.velocityDivergence);

			// 0 is our initial guess for the poisson equation solver
			GraphicsDevice.SetRenderTarget(pressure.Write);
			GraphicsDevice.Clear(Color.Black);
			GraphicsDevice.SetRenderTarget(null);
			pressure.Swap();

			this.poissonPressureEq.alpha = -1.0f * 1.0f;
            this.poissonPressureEq.Render(GraphicsDevice,this.pressure,this.velocityDivergence,this.pressure,this.boundary,1.0f);

            this.gradient.Render(this.GraphicsDevice,this.pressure,this.velocity,this.velocity);
            this.boundary.Render(GraphicsDevice, this.velocity, -1, this.velocity);
		}

		private void AddForcesFromMouse()
		{
			float f = 1000f;
			MouseState state = Mouse.GetState();
			if (state.X != lastmx || state.Y != lastmy) {
				if(state.LeftButton == ButtonState.Pressed) {
					Vector2 point = new Vector2((float)lastmx, (float)lastmy);
					Vector3 force = new Vector3((float)(state.X-lastmx) / w * f, (float)(state.Y-lastmy) / h * f, 0.0f);
					this.splat.Render(GraphicsDevice, this.velocity, force, point, 200f, this.velocity);
					this.boundary.Render(GraphicsDevice, this.velocity, -1, this.velocity);
				}
				if(state.RightButton == ButtonState.Pressed) {
					Vector2 point = new Vector2((float)state.X, (float)state.Y);
					this.splat.Render(GraphicsDevice, this.density, this.source, point, 200f, this.density);
				}
			}
			lastmx = state.X;
			lastmy = state.Y;
		}

		private void DrawBordersMarkers()
		{
			Rectangle r;

			DrawBorders();

			if(bodies != null)
			{
				for(int i = 0; i < bodies.Length; i++)
					if(bodies[i].IsTracked)
					{
						foreach(var joint in bodies[i].Joints)
						{
							float jx = joint.Value.Position.X;
							float jy = joint.Value.Position.Y;
							x = (int)((0.5 - jx) * w / 2 + w / 2);
							y = (int)((0.1 - jy) * h / 2 + h / 2);

							r = new Rectangle(x, y, 10, 10);
							spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
							spriteBatch.Draw(textureWhite, r, Color.White);
							spriteBatch.End();
						}
					}
			}		   
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
