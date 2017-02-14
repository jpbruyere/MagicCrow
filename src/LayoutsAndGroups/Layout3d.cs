using System;
using OpenTK;

namespace MagicCrow
{
	public abstract class Layout3d
	{
		public bool IsExpanded = false;

		public float x = 0.0f;
		public float y = 0.0f;
		public float z = 0.0f;
		public float xAngle = 0.0f;
		public float yAngle = 0.0f;
		public float zAngle = 0.0f;
		public float Scale = 1.0f;

		public float HorizontalSpacing = 0f;
		public float VerticalSpacing = 0f;
		public float MaxHorizontalSpace = 4.5f;

		public virtual Vector3 Position {
			get
			{ return new Vector3 (x, y, z); }
			set {
				x = value.X;
				y = value.Y;
				z = value.Z;
			}
		}
		public virtual Matrix4 Transformations {
			get {
				Matrix4 transformation;


				Matrix4 Rot = Matrix4.CreateRotationX (xAngle);
				Rot *= Matrix4.CreateRotationY (yAngle);
				Rot *= Matrix4.CreateRotationZ (zAngle);
				//Matrix4 Rot = Matrix4.CreateRotationZ(zAngle);
				transformation = Rot * Matrix4.CreateTranslation (x, y, z);

				return transformation;
			}

		}
			
		public abstract void UpdateLayout (bool anim = true);
		public abstract void toogleShowAll ();
	}
}

