using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackingAI : MonoBehaviour
{

    public float enemySpeed;
    public float distance;
    public float engageSpeed;

    Rigidbody2D theRigidBody;


    private bool movingRight = true;

    public Transform groundDetection;
    public Transform playerDetection;

    void Update()
    {
        transform.Translate(Vector2.right * enemySpeed * Time.deltaTime);

        RaycastHit2D groundInfo = Physics2D.Raycast(groundDetection.position, Vector2.down, distance);
        RaycastHit2D playerInfo = Physics2D.Raycast(playerDetection.position, Vector2.left, distance);
        if (playerInfo.collider == true)
        {
            Engage();
        }


        else
        {
            {
                if (groundInfo.collider == false)
                {
                    if (movingRight == true)
                    {
                        transform.eulerAngles = new Vector3(0, -180, 0);
                        movingRight = false;

                    }
                    else
                    {
                        transform.eulerAngles = new Vector3(0, 0, 0);
                        movingRight = true;
                    }
                }
            }
        }
        



    }

    void Engage()
    {
        //theRigidBody.AddForce(transform.forward * engageSpeed);
        transform.Translate(Vector3.forward * engageSpeed * Time.deltaTime);
    }
}

