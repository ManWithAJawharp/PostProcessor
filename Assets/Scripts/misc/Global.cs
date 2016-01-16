using UnityEngine;
using System.Collections;

public class Global : MonoBehaviour
{
    public static GameObject player;

    void Awake ()
    {
        player = FindObjectOfType<PlayerController>().gameObject;
    }
}
