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
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight, float scale)
    {
        //create 2d array for our noise map
        float[,] noiseMap = new float[mapWidth, mapHeight];

        if (scale <=0)
        {
            scale = 0.0001f;
        }

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float sampleX = x /scale;
                float sampleY = y /scale;

                float perlinNOiseValue = Mathf.PerlinNoise(sampleX, sampleY);
                noiseMap[x, y] = perlinNOiseValue;
            }
        }

        return noiseMap;
    }
}
