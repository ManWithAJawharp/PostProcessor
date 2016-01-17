using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The LightManager keeps a reference to al Lights that shine unto the players and renders an image of them to a texture.
/// The texture is then analysed to see whether anything is between the player and the light.
/// </summary>
public class LightManager : MonoBehaviour
{
    /////////////////////////
    //  Serialized Fields  //
    /////////////////////////

    [Header("Mode")]
    [SerializeField, Tooltip("If true, the result of the replaceTexture render will be shown (requires the debugQuad field to be set).")]
    private bool replaceShaderTestingMode = false;

    [Space, Header("Object References")]
    [SerializeField]
    private Player[] players = new Player[2];
    [SerializeField]
    private Light sun;
    [Space]
    [SerializeField]
    private GameObject debugQuad;

    [Space, Header("Render Settings")]
    [SerializeField, Tooltip("Time (s) to wait before RenderTexture is being read from gpu.")]
    float holdingTime = 0.05f;
    [SerializeField, Tooltip("Resolution of the rendered image.")]
    private int resolution = 16;

    /////////////////////////////
    //  Non-Serialized Fields  //
    /////////////////////////////

    /// <summary>Integer holding the index of the player that will be used for rendering next.</summary>
    private static int currentPlayerIndex;

    /// <summary>Generic List containing all Lights currently used in the scene.</summary>
    private static List<Light> lights;
    /// <summary>Integer holding the index of the Light that will be used for rendering next.</summary>
    private static int currentLightIndex;

    /// <summary>Camera component of this object from which renderCamera will copy its attributes.</summary>
    private static Camera masterCamera;

    private static GameObject renderCameraObject;
    private static Camera renderCamera;

    private static Shader replaceShader;
    
    /// <summary>RenderTexture to render the image unto.</summary>
    private RenderTexture renderTex;
    private static Texture2D resultImage;
    private static float timeHoldingTexture;

    private static float[] lightShadowRatios;

    public static float[] LightShadowRatios
    {
        get { return lightShadowRatios; }
    }

    //////////////////
    //  Properties  //
    //////////////////

    void Awake ()
    {
        State.OnStateChanged += State_OnStateChanged;

        lights = new List<Light>();
        AddLight(sun);
        currentLightIndex = 0;

        //  Create masterCamera.
        masterCamera = gameObject.AddComponent<Camera>();
        masterCamera.orthographic = true;
        masterCamera.enabled = false;   //  This Camera component will never be used for rendering.

        //  Create Camera object and add Camera Component.
        renderCameraObject = new GameObject("Render Camera");
        //renderCameraObject.hideFlags = HideFlags.HideAndDontSave;
        renderCameraObject.transform.parent = transform;

        renderCamera = renderCameraObject.AddComponent<Camera>();
        renderCamera.enabled = false;

        replaceShader = Shader.Find("Hidden/Light ReplaceShader");

        //  Initialize Texture2D, after reading the RenderTexture to this, the pixels can be read.
        resultImage = new Texture2D(resolution, resolution, TextureFormat.RGB24, false);
        resultImage.filterMode = FilterMode.Point;
        resultImage.wrapMode = TextureWrapMode.Clamp;
        
        timeHoldingTexture = 0f;

        lightShadowRatios = new float[] { 1, 0 };
    }

    void LateUpdate()
    {
        if (State.Current != State.States.Play) return;

        LockOntoPlayer(players[currentPlayerIndex]);

        if (renderTex == null)  //  Render an image if the RenderTexture is empty.
        {
            RenderLightView();

            if (holdingTime == 0f)
            {
                players[0].GetComponent<Renderer>().material.color = Color.Lerp(Color.blue, Color.red, CalculateLightShadowRatio());

                ReleaseTexture();
            }
        }
        else    //  Analyse the RenderTexture after it has been rendererd.
        {
            if (timeHoldingTexture >= holdingTime)
            {
                timeHoldingTexture = 0f;

                lightShadowRatios[currentPlayerIndex] = CalculateLightShadowRatio();
                //Debug.Log("Light " + currentLightIndex + "'s light to shadow ratio on player " + (currentPlayerIndex + 1) + ": " + lightShadowRatios[currentPlayerIndex]);

                ReleaseTexture();
            }

            timeHoldingTexture += Time.deltaTime;
        }
    }
    
    //////////////////////////
    //  Delegate Functions  //
    //////////////////////////

    private void State_OnStateChanged(State.States previousState, State.States newState)
    {
        
    }

    ////////////////////////
    //  Public Functions  //
    ////////////////////////

    public static void AddLight (Light light)
    {
        lights.Add(light);
    }

    public static void RemoveLight(Light light)
    {
        lights.Remove(light);
    }

    /////////////////////////
    //  Private Functions  //
    /////////////////////////

    /// <summary>Position the lamp in front of the player and adjust the camera frustum.</summary>
    /// <param name="player">Reference to the player object.</param>
	private void LockOntoPlayer(Player player)
    {
        //  Set directional light in front of the player.
        renderCameraObject.transform.position = player.transform.position
            + (lights[currentLightIndex].transform.position.y
            - player.transform.position.y) / lights[currentLightIndex].transform.forward.y
            * lights[currentLightIndex].transform.forward;
        renderCameraObject.transform.rotation = lights[currentLightIndex].transform.rotation;


        //	Trim frustum down to player's bounding box.
        masterCamera.orthographicSize = Mathf.Abs(Vector3.Dot(lights[currentLightIndex].transform.up.normalized, 0.5f * Vector3.up)) + 0.5f;
    }

    /// <summary>Renders the view as seen from the lamp object.</summary>
    private void RenderLightView()
    {
        //  Initialize Render Textures.
        renderTex = RenderTexture.GetTemporary(resolution, resolution, 24);
        renderTex.filterMode = FilterMode.Point;
        renderTex.Create();

        //  Set replaceCamera's variables.
        renderCamera.CopyFrom(masterCamera);
        LockOntoPlayer(players[currentPlayerIndex]);
        renderCamera.clearFlags = CameraClearFlags.SolidColor;
        renderCamera.backgroundColor = Color.green;
        renderCamera.targetTexture = renderTex;

        //  TODO Make non-rendered player temporarily invisible.

        //  Render 
        renderCamera.RenderWithShader(replaceShader, "RenderType");
        if (players[currentPlayerIndex].Respawning)
            Debug.Break();

        if (replaceShaderTestingMode)
            debugQuad.GetComponent<Renderer>().material.mainTexture = renderTex;

        //isHoldingTexture = true;
    }

    private float CalculateLightShadowRatio()
    {
        //  Transfer RenderTexture data to Texture2D by reading it to the active RenderTexture.
        RenderTexture.active = renderTex;

        resultImage.ReadPixels(new Rect(0, 0, resolution, resolution), 0, 0);
        resultImage.Apply();

        RenderTexture.active = null;

        //  Compute and return the ratio between shadow (yellow) and light (red) pixels on the player.
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

        return (float)red / (pixels.Length - green);
    }

    private void ReleaseTexture()
    {
        //	Release and clear the RenderTextures.
        Camera.main.targetTexture = null;
        RenderTexture.ReleaseTemporary(renderTex);
        renderTex = null;

        //isHoldingTexture = false;

        if (currentPlayerIndex < players.Length - 1)
        {
            currentPlayerIndex++;
        }
        else
        {
            currentPlayerIndex = 0;

            if (currentLightIndex < lights.Count - 1)
            {
                currentLightIndex++;
            }
            else
            {
                currentLightIndex = 0;
            }
        }
    }
}
