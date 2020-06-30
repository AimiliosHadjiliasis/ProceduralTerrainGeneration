using System.Collections;
using System.Collections.Generic;   //used to be able to modify the dictionary
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    public const float maxViewDistance = 450;   // how far the viewer can see
    public Transform viewer;    //reference the viewer possition

    public static Vector2 viewPosition; //save the viewer possition

    int chunkSize;  
    int chunkVisibleInViewDistance; //based on chunk size and chunk distance

    //create dictionary with key of vector 2 for the coordinates to the corresponding terrain chunk
    //in order to be able to know the chunks that have creted
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();

    List<TerrainChunk> terrainChuncksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        chunkSize = MapGenerator.mapChunkSize - 1;  //fetch chunk size from map generator script
        chunkVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);
    }

    void Update()
    {
        viewPosition = new Vector2(viewer.position.x, viewer.position.z); //Update position every frame
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        //set to invisible
        for (int i = 0; i < terrainChuncksVisibleLastUpdate.Count; i++)
        {
            terrainChuncksVisibleLastUpdate[i].SetVisible(false);
        }
        terrainChuncksVisibleLastUpdate.Clear();    //clear the list

        //get the coordinate that the viewer is standing on
        int currentChuckCoordX = Mathf.RoundToInt(viewer.position.x / chunkSize);
        int currentChuckCoordY = Mathf.RoundToInt(viewer.position.y / chunkSize);

        //loop through suroundings
        for (int yOffset = -chunkVisibleInViewDistance; yOffset <= chunkVisibleInViewDistance; yOffset++){
            for (int xOffset = -chunkVisibleInViewDistance; xOffset <= chunkVisibleInViewDistance; xOffset++){
                //the coords of chanck that we see
                Vector2 viewedChunkCoord = new Vector2(currentChuckCoordX + xOffset, currentChuckCoordY + yOffset);

                //if the key exist(we have created the chunk) then open it
                //else instansiate that chunk by giving the key and then create the new chunk
                if (terrainChunkDictionary.ContainsKey (viewedChunkCoord)){
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                    if (terrainChunkDictionary[viewedChunkCoord].IsVisible())
                    {
                        terrainChuncksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]); //add to list if its visible
                    }
                }
                else{
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform));
                }
            }
        }

    }

    //a class that represent our terrain chunk object
    public class TerrainChunk
    {
        GameObject meshObject;  //Reference/Instansiate the mesh object
        Vector2 position;   //coorddiinates

        //In order to find the point that is closest to another point we used thge Bounds method that is provided by unity
        Bounds bounds;

        public TerrainChunk(Vector2 coord, int size, Transform parent)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);    //possition in 3d space

            meshObject = GameObject.CreatePrimitive(PrimitiveType.Plane);
            meshObject.transform.position = positionV3;
            meshObject.transform.localScale = Vector3.one * size / 10f; //change the scale
            meshObject.transform.parent = parent;
            SetVisible(false);
        }

        //fnction that sets vsible the meshObject
        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        //Update terain itself if we are in the distance 
        //if we are not in the distance the disables the terrain
        public void UpdateTerrainChunk()
        {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewPosition));
            bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;
            SetVisible(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }
}
