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
		public static MagicCrow.Magic CurrentGameWin;

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
		public static Vector3 vEyeTarget = new Vector3(0f, -2.5f, 0f);
		public static Vector3 vEye;
		public static Vector3 vLookInit = Vector3.Normalize(new Vector3(0f, -0.65f, 0.75f));
		public static Vector3 vLook = vLookInit;  // Camera vLook Vector
		public float zFar = 60.0f;
		public float zNear = 0.1f;
		public float fovY = (float)Math.PI / 4;

		static float eyeDist = 12f;
		static float eyeDistTarget = 12f;
		float MoveSpeed = 0.02f;
		float RotationSpeed = 0.005f;
		float ZoomSpeed = 1f;
		float viewZangle, viewXangle;

		public Vector4 vLight = new Vector4 (0.5f, 0.5f, -1f, 0f);
		float[] clearColor = new float[] {0.5f,0.5f,0.9f,1.0f};

		public static Vector3 vFocusedPoint {
			get { return vEyeTarget + vLook * (eyeDist - 3f); }
		}
		public static Vector3 vGroupedFocusedPoint {
			get { return vEyeTarget + vLook * (eyeDist - 10f); }
		}
		public static float FocusAngle {
			get { return Vector3.CalculateAngle (vLook, Vector3.UnitZ); }
		}
		#endregion

		#region GL
		public static int numSamples = 1;
		MeshesGroup<MeshData> meshes;

		//static RenderCache mainCache;
		static MultiShader mainShader;
		static GameLib.EffectShader wirlpoolShader;
		//public static Mat4InstancedShader piecesShader;

		static InstancedVAO<MeshData, CardInstancedData> mainVao;
		static MeshPointer MPCards;
		static MeshPointer MPOverlay;
		static MeshPointer MPPointOverlay;
		static MeshPointer MPInfoOverlay;
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

			MPCards = meshes.Add (Mesh<MeshData>.CreateQuad (0, 0, 0, 1, 1.425f, 1, 1));
			MPOverlay = meshes.Add (Mesh<MeshData>.CreateQuad (0, 0, 0, 1, 1, 1, 1));
			MPPointOverlay = meshes.Add (Mesh<MeshData>.CreateQuad (0, 0, 0, 0.5f, 0.2f, 1, -1));
			MPInfoOverlay = meshes.Add (Mesh<MeshData>.CreateQuad (0, 0, 0, 1.2f, 0.2f, 1, -1));

			mainVao = new InstancedVAO<MeshData, CardInstancedData> (meshes);

			CardInstance.OverlayVBO = new InstancesVBO<CardInstancedData>();
			CardInstance.PointOverlayVBO = new InstancesVBO<CardInstancedData>();
			CardInstance.InfoOverlayVBO = new InstancesVBO<CardInstancedData>();

			mainShader = new MultiShader (
				"#MagicCrow.shaders.main.vert",
				"#MagicCrow.shaders.main.frag");
			
			//wirlpoolShader = new GameLib.EffectShader ("GGL.Shaders.GameLib.fire",64,64);
			wirlpoolShader = new GameLib.EffectShader ("GGL.Shaders.GameLib.lightBalls",64,64);
		}
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
				//cid [i].modelMats = Matrix4.Identity; //Matrix4.CreateTranslation (new Vector3(0.4f*i,0,0.02f*i));
				cid [i].picked = imgIdx;
				ci.cardVboIdx = i;
			}
			//Tetra.Texture.DefaultMinFilter = TextureMinFilter.LinearMipmapLinear;
			Tetra.Texture.DefaultMagFilter = TextureMagFilter.Linear;
			Tetra.Texture.GenerateMipMaps = false;

			cardTextures = Tetra.Texture.Load (TextureTarget.Texture2DArray, magicCardImgs.ToArray());
			Tetra.Texture.ResetToDefaultLoadingParams ();

			CardInstance.CardsVBO = new InstancesVBO<CardInstancedData> (cid);
			CardInstance.CardsVBO.UpdateVBO ();

			Players[0].CurrentState = MagicCrow.Player.PlayerStates.InitialDraw;
			Players[1].CurrentState = MagicCrow.Player.PlayerStates.InitialDraw;
		}

		void draw(){
			mainShader.Enable ();
			GL.ActiveTexture (TextureUnit.Texture0);
			GL.BindTexture(TextureTarget.Texture2DArray, cardTextures);

			mainShader.SetMVP(modelview * projection);

			mainVao.Bind ();
			mainShader.SetCardsPass ();
			mainVao.Render (BeginMode.TriangleStrip, MPCards, CardInstance.CardsVBO);
			if (CardInstance.PointOverlayVBO?.InstancedDatas?.Length > 0) {				
				GL.BindTexture(TextureTarget.Texture2DArray, CardInstance.PointOverlayTexture);
				mainVao.Render (BeginMode.TriangleStrip, MPPointOverlay, CardInstance.PointOverlayVBO);
			}
			if (CardInstance.InfoOverlayVBO?.InstancedDatas?.Length > 0) {				
				GL.BindTexture(TextureTarget.Texture2DArray, CardInstance.InfoOverlayTexture);
				mainVao.Render (BeginMode.TriangleStrip, MPInfoOverlay, CardInstance.InfoOverlayVBO);
			}
			if (CardInstance.OverlayVBO?.InstancedDatas?.Length > 0) {
				GL.ActiveTexture (TextureUnit.Texture10);
				GL.BindTexture(TextureTarget.Texture2D, wirlpoolShader.Texture);
				mainShader.SetUIPass ();
				mainVao.Render (BeginMode.TriangleStrip, MPOverlay, CardInstance.OverlayVBO);
			}
			mainVao.Unbind ();
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
			mainVao.Render (BeginMode.TriangleStrip, MPCards, CardInstance.CardsVBO);
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

			SelectedCardInstanceIdx = selDatas [ptrSel] - 1;
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
			mainShader.SelectedIndex = SelectedCardInstanceIdx;
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
		GraphicObject mstack = null, phasePannel;
		GraphicObject[] playerPannels;

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
			MouseMove += Game_Mouse_Move;
			MouseButtonDown += Game_Mouse_ButtonDown;
			MouseWheelChanged += Game_Mouse_WheelChanged;
			KeyboardKeyDown += MainWin_KeyboardKeyDown;
		}

		#region LOGS
		List<string> logBuffer = new List<string> ();
		public List<string> LogBuffer {
			get { return logBuffer; }
			set { logBuffer = value; }
		}
		public static void AddLog(string msg){
			CurrentGameWin.AddLog2(msg);
		}
		public void AddLog2(string msg)
		{
			if (string.IsNullOrEmpty (msg))
				return;
			foreach (string s in msg.Split('\n')) {
				if (string.IsNullOrEmpty (s))
					continue;
				LogBuffer.Add (msg);
			}
			NotifyValueChanged ("LogBuffer", logBuffer);
		}
		#endregion

		void onDeckListValueChange (object sender, SelectionChangeEventArgs e)
		{
			NotifyValueChanged ("CardEntries", (e.NewValue as DeckFile).CardEntries);	
		}
		void onPreconDeckListValueChange (object sender, SelectionChangeEventArgs e)
		{
			currentDeck = e.NewValue as DeckFile;
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
		void onCancelLastClick (object sender, MouseButtonEventArgs e){
			engine.MagicStack.PopMSE ();
		}
		void onPhaseClick (object sender, MouseButtonEventArgs e){			
			Border g = sender as Border;

			int phaseIdx = (int)Enum.Parse (typeof(GamePhases), g.Name);
			engine.ip.PhaseStops [phaseIdx] = !engine.ip.PhaseStops [phaseIdx];

			if (engine.ip.PhaseStops[phaseIdx])
				NotifyValueChanged (g.Name + "Opacity", 1.0);
			else
				NotifyValueChanged (g.Name + "Opacity", 0.35);
		}

		#endregion

		#region Game Logic
		public static string dataPath = "/mnt/data2/downloads/forge-gui-desktop-1.5.31/res/";
		public static string deckPath = dataPath + "quest/precons/";
		static string cardImgsBasePath = System.IO.Path.Combine (MagicData.cardsArtPath, "cards");

		MagicEngine engine;
		DeckFile[] deckList;
		public CardInstance[] cards;//cards in play array
		int cardTextures;
		int selCardInst = -1;//index of selected card in cardsVBO, given by selection texture

		public Player[] Players;

		DeckFile currentDeck;
		string[] cardList;
		public DeckFile[] DeckList {
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


		public List<MagicStackElement> MagicStack
		{ 
			get { 
				return engine == null ? new List<MagicStackElement> () :
					engine.MagicStack.ToList (); 
			}
		}
		/// <summary>
		/// index in cards[] of currently selected card.
		/// </summary>
		public int SelectedCardInstanceIdx {
			get { return selCardInst; }
			set {
				if (selCardInst == value)
					return;
				selCardInst = value;
				if (selCardInst>=0 && selCardInst < cards?.Length)
					CardInstance.selectedCard = cards [selCardInst];
				else
					CardInstance.selectedCard = null;
			}
		}
		public MagicCard SelectedCardModel {
			get {return CardInstance.selectedCard == null ? null : CardInstance.selectedCard.Model; }
		}
		public CardInstance SelectedCardInstance {
			get {return CardInstance.selectedCard == null ? null : CardInstance.selectedCard; }
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

			List<DeckFile> tmpList = new List<DeckFile> ();
			int i = 0;
			foreach (string f in editions)
			{
				tmpList.Add(new DeckFile(f));
				i++;
			}
			DeckList = tmpList.ToArray ();
		}
		void loadCardList()
		{
			CardList = MagicData.GetCardDataFileNames ();
		}
		#endregion

		void initGame () {
			loadPreconstructedDecks ();

			MagicData.Init ();

			Players = new Player[] 
			{ 
				new Player("player 1"),
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
			Players [1].InPlay.LandsLayout.x = -Players [1].InPlay.LandsLayout.x;
			Players [1].InPlay.LandsLayout.y = -Players [1].InPlay.LandsLayout.y;
			Players [1].InPlay.LandsLayout.zAngle = MathHelper.Pi;
			Players [1].InPlay.CreatureLayout.x = -Players [1].InPlay.CreatureLayout.x;
			Players [1].InPlay.CreatureLayout.y = -Players [1].InPlay.CreatureLayout.y;
			Players [1].InPlay.CreatureLayout.zAngle = MathHelper.Pi;
			Players [1].InPlay.CombatingCreature.x = -Players [1].InPlay.CombatingCreature.x;
			Players [1].InPlay.CombatingCreature.y = -Players [1].InPlay.CombatingCreature.y;
			Players [1].InPlay.CombatingCreature.zAngle = MathHelper.Pi;
			Players [1].InPlay.OtherLayout.x = -Players [1].InPlay.OtherLayout.x;
			Players [1].InPlay.OtherLayout.y = -Players [1].InPlay.OtherLayout.y;
			Players [1].InPlay.OtherLayout.zAngle = MathHelper.Pi;			
		}
		void updatePhaseStopsControls(){
			Group vsPhases = phasePannel.FindByName ("Phases") as Group;
			for (int i = 0; i < 12; i++) {
				if (engine.ip.PhaseStops [i])
					((vsPhases.Children [i] as Container).Child as Image).Opacity = 1.0;
			}
		}
		void startNewGame(){
			closeWindow ("#MagicCrow.ui.mainMenu.iml");
			loadWindow ("#MagicCrow.ui.log.iml");

			phasePannel = Load ("#MagicCrow.ui.phases.iml");
			AddWidget(phasePannel);
			phasePannel.DataSource = this;

			playerPannels = new GraphicObject[2];
			for (int i = 0; i < 2; i++) {
				playerPannels[i] = Load ("#MagicCrow.ui.player.iml");
				playerPannels[i].HorizontalAlignment = HorizontalAlignment.Left;
				playerPannels[i].DataSource = Players[i];
			}
			playerPannels[1].HorizontalAlignment = HorizontalAlignment.Right;

			new Deck(DeckList [P1DeckIdx],Players [0]);
			new Deck(DeckList [P2DeckIdx],Players [1]);

			Players [0].LoadDeckCards ();
			Players [1].LoadDeckCards ();

			engine = new MagicEngine (Players);
			MagicEngine.MagicEvent += MagicEngine_MagicEvent;

			engine.currentPlayerIndex = engine.interfacePlayer;

			mstack = Load ("#MagicCrow.ui.MagicStack.iml");				
			AddWidget(mstack);
			mstack.DataSource = engine.MagicStack;

			updatePhaseStopsControls ();
		}
		void closeCurrentGame(){			
			closeWindow ("#MagicCrow.ui.log.iml");
			DeleteWidget (phasePannel);

			for (int i = 0; i < 2; i++) {
				Players[i].Deck.Cards = new List<CardInstance>();
				Players[i].DeckLoaded = false;
				Players[i].CurrentState = MagicCrow.Player.PlayerStates.Init;
				DeleteWidget (playerPannels [i]);
			}

			engine = null;

			DeleteWidget (mstack);

			CardInstance.CardsVBO?.Dispose();

			cards = null;
			if (GL.IsTexture (cardTextures))
				GL.DeleteTexture (cardTextures);
			cardTextures = 0;

			loadWindow ("#MagicCrow.ui.mainMenu.iml");
		}

		void MagicEngine_MagicEvent (MagicEventArg arg)
		{
			Border b;

			AddLog (arg.ToString ());

			switch (arg.Type)
			{
			case MagicEventType.PlayerHasLost:
				closeCurrentGame ();
				break;
			case MagicEventType.Unset:
				break;
			case MagicEventType.BeginPhase:
				b = phasePannel.FindByName 
					((arg as PhaseEventArg).Phase.ToString ()) as Border;
				if (b!=null)
					b.Foreground = Color.White;				
				break;
			case MagicEventType.EndPhase:
				b = phasePannel.FindByName 
					((arg as PhaseEventArg).Phase.ToString ()) as Border;
				if (b!=null)
					b.Foreground = Color.Transparent;
				break;
			case MagicEventType.PlayLand:
				break;
			case MagicEventType.CastSpell:
				break;
			case MagicEventType.TapCard:
				break;
			case MagicEventType.ChangeZone:
				break;
			default:
				break;
			}
		}


		#endregion

		#region OTK window overrides
		protected override void OnLoad (EventArgs e)
		{
			CurrentGameWin = this;

			base.OnLoad (e);

			initInterface ();
			initOpenGL ();

			initGame ();



			loadWindow ("#MagicCrow.ui.mainMenu.iml");
			//loadWindow ("#MagicCrow.ui.engine.iml");
		}
		public override void GLClear ()
		{
			GL.ClearColor (clearColor [0], clearColor [1], clearColor [2], clearColor [3]);
			GL.Clear (ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit | ClearBufferMask.StencilBufferBit);
		}
		public override void OnRender (FrameEventArgs e)
		{
			if (CardInstance.CardsVBO != null)
				draw ();
		}
		protected override void OnResize (EventArgs e)
		{
			base.OnResize (e);
			initSelectionFbo ();
			UpdateViewMatrix();
		}
		int frame;
		float time;
		protected override void OnUpdateFrame (FrameEventArgs e)
		{
			base.OnUpdateFrame (e);

			unchecked{
				time += (float)e.Time;
				frame++;
			}

			GGL.Animation.ProcessAnimations ();

			if (engine == null)
				return;
			if (frame % 3 == 0)
				engine.Process ();

			wirlpoolShader.Update (time);

			Rectangle r = this.ClientRectangle;
			GL.Viewport( r.X, r.Y, r.Width, r.Height);

			if (CardInstance.CardsVBO != null) {
				CardInstance.CardsVBO.UpdateVBOSubData ();
				CardInstance.OverlayVBO.UpdateVBOSubData ();
				CardInstance.PointOverlayVBO.UpdateVBOSubData ();
				CardInstance.InfoOverlayVBO.UpdateVBOSubData ();
				updateSelectionMap ();
				updateTarget ();
			}
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
			case OpenTK.Input.Key.KeypadEnter:
				engine.ip.PhaseDone = true;
//				if (engine.pp == engine.ip && engine.cp != engine.pp)
//					engine.GivePriorityToNextPlayer ();
				break;
			case OpenTK.Input.Key.Escape:
				closeCurrentGame ();
				break;
			#if DEBUG
			case OpenTK.Input.Key.F1:
				if (CardInstance.selectedCard == null)
					break;				
				loadWindow ("#MagicCrow.ui.cardInstanceView.iml");
				NotifyValueChanged ("SelectedCardInstance", null);
				NotifyValueChanged ("SelectedCardInstance", SelectedCardInstance);
				break;
			case OpenTK.Input.Key.F2:
				if (CardInstance.selectedCard == null)
					break;				
				loadWindow ("#MagicCrow.ui.cardView.iml");
				NotifyValueChanged ("SelectedCardModel", SelectedCardModel);
				break;				
			case OpenTK.Input.Key.F3:
				loadWindow ("#MagicCrow.ui.MagicStackView.iml");				
				break;
			case OpenTK.Input.Key.F4:
				loadWindow ("#MagicCrow.ui.decks.iml");				
				break;
			case OpenTK.Input.Key.F6:
				loadWindow ("#MagicCrow.ui.MemberView.crow");
				IList<string>t;
				break;
			case OpenTK.Input.Key.Space:
				Players [0].DrawOneCard ();
				break;
			case OpenTK.Input.Key.H:
				if (e.Control){
					
				}else
					CardInstance.selectedCard?.ChangeZone(CardGroupEnum.Hand);
				break;
			case OpenTK.Input.Key.E:
				if (e.Control){
					foreach (CardInstance ci in Players[0].Deck.Cards.Where
						(c=>c.CurrentGroup.GroupName == CardGroupEnum.Library && c.Effects?.Count > 0)){
						ci.ChangeZone(CardGroupEnum.Hand);
					}
				}	
				break;
			case OpenTK.Input.Key.D:
				if (e.Control){
					foreach (CardInstance ci in Players[0].Deck.Cards.Where(c=>c.CurrentGroup.GroupName == CardGroupEnum.Hand)){
						ci.ChangeZone(CardGroupEnum.Library);
					}
				}	
				break;
			case OpenTK.Input.Key.L:
				if (e.Control){
					foreach (CardInstance ci in Players[0].Deck.Cards.Where(c=>c.HasType(CardTypes.Land)&&c.CurrentGroup.GroupName != CardGroupEnum.InPlay).Take(5)){
						ci.ChangeZone(CardGroupEnum.InPlay);
					}
				}else
					Players [0].Library.RevealToUIPlayer ();
				break;
			case OpenTK.Input.Key.O:
				engine.ip.Opponent.Hand.RevealToUIPlayer();
				break;			
			case OpenTK.Input.Key.U:
				if (e.Control){
					foreach (CardInstance ci in Players[0].Deck.Cards.Where(c=>c.HasType(CardTypes.Land)&&c.CurrentGroup.GroupName == CardGroupEnum.InPlay)){
						ci.tappedWithoutEvent = false;
					}
				}
				if (CardInstance.selectedCard == null)
					return;
				CardInstance.selectedCard.tappedWithoutEvent = false;
				break;			
			case OpenTK.Input.Key.Delete:
				if (CardInstance.selectedCard == null)
					return;
				CardInstance.selectedCard.Reset ();
				CardInstance.selectedCard.ChangeZone(CardGroupEnum.Hand);			
				break;
			case OpenTK.Input.Key.KeypadPlus:
				engine.ip.AllowedLandsToBePlayed++;
				break;
			case OpenTK.Input.Key.N:
				CardInstance nextInvalid = Players [0].Library.Cards.Where(c=>!c.Model.IsOk).FirstOrDefault();
				Players [0].Library.RemoveCard(nextInvalid);
				Players [0].Hand.AddCard(nextInvalid);
				break;
			#endif
			}
		}

		#endregion

		#region Mouse
		void Game_Mouse_ButtonDown (object sender, OpenTK.Input.MouseButtonEventArgs e)
		{			
			if (engine != null)
				engine.processMouseDown (e);
		}
		void Game_Mouse_Move(object sender, OpenTK.Input.MouseMoveEventArgs e)
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
//				Vector3 vMouse = GGL.glHelper.UnProject(ref projection, ref modelview, viewport, new Vector2 (e.X, e.Y)).Xyz;
//				Vector3 vMouseRay = Vector3.Normalize(vMouse - vEye);
//				float a = vEye.Z / vMouseRay.Z;
//				vMouse = vEye - vMouseRay * a;
//				Point newPos = new Point ((int)Math.Truncate (vMouse.X), (int)Math.Truncate (vMouse.Y));
			}

		}
		void Game_Mouse_WheelChanged(object sender, OpenTK.Input.MouseWheelEventArgs e)
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
