using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Since we dont going to apply this to any object in the scene
//there is no need to inherit from MonoBehaviour class so we remove it
//Also we are not going to create multiple ionstanses of the script so we
//initialise the class as static
public static class Noise 
{

    public enum NormalizeMode
    {
        Local,
        Global
    }

    //Generate noise map and have certain values of 0 and 1
    //mapWidth and mapHeight -> The dimensions of our scene
    //Scale -> is the scale of the noise
    //Note:
    //Frequency(x axis) -> lacunarity ^ num
    //Amplitude(y axis) -> presistance ^ num
    //so this means:
    //Increase of lacunaruty = increase of small features
    //Increase of presistance = increase the amount that small features will effect our overall shape
    //The goal is to create a lot of unique maps so we do this by sampling our points from radically different locations
    //so we add seed in order to get the same map in case ew use the same seed 
    public static float[,] GenerateNoiseMap(int mapWidth, int mapHeight,int seed, float scale, int octives, float presistance, float lacunnarity, Vector2 offset, NormalizeMode normalizeMode)
    {
        //create 2d array for our noise map with size mapWidth and mapHeight
        float[,] noiseMap = new float[mapWidth, mapHeight];

        //prng = pseudo random number generator
        System.Random prng = new System.Random(seed);
        //Here we want each octive to be sampled from diferent locations so create hare an array of Vector2
        //and add them in the loop of octives
        Vector2[] octiveOffsets = new Vector2[octives];

        float maxPossibleHeight = 0;

        //Create frequency(y axis) and amplitude(x axis)
        float amplitude = 1;
        float frequency = 1;

        //Loop through our octives
        for (int i = 0; i < octives; i++)
        {
            //we dont want to give a mathf.perlin noise that its too high because 
            //it will give us the same output again again
            float offsetX = prng.Next(-100000, 100000) + offset.x;
            float offsetY = prng.Next(-100000, 100000) - offset.y;
            octiveOffsets[i] = new Vector2(offsetX, offsetY);

            maxPossibleHeight += amplitude;
            amplitude *= presistance;
        }

        //In case that our scale is equal to 0 we get a division error since we can devide a number with 0
        //so we handle that error by assign a lowest scale to our value if the scale is equal to 0
        if (scale <=0){
            scale = 0.0001f;
        }

        //Keep track of lowestand highest values
        float maxLocalNoiseHeight = float.MinValue;  
        float minLocalNoiseHeight = float.MaxValue;

        //These are used in order to zoom in the midle of the screen instead of the top right corner
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        //Loop through the array
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                //Create frequency(y axis) and amplitude(x axis)
                amplitude = 1;
                frequency = 1;
                float noiseHeight = 0;  //Keep track of our current height value

                for (int i = 0; i < octives; i++)
                {
                    //Create our sample coordinates: figure out at which point we are sampling from
                    float sampleX = (x-halfWidth + octiveOffsets[i].x) / scale * frequency ;
                    float sampleY = (y-halfHeight + octiveOffsets[i].y) / scale * frequency + octiveOffsets[i].y;


                    //so now because we have our sample coordinates 
                    float perlinNoiseValue = Mathf.PerlinNoise(sampleX, sampleY) * 2 - 1; //by *2-1 we make it able to get values from -1 to 1
                    
                    //Instead of setting up the noise map directly equal to the perlin value
                    //we increase the noise height by the perlin value of each octive
                    noiseHeight += perlinNoiseValue * amplitude;

                    amplitude *= presistance;   //At the end of each octive (it decreases)
                    frequency *= lacunnarity;   //At the end of each octive (it increases)
                }

                //Normalise height value so it will be in range of 0-1
                if (noiseHeight > maxLocalNoiseHeight){
                    maxLocalNoiseHeight = noiseHeight;
                }
                else if (noiseHeight <minLocalNoiseHeight){
                    minLocalNoiseHeight = noiseHeight;
                }
                noiseMap[x, y] = noiseHeight;   //Apply the noise height to height map
              
            }
        }

        
        //So now that we know what range our noise map values are in 
        //we want to loop through this map values again 
        for (int y = 0; y < mapHeight; y++){
            for (int x = 0; x < mapWidth; x++){
                //InverseLerp return a value between 0 and 1
                //eg. if our noise map value is equal to the min node height then it will return 0
                //if its equal to our max noise height then it will return 1
                //if is halfway between the 2 it would return 0.5 etc
                //-----------------------------------------------------------------
                //This effectively normalise out noiseMap

                if (normalizeMode == NormalizeMode.Local)
                {
                    noiseMap[x, y] = Mathf.InverseLerp(minLocalNoiseHeight, maxLocalNoiseHeight, noiseMap[x, y]);
                }
                else
                {
                    float normalizedHeight = (noiseMap[x, y] + 1) / (2f * maxPossibleHeight / 1.85f);
                    noiseMap[x, y] = Mathf.Clamp(normalizedHeight, 0, int.MaxValue);
                }
            }
        }

        return noiseMap; //return the noise map
    }
}
