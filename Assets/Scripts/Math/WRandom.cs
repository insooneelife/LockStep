using System;

public class WRandom
{
	uint[] state = new uint[16];
	uint index = 0;

	public WRandom(uint nSeed)
	{
		uint s = nSeed;
		for (int i = 0; i < 16; i++)
		{
			state[i] = s;
			s += s + 73;
		}
	}

	internal uint Next(int minValue, int maxValue)
	{
		return (uint)((Next() % (maxValue - minValue)) + minValue);
	}

	public uint Next(uint maxValue)
	{
		return Next() % maxValue;
	}

	public uint Next()
	{
		uint a, b, c, d;

		a = state[index];
		c = state[(index + 13) & 15];
		b = a ^ c ^ (a << 16) ^ (c << 15);
		c = state[(index + 9) & 15];
		c ^= (c >> 11);
		a = state[index] = b ^ c;
		d = a ^ ((a << 5) & 0xda442d24U);
		index = (index + 15) & 15;
		a = state[index];
		state[index] = a ^ b ^ d ^ (a << 2) ^ (b << 18) ^ (c << 28);

		return state[index];
	}
}

