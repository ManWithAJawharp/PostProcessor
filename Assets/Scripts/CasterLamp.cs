using UnityEngine;
using System.Collections;

public class CasterLamp : MonoBehaviour
{
    /////////////////////////
    //  Serialized Fields  //
    /////////////////////////

    [SerializeField]
    private GameObject player;
	[SerializeField]
	private GameObject quad;

    [SerializeField, Tooltip("How many frames have to have passed before another calculation is made.")]
    int regularity = 1;
    [SerializeField, Tooltip("Resolution of RenderTexture.")]
    private int resolution = 16;
    [SerializeField, Range(0, 1), Tooltip("Allowed ratio between between lit and unlit pixels on the player.")]
    private float treshold = 0.2f;

    /////////////////////////////
    //  Non-Serialized Fields  //
    /////////////////////////////

    private Shader replaceShader;

    /// <summary>
    /// Camera component on this object from which renderCamera will copy its properties.
    /// </summary>
	private Camera lampCamera;
    /// <summary>
    /// GameObject containing renderCamera.
    /// </summary>
	private GameObject renderCameraObject;
    /// <summary>
    /// Actual camera component that will do the rendering.
    /// </summary>
	private Camera renderCamera;
    /// <summary>
    /// Texture to render the image unto.
    /// </summary>
	private RenderTexture renderTex;
    /// <summary>
    /// Resulting readable texture created from the RenderTexture.
    /// </summary>
    private Texture2D resultImage;

	void OnEnable ()
	{
        Debug.Log("Enable " + name);

		replaceShader = Shader.Find("Argia & Iluna/Shadows/Shadow Mask");

		lampCamera = gameObject.AddComponent<Camera> ();
		lampCamera.orthographic = true;
		lampCamera.enabled = false;

		//  Create Mask Camera object and add Camera Component.
		renderCameraObject = new GameObject("Render Camera");
		renderCameraObject.hideFlags = HideFlags.HideAndDontSave;
		renderCameraObject.transform.parent = transform;

		renderCamera = renderCameraObject.AddComponent<Camera>();
		renderCamera.enabled = false;

        resultImage = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);
        resultImage.filterMode = FilterMode.Point;
        resultImage.wrapMode = TextureWrapMode.Clamp;
    }

	void LateUpdate()
	{
        LockOntoPlayer(player);

        player.GetComponent<Renderer>().material.color = Color.Lerp(Color.blue, Color.red, RenderLightView());
    }

    void OnPreRender ()
    {
        //Debug.Log("PreRender");
    }

	void OnDisable()
	{
		Destroy(renderCameraObject);
		//DestroyImmediate(mat);
	}

    /////////////////////////
    //  Private Functions  //
    /////////////////////////

    /// <summary>
    /// Position the lamp in front of the player and adjust the camera frustum.
    /// </summary>
    /// <param name="player">Reference to the player object.</param>
	private void LockOntoPlayer(GameObject player)
	{
		//  Set lamp in front of the player.
		transform.position = player.transform.position + (transform.position.y - player.transform.position.y) / transform.forward.y * transform.forward;

        //	Trim frustum down to player's bounding box.
        lampCamera.orthographicSize = Mathf.Abs(Vector3.Dot(transform.up.normalized, 0.5f * Vector3.up)) + 0.5f;
	}

    /// <summary>
    /// Renders the view as seen from the lamp object.
    /// </summary>
    private float RenderLightView ()
    {
        //  Initialize Render Textures.
        renderTex = RenderTexture.GetTemporary(resolution, resolution, 24);
        renderTex.filterMode = FilterMode.Point;
        renderTex.Create();

        //  Set replaceCamera's variables.
        renderCamera.CopyFrom(lampCamera);
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = Color.green;
        renderCamera.targetTexture = renderTex;

        //  Render 
        renderCamera.RenderWithShader(replaceShader, "RenderType");

        //  Transfer RenderTexture data to Texture2D.
        RenderTexture.active = renderTex;

        resultImage.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        resultImage.Apply();

        RenderTexture.active = null;

        //quad.GetComponent<Renderer>().material.mainTexture = resultImage;

        Color[] pixels = resultImage.GetPixels();

        int green = 0;
        int red = 0;

        foreach (Color pixel in pixels)
        {
            if (pixel.r < 1)
            {
                green++;
            }
            else if (pixel.g < 1)
            {
                red++;
            }
        }

        //player.GetComponent<Renderer>().material.color = Color.Lerp(Color.blue, Color.red, (float)red / (pixels.Length - green));

        //	Release and clear the RenderTextures.
        Camera.main.targetTexture = null;
        RenderTexture.ReleaseTemporary(renderTex);
        renderTex = null;

        return (float)red / (pixels.Length - green);
    }
}