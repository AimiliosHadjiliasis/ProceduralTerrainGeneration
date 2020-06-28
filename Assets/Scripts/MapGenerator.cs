using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    //Create enum to determine in the inspector which mode we want to draw
    public enum DrawMode
    {
        NoiseMap,
        ColourMap
    };

    public DrawMode drawMode; 

    public int mapWidth;    //initise the mapWidth
    public int mapHeight;   //initise the mapHeight
    public float noiseScale;

    public int octives; 
    [Range (0,1)]   // Turn presistance to slider
    public float presistance;
    public float lacunarity;

    //initialise seed and offset
    public int seed;
    public Vector2 offset;
    
    //initialise bool to be able to update the map autoamtically every time we add a value in the inspector
    public bool autoUpdate; 

    public TerrainType[] regions;   //Create array of terrain types

    //Function that genereates the Map
    public void GenerateMap()
    {
        //fetching Noisemap from noise class
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, noiseScale, octives, presistance,lacunarity, offset);

        Color[] colourMap = new Color[mapWidth * mapHeight];    //Create colour map to save all colour in this array

        //Loop through the noise map that we receive
        for (int y = 0; y < mapHeight; y++){
            for (int x = 0; x < mapWidth; x++){
                //So the height in this point is equal to the coordinates of our noise map at cords x and y
                float currenHeight = noiseMap[x, y];

                //Loop throug all regions
                for (int i = 0; i < regions.Length; i++)
                {
                    //if current height falls to the region then this means that we have find the region
                    if (currenHeight <= regions[i].height)
                    {
                        colourMap[y * mapWidth + x] = regions[i].colour; //save the colour for this point
                        break;  //so we dont need to check the other regions
                    }
                }
            }
        }

        MapDisplay display = FindObjectOfType<MapDisplay>();    //Get reference to map display

        //Determine which drawMode we want to draw
        if (drawMode == DrawMode.NoiseMap){
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));   //Draw texture of noiseMap
        }
        else if (drawMode == DrawMode.ColourMap){
            display.DrawTexture(TextureGenerator.TextureFromColourMap(colourMap,mapWidth,mapHeight));   //Draw texture of colourMap
        }
    }

    //Keep the width and height always bigger than 0
    //Also keep lacunarity to always less than 1
    //And numebr of octives always bigger than 0
    void OnValidate()
    {
        if (mapWidth < 1){
            mapWidth = 1;
        }
        if (mapHeight < 1){
            mapHeight = 1;
        }
        if (lacunarity < 1){
            lacunarity = 1;
        }
        if (octives < 0 ){
            octives = 0;
        }
    }
}

[System.Serializable]   //Show in the inspector
public struct TerrainType
{
    public string name;
    public float height;
    public Color colour;
}