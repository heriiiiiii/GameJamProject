using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NF_ParticleController : MonoBehaviour
{
    [SerializeField] ParticleSystem movementParticle;

    [Range(0, 10)]
    [SerializeField] int occurAfterVelocity;

    [Range(0, 0.2f)]
    [SerializeField] float dustFormationPeriod;

    [SerializeField] Rigidbody2D playerRB;

    float counter;
    bool isOnGround;

    [SerializeField] ParticleSystem fallParticle;
    void Update()
    {
        counter += Time.deltaTime;
        
        if(isOnGround && Mathf.Abs(playerRB.velocity.x) > occurAfterVelocity)
        {
            if (counter > dustFormationPeriod)
            {
                movementParticle.Play();
                counter = 0;
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            fallParticle.Play();
            isOnGround = true;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            isOnGround = false;
        }
    }

}
