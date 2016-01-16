using UnityEngine;
using System.Collections;

public class Curtain : MonoBehaviour
{
    [SerializeField]
    GameObject curtain;

    [SerializeField]
    bool closeCurtain = true;

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            if (closeCurtain)
            {
                curtain.GetComponent<Animator>().Play("CloseCurtain");
                Game.player1.Spawn.GetComponent<ParticleSystem>().Stop();
                Game.player1.Spawn.position = transform.position + Vector3.up;
                Game.player1.Spawn.GetComponent<ParticleSystem>().Clear();
                Game.player1.Spawn.GetComponent<ParticleSystem>().Play();
            }
            else
            {
                curtain.GetComponent<Animator>().Play("OpenCurtain");
                Game.player2.Spawn.GetComponent<ParticleSystem>().Stop();
                Game.player2.Spawn.position = transform.position + Vector3.up;
                Game.player2.Spawn.GetComponent<ParticleSystem>().Clear();
                Game.player2.Spawn.GetComponent<ParticleSystem>().Play();
            }
        }

        //GetComponent<Collider>().enabled = false;
        StartCoroutine(DisappearCoroutine());
    }

    IEnumerator DisappearCoroutine ()
    {
        Vector3 startPos = transform.position;
        Vector3 endPos = transform.position + Vector3.down * 2f;

        float t = 0f;

        while (t < 1f)
        {
            transform.position = Vector3.Lerp(startPos, endPos, t);

            t += Time.deltaTime;

            yield return null;
        }

        yield return null;
    }
}
