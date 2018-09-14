using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingEnemyScript : MonoBehaviour {

    private CharacterController2D character;

    public float speed;
    public float range;
    public LayerMask playerLayer;
    public bool characterInRange;
    public bool goPastEnemy;

    // Use this for initialization
    void Start()
    {
        character = FindObjectOfType<CharacterController2D>();
    }

    // Update is called once per frame
    void Update()
    {

        characterInRange = Physics2D.OverlapCircle(transform.position, range, playerLayer);

        if (character.transform.position.x > transform.position.x && character.transform.localScale.x > 0)
        {
            goPastEnemy = true;
        }
        else
        {
            goPastEnemy = false;
        }

        if (characterInRange && goPastEnemy)
        {
            transform.position = Vector3.MoveTowards(transform.position, character.transform.position, speed * Time.deltaTime);
        }
    }

}
