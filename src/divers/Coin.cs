using System;
using OpenTK;
using GGL;
using OpenTK.Graphics.OpenGL;

namespace MagicCrow
{
	public class Coin : IRenderable, IAnimatable
	{
		public enum TossResultEnum{
			Tail,
			Head
		};
		public class TossEventArg : EventArgs
		{
			public Coin.TossResultEnum Result;
			public TossEventArg(Coin.TossResultEnum _result){
				Result = _result;
			}
		}

		Random rnd = new Random ();
		vaoMesh mesh;
		Matrix4 coinMat = Matrix4.Identity;

		int coinTex;
		float coinZ = 0f;
		float coinV = 10;
		float coinAVx = MathHelper.Pi * 5.55f;
		float coinAx = 0f;
		public bool running = false;

		#region CTOR
		public Coin ()
		{
			initCoin ();
			startTossing ();
		}

		~Coin(){
			GL.DeleteTexture (coinTex);
			mesh.Dispose ();
		}
		#endregion

		void initCoin()
		{
			mesh = vaoMesh.Load (@"meshes/coin.obj");
			coinTex = new Texture (@"meshes/dollar.jpg");
			coinMat = Matrix4.CreateTranslation (0, 0, coinZ);
		}
		void startTossing()
		{
			coinZ = 0.51f;
			coinV =(float)( 8 + rnd.NextDouble() * 4);
			coinAVx = MathHelper.Pi * (float)( 1 + rnd.NextDouble() * 6);
			float coinAx = 0f;
			running = true;
		}

		#region IRenderable implementation
		public Matrix4 ModelMatrix {
			get { return coinMat; }
			set { coinMat = value; }
		}
		public void Render ()
		{
//			Magic.texturedShader.ModelMatrix = coinMat;
//			Magic.texturedShader.UpdateUniforms ();
//			GL.BindTexture (TextureTarget.Texture2D, coinTex);
//			mesh.Render (BeginMode.Triangles);
//			GL.BindTexture (TextureTarget.Texture2D, 0);
		}
		#endregion

		#region IAnimatable implementation
		public event EventHandler<EventArgs> AnimationFinished = delegate { };
		public void Animate (float ellapseTime = 0f)
		{
			float a = -9.81f; // (m/s²)
			float et = ellapseTime;
			if (coinZ > 0.5f) {
				coinV += a * et;
				coinZ += coinV * et;
				coinAx += coinAVx * et;

			} else {
				coinAx = (coinAx % MathHelper.TwoPi) ;
				if (coinAx >= MathHelper.Pi) {
					coinAx = MathHelper.Pi;
					AnimationFinished (this, new TossEventArg (TossResultEnum.Head));
				} else {
					coinAx = 0f;
					AnimationFinished (this, new TossEventArg (TossResultEnum.Tail));
				}
				running = false;
			}
			coinMat = Matrix4.CreateRotationX (coinAx) * Matrix4.CreateTranslation (0, -5, coinZ);
		}
		#endregion


	}


}

