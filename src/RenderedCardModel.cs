using System;
using OpenTK;
using Tetra.DynamicShading;

namespace MagicCrow
{
	public abstract class RenderedCardModel
	{
		public static InstancesVBO<CardInstancedData> CardsVBO, OverlayVBO, PointOverlayVBO;
		public static byte[] PointOverlayBmp;
		public static int PointOverlayTexture;
		public const int pointOverlayWidth = 200;
		public const int pointOverlayHeight = 40;

		public int cardVboIdx, overlayVboIdx=-1, pointOverlayVboIdx=-1;

		protected float _x = 0.0f;
		protected float _y = 0.0f;
		protected float _z = 0.0f;
		protected float _xAngle = 0.0f;
		protected float _yAngle = 0.0f;
		protected float _zAngle = 0.0f;
		protected float _scale = 1.0f;

		public MagicCard Model;

		public virtual float x
		{
			get { return _x; }
			set { 
				if (_x == value)
					return;

				_x = value;
				updateInstacedDatas ();
			}
		}
		public virtual float y
		{
			get { return _y; }
			set { 
				if (_y == value)
					return;

				_y = value;
				updateInstacedDatas ();
			}        
		}
		public virtual float z
		{
			get { return _z; }
			set { 
				if (_z == value)
					return;

				_z = value;
				updateInstacedDatas ();
			}        
		}
		public virtual float xAngle
		{
			get { return _xAngle; }
			set {
				if (_xAngle == value)
					return;

				_xAngle = value; 
				updateInstacedDatas ();
			}
		}
		public virtual float yAngle
		{
			get { return _yAngle; }
			set {
				if (_yAngle == value)
					return;

				_yAngle = value; 
				updateInstacedDatas ();
			}
		}
		public virtual float zAngle
		{
			get { return _zAngle; }
			set {
				if (_zAngle == value)
					return;

				_zAngle = value; 
				updateInstacedDatas ();
			}
		}
		public virtual float Scale {
			get { return _scale; }
			set {
				if (_scale == value)
					return;
				_scale = value;
				updateInstacedDatas ();
			}
		}

		public virtual Vector3 Position
		{
			get
			{ return new Vector3(x, y, z); }
			set
			{
				if (value == Position)
					return;
				_x = value.X;
				_y = value.Y;
				_z = value.Z;
				updateInstacedDatas ();
			}
		}
		public void ResetPositionAndRotation()
		{
			x = y = z = xAngle = yAngle = zAngle = 0;
		}			

		public Matrix4 ModelMatrix {
			get
			{
				Matrix4 Rot = 
					Matrix4.CreateRotationX (xAngle) *
					Matrix4.CreateRotationY (yAngle) *
					Matrix4.CreateRotationZ (zAngle);

				return Matrix4.CreateScale(Scale) *  Rot * Matrix4.CreateTranslation(x, y, z);
			}
		}
		Matrix4 pointOverlayMatrix {
			get
			{
				Matrix4 tmp =					
					Matrix4.CreateTranslation (0.25f, -0.7f, 0.04f) *
					Matrix4.CreateScale (Scale) *
					Matrix4.CreateRotationX (xAngle) *
					Matrix4.CreateRotationY (yAngle) *
					//Matrix4.CreateRotationZ (zAngle) *
					Matrix4.CreateTranslation (x, y, z);
				return xAngle == 0f ? Matrix4.CreateRotationX (Magic.FocusAngle) * tmp : tmp;
			}
		}
		public void updateInstacedDatas(){
			if (CardsVBO == null)
				return;
			Matrix4 mod = ModelMatrix;

			CardsVBO.InstancedDatas[cardVboIdx].modelMats = mod;
			CardsVBO.SetInstanceIsDirty (cardVboIdx);
			if (overlayVboIdx >= 0) {				
				OverlayVBO.InstancedDatas [overlayVboIdx].modelMats = mod * Matrix4.CreateTranslation(0,0,0.1f);
				OverlayVBO.SetInstanceIsDirty (overlayVboIdx);
			}
			if (pointOverlayVboIdx >= 0) {				
				PointOverlayVBO.InstancedDatas [pointOverlayVboIdx].modelMats = pointOverlayMatrix;
				PointOverlayVBO.InstancedDatas [pointOverlayVboIdx].picked = pointOverlayVboIdx;
				PointOverlayVBO.SetInstanceIsDirty (pointOverlayVboIdx);
			}
		}
		public void updateOverlayDatas(){
			OverlayVBO.InstancedDatas [overlayVboIdx].modelMats = ModelMatrix * Matrix4.CreateTranslation(0,0,0.1f);
			OverlayVBO.SetInstanceIsDirty (overlayVboIdx);
		}
		public void updatePointOverlayDatas(){
			PointOverlayVBO.InstancedDatas [pointOverlayVboIdx].modelMats = pointOverlayMatrix;
			PointOverlayVBO.InstancedDatas [pointOverlayVboIdx].picked = pointOverlayVboIdx;
			PointOverlayVBO.SetInstanceIsDirty (pointOverlayVboIdx);
		}
	}
}

