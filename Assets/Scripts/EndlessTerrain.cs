using System.Collections;
using System.Collections.Generic;   //used to be able to modify the dictionary
using UnityEngine;

public class EndlessTerrain : MonoBehaviour
{
    const float scale = 5f; 

    const float viewerMoveThreshholdForChunkUpdate = 25f;
    const float squareViewerMoveThreshholdForChunkUpdate = viewerMoveThreshholdForChunkUpdate * viewerMoveThreshholdForChunkUpdate;

    public LODInfo[] detailLevels;
    public static float maxViewDistance;   // how far the viewer can see

    public Transform viewer;    //reference the viewer possition

    public Material mapMaterial;

    public static Vector2 viewerPosition; //save the viewer possition
    Vector2 viewerPositionOld;

    static MapGenerator mapGenerator;

    int chunkSize;  
    int chunkVisibleInViewDistance; //based on chunk size and chunk distance

    //create dictionary with key of vector 2 for the coordinates to the corresponding terrain chunk
    //in order to be able to know the chunks that have creted
    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();

    static List<TerrainChunk> terrainChuncksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        maxViewDistance = detailLevels[detailLevels.Length - 1].visibleDistanceThreadhold;
        chunkSize = MapGenerator.mapChunkSize - 1;  //fetch chunk size from map generator script
        chunkVisibleInViewDistance = Mathf.RoundToInt(maxViewDistance / chunkSize);

        UpdateVisibleChunks();
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale; //Update position every frame

        if((viewerPositionOld-viewerPosition).sqrMagnitude > squareViewerMoveThreshholdForChunkUpdate)
        {
            viewerPositionOld = viewerPosition;
            UpdateVisibleChunks();
        }

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
                if (terrainChunkDictionary.ContainsKey (viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize,detailLevels, transform, mapMaterial));
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

        MapData mapData;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        LODInfo[] detailLevels;
        LODMesh[] LODMeshes;
        int previousLODIndex = -1;

        bool mapDataReceived;

        public TerrainChunk(Vector2 coord, int size, LODInfo[]detailLevels, Transform parent,Material material)
        {
            this.detailLevels = detailLevels;

            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);    //possition in 3d space

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3 * scale;

            meshObject.transform.parent = parent;
            meshObject.transform.localScale = Vector3.one * scale; 
            SetVisible(false);

            LODMeshes = new LODMesh[detailLevels.Length];
            for (int i = 0; i < detailLevels.Length; i++)
            {
                LODMeshes[i] = new LODMesh(detailLevels[i].lod, UpdateTerrainChunk);
            }

            mapGenerator.RequestMapData(position, OnMapDataReceived);
        }

        void OnMapDataReceived(MapData mapData)
        {
            this.mapData = mapData;
            mapDataReceived = true;

            Texture2D texture = TextureGenerator.TextureFromColourMap(mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
            meshRenderer.material.mainTexture = texture;

            UpdateTerrainChunk();
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
            if (mapDataReceived)
            {
                float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
                bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;

                if (visible)
                {
                    int lodIndex = 0;

                    for (int i = 0; i < detailLevels.Length - 1; i++)
                    {
                        if (viewerDistanceFromNearestEdge > detailLevels[i].visibleDistanceThreadhold)
                        {
                            lodIndex = i + 1;
                        }
                        else
                        {
                            break;
                        }
                    }

                    if (lodIndex != previousLODIndex)
                    {
                        LODMesh LODMesh = LODMeshes[lodIndex];
                        if (LODMesh.hasMesh)
                        {
                            previousLODIndex = lodIndex;
                            meshFilter.mesh = LODMesh.mesh;
                        }
                        else if (!LODMesh.hasRequestMesh)
                        {
                            LODMesh.RequestMesh(mapData);
                        }
                    }

                    terrainChuncksVisibleLastUpdate.Add(this);
                }

                SetVisible(visible);
            }
           
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }

    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestMesh;
        public bool hasMesh;
        int lod;
        System.Action updateCallback;

        public LODMesh(int lod, System.Action updateCallback)
        {
            this.lod = lod;
            this.updateCallback = updateCallback;
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;

            updateCallback();
        }

        public void RequestMesh(MapData mapData)
        {
            hasRequestMesh = true;
            mapGenerator.RequestMeshData(mapData, lod, OnMeshDataReceived);
        }
    }

    [System.Serializable]
    public struct LODInfo
    {
        public int lod;
        public int visibleDistanceThreadhold;
    }
}
