using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
   
    public Renderer textureRenderer;    //Reference to renderer of the plane

    public MeshFilter meshFilter;   //reference to the mesh filter
    public MeshRenderer meshRenderer;   //reference to the renderer of the mesh

    //Function that draw the texture to the screen
    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture; //Apply texture to texture renderer (since we dont want to access the game every time)
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height); //set sixe of our renderer as the size of out map
    }

    //function that draws the mesh of the terrain
    public void DrawMesh(MeshData meshData, Texture2D texture)
    {
        meshFilter.sharedMesh = meshData.CreateMesh();
        meshRenderer.sharedMaterial.mainTexture = texture;
    }
}
