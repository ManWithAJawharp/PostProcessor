using UnityEngine;
using System.Collections;

public class CameraRail : MonoBehaviour
{
    private enum Mode
    {
        /// <summary>Sets camera to the railpoint closest to its target.</summary>
        NearestToTarget,
        /// <summary>Snaps camera to the node closest to its target (not finished).</summary>
        [Tooltip("Snaps camera to the node closest to its target (not finished).")]
        Snap,
        /// <summary>The camera's position can be set freely.</summary>
        Free
    }

    //////////////////////////////
    //  Non-Serialized Fields   //
    //////////////////////////////
        
    /// <summary>
    /// Start position of the rail.
    /// </summary>
    Transform startPosition;
    /// <summary>
    /// End position of the rail.
    /// </summary>
    Transform endPosition;

    /// <summary>
    /// Camera's target rail position.
    /// </summary>
    Vector3 cameraPositionTarget;

    //////////////////////////
    //  Serialized Fields   //
    //////////////////////////

    /// <summary>
    /// The camera that will be moved over the rail.
    /// </summary>
    [SerializeField]
    new Camera camera;

    /// <summary>Target for the camera to focus on. </summary>
    [SerializeField]
    Transform target;

    [Header("Rail variables")]
    [SerializeField]
    [Tooltip("Following Mode of the camera.")]
    Mode followingMode = Mode.Free;

    [SerializeField]
    float movementSpeed = 1f;

    [SerializeField]
    [Tooltip("Normalized position of the camera in Free Mode.")]
    [Range(0, 1)]
    float cameraNP;

    [Header("Movement arc")]

    [SerializeField]
    [Tooltip("The magnitude of the rails' arc.")]
    [Range(-10f, 10f)]
    float arcMagnitude = 1f;

    [SerializeField]
    [Tooltip("This probably isn't useful.")]
    [Range(1, 10)]
    float arcFrequency = 1f;

    [SerializeField]
    [Tooltip("Detail of Gizmo Arc (doesn't affect gameplay).")]
    [Range(1, 100)]
    int arcDetail = 20;

    //////////////////
    //  Properties  //
    //////////////////

    /// <summary>
    /// The camera's normalized position.
    /// </summary>
    public float CameraNP
    {
        get { return cameraNP; }
        set { cameraNP = value; }
    }

    ///// <summary>
    ///// The current camera position.
    ///// </summary>
    //public Vector3 CameraPosition
    //{
    //    get { return cameraPosition; }
    //}

    //////////////////////////
    //  Built-in Functions  //
    //////////////////////////

    void Awake ()
    {
        //camera = GetComponentInChildren<Camera>();
        startPosition = transform.FindChild("StartPosition");
        endPosition = transform.FindChild("EndPosition");
    }

    void Update ()
    {
        if (camera.enabled == false) return;

        Vector3 rail = endPosition.position - startPosition.position;

        Vector3 target = this.target.position - startPosition.position;

        float targetNP = PointToNP(this.target.position);

        //Vector3 perpendicular = Vector3.Cross(Vector3.down, rail).normalized;
        Vector3 perpendicular = Vector3.zero;

        //Debug.Log(targetNP);

        if (followingMode == Mode.NearestToTarget)
        {
            perpendicular = Vector3.Cross(Vector3.Lerp(-startPosition.up, -endPosition.up, targetNP), rail).normalized;

            cameraPositionTarget = startPosition.position + targetNP * rail.magnitude * Vector3.Normalize(endPosition.position - startPosition.position);

            //perpendicular *= arcMagnitude - Mathf.Pow(2 * targetNP - 1, 2) * arcMagnitude;
            perpendicular *= arcMagnitude * Mathf.Sin(arcFrequency * Mathf.PI * targetNP);
        }
        else if (followingMode == Mode.Snap)
        {
            if (Vector3.Distance(Vector3.zero, target) < Vector3.Distance(rail, target))
            {
                cameraPositionTarget = startPosition.position;
            }
            else
            {
                cameraPositionTarget = endPosition.position;
            }

            perpendicular = Vector3.Cross(Vector3.Lerp(-startPosition.up, -endPosition.up, PointToNP(camera.transform.position)), rail).normalized;

            //perpendicular *= arcMagnitude - Mathf.Pow(2 * PointToNP(camera.transform.position) - 1, 2) * arcMagnitude;
            perpendicular *= arcMagnitude * Mathf.Sin(arcFrequency * Mathf.PI * PointToNP(camera.transform.position));
        }
        else if (followingMode == Mode.Free)
        {
            perpendicular = Vector3.Cross(Vector3.Lerp(-startPosition.up, -endPosition.up, cameraNP), rail).normalized;

            cameraPositionTarget = startPosition.position + cameraNP * rail.magnitude * Vector3.Normalize(endPosition.position - startPosition.position);

            //perpendicular *= arcMagnitude - Mathf.Pow(2 * cameraNP - 1, 2) * arcMagnitude;
            perpendicular *= arcMagnitude * Mathf.Sin(arcFrequency * Mathf.PI * cameraNP);
        }

        camera.transform.position = Vector3.Lerp(camera.transform.position, cameraPositionTarget + perpendicular, Time.deltaTime * movementSpeed);
    }

    void OnDrawGizmos ()
    {
        startPosition = transform.FindChild("StartPosition");
        endPosition = transform.FindChild("EndPosition");

        Vector3 rail = Vector3.Normalize(endPosition.position - startPosition.position);

        for (float i = 0; i < arcDetail; i++)
        {
            Vector3 perpendicularStart = Vector3.Cross(Vector3.Lerp(-startPosition.up, -endPosition.up, i / arcDetail), rail).normalized;
            Vector3 perpendicularEnd = Vector3.Cross(Vector3.Lerp(-startPosition.up, -endPosition.up, (i + 1) / arcDetail), rail).normalized;

            Vector3 start = startPosition.position + rail * Vector3.Distance(startPosition.position, endPosition.position) * (i / arcDetail)
                /*+ perpendicular * (arcMagnitude - Mathf.Pow(2 * i / 20 - 1, 2) * arcMagnitude);*/
                + perpendicularStart * arcMagnitude * Mathf.Sin(arcFrequency * Mathf.PI * i / arcDetail);
            Vector3 end = startPosition.position + rail * Vector3.Distance(startPosition.position, endPosition.position) * ((i + 1) / arcDetail)
                /*+ perpendicular * (arcMagnitude - Mathf.Pow(2 * (i + 1) / 20 - 1, 2) * arcMagnitude);*/
                + perpendicularEnd * arcMagnitude * Mathf.Sin(arcFrequency * Mathf.PI * (i + 1) / arcDetail);

            Gizmos.DrawLine(start, end);
        }

        //Gizmos.DrawLine(startPosition.position, endPosition.position);

        Gizmos.color = Color.green;
        Gizmos.DrawSphere(startPosition.position, 0.3f);
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(endPosition.position, 0.15f);
    }

    //////////////////////////
    //  Public Functions    //
    //////////////////////////

    /// <summary>
    /// Returns the rail NP closest to the given point.
    /// </summary>
    /// <param name="point"></param>
    public float PointToNP (Vector3 point)
    {
        Vector3 rail = Vector3.Normalize(endPosition.position - startPosition.position);

        Vector3 target = Vector3.Normalize(point - startPosition.position);

        return Mathf.Clamp01(Vector3.Dot(target, rail));
    }

    /// <summary>
    /// Returns the nearest point to given rail NP.
    /// </summary>
    /// <param name="np">Normalized position on the rail.</param>
    public Vector3 NPToPoint (float np)
    {
        return Vector3.zero;
    }
}
