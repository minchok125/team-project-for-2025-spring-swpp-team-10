using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModeConverterController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject hamster;
    [SerializeField] private GameObject ball;

    private CapsuleCollider hamCol;
    private Renderer[] hamRds;
    private Rigidbody rb;
    

    private void Start()
    {
        hamCol = hamster.GetComponent<CapsuleCollider>();
        hamRds = hamster.GetComponentsInChildren<Renderer>();
        rb = GetComponent<Rigidbody>();

        PlayerManager.instance.isBall = false;
        rb.drag = 1f;
        transform.rotation = Quaternion.identity;
        rb.constraints |= RigidbodyConstraints.FreezeRotationX;
        rb.constraints |= RigidbodyConstraints.FreezeRotationY;
        rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
    }

    public void Convert()
    {
        if (PlayerManager.instance.isBall) { // sphere -> hamster
            HamsterSetActive(true);
            ball.SetActive(false);
            animator.SetTrigger("ChangeToHamster");
            
            rb.MovePosition(transform.position + Vector3.up * 0.5f);
            rb.drag = 1f;
            transform.rotation = Quaternion.identity;

            rb.constraints |= RigidbodyConstraints.FreezeRotationX;
            rb.constraints |= RigidbodyConstraints.FreezeRotationY;
            rb.constraints |= RigidbodyConstraints.FreezeRotationZ;
        }
        else { // hamster -> sphere
            animator.SetTrigger("ChangeToSphere");
            Invoke(nameof(ChangeToSphere_AfterSeconds), 0.4f);

            rb.MovePosition(transform.position + Vector3.up * 0.5f);
            rb.drag = 0.5f;

            rb.constraints &= ~RigidbodyConstraints.FreezeRotationX;
            rb.constraints &= ~RigidbodyConstraints.FreezeRotationY;
            rb.constraints &= ~RigidbodyConstraints.FreezeRotationZ;
        }

        PlayerManager.instance.isBall = !PlayerManager.instance.isBall;
    }

    private void ChangeToSphere_AfterSeconds()
    {
        rb.MovePosition(transform.position + Vector3.up * 0.5f);
        ball.SetActive(true);
        HamsterSetActive(false);
    }

    private void HamsterSetActive(bool value)
    {
        hamCol.enabled = value;
        foreach (Renderer rd in hamRds)
            rd.enabled = value;
    }
}
