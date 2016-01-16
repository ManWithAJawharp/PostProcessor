using UnityEngine;
using System.Collections;

public class Trigger : MonoBehaviour
{
    [SerializeField]
    GameObject actuator;

    [SerializeField]
    bool triggerAnimator = true;
    [SerializeField]
    bool triggerAudio = false;
    [SerializeField]
    bool triggerCameraSwitch = false;

    [SerializeField]
    int nextCamera;

    new Collider collider;

    Animator animator;
    new AudioSource audio;

	void Awake ()
    {
        State.OnStateChanged += State_OnStateChanged;
    }

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (triggerAnimator) animator.Play("Main");
            if (triggerAudio) audio.Play();

            if (triggerCameraSwitch) CameraManager.SetActiveCamera(nextCamera, 0.5f);

            Destroy(gameObject);
        }
    }

    private void State_OnStateChanged(State.States previousState, State.States newState)
    {
        if (newState == State.States.InitializeLevel)
        {
            if (actuator == null) Debug.LogWarning("There is no actuator for this trigger (" + name + ")!");
            else
            {
                if (triggerAnimator) animator = actuator.GetComponent<Animator>();
                if (triggerAudio) audio = actuator.GetComponent<AudioSource>();
            }

            collider = GetComponent<Collider>();

            if (collider == null)
            {
                collider = gameObject.AddComponent<BoxCollider>();
            }
        }
    }
}
