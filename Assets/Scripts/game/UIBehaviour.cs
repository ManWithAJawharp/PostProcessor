using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIBehaviour : MonoBehaviour
{
    //////////////////////////
    //  Serialized Fields   //
    //////////////////////////

    [SerializeField]
    RawImage fadeOverlay;

    [SerializeField]
    Text title;

    //////////////////////////////
    //  Non-Serialized Fields   //
    //////////////////////////////

    static UIBehaviour self;

    static Color blackAlpha0 = new Color(0, 0, 0, 0);
    static Color blackAlpha1 = new Color(0, 0, 0, 1);

    //////////////////
    //  Delegates   //
    //////////////////

    public delegate void CameraSwitchDelegate();
    public static event CameraSwitchDelegate OnFadedOut;

    //////////////////////////
    //  Built-in Functions  //
    //////////////////////////

    void Awake ()
    {
        State.OnStateChanged += State_OnStateChanged;
        Player.OnPlayerEnterShadow += PlayerController_OnPlayerEnterShadow;
        Player.OnPlayerExitShadow += PlayerController_OnPlayerExitShadow;

        self = this;

        //fadeOverlay.rectTransform.localScale = new Vector2(Screen.width, Screen.height);
        fadeOverlay.color = new Color(0, 0, 0, 1f);
    }

    //////////////////////////
    //  Built-in Functions  //
    //////////////////////////

    private void State_OnStateChanged(State.States previousState, State.States newState)
    {
        if (newState == State.States.StartLevel)
        {
            StartCoroutine("StartLevelCoroutine");
        }
        else if (newState == State.States.Play)
        {
            fadeOverlay.color = new Color(0, 0, 0, 0);
        }
    }

    private void PlayerController_OnPlayerEnterShadow(bool isLightPlayer)
    {
        //StopCoroutine("FadeCoroutine");
        //StartCoroutine("FadeCoroutine", new Color(0, 0, 0, 0f));
    }

    private void PlayerController_OnPlayerExitShadow(bool isLightPlayer)
    {
        //StopCoroutine("FadeCoroutine");
        //StartCoroutine("FadeCoroutine", new Color(1f, 0, 0, 0.5f));
    }

    //////////////////////////
    //  Public Functions    //
    //////////////////////////

    public static void SwitchCamera (float duration)
    {
        self.StopCoroutine("SwitchCameraCoroutine");

        self.StartCoroutine("SwitchCameraCoroutine", duration);
    }

    //////////////////
    //  Coroutines  //
    //////////////////

    IEnumerator StartLevelCoroutine ()
    {
        yield return new WaitForSeconds(1f);

        float t = 0f;

        while (t < 1f)
        {
            title.color = Color.Lerp(new Color(1, 1, 1, 0), Color.white, t);

            t += Time.deltaTime;

            yield return null;
        }

        yield return new WaitForSeconds(1f);

        t = 0f;

        while (t < 1f)
        {
            fadeOverlay.color = Color.Lerp(Color.black, new Color(0, 0, 0, 0), t);
            title.color = Color.Lerp(Color.white, new Color(1, 1, 1, 0), t);

            t += Time.deltaTime;
            yield return null;
        }

        fadeOverlay.color = new Color(0, 0, 0, 0);
        title.color = new Color(1, 1, 1, 0);

        State.Set(State.States.Play);

        yield return null;
    }

    IEnumerator FadeCoroutine (Color color)
    {
        Color startColor = fadeOverlay.color;

        float t = 0f;

        while (t < 1f)
        {
            fadeOverlay.color = Color.Lerp(startColor, color, t);

            t += Time.deltaTime;

            yield return null;
        }

        yield return null;
    }

    IEnumerator SwitchCameraCoroutine (float duration)
    {
        float t = 0f;

        while (t < 1f)
        {
            //Debug.Log("Fade Out -> t = " + t + ", t' = " + -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f);
            fadeOverlay.color = Color.Lerp(blackAlpha0, blackAlpha1, -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f);

            t += Time.deltaTime / duration * 2f;

            yield return null;
        }

        fadeOverlay.color = blackAlpha1;

        OnFadedOut();

        t = 0f;

        while (t < 1f)
        {
            //Debug.Log("Fade In -> t = " + t + ", t' = " + -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f);
            fadeOverlay.color = Color.Lerp(blackAlpha1, blackAlpha0, -(Mathf.Cos(Mathf.PI * t) - 1f) / 2f);

            t += Time.deltaTime / duration * 2f;

            yield return null;
        }

        fadeOverlay.color = blackAlpha0;

        yield return null;
    }
}
