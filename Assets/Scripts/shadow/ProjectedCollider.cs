using UnityEngine;

[AddComponentMenu("Argia-Iluna/Shadow Scripts/Projected Collider")]
public class ProjectedCollider : MonoBehaviour
{
    //  Light variables
    public Light lamp;
    private Vector3 lightDir;

    //  Source mesh variables
    private MeshFilter filter;
    private Mesh mesh;
    private Vector3[] vertices;
    private Vector3[] worldVertices;
    private Vector3[] normals;
    private bool[] projectable;

    //  Projection mesh variables
    private Mesh projection;

    private Vector3[] projectionVertices;
    private int[] projectionTriangles;
    private Vector2[] projectionUV;

    //  Child object variables
    private GameObject childObject;
    private MeshFilter childFilter;
    private MeshCollider childCollider;

    public Material shadowMaterial;

    //  Misc variables
    private int projectableN;
    private int i;  //  Index variable 'i' for use in for-loops.
    private float dot;  //  Variable to store the result of a dot product.
    public float projectionHeight;  //  The height of the horizontal plane upon which will be projected.

    private Vector3 prevPos;

    void Start ()
    {
        gameObject.isStatic = false;

        filter = GetComponent<MeshFilter>();
        mesh = filter.mesh;

        projection = new Mesh();

        //  Create child object to assign mesh and collider to.
        childObject = new GameObject("ProjectedShadow");
        //childObject.hideFlags = HideFlags.HideAndDontSave;
        childObject.transform.parent = transform;
        //childObject.transform.localPosition = Vector3.zero;
        childObject.transform.position = transform.position;

        childFilter = childObject.AddComponent<MeshFilter>();

        MeshRenderer childRenderer = childObject.AddComponent<MeshRenderer>();
        childRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        childRenderer.material = shadowMaterial;

        childCollider = childObject.AddComponent<MeshCollider>();
        childCollider.convex = true;
        childCollider.isTrigger = true;

        prevPos = Global.player.transform.position;
	}
	
	void Update ()
    {
        if (prevPos == transform.position) return;

        lightDir = lamp.transform.forward;

        vertices = mesh.vertices;
        worldVertices = mesh.vertices;

        mesh.RecalculateNormals();

        normals = mesh.normals;
        projectable = new bool[vertices.Length];


        Destroy(projection);
        //projection.Clear();
        projection = new Mesh();

        projectableN = 0;

        i = 0;

        for (i = 0; i < vertices.Length; i++)
        {
            vertices[i] = Vector3.Scale(vertices[i], transform.localScale);

            worldVertices[i] = vertices[i] + transform.position;

            //  Store vertex indices that satisfy the given conditions.
            dot = Vector3.Dot(normals[i], lightDir);

            if (dot > 0)    //  Condition to store vertex indices by.
            {
                projectable[i] = true;

                projectableN ++;
            }
            else
            {
                projectable[i] = false;
            }
        }

        //  Create a vertex array for all passed normal indices according to projectableN.
        //  projectableN is multiplied by two to accomodate for the actual projected vertices as well.
        projectionVertices = new Vector3[projectableN * 2];
        projectionTriangles = new int[projectableN * 2 * 3];
        projectionUV = new Vector2[projectableN * 2];

        //  Fill the vertex and normal arrays.
        for (i = 0; i < projectableN; i++)
        {
            if (projectable[i] == false) continue;

            projectionVertices[i] = vertices[i];

           /*
            *   Calculate projected vertices.
            */

            //  The angle along which the vertices are to be projected.
            Vector2 theta = new Vector2(lightDir.y / - lightDir.x, lightDir.y / - lightDir.z);

            //  Project vertices along angle theta.
            projectionVertices[i + projectableN] = new Vector3(worldVertices[i].y / theta.x + vertices[i].x,
                (projectionHeight - transform.position.y) / childObject.transform.lossyScale.y,
                worldVertices[i].y / theta.y + vertices[i].z);
        }
        
        for (i = 0; i < projectableN; i++)
        {
            //  Last set behaviour.
            if (i == projectableN - 1)
            {
                //  First trangle.
                projectionTriangles[6 * i] = i;
                projectionTriangles[6 * i + 1] = 0;
                projectionTriangles[6 * i + 2] = projectableN;

                // Second triangle.
                projectionTriangles[6 * i + 3] = projectableN - 1;
                projectionTriangles[6 * i + 4] = projectableN;
                projectionTriangles[6 * i + 5] = 2 * projectableN - 1;

                //  Assign UV's
                //projectionUV[4 * i] = Vector2.zero;
                //projectionUV[4 * i + 1] = Vector2.right;
                //projectionUV[4 * i + 2] = Vector2.down;
                //projectionUV[4 * i + 3] = Vector2.right + Vector2.down;

                continue;
            }

            //  First trangle.
            projectionTriangles[6 * i] = i;
            projectionTriangles[6 * i + 1] = i + 1;
            projectionTriangles[6 * i + 2] = projectableN + 1 + i;

            // Second triangle.
            projectionTriangles[6 * i + 3] = i;
            projectionTriangles[6 * i + 4] = projectableN + 1 + i;
            projectionTriangles[6 * i + 5] = projectableN + i;

            //  Assign UV's
            projectionUV[2 * i] = new Vector2(i % 2, 0);
            projectionUV[2 * i + 1] = new Vector2(i % 2, 1);
        }

        //  Assign mesh components.
        projection.vertices = projectionVertices;
        projection.triangles = projectionTriangles;
        projection.uv = projectionUV;

        projection.Optimize();

        //  Set child's MeshFilter & MeshCollider's mesh.
        childFilter.mesh = projection;
        childCollider.sharedMesh = projection;

        prevPos = transform.position;
    }
}
