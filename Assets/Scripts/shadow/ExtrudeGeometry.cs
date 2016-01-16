using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

[DisallowMultipleComponent]
public class ExtrudeGeometry : MonoBehaviour
{
    //////////////////////////
    //  Serialized Fiels    //
    //////////////////////////

    [SerializeField]
    [Tooltip("The light that acts on this object (only directional currently).")]
    new Light light;

    /// <summary>Material for the generated shadow.</summary>
    [SerializeField]
    [Tooltip("Material for the generated shadow.")]
    Material shadowMaterial;

    [SerializeField]
    [Tooltip("Update shadow in real time.")]
    bool realtime = false;

    [SerializeField]
    [Tooltip("Project shadow along ")]
    bool positiveProjection = false;

    //////////////////////////////
    //  Non-Serialized Fiels    //
    //////////////////////////////

    //  Old Mesh variables.
    Mesh oldMesh;
    int oldVertexCount;

    //  Child Object variables.
    GameObject childObject;
    MeshFilter childFilter;
    MeshRenderer childRenderer;
    MeshCollider childCollider;

    //  Child Mesh variables.
    Mesh newMesh;
    Vector3[] vertices;
    int[] triangles;
    Vector2[] uv;

    Vector3 lightDir;

    //  Collision fields.
    ///<summary>First vertex of the edge, acting as origin for collision calculations.</summary>
    Vector3 origin;
    ///<summary>Vector of the edge.</summary>
    Vector3 edge;
    ///<summary>Cross product of two edges of a triangle, giving a perpendicular to the edge.</summary>
    Vector3 normal;


    //////////////////////////
    //  Built-in Functions  //
    //////////////////////////

    void Awake ()
    {
        //  Initiate Old Mesh variables.
        oldMesh = GetComponent<MeshFilter>().mesh;
        oldMesh = SortVertexOrder(oldMesh);
        oldVertexCount = oldMesh.vertexCount;

        //  Initiate Child Object variables.
        childObject = new GameObject("Projected Shadow");
        childObject.transform.position = transform.position;
        childObject.transform.rotation = transform.rotation;
        childObject.transform.localScale = transform.localScale;
        childObject.transform.parent = transform;
        //childObject.hideFlags = HideFlags.HideAndDontSave;
        childObject.layer = LayerMask.NameToLayer("Shadow");
        
        childFilter = childObject.AddComponent<MeshFilter>();
        childRenderer = childObject.AddComponent<MeshRenderer>();
        childCollider = childObject.AddComponent<MeshCollider>();

        //  Initiate Child Mesh variables.
        childRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        childRenderer.receiveShadows = false;
        childRenderer.material = shadowMaterial;
        childRenderer.useLightProbes = false;
        childRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;

        childCollider.convex = true;
        childCollider.isTrigger = true;

        newMesh = new Mesh();
        vertices = new Vector3[oldVertexCount * 2];
        triangles = new int[oldVertexCount * 2 * 3];
        uv = new Vector2[oldVertexCount * 2];

        lightDir = light.transform.forward;

        if (!realtime)
        {
            int i;
            for (i = 0; i < oldVertexCount; i++)
            {
                ExtrudeVertices(i);
                AssignTriangles(i);
                SetUVCoordinates(i);
            }

            newMesh.vertices = vertices;
            newMesh.triangles = triangles;
            newMesh.uv = uv;
            newMesh.RecalculateNormals();
            newMesh.Optimize();

            childFilter.sharedMesh = newMesh;
            childCollider.sharedMesh = newMesh;
        }
    }

	void Update ()
    {
        if (realtime)
        {
            int i;
            for (i = 0; i < oldVertexCount; i++)
            {
                ExtrudeVertices(i);
                AssignTriangles(i);
                SetUVCoordinates(i);
            }

            newMesh.vertices = vertices;
            newMesh.triangles = triangles;
            newMesh.uv = uv;
            newMesh.RecalculateNormals();
            newMesh.Optimize();

            childFilter.sharedMesh = newMesh;
            childCollider.enabled = false;
            childCollider.sharedMesh = newMesh;
            childCollider.enabled = true;
        }

        //if (State.Current == State.States.Play)
        //    CheckPlanarCollision((CharacterController) Game.player2.GetComponent<Collider>());
	}

    void OnDrawGizmos()
    {
        //Vector3 origin = transform.TransformPoint(vertices[triangles[0]]);
        //Vector3 axisX = Vector3.Normalize(vertices[triangles[1]] - vertices[triangles[0]]);
        //Vector3 axisY = Vector3.Normalize(vertices[triangles[2]] - vertices[triangles[0]]);
        //Vector3 axisZ = Vector3.Cross(axisX, axisY);

        //Gizmos.color = Color.red;
        //Gizmos.DrawLine(origin, origin + axisX);
        //Gizmos.color = Color.green;
        //Gizmos.DrawLine(origin, origin + axisY);
        //Gizmos.color = Color.blue;
        //Gizmos.DrawLine(origin, origin + axisZ);

        //Vector3 edge;
        //Vector3 normal;

        //Vector3[] capsulePoints = new Vector3[4];

        //for (int i = 0; i < newMesh.triangles.Length; i += 1)
        //{
        //    if (i % 3 == 0)  //  Normal only has to be calculated once per triangle.
        //    {
        //        Gizmos.color = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), 1);

        //        Vector3 origin = transform.TransformPoint(vertices[triangles[i]]);
        //        Vector3 v1 = transform.TransformPoint(vertices[triangles[i + 1]]);
        //        Vector3 v2 = transform.TransformPoint(vertices[triangles[i + 2]]);

        //        normal = Vector3.Cross(v1 - origin, v2 - origin).normalized;
        //        Gizmos.DrawRay(origin, normal);
        //    }

        //    //edge = vertices[triangles[i + 1]] - vertices[triangles[i]];
        //    if (i > 0 && i % 3 == 2)
        //    {
        //        Gizmos.DrawLine(transform.TransformPoint(vertices[triangles[i]]), transform.TransformPoint(vertices[triangles[i - 2]]));
        //    }
        //    else
        //    {
        //        Gizmos.DrawLine(transform.TransformPoint(vertices[triangles[i]]), transform.TransformPoint(vertices[triangles[i + 1]]));
        //    }
            
        //}

        //CharacterController player = Game.player2.GetComponent<CharacterController>();
        //Gizmos.color = Color.white;
        //Gizmos.DrawWireSphere(player.transform.position + (player.height / 2 - player.radius) * player.transform.up, player.radius);
        //Gizmos.DrawWireSphere(player.transform.position - (player.height / 2 - player.radius) * player.transform.up, player.radius);
    }


    //////////////////////////
    //  Private Functions   //
    //////////////////////////

    /// <summary>Calculates the angle of a Vector.</summary>
    /// <param name="vector">The Vector of which the angle will be calculated.</param>
    float VectorAngle(Vector3 vector)
    {
        //return Mathf.Atan2(vector.y, vector.x);

        vector = transform.TransformPoint(vector) - transform.position;

        return Mathf.Atan2(Vector3.Dot(vector, transform.up), Vector3.Dot(vector, transform.right));
    }

    /// <summary>Sorts the vertices of a mesh in order of angle to the object's origin and returns the sorted vertices in a mesh.</summary>
    /// <param name="oldMesh">The mesh to sort.</param>
    Mesh SortVertexOrder(Mesh oldMesh)
    {
        //  Transfer vertices to sortable List<>.
        List<Vector3> sortedVerticesList = oldMesh.vertices.ToList();

        //  Sort the List<>.
        sortedVerticesList.Sort(new Comparison<Vector3>((x, y) => VectorAngle(x).CompareTo(VectorAngle(y))));
        
        //  Transfer vertices back to an array.
        Vector3[] sortedVertices = sortedVerticesList.ToArray();

        Mesh sortedMesh = new Mesh();
        sortedMesh.vertices = sortedVertices;

        return sortedMesh;
    }

    /// <summary>Assign vertex 2 * i to the new mesh and project 2 * i + 1 to the plane.</summary>
    /// <param name="i">Vertex index.</param>
    void ExtrudeVertices (int i)
    {
        vertices[2 * i] = oldMesh.vertices[i];
        vertices[2 * i + 1] = oldMesh.vertices[i] + Vector3.back * 2;
        
        /*  Calculate projected vertices.   */

        if (transform.position.y + vertices[2 * i].y * transform.localScale.y == 0)
        {
            //  Don't bother projecting vertices already on the projection plane.
            vertices[2 * i + 1] = vertices[2 * i];
            return;
        }

        Vector3 pos = transform.TransformPoint(vertices[2 * i]);
        lightDir = light.transform.forward;

        vertices[2 * i + 1] = transform.InverseTransformPoint(new Vector3(
            pos.x - (pos.y * lightDir.x) / lightDir.y,
            0,
            pos.z - (pos.y * lightDir.z) / lightDir.y
        ));
    }

    /// <summary>Assign vertex data to triangles.</summary>
    /// <param name="i">Vertex index.</param>
    void AssignTriangles (int i)
    {
        if (positiveProjection)
        {
            if (i < oldVertexCount - 1)
            {
                triangles[6 * i] = i * 2 + 1;
                triangles[6 * i + 1] = i * 2;
                triangles[6 * i + 2] = i * 2 + 2;

                triangles[6 * i + 3] = i * 2 + 1;
                triangles[6 * i + 4] = i * 2 + 2;
                triangles[6 * i + 5] = i * 2 + 3;
            }
            else
            {
                triangles[6 * i] = i * 2 + 1;
                triangles[6 * i + 1] = i * 2;
                triangles[6 * i + 2] = 0;

                triangles[6 * i + 3] = i * 2 + 1;
                triangles[6 * i + 4] = 0;
                triangles[6 * i + 5] = 1;
            }
        }
        else
        {
            if (i < oldVertexCount - 1)
            {
                triangles[6 * i] = i * 2;
                triangles[6 * i + 1] = i * 2 + 1;
                triangles[6 * i + 2] = i * 2 + 2;

                triangles[6 * i + 3] = i * 2 + 2;
                triangles[6 * i + 4] = i * 2 + 1;
                triangles[6 * i + 5] = i * 2 + 3;
            }
            else
            {
                triangles[6 * i] = i * 2;
                triangles[6 * i + 1] = i * 2 + 1;
                triangles[6 * i + 2] = 0;

                triangles[6 * i + 3] = 0;
                triangles[6 * i + 4] = i * 2 + 1;
                triangles[6 * i + 5] = 1;
            }
        }
        
    }

    void SetUVCoordinates (int i)
    {
        uv[2 * i] = new Vector2(i % 2, 0);
        uv[2 * i + 1] = new Vector2(i % 2, 1);
    }

    //////////////////////////
    //  Public Functions    //
    //////////////////////////

    /// <summary>Project CapsuleCollider \collider\ on each edge of all triangles of the shadow mesh to check for collision.</summary>
    public bool CheckTriangularCollision (CharacterController collider)
    {
        bool inner;
        bool outer;
        bool collision = false;
        
        Vector3[] capsulePoints = new Vector3[4];

        int triangleN;
        int capsulePointM;
        for (triangleN = 0; triangleN < newMesh.triangles.Length; triangleN += 1)
        {
            inner = false;
            outer = false;

            origin = transform.TransformPoint(vertices[triangles[triangleN]]);

            if (triangleN % 3 ==0)  //  Normal only has to be calculated once per triangle.
            {
                //normal = transform.TransformVector(Vector3.Cross(vertices[triangles[i + 1]] - origin, vertices[triangles[i + 2]] - origin)).normalized;
                normal = Vector3.Cross(transform.TransformPoint(vertices[triangles[triangleN + 1]]) - origin, transform.TransformPoint(vertices[triangles[triangleN + 2]]) - origin).normalized;
            }

            if (triangleN > 0 && triangleN % 3 == 2)    //  Third edge connecting the first and last vertex of a triangle.
            {
                edge = transform.TransformPoint(vertices[triangles[triangleN - 2]]) - origin;
            }
            else
            {
                edge = transform.TransformPoint(vertices[triangles[triangleN + 1]]) - origin;
            }

            //  Get the projections of the two spheres of the capsule onto the normal vector.
            capsulePoints[0] = collider.transform.position + (collider.height / 2 - collider.radius) * collider.transform.up + collider.radius * normal - origin;
            capsulePoints[1] = collider.transform.position + (collider.height / 2 - collider.radius) * collider.transform.up - collider.radius * normal - origin;
            capsulePoints[2] = collider.transform.position - (collider.height / 2 - collider.radius) * collider.transform.up + collider.radius * normal - origin;
            capsulePoints[3] = collider.transform.position - (collider.height / 2 - collider.radius) * collider.transform.up - collider.radius * normal - origin;
            
            for (capsulePointM = 0; capsulePointM < capsulePoints.Length; capsulePointM++)
            {
                if (Vector3.Dot(capsulePoints[capsulePointM], normal.normalized) > 0)
                {
                    //  Capsulepoint M is on the outside of the mesh.
                    outer = true;
                }
                else
                {
                    //  Capsulepoint M is on the inside of the mesh.
                    inner = true;
                }

                if (inner && outer) //  If capsule points are both on the in- and outside, a collision is present and further checks aren't necessary.
                {
                    collision = true;
                    break;
                    //return true;
                }
            }

            if (collision)
            {
                //Debug.Log("Collision");
                return true;
            }
        }

        return false;
    }
}
