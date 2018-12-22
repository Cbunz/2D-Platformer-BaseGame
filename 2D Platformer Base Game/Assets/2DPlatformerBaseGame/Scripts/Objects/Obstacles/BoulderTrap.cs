using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderTrap : MonoBehaviour
{
    private Transform boulder;
    private Rigidbody2D boulderRB;
    private BoxCollider2D trigger;
    private Damager damager;
    private BoxCollider2D coll;

	private void Awake ()
    {
        boulder = transform.Find("Boulder");
        boulderRB = boulder.GetComponent<Rigidbody2D>();
        trigger = GetComponent<BoxCollider2D>();
	}

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == 9)
        {
            boulderRB.bodyType = RigidbodyType2D.Dynamic;
        }
    }

    public void DisableBoulder()
    {
        coll.enabled = false;
        damager.enabled = false;
    }
}
