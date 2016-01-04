using UnityEngine;
using System.Collections;

public class CasterLamp : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
	[SerializeField]
	private GameObject quad;

	private Shader replaceShader;

	private Camera lampCamera;
	private GameObject maskCamera;
	private Camera replaceCamera;
	private RenderTexture mask;

	void OnEnable ()
	{
        Debug.Log("Enable " + name);

		replaceShader = Shader.Find("Argia & Iluna/Shadows/Shadow Mask");

		lampCamera = gameObject.AddComponent<Camera> ();
		lampCamera.orthographic = true;
		lampCamera.enabled = false;

		//  Create Mask Camera object and add Camera Component.
		maskCamera = new GameObject("Mask Camera");
		maskCamera.hideFlags = HideFlags.HideAndDontSave;
		maskCamera.transform.parent = transform;

		replaceCamera = maskCamera.AddComponent<Camera>();
		replaceCamera.enabled = false;
	}

	void FixedUpdate ()
	{
		LockOntoPlayer (player);

		//  Initialize Render Textures.
		mask = RenderTexture.GetTemporary(16, 16, 16, RenderTextureFormat.ARGBInt);
	    mask.filterMode = FilterMode.Point;
		mask.Create();

		//  Set replaceCamera's variables.
		replaceCamera.CopyFrom(lampCamera);
		replaceCamera.clearFlags = CameraClearFlags.SolidColor;
		replaceCamera.backgroundColor = Color.black;
		replaceCamera.renderingPath = RenderingPath.Forward;
		replaceCamera.targetTexture = mask;

		//  Render Shadow Mask pass.
		replaceCamera.RenderWithShader(replaceShader, "RenderType");
		//replaceCamera.Render();

		quad.GetComponent<Renderer> ().material.mainTexture = mask;

		//	Release and clear the RenderTextures.
		RenderTexture.ReleaseTemporary(mask);
		mask = null;
	}

	void OnDisable()
	{
		DestroyImmediate(maskCamera);
		//DestroyImmediate(mat);
	}

	private void LockOntoPlayer(GameObject player)
	{
		//  Set lamp in front of the player.
		transform.position = player.transform.position + (transform.position.y - player.transform.position.y) / transform.forward.y * transform.forward;

		//	Trim frustrum down to player's bounding box.
		//Debug.Log (transform.eulerAngles.x);
		//lampCamera.orthographicSize = 1f / Mathf.Cos(90 - transform.eulerAngles.x);
		lampCamera.orthographicSize = Mathf.Max(0.8f, Vector3.Dot(transform.up, Vector3.up));
	}
}
