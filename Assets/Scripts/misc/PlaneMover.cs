using UnityEngine;
using System.Collections;

public class PlaneMover : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.position = new Vector3(Mathf.Sin(Time.time) * 2, Mathf.Cos(Time.time) + 5, 0);
	}
}
