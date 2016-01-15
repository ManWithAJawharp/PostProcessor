using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LightManager : MonoBehaviour
{

    /// <summary>Generic List containing all lights currently used in the scene.</summary>
    private List<Light> lights;

    void Awake ()
    {
        //  TODO: Subscribe to delegates (e.g.: StateManager).

        lights = new List<Light>();
    }

	////////////////////////
    //  Public Functions  //
    ////////////////////////

    public void AddLight (Light light)
    {
        lights.Add(light);
    }

    public void RemoveLight (Light light)
    {
        lights.Remove(light);
    }
}
