using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileBehaviour : MonoBehaviour
{

    [SerializeField] private float speed;
    [SerializeField] private float maxLifetime;

    private float lifeTime;
    
    private Vector3 direction;

    private Vector3 previousPos;


    private bool active;

    private Rigidbody _rigidBody;


    private void Awake()
    {
        _rigidBody = GetComponent<Rigidbody>();
    }

    void Start()
    {
        
    }


    private void FixedUpdate()
    {
        lifeTime += Time.fixedDeltaTime;

        if(lifeTime >= maxLifetime) Destroy(gameObject);

        if (active)
        {
            var newPos = _rigidBody.position;

            newPos += direction * speed * Time.fixedDeltaTime;

            _rigidBody.MovePosition(newPos);
        }
    }

    public void LaunchProjectile(Vector3 dir)
    {
        direction = dir;
        transform.forward = dir;
        active = true;
    }

    

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.layer != gameObject.layer)
        {
            //Debug.Log("I'M DEAD");
            Destroy(gameObject);
        }
        
    }



}
