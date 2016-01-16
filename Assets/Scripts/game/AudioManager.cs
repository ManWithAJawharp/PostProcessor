using UnityEngine;
using System.Collections;

public class AudioManager : MonoBehaviour
{
    static AudioManager self;

    void Awake ()
    {
        self = this;
    }

    public static void StartSound (Vector3 position, AudioClip clip)
    {
        self.StartCoroutine(soundCoroutine(position, clip));
    }

    static IEnumerator soundCoroutine (Vector3 position, AudioClip clip)
    {
        GameObject gameObject = new GameObject();
        gameObject.transform.position = position;
        gameObject.hideFlags = HideFlags.HideAndDontSave;

        AudioSource source = gameObject.AddComponent<AudioSource>();
        gameObject = Instantiate(gameObject);

        source.clip = clip;
        source.pitch = Random.Range(0.8f, 1.2f);
        source.Play();

        while (true)
        {
            if (!source.isPlaying)
            {
                Destroy(source);
                Destroy(gameObject);
                yield break;
            }

            yield return null;
        }
    }
}
