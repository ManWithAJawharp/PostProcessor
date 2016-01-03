using UnityEngine;
using System.Collections;

public class BlurEffect : MonoBehaviour
{
    private Material mat;

    void Start ()
    {
        mat = new Material(Shader.Find("Argia & Iluna/Image Effects/Blur Effect"));
    }

	void OnRenderImage (RenderTexture src, RenderTexture dst)
    {
        Graphics.Blit(src, dst, mat);
    }
}
