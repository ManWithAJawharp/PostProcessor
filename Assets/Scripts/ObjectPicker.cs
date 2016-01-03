using UnityEngine;
using System.Collections;

[AddComponentMenu("Argia-Iluna/Image Effects/Object Picker")]
public class ObjectPicker : MonoBehaviour
{
    public Shader imageEffectShader;
    public Shader replaceShader;

    public Color lightColor = new Color(1, 1, 1, 1);
    public Color shadowColor = new Color(1, 1, 1, 1);

    private Material mat;

    private GameObject maskCamera;
    private Camera replaceCamera;
    private RenderTexture mask;
    private RenderTexture secondPass;

	void Start ()
    {
        mat = new Material(imageEffectShader);
        mat.SetColor("_LightColor", lightColor);
        mat.SetColor("_ShadowColor", shadowColor);

        //  Create Mask Camera object and add Camera Component.
        maskCamera = new GameObject("Mask Camera");
        maskCamera.hideFlags = HideFlags.HideAndDontSave;
        replaceCamera = maskCamera.AddComponent<Camera>();
        replaceCamera.enabled = false;
    }

    void OnPreRender()
    {
        //  Initialize Render Textures/
        mask = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBInt);
        mask.Create();

        secondPass = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBInt);
        secondPass.Create();
        
        //  Set replaceCamera's variables.
        replaceCamera.CopyFrom(GetComponent<Camera>());
        replaceCamera.clearFlags= CameraClearFlags.SolidColor;
        replaceCamera.backgroundColor = Color.black;
        replaceCamera.renderingPath = RenderingPath.DeferredShading;
        replaceCamera.targetTexture = mask;

        //  Render Shadow Mask pass.
        replaceCamera.RenderWithShader(replaceShader, "RenderType");
    }

    void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(mask, secondPass, mat, 0);

        mat.SetTexture("_ShadowMask", secondPass);
        Graphics.Blit(source, destination, mat, 1);

        //  Release and clear the RenderTextures.
        RenderTexture.ReleaseTemporary(mask);
        mask = null;
        RenderTexture.ReleaseTemporary(secondPass);
        secondPass = null;
    }

    void OnDisable()
    {
        DestroyImmediate(maskCamera);
        DestroyImmediate(mat);
    }
}
