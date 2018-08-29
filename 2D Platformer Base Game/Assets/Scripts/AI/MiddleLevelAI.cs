using UnityEngine;
using System.Collections;

public class AI : MonoBehaviour
{

    public float fpsTargetDistance;
    public float enemyLookDistance;
    public float engageDistance;
    public float attackDistance;
    public float enemyMovementSpeed;
    public float damping;
    public Transform fpsTarget;
    Rigidbody theRigidbody;
    Renderer myRender;
    //Animator m_Animator;    //Eric's changes
    private Animation anim;


    // Use this for initialization
    void Start()
    {
        myRender = GetComponent<Renderer>();
        theRigidbody = GetComponent<Rigidbody>();
        anim = GetComponent<Animation>();
    }

    // Update is called once per frame
    void Update()
    {
        fpsTargetDistance = Vector3.Distance(fpsTarget.position, transform.position);
        if (fpsTargetDistance < enemyLookDistance)
        {
            lookAtPlayer();
        }
        if (fpsTargetDistance < engageDistance)
        {
            attackPlayer();
            print("attack");
        }
    }

    void lookAtPlayer()
    {

        //Quaternion rotation = Quaternion.LookRotation (fpsTarget.position - transform.position);
        //    transform.rotation = Quaternion.Slerp (transform.rotation, rotation, Time.deltaTime * damping);
        transform.LookAt(new Vector3(fpsTarget.position.x, transform.position.y, fpsTarget.position.z), Vector3.up);

    }
    void attackPlayer()
    {


        if (fpsTargetDistance < attackDistance)
        {
            anim.Play("Attack_Right");
        }

        else
        {
            theRigidbody.AddForce(transform.forward * enemyMovementSpeed);
        }
    }
}
