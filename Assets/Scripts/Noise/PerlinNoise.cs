using UnityEngine;
using System.Collections;

public class PerlinNoise : NoiseModule
{
	public PerlinNoise(int seed) : base(seed)
	{
	}
	
	public override float FractalNoise2D(float x, float y, int octNum, float frq, float amp)
	{
		float gain = 1.0f;
		float sum = 0.0f;
		
		for(int i = 0; i < octNum; i++)
		{
			sum += Noise2D(x*gain/frq, y*gain/frq) * amp/gain;
			gain *= 2.0f;
		}
		return sum;
	}
	
}













