using System;
using OpenTK;
using Tetra.DynamicShading;

namespace MagicCrow
{
	public abstract class RenderedCardModel : IRenderable
	{
		public static InstancesVBO<CardInstancedData> CardsVBO;
		public int instanceIdx;

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
				updateInstacedVBO ();
			}
		}
		public virtual float y
		{
			get { return _y; }
			set { 
				if (_y == value)
					return;

				_y = value;
				updateInstacedVBO ();
			}        
		}
		public virtual float z
		{
			get { return _z; }
			set { 
				if (_z == value)
					return;

				_z = value;
				updateInstacedVBO ();
			}        
		}
		public virtual float xAngle
		{
			get { return _xAngle; }
			set {
				if (_xAngle == value)
					return;

				_xAngle = value; 
				updateInstacedVBO ();
			}
		}
		public virtual float yAngle
		{
			get { return _yAngle; }
			set {
				if (_yAngle == value)
					return;

				_yAngle = value; 
				updateInstacedVBO ();
			}
		}
		public virtual float zAngle
		{
			get { return _zAngle; }
			set {
				if (_zAngle == value)
					return;

				_zAngle = value; 
				updateInstacedVBO ();
			}
		}

		public virtual Vector3 Position
		{
			get
			{ return new Vector3(x, y, z); }
			set
			{
				_x = value.X;
				_y = value.Y;
				_z = value.Z;
				updateInstacedVBO ();
			}
		}
		public void ResetPositionAndRotation()
		{
			x = y = z = xAngle = yAngle = zAngle = 0;
		}

		#region IRenderable implementation

		public abstract void Render ();
		public Matrix4 ModelMatrix {
			get
			{
				Matrix4 transformation;


				Matrix4 Rot = 
					Matrix4.CreateRotationX (xAngle) *
					Matrix4.CreateRotationY (yAngle) *
					Matrix4.CreateRotationZ (zAngle);

				//Matrix4 Rot = Matrix4.CreateRotationZ(zAngle);
				transformation = Matrix4.CreateScale(Scale) *  Rot * Matrix4.CreateTranslation(x, y, z);

				return transformation;
			}
			set {
				throw new NotImplementedException ();
			}
		}

		public void updateInstacedVBO(){
			if (CardsVBO == null)
				return;
			Matrix4 Rot = 
				Matrix4.CreateRotationX (xAngle) *
				Matrix4.CreateRotationY (yAngle) *
				Matrix4.CreateRotationZ (zAngle);

			//Matrix4 Rot = Matrix4.CreateRotationZ(zAngle);
			CardsVBO.InstancedDatas[instanceIdx].modelMats = Matrix4.CreateScale(Scale) *  Rot * Matrix4.CreateTranslation(x, y, z);
			CardsVBO.SetInstanceIsDirty (instanceIdx);
		}
		#endregion


		public virtual float Scale {
			get {
				return _scale;
			}
			set {
				_scale = value;
			}
		}
	}
}

