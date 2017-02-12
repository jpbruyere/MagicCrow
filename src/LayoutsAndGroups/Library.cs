using System;
using OpenTK;

namespace MagicCrow
{
	public class Library : CardGroup
	{
		public Library()
			: base(CardGroupEnum.Library)
		{
			x = -6;
			y = -1.0f;
			xAngle = MathHelper.Pi;
			VerticalSpacing = 0.01f;
		}
	}
}

