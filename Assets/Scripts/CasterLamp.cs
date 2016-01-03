using UnityEngine;
using System.Collections;

public class CasterLamp : MonoBehaviour
{
	[SerializeField]
	private GameObject quad;

	private Shader replaceShader;
	//private Material mat;

	private Camera lampCamera;
	private GameObject maskCamera;
	private Camera replaceCamera;
	private RenderTexture mask;

	void Awake ()
	{
		replaceShader = Shader.Find("Argia & Iluna/Shadows/Shadow Mask");

		lampCamera = gameObject.AddComponent<Camera> ();
		lampCamera.orthographic = false;
		lampCamera.enabled = false;

		//  Create Mask Camera object and add Camera Component.
		maskCamera = new GameObject("Mask Camera");
		maskCamera.hideFlags = HideFlags.HideAndDontSave;
		maskCamera.transform.parent = transform;

		replaceCamera = maskCamera.AddComponent<Camera>();
		replaceCamera.enabled = false;
	}

	void Update ()
	{
		//  Initialize Render Textures.
		mask = RenderTexture.GetTemporary(512, 512, 0, RenderTextureFormat.ARGBInt);
		//mask.filterMode = FilterMode.Point;
		mask.Create();

		//  Set replaceCamera's variables.
		replaceCamera.CopyFrom(lampCamera);
		replaceCamera.clearFlags = CameraClearFlags.SolidColor;
		replaceCamera.backgroundColor = Color.black;
		replaceCamera.renderingPath = RenderingPath.DeferredShading;
		replaceCamera.targetTexture = mask;

		//  Render Shadow Mask pass.
		replaceCamera.RenderWithShader(replaceShader, "RenderType");
		//replaceCamera.Render();

		quad.GetComponent<Renderer> ().material.mainTexture = mask;

		//	Release and clear the RenderTextures.
		RenderTexture.ReleaseTemporary(mask);
		mask = null;
	}

//	void OnRenderImage(RenderTexture src, RenderTexture dst)
//	{
//		mat.SetTexture("_ShadowMask", mask);
//
//		Graphics.Blit(src, dst, mat);
//
//		//  Release and clear the RenderTextures.
//		RenderTexture.ReleaseTemporary(mask);
//		mask = null;
//	}

	void OnDestroy()
	{
		DestroyImmediate(maskCamera);
		//DestroyImmediate(mat);
	}
}
