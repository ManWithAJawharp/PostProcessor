using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ImageEffect : MonoBehaviour
{
	[SerializeField, Range(0, 5)]
	private float intensity;
    [SerializeField, Range(4, 32)]
    private int iterations;

	private Material material;

	// Creates a private material used to the effect
	void OnEnable ()
	{
		material = new Material( Shader.Find("Hidden/ChromaticAberration") );
		Camera.main.depthTextureMode = DepthTextureMode.Depth;
	}

	[ExecuteInEditMode]
	void OnRenderImage (RenderTexture source, RenderTexture destination)
	{
		if (intensity == 0)
		{
			Graphics.Blit (source, destination);
			return;
		}

		material.SetFloat("_Intensity", intensity);
        material.SetInt("_Iterations", iterations);
		Graphics.Blit (source, destination, material);
	}
}