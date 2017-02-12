using System;
using System.Collections.Generic;

namespace MagicCrow
{
	public class CardsGroupAnimation : List<CardInstance>,  IAnimatable
	{
		public CardsGroupAnimation ()
		{
		}

		public void AddCard(CardInstance c)
		{
			//c.AnimationFinished += onCardAnimFinished;
			this.Add (c);
		}

		int currentIndex = 0;

		#region IAnimatable implementation

		public event EventHandler<EventArgs> AnimationFinished  = delegate {};

		public void Animate (float ellapseTime = 0f)
		{
//			while(currentIndex<Count){
//				this [currentIndex].Animate (ellapseTime);
//				currentIndex++;
//			}
//			currentIndex = 0;
		}

		#endregion

		void onCardAnimFinished(object sender, EventArgs e){
			this.Remove (sender as CardInstance);
			if (Count == 0)
				AnimationFinished (this, null);
		}
	}
}

