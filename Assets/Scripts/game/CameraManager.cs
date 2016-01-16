using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Keeps track of camera activity and movement.
/// </summary>
public class CameraManager : MonoBehaviour
{
    ///////////////////////////
    //  Serialized Fields    //
    ///////////////////////////

    /// <summary>Array of all available camera's.
    /// First entry is the initial camera.</summary>
    [SerializeField]
    [Tooltip("All available camera's.")]
    Camera[] cameraArray;

    /// <summary>A List of additional targets the active camera should keep in its frustrum.</summary>
    [SerializeField]
    [Tooltip("Additional targets the active camera will try to keep within its viewing frustrum.")]
    List<Transform> additionalTargets;

    [Header("Debugging")]

    /// <summary>If set to true, cameras can be switched with the arrow keys.</summary>
    [SerializeField]
    [Tooltip("If set to true, cameras can be switched with the arrow keys.")]
    bool debugSwitch = false;

    //////////////////////////////
    //  Non-Serialized Fields   //
    //////////////////////////////

    static Camera[] cameras;

    /// <summary>Index of the currently active Camera in 'cameras'.</summary>
    static int activeCameraIndex;
    /// <summary>Stores the index of the active camera after the screen fade.</summary>
    static int newActiveCameraIndex;
    /// <summary>Stores the index of the active camera before the screen fade.</summary>
    static int previousActiveCameraIndex;

    /// <summary>Generic List of subjects that are to be kept in the view frustrum.</summary>
    static List<Transform> subjects;

    /// <summary>The average positions of all targets on the camera's viewing plane.</summary>
    Vector3 averagePosition;

    //////////////////
    //  Properties  //
    //////////////////

    public static Camera ActiveCamera
    {
        get { return cameras[activeCameraIndex]; }
    }
    public static Camera PreviousActiveCamera
    {
        get { return cameras[previousActiveCameraIndex]; }
    }

    public static Camera[] CameraArray
    {
        get { return cameras; }
    }

    //////////////////
    //  Delegates   //
    //////////////////

    public delegate void SwitchCameraDelegate();
    public static event SwitchCameraDelegate OnSwitchCamera;

    //////////////////////////
    //  Built-in Functions  //
    //////////////////////////

    void Awake ()
    {
        State.OnStateChanged += State_OnStateChanged;
        UIBehaviour.OnFadedOut += UIBehaviour_OnFadedOut;

        cameras = cameraArray;

        //activeCamera = transform.FindChild("Camera1").GetComponent<Camera>();
        activeCameraIndex = 0;

        SetActiveCamera(activeCameraIndex);

        averagePosition = new Vector3();
	}

    void Start ()
    {
        
    }

	void Update ()
    {
        if (State.Current != State.States.Play) return;

        //  Find average projected position of subjects.
        averagePosition = Vector3.zero;

        foreach (Transform subject in subjects)
        {
            averagePosition += cameras[activeCameraIndex].WorldToViewportPoint(subject.position);
        }

        averagePosition /= subjects.Count;

        // Turn camera to the average position.
        Camera cam = cameras[activeCameraIndex];
        cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, Quaternion.LookRotation(cam.ViewportToWorldPoint(averagePosition) - cam.transform.position), Time.deltaTime * 5f);

        //  Switch cameras with the arrow keys.
        if (debugSwitch)
        {
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                SetPreviousActiveCamera(0.5f);
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                SetNextActiveCamera(0.5f);
            }

            if (Input.GetButton("SwitchCamera"))
            {
                SetNextActiveCamera(0.5f);
            }
        }
    }

    void OnDrawGizmos ()
    {
        if (debugSwitch)
            Gizmos.DrawSphere(averagePosition, 0.25f);
    }

    //////////////////////////
    //  Delegate Functions  //
    //////////////////////////

    private void State_OnStateChanged(State.States previousState, State.States newState)
    {
        if (newState == State.States.InitializeLevel)
        {
            subjects = new List<Transform>();
            subjects.Add(Game.player1.transform);
            subjects.Add(Game.player2.transform);
            subjects.AddRange(additionalTargets);
        }
    }

    private static void UIBehaviour_OnFadedOut()
    {
        previousActiveCameraIndex = activeCameraIndex;
        activeCameraIndex = newActiveCameraIndex;

        for (int i = 0; i < cameras.Length; i++)
        {
            if (i == activeCameraIndex)
            {
                cameras[i].gameObject.SetActive(true);
            }
            else
            {
                cameras[i].gameObject.SetActive(false);
            }
        }

        if (OnSwitchCamera != null) OnSwitchCamera();
    }

    //////////////////////////
    //  Public Functions    //
    //////////////////////////

    /// <summary>
    /// Set a new active Camera.
    /// </summary>
    /// <param name="index">Index of the new active Camera in 'cameras'.</param>
    /// <param name="duration">Duration of the fading effect. This causes a delay in the switching.</param>
    public static void SetActiveCamera (int index, float duration = 0f)
    {
        newActiveCameraIndex = index;

        if (duration > 0)
        {
            UIBehaviour.SwitchCamera(duration);
        }
        else
        {
            UIBehaviour_OnFadedOut();
        }
    }

    /// <summary>
    /// Set the next camera in 'cameras' to active.
    /// </summary>
    /// <param name="duration">Duration of the fading effect. This causes a delay in the switching.</param>
    public static void SetNextActiveCamera (float duration = 0f)
    {
        if (activeCameraIndex + 1 < cameras.Length)
            SetActiveCamera(activeCameraIndex + 1, duration);
        else
            SetActiveCamera(0, duration);
    }

    /// <summary>
    /// Set the previous camera in 'cameras' to active.
    /// </summary>
    /// <param name="duration">Duration of the fading effect. This causes a delay in the switching.</param>
    public static void SetPreviousActiveCamera (float duration = 0f)
    {
        if (activeCameraIndex - 1 >= 0)
            SetActiveCamera(activeCameraIndex - 1, duration);
        else
            SetActiveCamera(cameras.Length - 1, duration);
    }

    /// <summary>
    /// Adds a target to the list of camera subjects.
    /// </summary>
    /// <param name="target">The target to be added.</param>
    public static void AddTarget(Transform target)
    {
        subjects.Add(target);
    }

    /// <summary>
    /// Removes a target from the list of camera subjects.
    /// </summary>
    /// <param name="target">The target to be removed.</param>
    public static void RemoveTarget(Transform target)
    {
        subjects.Remove(target);
    }
}
