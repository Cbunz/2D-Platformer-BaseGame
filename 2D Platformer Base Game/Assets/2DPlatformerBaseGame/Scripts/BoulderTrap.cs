using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoulderTrap : MonoBehaviour {

    private Rigidbody2D boulderRB;
    private BoxCollider2D trigger;

	void Start ()
    {
        boulderRB = GetComponentInChildren<Rigidbody2D>();
        trigger = GetComponentInChildren<BoxCollider2D>();
	}
	
	void Update ()
    {
		
	}
}
