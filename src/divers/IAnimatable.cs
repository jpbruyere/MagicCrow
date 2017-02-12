using System;

namespace MagicCrow
{
	public interface IAnimatable
	{
		event EventHandler<EventArgs> AnimationFinished;

		//float EllapseTime { get; set; }
		void Animate(float ellapseTime = 0f);
	}
}

