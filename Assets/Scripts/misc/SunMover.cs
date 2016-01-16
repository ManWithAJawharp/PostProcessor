using UnityEngine;
using System.Collections;

public class SunMover : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
        transform.eulerAngles = new Vector3(45, Mathf.Sin(Time.time) * 45, 0);
	}
}
