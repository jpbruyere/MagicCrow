using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using Cairo;
using GGL;
//using Crow;
using OpenTK;
using OpenTK.Graphics.OpenGL;

//using GLU = OpenTK.Graphics.Glu;
using Tetra.DynamicShading;

namespace MagicCrow
{
    [Serializable]
	public class CardInstance : RenderedCardModel, IDamagable
    {
		#region CardAnimEvent
		public class CardAnimEventArg : EventArgs
		{
			public CardInstance card;
			public CardAnimEventArg(CardInstance _card){
				card = _card;
			}
		}
		#endregion

		public string Edition;
		public MagicAction BindedAction = null;

		#region CTOR
		public CardInstance(MagicCard mc = null)
		{
			Model = mc;
		}
		public CardInstance(MagicAction _bindedAction = null)
		{
			BindedAction = _bindedAction;
			Model = _bindedAction.CardSource.Model;
		}
		#endregion

        #region selection
		/// <summary>
		/// The focused card zoomed in the middle of screen
		/// </summary>
		public static CardInstance focusedCard = null;
        public static CardInstance selectedCard;

		public static Vector4 notSelectedColor = new Vector4(1.0f, 1.0f, 1.0f, 1f);
		public static Vector4 SelectedColor = new Vector4(1.2f, 1.2f, 1.2f, 1.2f);
		public static Vector4 AttackingColor = new Vector4(1.0f, 0.8f, 0.8f, 1f);
        #endregion

		const float attachedCardsSpacing = 0.03f;

		int _power = int.MinValue;
		int _toughness = int.MinValue;
		bool _isTapped = false;
		bool _combating;
		Player _controler;

		Player _originalControler;
		        
        public CardGroup CurrentGroup;
		public bool HasFocus = false;
		public bool hasSummoningSickness = false;
		public bool IsToken = false;
		public bool Kicked = false;

		public String Name {
			get { return Model.Name; }
		}
		public Player Controler
		{
			get
			{
				return _controler ?? _originalControler;
			}
			set { _originalControler = value; }
		}
		public int 	Power
		{
			get
			{
				return _power == int.MinValue ? Model.Power : _power;
			}
		}
		public int Toughness
		{
			get
			{
				return _toughness == int.MinValue ? Model.Toughness : _toughness;
			}
		}

		public bool HasSummoningSickness {
			get { return hasSummoningSickness; }
			set {
				if (hasSummoningSickness == value)
					return;
				hasSummoningSickness = value;

				if (hasSummoningSickness)
					overlayVboIdx = OverlayVBO.AddInstance ();				
				else {
					OverlayVBO.RemoveInstance (overlayVboIdx);
					foreach (CardInstance ci in Magic.CurrentGameWin.cards.Where(c=>c.overlayVboIdx>0)) {
						if (ci == this)
							continue;
						if (ci.overlayVboIdx > overlayVboIdx) {
							ci.overlayVboIdx--;
							ci.updateOverlayDatas ();
						}
					}
					overlayVboIdx = -1;
				}
				updateInstacedDatas ();
				OverlayVBO.UpdateVBO ();

			}
		}


		public IList<EffectGroup> PumpEffect = new List<EffectGroup> ();
		public IList<EffectGroup> Effects{
			get { return Model.SpellEffects.Concat(PumpEffect).ToList(); }
		}
//		public IList<Effect> ActiveEffects{
//			get { }
//		}
		public List<CardInstance> BlockingCreatures = new List<CardInstance>();
		public List<Damage> Damages = new List<Damage>();
		public CardInstance BlockedCreature = null;

		#region Attachment
        public List<CardInstance> AttachedCards = new List<CardInstance>();
		public void AttachCard(CardInstance c)
		{			
			c.AttachedTo = this;
			AttachedCards.Add (c);

			updateArrows ();

			Controler.InPlay.UpdateLayout ();
		}
		public void DetacheCard(CardInstance c)
		{
			c.AttachedTo = null;
			AttachedCards.Remove (c);

			updateArrows ();

			Controler.InPlay.UpdateLayout ();

			if (!c.HasType (CardTypes.Equipment))
				c.PutIntoGraveyard ();			
		}
		public CardInstance AttachedTo = null;
		public bool IsAttached {
			get { return AttachedTo == null ? false : true; }
		}
		public bool IsAttachedToACardInTheSameCamp {
			get { 
				if (!IsAttached)
					return false;
				return (AttachedTo.Controler == this.Controler);
			}
		}
		#endregion
			
        public void AddDamages(Damage d)
        {
            Damages.Add(d);
            
            MagicEngine.CurrentEngine.RaiseMagicEvent(new DamageEventArg(d));
            
            if (Toughness < 1)
                PutIntoGraveyard();
            else
                UpdatePointsOverlaySurface();
        }
		public void ChangeZone(CardGroupEnum _newZone){			
			CardGroupEnum _oldZone = CurrentGroup.GroupName;

			CurrentGroup.RemoveCard (this);
			Controler.GetGroup(_newZone).AddCard (this);

			MagicEngine.CurrentEngine.RaiseMagicEvent (
				new ChangeZoneEventArg (this, _oldZone, _newZone));

			if (_oldZone == _newZone)
				return;
				
			if (HasType (CardTypes.Creature)) {
				if (_newZone == CardGroupEnum.InPlay) {
					pointOverlayVboIdx = PointOverlayVBO.AddInstance ();
					PointOverlayVBO.UpdateVBO ();
					byte[] newPObmp = new byte[pointOverlayWidth * pointOverlayHeight * 4 * 
						PointOverlayVBO.InstancedDatas.Length];
					if (PointOverlayBmp != null)
						Array.Copy (PointOverlayBmp, newPObmp, PointOverlayBmp.Length);

					PointOverlayBmp = newPObmp;

					if (!GL.IsTexture (PointOverlayTexture)) {
						PointOverlayTexture = GL.GenTexture ();
						GL.BindTexture(TextureTarget.Texture2DArray, PointOverlayTexture);
						GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
						GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
						GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
					}else
						GL.BindTexture(TextureTarget.Texture2DArray, PointOverlayTexture);
					
					GL.TexImage3D (TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgba,
						pointOverlayWidth, pointOverlayHeight, PointOverlayVBO.InstancedDatas.Length, 0,
						OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, PointOverlayBmp);
					GL.BindTexture(TextureTarget.Texture2DArray, 0);

					UpdatePointsOverlaySurface ();
				}else if (_oldZone == CardGroupEnum.InPlay) {
					PointOverlayVBO.RemoveInstance (pointOverlayVboIdx);
					if (PointOverlayVBO.InstancedDatas.Length > 0) {
						foreach (CardInstance ci in Magic.CurrentGameWin.Players.SelectMany(p => p.InPlay.Cards.Where(c => c.pointOverlayVboIdx>0))) {
							if (ci == this)
								continue;
							if (ci.pointOverlayVboIdx > pointOverlayVboIdx) {
								ci.pointOverlayVboIdx--;
								ci.updatePointOverlayDatas ();
							}
						}
						int oSize = pointOverlayWidth * pointOverlayHeight * 4;
						byte[] newPObmp = new byte[oSize * PointOverlayVBO.InstancedDatas.Length];
						Array.Copy (PointOverlayBmp, 0, newPObmp, 0, oSize * pointOverlayVboIdx);
						Array.Copy (PointOverlayBmp, oSize * (pointOverlayVboIdx + 1), newPObmp, oSize * pointOverlayVboIdx,
							oSize * (PointOverlayVBO.InstancedDatas.Length - pointOverlayVboIdx - 1));
						
						PointOverlayBmp = newPObmp;
						GL.BindTexture(TextureTarget.Texture2DArray, PointOverlayTexture);
						GL.TexImage3D (TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgba,
							pointOverlayWidth, pointOverlayHeight, PointOverlayVBO.InstancedDatas.Length, 0,
							OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, PointOverlayBmp);
						GL.BindTexture(TextureTarget.Texture2DArray, 0);
					}
					pointOverlayVboIdx = -1;
					PointOverlayVBO.UpdateVBO ();
					UpdatePointsOverlaySurface ();
				}

			}
		}
        public void PutIntoGraveyard()
        {
			while (AttachedCards.Count > 0)
				this.DetacheCard (AttachedCards.First());			

            Reset();
			ChangeZone (CardGroupEnum.Graveyard);			
        }

		public void Reset(bool _positionReset = false)
        {
			if (_positionReset)
            	ResetPositionAndRotation();
            ResetOverlay();

			Damages.Clear();
            Combating = false;
			Kicked = false;
            if (BlockedCreature != null)
            {
                BlockedCreature.BlockingCreatures.Remove(this);
                BlockedCreature = null;
            }
            tappedWithoutEvent = false;
			HasSummoningSickness = false;

			if (IsAttached)				
				AttachedTo.DetacheCard (this);
        }

		public IEnumerable<Ability> getAllAbilities()
		{
			List<Ability> abs = new List<Ability>();

			foreach (CardInstance ci in MagicEngine.CurrentEngine.CardsInPlayHavingEffects) {
				bool valid = false;
				foreach (EffectGroup eg in ci.Effects) {
					foreach (CardTarget ct in eg.Affected.OfType<CardTarget>()) {
						if (!ct.Accept (this, ci)) {
							valid = false;
							break;
						} else
							valid = true;
					}
					if (!valid)
						continue;

					foreach (AbilityEffect e in  eg.OfType<AbilityEffect>()) {
						switch (e.TypeOfEffect) {
						case EffectType.Gain:
							abs.Add (e.Ability);
							break;
						case EffectType.Loose:
							abs.RemoveAll (a => a.AbilityType == e.Ability.AbilityType);
							break;
						}
					}
				}
			}
			return Model.Abilities.Concat(abs);
		}
		public Ability[] getAbilitiesByType(AbilityEnum ae)
        {
			return getAllAbilities().Where (a => a.AbilityType == ae).ToArray();
        }
			        
        public bool HasEffect(EffectType et)
        {
			foreach (CardInstance ca in AttachedCards) {
				foreach (EffectGroup eg in ca.Effects) {
					foreach (Effect e in eg) {
						if (e.TypeOfEffect == et)
							return true;
					}
				}
			}
            return false;
        }
        public bool HasAbility(AbilityEnum ab)
        {
			return getAbilitiesByType (ab).Count () > 0;
        }
		public bool HasColor(ManaTypes color)
		{
			return Model.Colors == null ?
				Model.Cost.GetDominantMana() == color : 
				Model.Colors.Contains (color);
			//TODO:test with color gain or loose effects
		}
		public bool HasType(CardTypes t)
		{
			return Model.Types == t;
		}

        public bool Combating
        {
            get { return _combating; }
            set 
            { 
                _combating = value;                 
            }
        }
        public bool CanAttack
        {
            get
            {
				if (_isTapped || HasSummoningSickness ||!HasType(CardTypes.Creature))
                    return false;

                if (HasAbility (AbilityEnum.Defender))
                    return false;

                if (HasEffect(EffectType.CantAttack))
                    return false;
                
                return true;
            }
        }
		/// <summary>
		/// Determines whether this instance can block the specified CardInstance.
		/// </summary>
		/// <param name="blockedCard">Null value can be passed to check basic conditions for defenders</param>
		public bool CanBlock(CardInstance blockedCard = null)
        { 			
			if (_isTapped || !HasType (CardTypes.Creature))
                return false;
			
			if (HasEffect(EffectType.CantBlock))
				return false;

			if (blockedCard == null)
				return true;

            if ( blockedCard.HasAbility(AbilityEnum.Flying) && 
                ! (this.HasAbility(AbilityEnum.Flying) || this.HasAbility(AbilityEnum.Reach)))
                return false;
							
            return true;
        }
       
		//TODO: remove redundant function
        public bool IsTapped
        {
            get { return _isTapped; }
        }
        /// <summary> Tap card without raising MagicEvent, usefull when card enter battelfield tapped </summary>        
		public bool tappedWithoutEvent
        {
            get { return _isTapped; }
            set
            {
                if (value == _isTapped)
                    return;

                _isTapped = value;

				if (_isTapped)
					Animation.StartAnimation(new FloatAnimation(this, "zAngle", -MathHelper.PiOver2, MathHelper.Pi * 0.1f));
				else
					Animation.StartAnimation(new FloatAnimation(this, "zAngle", 0f, MathHelper.Pi * 0.1f));
				
            }
        }
        public void Tap()
        {
            tappedWithoutEvent = true;
            MagicEngine.CurrentEngine.RaiseMagicEvent(new MagicEventArg(MagicEventType.TapCard, this));
        }
        public void TryToUntap()
        {
//            Effect e = Effects.Where(ef => ef.TypeOfEffect == EffectType.DoesNotUntap).LastOrDefault();
//
//            if (e == null)
                tappedWithoutEvent = false;
        }
		public void UpdatePowerAndToughness()
		{
			Magic.AddLog ("DEBUG => Compute Power/Touchness : " + this.Name);
			_power = int.MinValue;
			_toughness = int.MinValue;

			foreach (CardInstance ci in MagicEngine.CurrentEngine.CardsInPlayHavingEffects) {
				bool valid = false;
				foreach (EffectGroup eg in ci.Effects) {						
					foreach (CardTarget ct in eg.Affected.OfType<CardTarget>()) {
						if (!ct.Accept (this, ci)) {
							valid = false;
							break;
						} else
							valid = true;
					}
					if (!valid)
						continue;
					foreach (NumericEffect e in  eg.OfType<NumericEffect>()) {
						switch (e.TypeOfEffect) {
						case EffectType.AddPower:
							if (_power == int.MinValue)
								_power = Model.Power;
							_power += e.Amount.GetValue(ci) * e.Multiplier;
							break;
						case EffectType.SetPower:
							if (_power == int.MinValue)
								_power = Model.Power;
							_power = e.Amount.GetValue(ci) * e.Multiplier;
							break;
						case EffectType.AddTouchness:
							if (_toughness == int.MinValue)
								_toughness = Model.Toughness;
							_toughness += e.Amount.GetValue (ci);
							break;
						case EffectType.SetTouchness:
							if (_toughness == int.MinValue)
								_toughness = Model.Toughness;
							_toughness = e.Amount.GetValue (ci);
							break;
						}
					}						
				}
			}
			int damages = 0;
			foreach (Damage d in Damages)
				damages += d.Amount;

			if (damages == 0)
				return;

			if (_toughness == int.MinValue)
				_toughness = Model.Toughness;
			_toughness -= damages;
		}
		public bool UpdateControler()
		{
			_controler = null;
			foreach (CardInstance ci in MagicEngine.CurrentEngine.CardsInPlayHavingEffect(EffectType.GainControl)) {
				bool valid = false;
				foreach (EffectGroup eg in ci.Effects) {						
					foreach (CardTarget ct in eg.Affected.OfType<CardTarget>()) {
						if (!ct.Accept (this, ci)) {
							valid = false;
							break;
						} else
							valid = true;
					}
					if (!valid)
						continue;
					foreach (Effect e in  eg) {
						switch (e.TypeOfEffect) {
						case EffectType.GainControl:
							_controler = ci.Controler;
							break;
						}
					}						
				}
			}
			return _controler != null;
		}

		#region layouting
		//TODO: create function for attached card position update instead of
		//		copying again and again the same code
//        public override float x
//        {
//            get { return _x; }
//            set { 
//				if (_x == value)
//					return;
//				
//				_x = value; 
//
//				float a = _x;
//				foreach (CardInstance ac in AttachedCards) {
//					if (ac.Controler != Controler) {
//						updateArrows ();		
//						continue;
//					}
//					a += 0.15f;
//					if (Math.Abs (a - ac.x) > 1.0)
//						Animation.StartAnimation (new FloatAnimation (ac, "x", a, 0.2f));
//					else
//						ac.x = a;
//				}
//			}
//        }
//        public override float y
//        {
//            get { return _y; }
//			set { 
//				if (_y == value)
//					return;
//
//				_y = value; 
//
//				float a = _y;
//				foreach (CardInstance ac in AttachedCards) {
//					if (ac.Controler != Controler) {
//						updateArrows ();		
//						continue;
//					}
//					a += 0.15f;
//					if (Math.Abs (a - ac.y) > 1.0)
//						Animation.StartAnimation (new FloatAnimation (ac, "y", a, 0.2f));
//					else
//						ac.y = a;
//				}					
//			}        
//		}
//        public override float z
//        {
//            get { return _z; }
//			set { 
//				if (_z == value)
//					return;
//
//				_z = value; 
//
//				float a = _z;
//				foreach (CardInstance ac in AttachedCards) {
//					if (ac.Controler != Controler) {
//						updateArrows ();		
//						continue;
//					}
//					a -=  attachedCardsSpacing;
//					if (Math.Abs (a - ac.z) > 1.0)
//						Animation.StartAnimation (new FloatAnimation (ac, "z", a, 0.2f));
//					else
//						ac.z = a;
//				}
//				//updateArrows ();
//			}        
//		}
//		public override float xAngle
//        {
//            get { return _xAngle; }
//            set {
//				if (_xAngle == value)
//					return;
//				
//				_xAngle = value; 
//
//				foreach (CardInstance ac in AttachedCards) {
//					if (ac.Controler != Controler)
//						continue;
//				
//					if (Math.Abs (_xAngle - ac.xAngle) > 1.0)
//						Animation.StartAnimation (new FloatAnimation (ac, "z", _xAngle, 0.2f));
//					else
//						ac.xAngle = _xAngle;
//				}
//			}
//        }
//		public override float yAngle
//        {
//            get { return _yAngle; }
//            set { _yAngle = value; }
//        }
//		public override float zAngle
//        {
//            get { return _zAngle; }
//            set { _zAngle = value; }
//        }
//		public override float Scale {
//			get {
//				return _scale;
//			}
//			set {
//				_scale = value;
//			}
//		}

        public float saved_x = 0.0f;
        public float saved_y = 0.0f;
        public float saved_z = 0.0f;
        public float saved_xAngle = 0.0f;
        public float saved_yAngle = 0.0f;
        public float saved_zAngle = 0.0f;
		public float saved_scale = 0.0f;


       	public void SwitchFocus()
        {
            HasFocus = !HasFocus;

            if (HasFocus)
            {
                //Debug.WriteLine("{0} => {1}", this.Model.Name, this.Model.Oracle);
                if (focusedCard != null)
                    focusedCard.SwitchFocus();

				Vector3 v = Magic.vFocusedPoint;
                SavePosition();
				                
				v = v.Transform(Matrix4.Invert(Controler.Transformations));
				Animation.StartAnimation(new FloatAnimation(this, "x", v.X, 0.9f));
				Animation.StartAnimation(new FloatAnimation(this, "y", v.Y, 1.2f));
				Animation.StartAnimation(new FloatAnimation(this, "z", v.Z, 0.9f));
				float aCam = Magic.FocusAngle;
				Animation.StartAnimation(new AngleAnimation(this, "xAngle", aCam, MathHelper.Pi * 0.1f));
                //Animation.StartAnimation(new AngleAnimation(this, "yAngle", -Controler.Value.zAngle, MathHelper.Pi * 0.03f));
                Animation.StartAnimation(new AngleAnimation(this, "zAngle", -Controler.zAngle, MathHelper.Pi * 0.3f));
				Animation.StartAnimation (new FloatAnimation (this, "Scale", 1.0f, 0.05f));


                focusedCard = this;
            }
            else
            {
                focusedCard = null;
                RestoreSavedPosition();
            }
        }

        public void SavePosition()
        {
            saved_x = x;
            saved_xAngle = xAngle;
            saved_y = y;
            saved_yAngle = yAngle;
            saved_z = z;
            saved_zAngle = zAngle;
			saved_scale = Scale;
        }
        public void RestoreSavedPosition()
        {
     		Animation.StartAnimation(new FloatAnimation(this, "x", saved_x, 0.5f));
            Animation.StartAnimation(new FloatAnimation(this, "y", saved_y, 0.5f));
            Animation.StartAnimation(new FloatAnimation(this, "z", saved_z, 0.5f));
            Animation.StartAnimation(new AngleAnimation(this, "xAngle", saved_xAngle, 0.5f));
            Animation.StartAnimation(new AngleAnimation(this, "yAngle", saved_yAngle, 0.5f));
            Animation.StartAnimation(new AngleAnimation(this, "zAngle", saved_zAngle, 0.5f));
			Animation.StartAnimation (new FloatAnimation (this, "Scale", saved_scale, 0.05f));
        }
        
		public Rectangle<float> getProjectedBounds()
		{
//			Matrix4 M = ModelMatrix * Controler.Transformations *
//			            Magic.texturedShader.ModelViewMatrix *
//						Magic.texturedShader.ProjectionMatrix;
			Rectangle<float> projR = Rectangle<float>.Zero;
//			Point<float> topLeft, bottomRight;
//			if (_isTapped) {
//				topLeft = MagicData.CardBounds.BottomLeft;
//				bottomRight = MagicData.CardBounds.TopRight;
//			} else {
//				topLeft = MagicData.CardBounds.TopLeft;
//				bottomRight = MagicData.CardBounds.BottomRight;
//			}
//			
//			Point<float> pt1 = glHelper.Project (topLeft, M, Magic.viewport [2], Magic.viewport [3]);
//			Point<float> pt2 = glHelper.Project (bottomRight, M, Magic.viewport [2], Magic.viewport [3]);
//			if (pt1 < pt2) {
//				projR.TopLeft = pt1;
//				projR.BottomRight = pt2;
//			} else {
//				projR.TopLeft = pt2;
//				projR.BottomRight = pt1;
//			}
			return projR;
		}
		public bool mouseIsIn(Point<float> m)
		{
			Rectangle<float> r = getProjectedBounds ();
				return r.ContainsOrIsEqual (m);
		}
		#endregion


		#region Arrows
		GGL.vaoMesh arrows;
		public void updateArrows(){
			if (arrows!=null)
				arrows.Dispose ();
			arrows = null;

			float z = 1.0f;
			foreach (CardInstance ac in AttachedCards.Where(c=>c.Controler != this.Controler)) {
				arrows += new Arrow3d (
					Vector3.TransformPosition(ac.Position, ac.Controler.Transformations), 
					Vector3.TransformPosition(this.Position, this.Controler.Transformations),
					Vector3.UnitZ * z);
				z += 0.2f;
			}
			if (AttachedTo != null)
				AttachedTo.updateArrows ();
		}
		void renderArrow(){
//			Magic.arrowShader.Enable ();
//			Magic.arrowShader.ProjectionMatrix = Magic.projection;
//			Magic.arrowShader.ModelViewMatrix = Magic.modelview;
//			Magic.arrowShader.ModelMatrix = Matrix4.Identity;
//			GL.PointSize (2f);
//			GL.Disable (EnableCap.CullFace);
//			arrows.Render (BeginMode.TriangleStrip);
//			GL.Enable (EnableCap.CullFace);
//			Magic.arrowShader.Disable ();
		}
		#endregion

		#region Overlays
		int _lastPaintedPower = int.MinValue, _lastPaintedToughness = int.MinValue;
		IEnumerable<AbilityEnum> _lastKnownAbilities;
		public bool AbilityChangesDetected = false;

		/// <summary>
		/// return true if abs has changes
		/// </summary>
		public void CheckAbilityChanges(){
			IEnumerable<AbilityEnum> abs = getAllAbilities ().Select (a => a.AbilityType).Distinct ();
			int abCount = abs.Count ();

			if (abCount > 0) {
				if (infoOverlayVboIdx < 0) {
					_lastKnownAbilities = abs;
					createInfoOverlay ();
					return;
				}
			} else if (infoOverlayVboIdx >= 0) {
				removeInfoOverlay ();
				_lastKnownAbilities = null;
				return;
			} else
				return;


			int lkCount = _lastKnownAbilities.Count ();
				
			if (lkCount != abCount) {
				_lastKnownAbilities = abs;
				AbilityChangesDetected = true;
				return;
			}

			IEnumerator<AbilityEnum> e_abs = abs.GetEnumerator ();
			IEnumerator<AbilityEnum> e_lk_abs = abs.GetEnumerator ();

			while (e_abs.MoveNext ()) {
				e_lk_abs.MoveNext ();
				if (e_abs.Current == e_lk_abs.Current)
					continue;
				AbilityChangesDetected = true;
				break;
			}

			_lastKnownAbilities = abs;
		}
		/// <summary>
		/// Creates vbo instance and overlay bitmap.
		/// </summary>
		void createInfoOverlay(){
			AbilityChangesDetected = true;
			infoOverlayVboIdx = InfoOverlayVBO.AddInstance ();
			InfoOverlayVBO.UpdateVBO ();
			byte[] newPObmp = new byte[infoOverlayWidth * infoOverlayHeight * 4 * 
				InfoOverlayVBO.InstancedDatas.Length];
			if (InfoOverlayBmp != null)
				Array.Copy (InfoOverlayBmp, newPObmp, InfoOverlayBmp.Length);

			InfoOverlayBmp = newPObmp;

			if (!GL.IsTexture (InfoOverlayTexture)) {
				InfoOverlayTexture = GL.GenTexture ();
				GL.BindTexture(TextureTarget.Texture2DArray, InfoOverlayTexture);
				GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
				GL.TexParameter(TextureTarget.Texture2DArray, TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge);
			}else
				GL.BindTexture(TextureTarget.Texture2DArray, InfoOverlayTexture);

			GL.TexImage3D (TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgba,
				infoOverlayWidth, infoOverlayHeight, InfoOverlayVBO.InstancedDatas.Length, 0,
				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, InfoOverlayBmp);
			GL.BindTexture(TextureTarget.Texture2DArray, 0);

			UpdateInfoOverlaySurface ();
		}
		void removeInfoOverlay(){
			AbilityChangesDetected = false;
			InfoOverlayVBO.RemoveInstance (infoOverlayVboIdx);
			if (InfoOverlayVBO.InstancedDatas.Length > 0) {
				foreach (CardInstance ci in Magic.CurrentGameWin.Players.SelectMany(p => p.InPlay.Cards.Where(c => c.infoOverlayVboIdx>0))) {
					if (ci == this)
						continue;
					if (ci.infoOverlayVboIdx > infoOverlayVboIdx) {
						ci.infoOverlayVboIdx--;
						ci.updateInfoOverlayDatas ();
					}
				}
				int oSize = infoOverlayWidth * infoOverlayHeight * 4;
				byte[] newPObmp = new byte[oSize * InfoOverlayVBO.InstancedDatas.Length];
				Array.Copy (InfoOverlayBmp, 0, newPObmp, 0, oSize * infoOverlayVboIdx);
				Array.Copy (InfoOverlayBmp, oSize * (infoOverlayVboIdx + 1), newPObmp, oSize * infoOverlayVboIdx,
					oSize * (InfoOverlayVBO.InstancedDatas.Length - infoOverlayVboIdx - 1));

				InfoOverlayBmp = newPObmp;
				GL.BindTexture(TextureTarget.Texture2DArray, InfoOverlayTexture);
				GL.TexImage3D (TextureTarget.Texture2DArray, 0, PixelInternalFormat.Rgba,
					infoOverlayWidth, infoOverlayHeight, InfoOverlayVBO.InstancedDatas.Length, 0,
					OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, InfoOverlayBmp);
				GL.BindTexture(TextureTarget.Texture2DArray, 0);
			}
			infoOverlayVboIdx = -1;
			InfoOverlayVBO.UpdateVBO ();
			UpdateInfoOverlaySurface ();
		}
		/// <summary>
		/// Cairo surface draw of Abilities for this instance and
		/// update texture subData
		/// </summary>
		public void UpdateInfoOverlaySurface()
		{
			if (!AbilityChangesDetected)
				return;
			AbilityChangesDetected = false;

			Magic.AddLog ("DEBUG => UPDATE INFO OVERLAY SURF : " + this.Name);
			int x = 0;
			int y = infoOverlayVboIdx * infoOverlayHeight;

			int stride = 4 * infoOverlayWidth;

			using (ImageSurface draw =
				new ImageSurface(InfoOverlayBmp, Format.Argb32,
					infoOverlayWidth, infoOverlayHeight * InfoOverlayVBO.InstancedDatas.Length, stride))
			{
				using (Context gr = new Context(draw))
				{
					Cairo.Rectangle r = new Cairo.Rectangle(x, y, infoOverlayWidth, infoOverlayHeight);

					//gr.SetSourceRGBA (1,1,1,1);
					//gr.Rectangle(r);
					//gr.FillPreserve();
					//gr.SetSourceRGB (0.9f,0.2f,0.2f);
					//gr.LineWidth = 2.0f;
					//gr.Stroke();

					gr.Translate (0, y);
					gr.Scale (0.34, 0.32);
					using (IEnumerator<AbilityEnum> e = _lastKnownAbilities.GetEnumerator()){
						while (e.MoveNext ()) {
							MagicData.hSVGsymbols.RenderCairoSub (gr, "#" + e.Current.ToString ());
							gr.Translate (100, 0);
						}
					}

					draw.Flush();
				}
			}

			GL.BindTexture(TextureTarget.Texture2DArray, InfoOverlayTexture);
			int oSize = infoOverlayWidth * infoOverlayHeight * 4;
			byte[] tmp = new byte[oSize];
			Array.Copy (InfoOverlayBmp, infoOverlayVboIdx * oSize, tmp, 0, oSize);
			GL.TexSubImage3D (TextureTarget.Texture2DArray, 0, 0, 0, infoOverlayVboIdx, infoOverlayWidth, infoOverlayHeight, 1,
				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, tmp);
			GL.BindTexture(TextureTarget.Texture2DArray, 0);
		}
		/// <summary>
		/// Cairo surface draw of Power/Touchness for this instance and
		/// update texture subData
		/// </summary>
		public void UpdatePointsOverlaySurface()
		{
			Magic.AddLog ("DEBUG => ********* CALL UPDATE POINT OVERLAY SURF : " + this.Name);
			if (pointOverlayVboIdx < 0)
				return;
			#if DEBUG
			if (!(this.HasType (CardTypes.Creature) && CurrentGroup.GroupName == CardGroupEnum.InPlay))
				Debugger.Break ();
			#endif

			if (Power == _lastPaintedPower && Toughness == _lastPaintedToughness)
				return;

			_lastPaintedPower = Power;
			_lastPaintedToughness = Toughness;

			Magic.AddLog ("DEBUG => UPDATE POINT OVERLAY SURF : " + this.Name);
			int x = 0;
			int y = pointOverlayVboIdx * pointOverlayHeight;

			int stride = 4 * pointOverlayWidth;

			using (ImageSurface draw =
				new ImageSurface(PointOverlayBmp, Format.Argb32,
					pointOverlayWidth, pointOverlayHeight * PointOverlayVBO.InstancedDatas.Length, stride))
			{
				using (Context gr = new Context(draw))
				{
					Cairo.Rectangle r = new Cairo.Rectangle(x, y, pointOverlayWidth, pointOverlayHeight);

					gr.SetSourceRGB (1,1,1);
					gr.Rectangle(r);
					gr.FillPreserve();
					gr.SetSourceRGB (0.2f,0.2f,0.2f);
					gr.LineWidth = 2.0f;
					gr.Stroke();

					gr.SelectFontFace("Times New Roman", FontSlant.Normal, FontWeight.Bold);
					gr.SetFontSize(38);

					if (Damages.Count == 0)
						gr.SetSourceRGB (0.2f,0.2f,0.2f);
					else
						gr.SetSourceRGB (1.0f,0.3f,0.4f);

					string text = Power.ToString() + "/" + Toughness.ToString();

					FontExtents fe = gr.FontExtents;
					TextExtents te = gr.TextExtents(text);
					double xt = pointOverlayWidth / 2 - te.Width / 2;
					double yt = pointOverlayHeight / 2 - fe.Height / 2 + y + fe.Ascent;

					gr.MoveTo(xt, yt);
					gr.ShowText(text);

					draw.Flush();
				}
			}

			GL.BindTexture(TextureTarget.Texture2DArray, PointOverlayTexture);
			int oSize = pointOverlayWidth * pointOverlayHeight * 4;
			byte[] tmp = new byte[oSize];
			Array.Copy (PointOverlayBmp, pointOverlayVboIdx * oSize, tmp, 0, oSize);
			GL.TexSubImage3D (TextureTarget.Texture2DArray, 0, 0, 0, pointOverlayVboIdx, pointOverlayWidth, pointOverlayHeight, 1,
				OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, tmp);
			GL.BindTexture(TextureTarget.Texture2DArray, 0);
			//bmp.Save(@"d:/test.png");
		}
		public void ResetOverlay()
		{

		}

		#endregion

        public override string ToString()
        {
            return string.Format("{0} | {1} | {2}", Model.Name, Model.Types, Model.Cost);
        }
    }
}
