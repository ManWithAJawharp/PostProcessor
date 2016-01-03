using UnityEngine;
using System.Collections;

public class CasterLamp : MonoBehaviour
{
	private Camera camera;
	private Material material;

	// Creates a private material used to the effect
	void OnEnable ()
	{
		camera = gameObject.AddComponent<Camera> ();
		camera.orthographic = true;

		material = new Material( Shader.Find("Hidden/PlayerDiscriminatorShader") );
		camera.depthTextureMode = DepthTextureMode.Depth;
	}

	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		Graphics.Blit (source, destination, material);
	}
}
