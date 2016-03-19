using UnityEngine;
using System.Collections;

public class Reticle : MonoBehaviour
{

    public Texture2D Tex;

    void OnGUI()
    {
        GUI.DrawTexture(new Rect((Screen.width - Tex.width) / 2, (Screen.height - Tex.height) / 2, Tex.width, Tex.height), Tex);
    }
}
