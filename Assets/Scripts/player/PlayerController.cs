using UnityEngine;

[AddComponentMenu("Argia-Iluna/Player Scripts/Player Controller")]
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [SerializeField]
    Camera cam;

    [SerializeField]
    bool isLightPlayer = true;

    float walkSpeed = 6.7f;

    CharacterController controller;

    bool wasInShadow;
    bool isInShadow;

    public delegate void CollisionDelegate(bool isLightPlayer);
    public static event CollisionDelegate OnPlayerEnterShadow;
    public static event CollisionDelegate OnPlayerExitShadow;

    void Start ()
    {
        cam.tag = "MainCamera";

        controller = GetComponent<CharacterController>();

        wasInShadow = isLightPlayer;
        isInShadow = isLightPlayer;
	}
	
	void Update ()
    {
        // Turn camera to player
        cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, Quaternion.LookRotation(transform.position - cam.transform.position), 0.1f);

        //  Reset the velocity vector.
        Vector3 velocity = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
        {
            velocity += Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1));
        }
        else if (Input.GetKey(KeyCode.S))
        {
            velocity -= Vector3.Scale(cam.transform.forward, new Vector3(1, 0, 1));
        }

        if (Input.GetKey(KeyCode.A))
        {
            velocity -= cam.transform.right;
        }
        else if (Input.GetKey(KeyCode.D))
        {
            velocity += cam.transform.right;
        }

        velocity = velocity.normalized * walkSpeed * Time.deltaTime;

        velocity.y -= 9.81f * Time.deltaTime;

        controller.Move(velocity);
	}

    void FixedUpdate ()
    {
        if (isInShadow)
        {
            if (wasInShadow)
            {

            }
            else
            {
                OnPlayerEnterShadow(isLightPlayer);
                Debug.Log("Enter shadow");

                wasInShadow = true;
            }
        }
        else
        {
            if (wasInShadow)
            {
                OnPlayerExitShadow(isLightPlayer);
                Debug.Log("Exit shadow");

                wasInShadow = false;
            }
            else
            {
                
                
            }
        }

        isInShadow = false;
    }

    void OnTriggerEnter(Collider col)
    {
        //if (col.gameObject.layer == LayerMask.NameToLayer("Shadow"))
        //{
        //    //Debug.Log("Entering shadow.");

        //    if (isLightPlayer)
        //    {
        //        if (StartHurting != null) StartHurting(isLightPlayer);
        //    }
        //    else
        //    {
        //        if (EndHurting != null) EndHurting(isLightPlayer);
        //    }
        //}
    }

    void OnTriggerStay (Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Shadow"))
        {
            isInShadow = true;
        }
    }

    void OnTriggerExit(Collider col)
    {
        //if (col.gameObject.layer == LayerMask.NameToLayer("Shadow"))
        //{
        //    //Debug.Log("Exiting shadow.");

        //    if (isLightPlayer)
        //    {
        //        if (StartHurting != null) EndHurting(isLightPlayer);
        //    }
        //    else
        //    {
        //        if (EndHurting != null) StartHurting(isLightPlayer);
        //    }
        //}
    }
}
