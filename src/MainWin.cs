//
//  MainWin.cs
//
//  Author:
//       Jean-Philippe Bruyère <jp.bruyere@hotmail.com>
//
//  Copyright (c) 2016 jp
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using System;
using Crow;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Tetra;
using System.IO;
using Tetra.DynamicShading;
using System.Reflection;

namespace MagicCrow
{	
	[StructLayout (LayoutKind.Sequential, Pack = 1)]
	public struct CardInstancedData
	{
		public Matrix4 modelMats;
		public int picked;//0,1
	}
	class Magic : CrowWindow
	{
		#region  scene matrix and vectors
		public static Matrix4 modelview;
		public static Matrix4 reflectedModelview;
		public static Matrix4 orthoMat//full screen quad rendering
		= OpenTK.Matrix4.CreateOrthographicOffCenter (-0.5f, 0.5f, -0.5f, 0.5f, 1, -1);
		public static Matrix4 projection;
		public static Matrix4 invMVP;
		public static int[] viewport = new int[4];

		public float EyeDist {
			get { return eyeDist; }
			set {
				eyeDist = value;
				UpdateViewMatrix ();
			}
		}
		public static Vector3 vEyeTarget = new Vector3(0f, 0f, 0f);
		public static Vector3 vEye;
		public static Vector3 vLookInit = Vector3.Normalize(new Vector3(0.0f, -0.7f, 0.7f));
		public static Vector3 vLook = vLookInit;  // Camera vLook Vector
		public float zFar = 60.0f;
		public float zNear = 0.1f;
		public float fovY = (float)Math.PI / 4;

		static float eyeDist = 20f;
		static float eyeDistTarget = 20f;
		float MoveSpeed = 0.02f;
		float RotationSpeed = 0.005f;
		float ZoomSpeed = 2f;
		float viewZangle, viewXangle;

		public Vector4 vLight = new Vector4 (0.5f, 0.5f, -1f, 0f);
		float[] clearColor = new float[] {0.5f,0.5f,0.9f,1.0f};
		#endregion

		public static Vector3 vFocusedPoint {
			get { return vEyeTarget + vLook * (eyeDist - 3f); }
		}
		public static Vector3 vGroupedFocusedPoint {
			get { return vEyeTarget + vLook * (eyeDist - 10f); }
		}
		public static float FocusAngle {
			get { return Vector3.CalculateAngle (vLook, Vector3.UnitZ); }
		}

		#region GL
		public static int numSamples = 1;
		MeshesGroup<MeshData> meshes;

		static RenderCache mainCache;
		static MultiShader mainShader;
		public static Mat4InstancedShader piecesShader;

		static InstancedVAO<MeshData, CardInstancedData> mainVao;
		static InstancedModel<CardInstancedData> vaoiCards;
//		static VAOItem<CardInstancedData> vaoiPlate;
//		static VAOItem<CardInstancedData> vaoiFSQ;

		void initOpenGL()
		{
			GL.Enable (EnableCap.Multisample);
			GL.Enable(EnableCap.SampleShading);
			//GL.Enable(EnableCap.CullFace);
			//GL.CullFace(CullFaceMode.Back);
			//GL.Hint (HintTarget.GenerateMipmapHint, HintMode.Fastest);

			GL.Enable(EnableCap.DepthTest);
			GL.DepthFunc (DepthFunction.Lequal);

			GL.PrimitiveRestartIndex (int.MaxValue);
			GL.Enable (EnableCap.PrimitiveRestart);

			GL.Enable (EnableCap.Blend);
			GL.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);


			//mainCache = new RenderCache (ClientSize);

			initScene ();

			ErrorCode err = GL.GetError ();
			Debug.Assert (err == ErrorCode.NoError, "OpenGL Error");
		}
		void initScene (){
			meshes = new MeshesGroup<MeshData>();
			vaoiCards = new InstancedModel<CardInstancedData> (meshes.Add (Mesh<MeshData>.CreateQuad (0, 0, 0, 1, 1.425f, 1, 1)));

			mainVao = new InstancedVAO<MeshData, CardInstancedData> (meshes);

			vaoiCards.AddInstance ();

			mainShader = new MultiShader (
				"#MagicCrow.shaders.main.vert",
				"#MagicCrow.shaders.main.frag");
			
		}

		void draw(){
			mainShader.Enable ();
			mainShader.SetMVP(modelview * projection);
			mainShader.SetCardsPass ();
			mainVao.Bind ();
			GL.ActiveTexture (TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2DArray, cardTextures);
			mainVao.Render (BeginMode.TriangleStrip, vaoiCards.VAOPointer, vaoiCards.Instances);
			mainVao.Unbind ();
		}
		void draw(InstancedModel<CardInstancedData> model, BeginMode beginMode = BeginMode.Triangles){			
			GL.BindTexture (TextureTarget.Texture2D, model.Diffuse);
			mainVao.Render (beginMode, model.VAOPointer, model.Instances);
		}

		void paintCacheAndUI()
		{
//			mainShader.SetMVP(orthoMat);
//			//mainShader.SetCachePass ();
//			//mainVao.Render (PrimitiveType.TriangleStrip, vaoiFSQ);
//
//			mainShader.SetUIPass ();
//			GL.Disable (EnableCap.DepthTest);
//			GL.ActiveTexture (TextureUnit.Texture10);
//			GL.BindTexture (TextureTarget.Texture2D, texID);
//
//			mainVao.Render (PrimitiveType.TriangleStrip, vaoiFSQ);
//
//			GL.Enable (EnableCap.DepthTest);
//			mainShader.SetMVP(modelview * projection);
		}
		void renderCards(){
			//mainCache.Bind (true);
//			mainShader.SetCardsPass ();
//			mainVao.Render (PrimitiveType.Triangles, vaoiCards);
//			mainShader.SetUIPass ();
//			GL.ActiveTexture (TextureUnit.Texture10);
//			GL.BindTexture (TextureTarget.Texture2D, texTable);
//			mainVao.Render (PrimitiveType.Triangles, vaoiPlate);

		}
		void updateSelectionMap(){
			GL.Disable (EnableCap.Blend);//alpha not used
			GL.BindFramebuffer (FramebufferTarget.Framebuffer, fboSelection);

			getSelectionDatas ();

			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
			mainShader.Enable ();
			mainShader.SetSelectionPass ();
			mainVao.Bind ();
			mainVao.Render (BeginMode.TriangleStrip, vaoiCards.VAOPointer, vaoiCards.Instances);
			mainVao.Unbind ();

			GL.Enable (EnableCap.Blend);

			GL.BindFramebuffer (FramebufferTarget.Framebuffer, 0);
		}
		void updateTarget ()
		{
			if (selDatas == null)
				return;
			if (Mouse.Y > ClientSize.Height || Mouse.X > ClientSize.Width) {
				mainShader.SelectedIndex = -1;
				return;
			}
			int ptrSel = (ClientSize.Height - 1 - Mouse.Y) * ClientSize.Width + Mouse.X;
			SelectedCardInstance = selDatas [ptrSel];
//			if (targetedGroup != null) {				
//				if (Selection == null) {
//					if (targetedGroup.Count > 0) {
//						if (targetedGroup.Picking == PickPolicy.Last)
//							TargetIdx = targetedGroup.Last.Index;
//						else if (targetedGroup.Picking == PickPolicy.First)
//							TargetIdx = targetedGroup [0].Index;
//						else if (targetedCard != null) {
//							if (!targetedCard.IsPickable)
//								TargetIdx = -1;
//						} else
//							TargetIdx = -1;
//					}
//				} else {					
//					Card c = Selection [0];
//					if (!targetedGroup.CardIsValidForAdd (c))
//						TargetIdx = -1;
//				}
//			}
			mainShader.SelectedIndex = SelectedCardInstance;
		}

		#region Selection FBO & PBO
		Tetra.Texture selTex;
		int fboSelection, rbufSelDepth;
		int[] pbos = new int[2];
		byte[] selDatas;
		bool evenCycle = false;

		void initPBOs(){
			if (GL.IsBuffer (pbos [0]))
				GL.DeleteBuffers (2, pbos);
			GL.GenBuffers (2, pbos);
			GL.BindBuffer (BufferTarget.PixelPackBuffer, pbos [0]);
			GL.BufferData (BufferTarget.PixelPackBuffer, ClientSize.Width * ClientSize.Height,
				IntPtr.Zero, BufferUsageHint.StreamRead);
			GL.BindBuffer (BufferTarget.PixelPackBuffer, pbos [1]);
			GL.BufferData (BufferTarget.PixelPackBuffer, ClientSize.Width * ClientSize.Height,
				IntPtr.Zero, BufferUsageHint.StreamRead);
			GL.BindBuffer (BufferTarget.PixelPackBuffer, 0);
			selDatas = new byte[ClientSize.Width * ClientSize.Height];
		}
		void initSelectionFbo()
		{
			if (selTex != null)
				selTex.Dispose ();
			if (GL.IsFramebuffer (fboSelection))
				GL.DeleteFramebuffer (fboSelection);

			GL.ActiveTexture (TextureUnit.Texture3);

			GL.GenFramebuffers(1, out fboSelection);
			GL.BindFramebuffer(FramebufferTarget.Framebuffer, fboSelection);
			GL.DrawBuffers(1, new DrawBuffersEnum[]	{ DrawBuffersEnum.ColorAttachment0 });

			Tetra.Texture.DefaultTarget = TextureTarget.Texture2D;
			Tetra.Texture.GenerateMipMaps = false;
			selTex = new Tetra.Texture()
			{
				Width = ClientSize.Width,
				Height = ClientSize.Height,
				InternalFormat = PixelInternalFormat.R8,
				PixelFormat = PixelFormat.Red,
				PixelType = PixelType.UnsignedByte,
			}; selTex.Create ();

			rbufSelDepth = GL.GenRenderbuffer();
			GL.BindRenderbuffer (RenderbufferTarget.Renderbuffer, rbufSelDepth);
			GL.RenderbufferStorage (RenderbufferTarget.Renderbuffer, RenderbufferStorage.DepthComponent24, ClientSize.Width, ClientSize.Height);
			GL.FramebufferRenderbuffer (FramebufferTarget.Framebuffer, FramebufferAttachment.DepthAttachment,
				RenderbufferTarget.Renderbuffer, rbufSelDepth);
			GL.FramebufferTexture2D (FramebufferTarget.Framebuffer, FramebufferAttachment.ColorAttachment0, TextureTarget.Texture2D, selTex, 0);
			Tetra.Texture.ResetToDefaultLoadingParams ();

			if (GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer) != FramebufferErrorCode.FramebufferComplete)
			{
				throw new Exception(GL.CheckFramebufferStatus(FramebufferTarget.Framebuffer).ToString());
			}

			GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);

			initPBOs ();

			GL.BindTexture (TextureTarget.Texture2D, selTex);
		}
		void getSelectionDatas()
		{
			int pboMapped, pboRead;
			if (evenCycle) {
				pboMapped = pbos [0];
				pboRead = pbos [1];
			}else{
				pboMapped = pbos [1];
				pboRead = pbos [0];
			}
			GL.BindBuffer (BufferTarget.PixelPackBuffer, pboRead);
			GL.ReadPixels (0, 0, ClientSize.Width, ClientSize.Height, PixelFormat.Red, PixelType.UnsignedByte, IntPtr.Zero);

			GL.BindBuffer (BufferTarget.PixelPackBuffer, pboMapped);
			IntPtr ptrSelData = GL.MapBuffer (BufferTarget.PixelPackBuffer, BufferAccess.ReadOnly);
			if (ptrSelData != IntPtr.Zero) {
				Marshal.Copy (ptrSelData, selDatas, 0, selDatas.Length);
				GL.UnmapBuffer (BufferTarget.PixelPackBuffer);
			}

			GL.BindBuffer (BufferTarget.PixelPackBuffer, 0);

			evenCycle = !evenCycle;
		}
		#endregion

		#endregion

		#region Interface
		public static MagicCrow.Magic CurrentGameWin;
		void loadWindow(string path){
			try {
				GraphicObject g = FindByName (path);
				if (g != null)
					return;
				g = Load (path);
				g.Name = path;
				g.DataSource = this;
			} catch (Exception ex) {
				Debug.WriteLine (ex.ToString ());
			}
		}
		void closeWindow (string path){
			GraphicObject g = FindByName (path);
			if (g != null)
				ifaceControl [0].CrowInterface.DeleteWidget (g);
		}

		void initInterface(){
			MouseMove += Mouse_Move;
			MouseButtonDown += Mouse_ButtonDown;
			MouseWheelChanged += Mouse_WheelChanged;
			KeyboardKeyDown += MainWin_KeyboardKeyDown;
		}
		public static void AddLog(string msg){
			Debug.WriteLine("LOG: " + msg);
		}

		void onDeckListValueChange (object sender, SelectionChangeEventArgs e)
		{
			NotifyValueChanged ("CardEntries", (e.NewValue as Deck).CardEntries);	
		}
		void onPreconDeckListValueChange (object sender, SelectionChangeEventArgs e)
		{
			currentDeck = e.NewValue as Deck;
			loadWindow ("#MagicCrow.ui.deckCards.iml");
			NotifyValueChanged ("SelectedDeck", currentDeck);
			NotifyValueChanged ("CardEntries", currentDeck.CardEntries);
			//Load ("#MagicCrow.ui.deckCards.iml").DataSource = d;
			//d.LoadCards ();
		}
		void onCardListValueChange (object sender, SelectionChangeEventArgs e)
		{
			if (e.NewValue != null)
				Load ("#MagicCrow.ui.cardModel.iml").DataSource = (e.NewValue as MainLine).Card;
		}
		void onStartNewGame (object sender, MouseButtonEventArgs e)
		{
			closeWindow ("#MagicCrow.ui.mainMenu.iml");
			startNewGame ();
		}
		void onShowDecks (object sender, MouseButtonEventArgs e)
		{
			closeWindow ("#MagicCrow.ui.mainMenu.iml");
			loadWindow ("#MagicCrow.ui.decks.iml");
		}
		void onSaveInCache (object sender, MouseButtonEventArgs e){			
			currentDeck.CacheAllCards ();
		}
		#endregion

		#region Game Logic
		public static string dataPath = "/mnt/data2/downloads/forge-gui-desktop-1.5.31/res/";
		public static string deckPath = dataPath + "quest/precons/";
		static string cardImgsBasePath = System.IO.Path.Combine (MagicData.cardsArtPath, "cards");

		int cardTextures;

		Deck[] deckList;
		string[] cardList;
		Deck currentDeck;
		MagicEngine engine;
		int selCardInst = -1;//index of selected card in cardsVBO, given by selection texture

		public Player[] Players;
		public Deck[] DeckList {
			get { return deckList; }
			set {
				if (deckList == value)
					return;
				deckList = value;
				NotifyValueChanged ("DeckList", deckList);
			}
		}
		public string[] CardList {
			get { return cardList; }
			set {
				if (cardList == value)
					return;
				cardList = value;
				NotifyValueChanged ("CardList", cardList);
			}			
		}

		public int SelectedCardInstance {
			get { return selCardInst; }
			set {
				if (selCardInst == value)
					return;
				selCardInst = value;
				if (cards != null) {
					if (selCardInst < cards.Length)
						CardInstance.selectedCard = cards [selCardInst];
				}
			}
		}
		public int P1DeckIdx {
			get { return Crow.Configuration.Get<int> ("Player1DeckIdx"); }
			set {
				if (P1DeckIdx == value)
					return;
				Crow.Configuration.Set ("Player1DeckIdx", value);
				NotifyValueChanged ("P1DeckIdx", value);
			}
		}
		public int P2DeckIdx {
			get { return Crow.Configuration.Get<int> ("Player2DeckIdx"); }
			set {
				if (P1DeckIdx == value)
					return;
				Crow.Configuration.Set ("Player2DeckIdx", value);
				NotifyValueChanged ("P1Deck2dx", value);
			}
		}

		#region Loading
		void loadPreconstructedDecks()
		{
			string[] editions = Directory.GetFiles(Magic.deckPath, "*.dck");

			List<Deck> tmpList = new List<Deck> ();
			int i = 0;
			foreach (string f in editions)
			{
				tmpList.Add(Deck.PreLoadDeck (f));
				i++;
			}
			DeckList = tmpList.ToArray ();
		}
		void loadCardList()
		{
			CardList = MagicData.GetCardDataFileNames ();
		}
		#endregion

		void startNewGame(){
			Players [0].Deck = DeckList [P1DeckIdx];
			Players [1].Deck = DeckList [P2DeckIdx];

			Players [0].LoadDeckCards ();
			Players [1].LoadDeckCards ();

			engine = new MagicEngine (Players);
			MagicEngine.MagicEvent += MagicEngine_MagicEvent;

			engine.currentPlayerIndex = engine.interfacePlayer;
		}

		void MagicEngine_MagicEvent (MagicEventArg arg)
		{
			if (arg.Type == MagicEventType.BeginPhase)
				NotifyValueChanged ("CurrentPhase", (arg as PhaseEventArg).Phase);
		}
		CardInstance[] cards;
		public void CreateGLCards(){
			Random rnd = new Random();
			cards = Players[0].Deck.Cards.Concat(Players[1].Deck.Cards).ToArray();
			List<string> magicCardImgs = new List<string>();
			CardInstancedData[] cid = new CardInstancedData[cards.Length];

			magicCardImgs.Add ("#MagicCrow.images.card_back.jpg");

			for (int i = 0; i < cards.Length; i++) {
				CardInstance ci = cards [i];
				string imgPath = cardImgsBasePath;
				string editionPicsPath = System.IO.Path.Combine (cardImgsBasePath, ci.Edition);
				if (Directory.Exists (editionPicsPath))
					imgPath = editionPicsPath;

				if (ci.Model.nbrImg == 1)
					imgPath = System.IO.Path.Combine (imgPath, ci.Name + ".full.jpg");
				else
					imgPath = System.IO.Path.Combine (imgPath, ci.Name + rnd.Next(1,ci.Model.nbrImg) + ".full.jpg");
				int imgIdx = magicCardImgs.IndexOf (imgPath);
				if (imgIdx < 0) {
					imgIdx = magicCardImgs.Count;
					magicCardImgs.Add (imgPath);
				}
				cid [i].modelMats = Matrix4.Identity; //Matrix4.CreateTranslation (new Vector3(0.4f*i,0,0.02f*i));
				cid [i].picked = imgIdx;
				ci.instanceIdx = i;
			}
			//Tetra.Texture.DefaultMinFilter = TextureMinFilter.LinearMipmapLinear;
			Tetra.Texture.DefaultMagFilter = TextureMagFilter.Linear;
			Tetra.Texture.GenerateMipMaps = false;

			cardTextures = Tetra.Texture.Load (TextureTarget.Texture2DArray, magicCardImgs.ToArray());
			Tetra.Texture.ResetToDefaultLoadingParams ();

			vaoiCards.Instances = new InstancesVBO<CardInstancedData> (cid);
			vaoiCards.Instances.UpdateVBO ();
			vaoiCards.Diffuse = cardTextures;

			CardInstance.CardsVBO = vaoiCards.Instances;

			Players[0].CurrentState = MagicCrow.Player.PlayerStates.InitialDraw;
			Players[1].CurrentState = MagicCrow.Player.PlayerStates.InitialDraw;
		}

		#endregion

		#region OTK window overrides
		protected override void OnLoad (EventArgs e)
		{
			CurrentGameWin = this;

			base.OnLoad (e);

			initInterface ();
			initOpenGL ();

			loadPreconstructedDecks ();

			Players = new Player[] 
			{ 
				new Player("player 1"), //"Kor Armory.dck"
				new AiPlayer("player 2")
			};

			Players [1].Library.x = -Players [1].Library.x;
			Players [1].Library.y = -Players [1].Library.y;
			Players [1].Library.zAngle = MathHelper.Pi;
			Players [1].Graveyard.x = -Players [1].Graveyard.x;
			Players [1].Graveyard.y = -Players [1].Graveyard.y;
			Players [1].Graveyard.zAngle = MathHelper.Pi;
			Players [1].Hand.y = -Players [1].Hand.y;
			Players [1].Hand.zAngle = MathHelper.Pi;

			GraphicObject g = Load ("#MagicCrow.ui.player.iml");
			g.HorizontalAlignment = HorizontalAlignment.Left;
			g.DataSource = Players[0];

			g = Load ("#MagicCrow.ui.player.iml");
			g.HorizontalAlignment = HorizontalAlignment.Right;
			g.DataSource = Players[1];

			loadWindow ("#MagicCrow.ui.mainMenu.iml");
			loadWindow ("#MagicCrow.ui.engine.iml");
		}
		public override void GLClear ()
		{
			GL.ClearColor (clearColor [0], clearColor [1], clearColor [2], clearColor [3]);
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
		}
		public override void OnRender (FrameEventArgs e)
		{
			draw ();
		}
		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
			initSelectionFbo ();
			UpdateViewMatrix();
		}

		protected override void OnUpdateFrame (FrameEventArgs e)
		{
			base.OnUpdateFrame (e);

			GGL.Animation.ProcessAnimations ();

			if (CardInstance.CardsVBO != null)
				CardInstance.CardsVBO.UpdateVBOSubData ();

			updateSelectionMap ();

			updateTarget ();

			if (engine != null)
				engine.Process ();
		}
		#endregion

		#region vLookCalculations
		public void UpdateViewMatrix()
		{
			Rectangle r = this.ClientRectangle;
			GL.Viewport( r.X, r.Y, r.Width, r.Height);
			projection = Matrix4.CreatePerspectiveFieldOfView (fovY, r.Width / (float)r.Height, zNear, zFar);
			vLook = vLookInit.Transform(
				Matrix4.CreateRotationX (viewXangle)*
				Matrix4.CreateRotationZ (viewZangle));
			vLook.Normalize();
			vEye = vEyeTarget + vLook * eyeDist;
			modelview = Matrix4.LookAt(vEye, vEyeTarget, Vector3.UnitZ);
			GL.GetInteger(GetPName.Viewport, viewport);
			invMVP = Matrix4.Invert(modelview) * Matrix4.Invert(projection);
			reflectedModelview =
				Matrix4.CreateScale (1.0f, 1.0f, -1.0f) * modelview;			
		}
		#endregion

		#region Keyboard
		void MainWin_KeyboardKeyDown (object sender, OpenTK.Input.KeyboardKeyEventArgs e)
		{
			switch (e.Key) {
			case OpenTK.Input.Key.Space:
//				for (int i = 0; i < currentDeck.Cards.Count; i++) {
//					CardInstance ci = currentDeck.Cards [i];
//					GGL.Animation.StartAnimation (new GGL.FloatAnimation (ci, "x", 0.4f * i, 0.5f));
//					GGL.Animation.StartAnimation(new GGL.FloatAnimation(ci, "z", 0.05f * i, 0.4f));
//				}
				Players[0].DrawOneCard();
				break;
			case OpenTK.Input.Key.Q:
				for (int i = 0; i < currentDeck.Cards.Count; i++) {
					CardInstance ci = currentDeck.Cards [i];
					GGL.Animation.StartAnimation (new GGL.FloatAnimation (ci, "x", 0f, 0.5f));
				}
				//
				break;
			case OpenTK.Input.Key.I:
				CardInstance c = currentDeck.Cards [SelectedCardInstance];
				GGL.Animation.StartAnimation (new GGL.PathAnimation(c, "Position",new GGL.Path(c.Position,vFocusedPoint)));
				GGL.Animation.StartAnimation (new GGL.AngleAnimation(c, "xAngle",FocusAngle));
				//
				break;
			case OpenTK.Input.Key.R:
				Players [0].Library.RevealToUIPlayer ();
				break;
//			case OpenTK.Input.Key.Space:
//				for (int i = 5; i < 6; i++) {
//					CardInstance ci = currentDeck.Cards [i];
//					GGL.Animation.StartAnimation (new GGL.FloatAnimation (ci, "x", 0.4f * i, 0.2f));
//					GGL.Animation.StartAnimation(new GGL.FloatAnimation(ci, "z", 0.05f * i, 0.2f));
//				}
//				//
//				break;
//			case OpenTK.Input.Key.Q:
//				for (int i = 5; i < 6; i++) {
//					CardInstance ci = currentDeck.Cards [i];
//					GGL.Animation.StartAnimation (new GGL.FloatAnimation (ci, "x", 0f, 0.2f));
//				}
//				//
//				break;

			}
		}

		#endregion

		#region Mouse
		void Mouse_ButtonDown (object sender, OpenTK.Input.MouseButtonEventArgs e)
		{
			if (e.Mouse.LeftButton != OpenTK.Input.ButtonState.Pressed)
				return;
			if (engine != null)
				engine.processMouseDown (e);
		}
		void Mouse_Move(object sender, OpenTK.Input.MouseMoveEventArgs e)
		{
			if (e.XDelta != 0 || e.YDelta != 0)
			{
				if (e.Mouse.MiddleButton == OpenTK.Input.ButtonState.Pressed) {
					viewZangle -= (float)e.XDelta * RotationSpeed;
					viewXangle -= (float)e.YDelta * RotationSpeed;
					if (viewXangle < - 0.75f)
						viewXangle = -0.75f;
					else if (viewXangle > MathHelper.PiOver4)
						viewXangle = MathHelper.PiOver4;
					UpdateViewMatrix ();
				}else if (e.Mouse.LeftButton == OpenTK.Input.ButtonState.Pressed) {
					return;
				}else if (e.Mouse.RightButton == OpenTK.Input.ButtonState.Pressed) {
					Vector2 v2Look = vLook.Xy.Normalized ();
					Vector2 disp = v2Look.PerpendicularLeft * e.XDelta * MoveSpeed +
					               v2Look * e.YDelta * MoveSpeed;
					vEyeTarget -= new Vector3 (disp.X, disp.Y, 0);
					UpdateViewMatrix();
				}
				Vector3 vMouse = GGL.glHelper.UnProject(ref projection, ref modelview, viewport, new Vector2 (e.X, e.Y)).Xyz;
				Vector3 vMouseRay = Vector3.Normalize(vMouse - vEye);
				float a = vEye.Z / vMouseRay.Z;
				vMouse = vEye - vMouseRay * a;
				Point newPos = new Point ((int)Math.Truncate (vMouse.X), (int)Math.Truncate (vMouse.Y));
			}

		}
		void Mouse_WheelChanged(object sender, OpenTK.Input.MouseWheelEventArgs e)
		{
			float speed = ZoomSpeed;
			if (Keyboard[OpenTK.Input.Key.ShiftLeft])
				speed *= 0.1f;
			else if (Keyboard[OpenTK.Input.Key.ControlLeft])
				speed *= 20.0f;

			eyeDistTarget -= e.Delta * speed;
			if (eyeDistTarget < zNear+1)
				eyeDistTarget = zNear+1;
			else if (eyeDistTarget > zFar-6)
				eyeDistTarget = zFar-6;

			GGL.Animation.StartAnimation(new GGL.Animation<float> (this, "EyeDist", eyeDistTarget, (eyeDistTarget - eyeDist) * 0.1f));
		}
		#endregion

		#region CTOR and Main
		public Magic ()
			: base(1024, 800, "MagicCrow", 32, 24, 1, Crow.Configuration.Get<int> ("Samples"))
		{}

		[STAThread]
		static void Main ()
		{
			using (Magic win = new Magic( )) {
				win.VSync = VSyncMode.Off;
				win.Run (30.0);
			}
		}
		#endregion
	}
}
