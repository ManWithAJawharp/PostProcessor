using UnityEngine;
using System.Collections;

[AddComponentMenu("Argia-Iluna/Image Effects/Shadow Highlighting")]
public class ShadowHighlight : MonoBehaviour
{
    public Shader shader;

    void Start ()
    {
        GetComponent<Camera>().CopyFrom(Camera.main);
        GetComponent<Camera>().RenderWithShader(shader, "RenderType");
    }
	
	void OnRenderImage (RenderTexture source, RenderTexture destination)
    {
        
	}
}
