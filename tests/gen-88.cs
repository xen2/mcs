using System;

public struct KeyValuePair<K,V>
{
	public KeyValuePair (K k, V v)
	{ }

	public KeyValuePair (K k)
	{ }
}

class X
{
	static void Main ()
	{
		new KeyValuePair<int,long> ();
	}
}
