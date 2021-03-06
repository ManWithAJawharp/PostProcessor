﻿using UnityEngine;
using System.Collections;

[AddComponentMenu("Argia-Iluna/Image Effects/Edge Blur")]
public class EdgeBlur : MonoBehaviour
{
    public Color lightColor = Color.yellow;
    public Color lightGradColor = Color.red;
    public Color shadowGradColor = Color.red;
    public Color shadowColor = Color.blue;

    private Shader imageEffect;
    private Shader replaceShader;
    private Material mat;

    private GameObject maskCamera;
    private Camera replaceCamera;
    private RenderTexture mask;

    void Awake ()
    {
        imageEffect = Shader.Find("Argia & Iluna/Image Effects/Edge Blur");
        replaceShader = Shader.Find("Argia & Iluna/Shadows/Shadow Mask");
        mat = new Material(imageEffect);

        mat.SetColor("_Light", lightColor);
        mat.SetColor("_LightGrad", lightGradColor);
        mat.SetColor("_ShadowGrad", shadowGradColor);
        mat.SetColor("_Shadow", shadowColor);

        //  Create Mask Camera object and add Camera Component.
        maskCamera = new GameObject("Mask Camera");
        maskCamera.hideFlags = HideFlags.HideAndDontSave;
        replaceCamera = maskCamera.AddComponent<Camera>();
        replaceCamera.enabled = false;
    }

    void OnPreRender()
    {
        //  Initialize Render Textures.
        mask = RenderTexture.GetTemporary(Screen.width, Screen.height, 0, RenderTextureFormat.ARGBInt);
        mask.Create();

        //  Set replaceCamera's variables.
        replaceCamera.CopyFrom(GetComponent<Camera>());
        replaceCamera.clearFlags = CameraClearFlags.SolidColor;
        replaceCamera.backgroundColor = Color.black;
        replaceCamera.renderingPath = RenderingPath.DeferredShading;
        replaceCamera.targetTexture = mask;

        //  Render Shadow Mask pass.
        replaceCamera.RenderWithShader(replaceShader, "RenderType");
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        mat.SetTexture("_ShadowMask", mask);

        Graphics.Blit(src, dst, mat);

        //  Release and clear the RenderTextures.
        RenderTexture.ReleaseTemporary(mask);
        mask = null;
    }

    void OnDisable()
    {
        DestroyImmediate(maskCamera);
        DestroyImmediate(mat);
    }
}
