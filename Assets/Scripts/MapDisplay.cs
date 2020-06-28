using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
   
    public Renderer textureRenderer;    //Reference to renderer of the plane

    //Function that draw the texture to the screen
    public void DrawTexture(Texture2D texture)
    {
        textureRenderer.sharedMaterial.mainTexture = texture; //Apply texture to texture renderer (since we dont want to access the game every time)
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height); //set sixe of our renderer as the size of out map
    }
}
