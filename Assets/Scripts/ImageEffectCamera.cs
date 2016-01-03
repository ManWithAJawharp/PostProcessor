using UnityEngine;
using System.Collections;

public class ImageEffectCamera : MonoBehaviour
{
    public delegate void PreRenderEvent(bool isInShadow);
    public PreRenderEvent onPreRender;

    public delegate void RenderImageEvent(RenderTexture src, RenderTexture dst);
    public RenderImageEvent onRenderImage;

    private bool isInShadow = false;

    void Update ()
    {
        if (Physics.Raycast(transform.position, transform.forward, LayerMask.NameToLayer("Shadow")))
        {
            isInShadow = true;
        }
        else
        {
            isInShadow = false;
        }
    }

	void OnPreRender ()
    {
        if (onPreRender != null)
            onPreRender(isInShadow);
    }

    void OnRenderImage(RenderTexture src, RenderTexture dst)
    {
        if (onRenderImage != null)
            onRenderImage(src, dst);
    }
}
