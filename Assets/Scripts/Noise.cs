using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Since we dont going to apply this to any object in the scene
//there is no need to inherit from MonoBehaviour class so we remove it
//Also we are not going to create multiple ionstanses of the script so we
//initialise the class as static
public static class Noise 
{
    //Generate noise map and have certain values of 0 and 1
    //mapWidth and mapHeight -> The dimensions of our scene
    //Scale -> is the scale of the noise
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight,int seed, float scale, int octives, float presistance, float lacunnarity, Vector2 offset)
    {
        //create 2d array for our noise map with size mapWidth and mapHeight
        float[,] noiseMap = new float[mapWidth, mapHeight];

        System.Random prng = new System.Random(seed);
        Vector2[] octiveOffsets = new Vector2[octives];
        for (int i = 0; i < octives; i++)
        {
            float offsetX = prng.Next(-10000, 10000) + offset.x;
            float offsetY = prng.Next(-10000, 10000) + offset.y;
            octiveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        //In case that our scale is equal to 0 we get a division error since we can devide a number with 0
        //so we handle that error by assign a lowest scale to our value if the scale is equal to 0
        if (scale <=0){
            scale = 0.0001f;
        }

        float maxNoiseHeight = float.MinValue;
        float minNoiseHeight = float.MaxValue;

        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        //Loop through the array
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {

                float amplitude = 1;
                float frequency = 0.5f;
                float noiseHeight = 0;

                for (int i = 0; i < octives; i++)
                {
                    //Create our sample coordinates: figure out at which point we are sampling from
                    float sampleX = x-halfWidth / scale * frequency + octiveOffsets[i].x;
                    float sampleY = y-halfHeight / scale * frequency + octiveOffsets[i].y;


                    //so now because we have our sample coordinates 
                    float perlinNoiseValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1;
                    //noiseMap[x, y] = perlinNOiseValue; //assing perlin value to noiseMap

                    noiseHeight += perlinNoiseValue * amplitude;

                    amplitude *= presistance;
                    frequency *= lacunnarity;
                }
                if (noiseHeight > maxNoiseHeight)
                {
                    maxNoiseHeight = noiseHeight;
                }
                else if (noiseHeight <minNoiseHeight)
                {
                    minNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;
              
            }
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap; //return the noise map
    }
}
