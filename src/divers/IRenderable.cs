using System;
using OpenTK;

namespace MagicCrow
{
	public interface IRenderable
	{		
		Matrix4 ModelMatrix { get; set; }
		void Render();
	}
}

