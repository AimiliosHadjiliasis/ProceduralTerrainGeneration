using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;   //used to get access to actions
using System.Threading; //used to use threads

public class MapGenerator : MonoBehaviour
{
    //Create enum to determine in the inspector which mode we want to draw
    public enum DrawMode
    {
        NoiseMap,
        ColourMap,
        Mesh
    };

    public DrawMode drawMode;

    public Noise.NormalizeMode normalizeMode;

    //ADD SUPPORT FOR MULTIPLE MESH RESOLUTION
    //We use chunk size of 241, this is because unity supports meshes that has a number of vertices
    //v=sqr(w) <= 255^2 = 65025
    //We use the number 241 since we have w-1 = 240 and 240 is dividable with all odd numbers from 2-12
    //which give us a nice way to work with our i 
    //(also) map width and map height is replaced with that mapChunk variable
    public const int mapChunkSize = 241;
    [Range(0,6)] //limit lod to 0-6 so we can only multiply by 2 to get the value of the increment
    public int editorPreviewLOD;

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

    public float meshHeightMultiplier; //used to multiply the y and give height to the scene objects
    public AnimationCurve meshHeightCurve;  //Create curve to determine how the different height levels should affected by the height multiplier

    public TerrainType[] regions;   //Create array of terrain types


    Queue<MapThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<MapThreadInfo<MapData>>();
    Queue<MapThreadInfo<MeshData>> meshDataThreadInfoQueue = new Queue<MapThreadInfo<MeshData>>();
    


    public void DrawMapInEditor()
    {
        //create a variable to hold the generation of map
        MapData mapData = GenerateMapData(Vector2.zero);
        
        //Get reference to map display
        MapDisplay display = FindObjectOfType<MapDisplay>();    

        //Determine which drawMode we want to draw
        if (drawMode == DrawMode.NoiseMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(mapData.heighMap));   //Draw texture of noiseMap
        }
        else if (drawMode == DrawMode.ColourMap)
        {
            display.DrawTexture(TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));   //Draw texture of colourMap
        }
        else if (drawMode == DrawMode.Mesh)
        {
            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(mapData.heighMap, meshHeightMultiplier, meshHeightCurve, editorPreviewLOD), TextureGenerator.TextureFromColourMap(mapData.colourMap, mapChunkSize, mapChunkSize));
        }
    }

    public void RequestMapData(Vector2 centre, Action<MapData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MapDataThread(centre, callback);
        };

        new Thread(threadStart).Start();
    }

    void MapDataThread(Vector2 centre, Action<MapData> callback)
    {
        MapData mapData = GenerateMapData(centre);
        lock (mapDataThreadInfoQueue)
        {
            mapDataThreadInfoQueue.Enqueue(new MapThreadInfo<MapData>(callback, mapData));
        }
    }

    public void RequestMeshData(MapData mapData, int lod, Action<MeshData> callback)
    {
        ThreadStart threadStart = delegate
        {
            MeshDataThread(mapData, lod, callback);
        };

        new Thread(threadStart).Start();
    }

    void MeshDataThread(MapData mapData, int lod, Action<MeshData> callback)
    {
        MeshData meshData = MeshGenerator.GenerateTerrainMesh(mapData.heighMap, meshHeightMultiplier, meshHeightCurve, lod);
        lock (meshDataThreadInfoQueue)
        {
            meshDataThreadInfoQueue.Enqueue(new MapThreadInfo<MeshData>(callback, meshData));
        }
    }

    void Update()
    {
        if (mapDataThreadInfoQueue.Count >0)
        {
            for (int i = 0; i < mapDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }

        if (meshDataThreadInfoQueue.Count > 0 )
        {
            for (int i = 0; i < meshDataThreadInfoQueue.Count; i++)
            {
                MapThreadInfo<MeshData> threadInfo = meshDataThreadInfoQueue.Dequeue();
                threadInfo.callback(threadInfo.parameter);
            }
        }
    }

    //Function that genereates the Map data
    MapData GenerateMapData(Vector2 centre)
    {
        //fetching Noisemap from noise class
        float[,] noiseMap = Noise.GenerateNoiseMap(mapChunkSize, mapChunkSize, seed, noiseScale, octives, presistance,lacunarity, centre + offset, normalizeMode);

        Color[] colourMap = new Color[mapChunkSize * mapChunkSize];    //Create colour map to save all colour in this array

        //Loop through the noise map that we receive
        for (int y = 0; y < mapChunkSize; y++){
            for (int x = 0; x < mapChunkSize; x++){
                //So the height in this point is equal to the coordinates of our noise map at cords x and y
                float currenHeight = noiseMap[x, y];

                //Loop throug all regions
                for (int i = 0; i < regions.Length; i++)
                {
                    //if current height falls to the region then this means that we have find the region
                    if (currenHeight >= regions[i].height)
                    {
                        colourMap[y * mapChunkSize + x] = regions[i].colour; //save the colour for this point
                    }
                    else
                    {
                        break;  //so we dont need to check the other regions
                    }
                }
            }
        }

        return new MapData(noiseMap, colourMap);
    }

    //Keep the width and height always bigger than 0
    //Also keep lacunarity to always less than 1
    //And numebr of octives always bigger than 0
    void OnValidate()
    {

        if (lacunarity < 1){
            lacunarity = 1;
        }
        if (octives < 0 ){
            octives = 0;
        }
    }

    struct MapThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public MapThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
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

//Stuct that holds the height map and the colour map of the map
//This one is used to the DrawMapInEditor function 
public struct MapData
{
    public readonly float[,] heighMap;
    public readonly Color[] colourMap;

    //Constructor to initialise the colour and the height data
    public MapData(float[,] heighMap, Color[] colourMap)
    {
        this.heighMap = heighMap;
        this.colourMap = colourMap;
    }
}