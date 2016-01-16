using UnityEngine;
using System.Collections;

public class Game : MonoBehaviour
{
    //////////////////////////
    //  Serialized Fields   //
    //////////////////////////

    /// <summary>Log debug information foor the global state machine?</summary>
    [SerializeField]
    [Tooltip("Log debug information foor the global state machine?")]
    bool printStateDebug = false;

    /// <summary>Play intro scene?</summary>
    [SerializeField]
    bool playIntro = true;

    [Header("Post Processing")]

    /// <summary>Switch for enabling/disabling post processing functions.</summary>
    [SerializeField]
    [Tooltip("")]
    bool enablePostProcessing = true;
    /// <summary>Color of plain light.</summary>
    [SerializeField]
    [Tooltip("Color of plain light.")]
    Color lightColor = new Color(0.809f, 0.758f, 0.621f, 0f);
    /// <summary>Color of light at edges.</summary>
    [SerializeField]
    [Tooltip("Color of light at edges.")]
    Color lightGradColor = new Color(1f, 0.668f, 0.531f, 0f);
    /// <summary>Color of shadow at edges.</summary>
    [SerializeField]
    [Tooltip("Color of shadow at edges.")]
    Color shadowGradColor = new Color(0.340f, 0.195f, 0.621f, 0f);
    /// <summary>Color of plain shadow.</summary>
    [SerializeField]
    [Tooltip("Color of plain shadow.")]
    Color shadowColor = new Color(0.563f, 0.594f, 0.738f, 0f);

    //////////////////////////////
    //  Non-Serialized Fields   //
    //////////////////////////////

    /// <summary>The players.</summary>
    static Player[] players;

    /// <summary>The image effect shader program.</summary>
    private Shader imageEffect;
    /// <summary>The replace shader program.</summary>
    private Shader replaceShader;
    /// <summary>The material to apply the image effect to.</summary>
    private Material material;

    /// <summary>Currently active camera as set by the camera manager.</summary>
    private Camera activeCamera;
    /// <summary>Game Object to hold the replacecamera.</summary>
    private GameObject maskCamera;
    /// <summary>The replacecamera component.</summary>
    private Camera replaceCamera;
    /// <summary>RenderTexture to render the replacecamera's pass unto.</summary>
    private RenderTexture maskTexture;
    private RenderTexture inShadowTexture;

    //////////////////
    //  Properties  //
    //////////////////

    /// <summary>Player 1.</summary>
    public static Player player1
    {
        get { return players[0]; }
    }
    /// <summary>Player 2.</summary>
    public static Player player2
    {
        get { return players[1]; }
    }

    //////////////////////////
    //  Built-in Functions  //
    //////////////////////////

    void Awake ()
    {
        State.OnStateChanged += State_OnStateChanged;

        State.PrintDebug = printStateDebug;

        DontDestroyOnLoad(gameObject);
    }

    void Start ()
    {
        State.Set(State.States.LoadScene, "Loading the scene.");
    }

    //////////////////////////
    //  Delegate Functions  //
    //////////////////////////

    private void State_OnStateChanged(State.States previousState, State.States newState)
    {
        if (newState == State.States.LoadScene)
        {
            players = FindObjectsOfType<Player>();

            /*  Post-processing.  */

            foreach (Camera camera in CameraManager.CameraArray)
            {
                camera.GetComponent<ImageEffectCamera>().onPreRender += ImageEffectCamera_OnPreRender;
                camera.GetComponent<ImageEffectCamera>().onRenderImage += ImageEffectCamera_OnRenderImage;
            }

            imageEffect = Shader.Find("Argia & Iluna/Image Effects/Edge Blur");
            replaceShader = Shader.Find("Argia & Iluna/Shadows/Shadow Mask");
            material = new Material(imageEffect);

            material.SetColor("_Light", lightColor);
            material.SetColor("_LightGrad", lightGradColor);
            material.SetColor("_ShadowGrad", shadowGradColor);
            material.SetColor("_Shadow", shadowColor);

            //  Create Mask Camera object and add Camera Component.
            maskCamera = new GameObject("Mask Camera");
            maskCamera.hideFlags = HideFlags.HideAndDontSave;
            replaceCamera = maskCamera.AddComponent<Camera>();
            replaceCamera.enabled = false;

            //inShadowTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBInt);

            //Color fillColor = new Color(1, 0, 0);
            //var fillColorArray = tex2.GetPixels();

            //for (var i = 0; i < fillColorArray.Length; ++i)
            //{
            //    fillColorArray[i] = fillColor;
            //}

            //tex2.SetPixels(fillColorArray);

            //tex2.Apply();

            /*  Setting next state. */

            State.Set(State.States.InitializeLevel, "Initializing Level.");
        }
        else if (newState == State.States.InitializeLevel)
        {
            if (playIntro) State.Set(State.States.StartLevel, "Start intro.");
            else State.Set(State.States.Play, "Skipped intro, start playing.");
        }
    }

    private void ImageEffectCamera_OnPreRender (bool isInShadow)
    {
        if (!enablePostProcessing) return;

        //  Initialize Render Textures.
        maskTexture = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBInt);
        maskTexture.Create();

        //if (isInShadow)
        //{
        //    RenderTexture.active = maskTexture;
        //    GL.Begin(GL.TRIANGLES);
        //    GL.Clear(true, true, new Color(0f, 0f, 0f, 0));
        //    GL.End();
        //}
        //else
        //{
            //  Set replaceCamera's variables.
            replaceCamera.CopyFrom(CameraManager.ActiveCamera);
            replaceCamera.clearFlags = CameraClearFlags.SolidColor;
            replaceCamera.backgroundColor = Color.black;
            replaceCamera.renderingPath = RenderingPath.DeferredShading;
            replaceCamera.targetTexture = maskTexture;

            //  Render Shadow Mask pass.
            replaceCamera.RenderWithShader(replaceShader, "RenderType");
        //}
    }

    private void ImageEffectCamera_OnRenderImage (RenderTexture src, RenderTexture dst)
    {
        if (!enablePostProcessing) return;

        material.SetTexture("_ShadowMask", maskTexture);

        Graphics.Blit(src, dst, material);

        //  Release and clear the RenderTextures.
        RenderTexture.ReleaseTemporary(maskTexture);
        maskTexture = null;
    }
}
