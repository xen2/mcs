/*
  Copyright (C) 2009 Jeroen Frijters

  This software is provided 'as-is', without any express or implied
  warranty.  In no event will the authors be held liable for any damages
  arising from the use of this software.

  Permission is granted to anyone to use this software for any purpose,
  including commercial applications, and to alter it and redistribute it
  freely, subject to the following restrictions:

  1. The origin of this software must not be misrepresented; you must not
     claim that you wrote the original software. If you use this software
     in a product, an acknowledgment in the product documentation would be
     appreciated but is not required.
  2. Altered source versions must be plainly marked as such, and must not be
     misrepresented as being the original software.
  3. This notice may not be removed or altered from any source distribution.

  Jeroen Frijters
  jeroen@frijters.net
  
*/
using System;
using System.Collections.Generic;
using System.Text;

namespace IKVM.Reflection
{
	public sealed class StrongNameKeyPair
	{
		internal readonly System.Reflection.StrongNameKeyPair keyPair;

		internal StrongNameKeyPair(System.Reflection.StrongNameKeyPair keyPair)
		{
			this.keyPair = keyPair;
		}

		public StrongNameKeyPair(string keyPairContainer)
		{
			this.keyPair = new System.Reflection.StrongNameKeyPair(keyPairContainer);
		}

		public StrongNameKeyPair(byte[] keyPairArray)
		{
			this.keyPair = new System.Reflection.StrongNameKeyPair(keyPairArray);
		}

		public StrongNameKeyPair(System.IO.FileStream fs)
		{
			this.keyPair = new System.Reflection.StrongNameKeyPair(fs);
		}

		public byte[] PublicKey
		{
			get { return keyPair.PublicKey; }
		}
	}
}
