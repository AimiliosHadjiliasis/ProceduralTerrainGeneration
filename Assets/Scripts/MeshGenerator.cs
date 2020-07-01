using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public static class MeshGenerator 
{
    public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, AnimationCurve height_Curve, int levelOfDetail)
    {

        AnimationCurve heightCurve = new AnimationCurve(height_Curve.keys);
        //Figure out the width and height of the map
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);

        //make the mesh data (x,y) perfectly in the middle
        //in order to create 2 new positions for x and y
        //with the use of the following formula
        //x=(w-1)/2 and the same for the z axis
        float topLeftX = (width - 1) / -2f;
        float topLeftZ = (height - 1) / 2f;

        int meshSimplificationIncrement = (levelOfDetail == 0) ? 1: levelOfDetail * 2;
        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1; //figure out the number of vertices per line

        MeshData meshData = new MeshData(verticesPerLine, verticesPerLine);
        int vertexIndex = 0;

        //Loop through height map
        for (int y = 0; y < height; y+= meshSimplificationIncrement){
            for (int x = 0; x < width; x+=meshSimplificationIncrement){
                
                //Create vertices
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x,heightCurve.Evaluate(heightMap[x,y]) *  heightMultiplier, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height); //create a persentage to see where the uvs are located

                //Setting triangles 
                if (x < width - 1 && y < height -1 )
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1); 
                }

                vertexIndex++;
            }
        }

        //returning data instead of mesh itself because we are using threding so the environment
        //wont freeze up as the game loads more places.
        //This is becauuse unity sets as a limitation to pass the data of themesh in order to be 
        //created instead of passing the mesh itself
        return meshData;
    }
}

//Create a class that will be used as the data that we will use for the mesh
public class MeshData
{
    public Vector3[] vertices;
    public int[] triangles;
    public Vector2[] uvs;   //create uv map to apply our data

    int triangleIndex;  //keep track of triangle index

    public MeshData(int meshWidth, int meshHeight)
    {
        vertices = new Vector3[meshWidth * meshHeight]; //size of vertices array: v=w*h
        uvs = new Vector2[meshWidth * meshHeight];  
        triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];    //size of triangles array (amount of squares): t=(w-1)(h-1)*6
    }

    //Method for adding triangles
    public void AddTriangle(int a, int b, int c)
    {
        triangles[triangleIndex] = a;
        triangles[triangleIndex+1] = b;
        triangles[triangleIndex+2] = c;
        triangleIndex += 3; //increment by 3 so we move to next triangle
    }

    //Methods that getting the mesh from the mesh data
    public Mesh CreateMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals(); //used for lightning in order to work nicely
        return mesh;
    }
}