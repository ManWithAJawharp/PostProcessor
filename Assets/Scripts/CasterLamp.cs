using UnityEngine;
using System.Collections;

public class CasterLamp : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
	[SerializeField]
	private GameObject quad;

    [SerializeField]
    private int resolution = 16;
    [SerializeField]
    private float treshold = 0.2f;

	private Shader replaceShader;

	private Camera lampCamera;
	private GameObject maskCamera;
	private Camera replaceCamera;
	private RenderTexture mask;
    private Texture2D resultImage;

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

        resultImage = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);
        resultImage.filterMode = FilterMode.Point;
        resultImage.wrapMode = TextureWrapMode.Clamp;
    }

	void FixedUpdate ()
	{
        LockOntoPlayer(player);

        //  Initialize Render Textures.
        mask = RenderTexture.GetTemporary(resolution, resolution, 24);
        mask.filterMode = FilterMode.Point;
        mask.Create();

        //  Set replaceCamera's variables.
        replaceCamera.CopyFrom(lampCamera);
        replaceCamera.clearFlags = CameraClearFlags.SolidColor;
        replaceCamera.backgroundColor = Color.white;
        replaceCamera.targetTexture = mask;

        //  Render Shadow Mask pass.
        replaceCamera.RenderWithShader(replaceShader, "RenderType");
        //replaceCamera.Render();

        RenderTexture.active = mask;

        resultImage.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        resultImage.Apply();

        RenderTexture.active = null;

        quad.GetComponent<Renderer>().material.mainTexture = resultImage;

        Color[] pixels = resultImage.GetPixels();
        //Debug.Log("Maximum amount of pixels to check:   " + pixels.Length);

        float output = 1;

        foreach (Color pixel in pixels)
        {
            output *= pixel.r;

            if (output < treshold)
            {
                Debug.Log("Player is visible.");
                break;
            }
        }

        if (output >= treshold)
        {
            Debug.Log("Player is not visible.");
        }

        //	Release and clear the RenderTextures.
        Camera.main.targetTexture = null;
        RenderTexture.ReleaseTemporary(mask);
        mask = null;
    }

    void OnPostRender ()
    {
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
        lampCamera.orthographicSize = Mathf.Abs(Vector3.Dot(transform.up.normalized, 0.5f * Vector3.up)) + 0.5f;
	}
}
