using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AcidBall : MonoBehaviour
{
    public AIBotController aIBotController;
    // Start is called before the first frame update
    void Start()
    {
        GetComponent<ParticleSystem>().Stop();
        //aIBotController = GetComponentInParent<AIBotController>();
    }

    void OnCollisionEnter(Collision collision)
    {
        foreach (ContactPoint contact in collision.contacts)
        {
            aIBotController.HitPlayer(contact.otherCollider.transform);
            Debug.DrawRay(contact.point, contact.normal, Color.white);
            StartCoroutine(Explode());
        }
    }
    IEnumerator Explode()
    {
        Destroy(GetComponent<Rigidbody>());
        Destroy(GetComponent<MeshFilter>());
        GetComponent<ParticleSystem>().Play();
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
    
}
