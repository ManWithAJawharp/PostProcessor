using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    private enum States
    {
        Playing,
        Paused,
        Respawning
    }

    struct TransformCopy
    {
        public Vector3 position;
        public Quaternion rotation;
        public Vector3 scale;
        public Vector3 forward;
        public Vector3 right;
        public Vector3 up;
    };

    //////////////////////////
    //  Serialized Fields   //
    //////////////////////////

    [Header("Player variables.")]
    [SerializeField]
    [Tooltip("Walking speed of the player.")]
    [Range(0, 10)]
    float walkSpeed = 6.7f;

    /// <summary>
    /// The respawn object for this player.
    /// </summary>
    [SerializeField]
    [Tooltip("The respawn object for this player.")]
    GameObject spawnPrefab;

    [SerializeField]
    Color spawnColor = new Color(1, 1, 1, 1);

    /// <summary>
    /// If true, this player is only able to walk in (inverse) shadows.
    /// </summary>
    [Header("Shadow variables.")]
    [SerializeField]
    [Tooltip("If true, this player is not able to walk in (inverse) shadows.")]
    bool isLightPlayer = true;

    [SerializeField]
    [Tooltip("Delay (s) before respwaning player.")]
    float deathDelay = 0.1f;

    [SerializeField]
    [Tooltip("Use advanced collision for shadow player (still in progress).")]
    bool advancedCollision = true;

    [Header("Audio")]
    [SerializeField]
    AudioClip soundDie;
    [SerializeField]
    AudioClip soundRespawn;

    //////////////////////////////
    //  Non-Serialized Fields   //
    //////////////////////////////

    /// <summary>
    /// Current player state.
    /// </summary>
    States state = States.Paused;

    Transform oldCameraTransform;
    Transform cameraTransform;

    GameObject spawn;

    Transform avatar;
    Vector3 avatarDirection;

    CharacterController characterController;

    ParticleSystem spawnParticles;

    bool wasInShadow;
    bool isInShadow;
    int collisionCount;

    float health;

    //////////////////
    //  Properties  //
    //////////////////

    /// <summary>
    /// The respawn object for this player.
    /// </summary>
    public Transform Spawn
    {
        get { return spawn.transform; }
    }

    public bool Respawning
    {
        get { return state == States.Respawning; }
    }

    //////////////////
    //  Delegates   //
    //////////////////

    public delegate void CollisionDelegate(bool isLightPlayer);
    public static event CollisionDelegate OnPlayerEnterShadow;
    public static event CollisionDelegate OnPlayerExitShadow;

    ///////////////////////////
    //  Built-in Functions   //
    ///////////////////////////

    void Awake ()
    {
        CameraManager.OnSwitchCamera += CameraManager_OnSwitchCamera;
        State.OnStateChanged += State_OnStateChanged;

        characterController = GetComponent<CharacterController>();

        spawn = (GameObject) Instantiate(spawnPrefab, transform.position, transform.rotation);
        spawn.GetComponentInChildren<ParticleSystem>().startColor = spawnColor;
        //spawnParticles.startColor = (isLightPlayer ? Color.red : Color.blue);

        avatar = transform.FindChild("Avatar");
        avatarDirection = transform.forward;

        //wasInShadow = isLightPlayer;
        //isInShadow = !isLightPlayer;

        health = deathDelay;
	}

    void Start ()
    {
        cameraTransform = CameraManager.ActiveCamera.transform;
        SwitchTransform();
    }

    void Update ()
    {
        if (state == States.Playing)
        {
            //  Reset the velocity vector.
            Vector3 velocity = Vector3.zero;

            if (isLightPlayer)
            {
                //  Controller input.
                //////////  Replace functionality with InputHandler.
                velocity = Input.GetAxis("Horizontal2") * cameraTransform.transform.right
                    + Input.GetAxis("Vertical2") * Vector3.Scale(cameraTransform.transform.forward, new Vector3(1, 0, 1));
            }
            else
            {
                //  Controller input.
                //////////  Replace functionality with InputHandler.
                velocity = Input.GetAxis("Horizontal1") * cameraTransform.transform.right
                    + Input.GetAxis("Vertical1") * Vector3.Scale(cameraTransform.transform.forward, new Vector3(1, 0, 1));
            }



            if (velocity.magnitude != 0)
            {
                avatarDirection = velocity;
                //Debug.Log(velocity);
            }

            velocity *= walkSpeed * Time.deltaTime;

            velocity.y -= 9.81f * Time.deltaTime;

            characterController.Move(velocity);

            //if (isLightPlayer == isInShadow)
            //{
            //    health -= Time.deltaTime;

            //    if (health <= 0f)
            //    {
            //        if (advancedCollision)
            //        {
            //            if (collisionCount <= 1) Respawn();
            //        }
            //        else
            //        {
            //            Respawn();
            //        }
            //    }
            //}
            //else if (health < deathDelay)
            //{
            //    health += Time.deltaTime;
            //}

            if (isLightPlayer)
            {
                if (LightManager.LightShadowRatios[0] < 0.8f)
                {
                    Respawn();
                }
            }
            else
            {
                if (LightManager.LightShadowRatios[1] > 0.2f)
                {
                    Respawn();
                }
            }

            //  Rotate player in movement direction.
            avatar.rotation = Quaternion.Lerp(avatar.rotation, Quaternion.LookRotation(avatarDirection), Time.deltaTime * 15f);

            //  Rotate camera to new transform.
            //cameraTransform.rotation = Quaternion.Lerp(oldCameraTransform.rotation, cameraTransform.rotation, Time.deltaTime * 5f);
        }
    }

    void FixedUpdate ()
    {
        //if (state == States.Paused) return;

        //if (!advancedCollision)
        //{
        //    if (isInShadow && !wasInShadow)         //  Entering the shadow.
        //    {
        //        if (OnPlayerEnterShadow != null)
        //        {
        //            OnPlayerEnterShadow(isLightPlayer);
        //        }

        //        wasInShadow = true;
        //    }
        //    else if (!isInShadow && wasInShadow)    //  Leaving the shadow.
        //    {
        //        if (OnPlayerExitShadow != null)
        //        {
        //            OnPlayerExitShadow(isLightPlayer);
        //        }

        //        wasInShadow = false;
        //    }

        //    isInShadow = false;
        //}
    }

    void OnTriggerEnter (Collider col)
    {
        //  Instantly respawn player if it collides with an object from the Kill-layer.
        if (col.gameObject.layer == LayerMask.NameToLayer("Kill"))
        {
            Respawn();
        }
        //else if (state == States.Playing && col.gameObject.layer == LayerMask.NameToLayer("Shadow"))
        //{
        //    //Debug.Log("Entered new shadow.");

        //    collisionCount++;
        //}
    }

    void OnTriggerStay (Collider col)
    {
        //if (state == States.Playing && col.gameObject.layer == LayerMask.NameToLayer("Shadow"))
        //{
        //    if (isLightPlayer)  //  Light player immediately registers as 'in the shadow' when colliding with a shadow object.
        //    {
        //        isInShadow = true;
        //    }
        //    else  //  Shadow player only registers when its not touching the edges of a shadow object in advanced collision.
        //    {
        //        ExtrudeGeometry shadow = col.transform.parent.GetComponent<ExtrudeGeometry>();

        //        if ((!advancedCollision && shadow != null) || (shadow != null && advancedCollision && !shadow.CheckTriangularCollision(characterController)))
        //        {
        //            isInShadow = true;
        //        }
        //        else
        //        {
        //            isInShadow = false;
        //        }
        //    }
        //}
        //else
        //{
        //    //isInShadow = false;
        //}
    }

    void OnTriggerExit (Collider col)
    {
        //if (state == States.Playing && col.gameObject.layer == LayerMask.NameToLayer("Shadow"))
        //{
        //    collisionCount--;
        //}
    }

    void OnDrawGizmos ()
    {
        if (isLightPlayer)
            Gizmos.DrawIcon(transform.position + Vector3.up * 2, "icon_sun.png");
        else
            Gizmos.DrawIcon(transform.position + Vector3.up * 2, "icon_moon.png");
    }

    ///////////////////////////
    //  Delegate Functions   //
    ///////////////////////////

    private void State_OnStateChanged(State.States previousState, State.States newState)
    {

        //Debug.Log(name + " went from " + previousState + " to " + newState);

        if (newState == State.States.Play)
        {
            state = States.Playing;
        }
    }

    private void CameraManager_OnSwitchCamera()
    {
        SwitchTransform();
    }

    //////////////////////////
    //  Private Functions   //
    //////////////////////////

    void Respawn ()
    {
        if (state != States.Respawning)
        {
            state = States.Respawning;

            StartCoroutine("RespawnCoroutine");
        }
    }

    void SwitchTransform()
    {
        Transform camera = CameraManager.ActiveCamera.transform;

        oldCameraTransform = cameraTransform;

        cameraTransform = camera;
        //cameraTransform = new TransformCopy();
        //cameraTransform.position = camera.position;
        //cameraTransform.rotation = camera.rotation;
        //cameraTransform.scale = camera.localScale;
        //cameraTransform.forward = camera.forward;
        //cameraTransform.right = camera.right;
        //cameraTransform.up = camera.up;
    }

    ///////////////////
    //  Coroutines   //
    ///////////////////

    /// <summary>
    /// Despawns, transports, and respawns player.
    /// </summary>
    IEnumerator RespawnCoroutine ()
    {
        //  Despawn

        GetComponent<ParticleSystem>().Play();
        AudioManager.StartSound(transform.position, soundDie);

        float t = 0f;

        while (t < 1f)
        {
            avatar.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);

            t += Time.deltaTime * 2f;

            yield return null;
        }

        //  Transport

        AudioManager.StartSound(transform.position, soundRespawn);

        t = 0f;

        Vector3 startPos = transform.position;

        while (t < 1f)
        {
            transform.position = Vector3.Lerp(startPos, spawn.transform.position, t);

            t += Time.deltaTime;

            yield return null;
        }

        //  Respawn

        t = 0f;

        while (t < 1f)
        {
            avatar.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);

            t += Time.deltaTime * 2f;

            yield return null;
        }

        GetComponent<ParticleSystem>().Stop();

        health = deathDelay;
        state = States.Playing;
        isInShadow = !isLightPlayer;

        yield return null;
    }
}
