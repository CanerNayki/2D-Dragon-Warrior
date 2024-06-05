using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    
    [SerializeField] private float speed;
    [SerializeField] private float jumpPower;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask wallLayer;
    [SerializeField] private float coyoteTime;
    private float coyoteCounter;
    [SerializeField] private int extraJumps;
    private int jumpCounter;
    [SerializeField] private float wallJumpX;
    [SerializeField] private float wallJumpY;


    private Rigidbody2D rb;
    private Animator anim;
    private BoxCollider2D boxCollider;
    private float wallJumpCooldown;
    private float horizontalInput;

    [Header("SFX")]
    [SerializeField] private AudioClip jumpSound;




    private void Awake()
    {   // kullanýlan componentlere referans verme
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        boxCollider = GetComponent<BoxCollider2D>();
    }
    void Start()
    {
        
    }


    void Update()
    {
        horizontalInput = Input.GetAxis("Horizontal");

        // karekterin saða sola dönmesi
        if (horizontalInput > 0.01f)
        {
            transform.localScale = Vector3.one;
        }
        else if (horizontalInput < -0.01f)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }

        // animator deðerleri
        anim.SetBool("run", horizontalInput != 0);
        anim.SetBool("grounded", isGrounded());

        //zýplama

        if (Input.GetKeyDown(KeyCode.Space))
            Jump();

        if (Input.GetKeyUp(KeyCode.Space) && rb.velocity.y > 0)
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y / 2);

        if(onWall())
        {
            rb.gravityScale = 0;
            rb.velocity = Vector2.zero;
        }
        else
        {
            rb.gravityScale = 7;
            rb.velocity = new Vector2(horizontalInput * speed, rb.velocity.y);

            if(isGrounded())
            {
                coyoteCounter = coyoteTime;
                jumpCounter = extraJumps;
            }
            else
                coyoteCounter -= Time.deltaTime;
        }
    }

    private void Jump()
    {
        if(coyoteCounter <= 0 && !onWall() && jumpCounter <= 0) return;

        SoundManager.instance.PlaySound(jumpSound);

        if(onWall())
            WallJump();

        else
        {
            if (isGrounded())
                rb.velocity = new Vector2(rb.velocity.x, jumpPower);
            else
            {
                if( coyoteCounter > 0)
                    rb.velocity = new Vector2(rb.velocity.x, jumpPower);
                else
                {
                    if(jumpCounter > 0)
                    {
                        rb.velocity = new Vector2(rb.velocity.x, jumpPower);
                        jumpCounter--;  
                    }
                        

                }
            }

            coyoteCounter = 0;
        }
    }
   
    private void WallJump()
    {
        rb.AddForce(new Vector2(-Mathf.Sign(transform.localScale.x) * wallJumpX, wallJumpY));
        wallJumpCooldown = 0;
    }
    private bool isGrounded()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, Vector2.down, 0.1f, groundLayer);
        return raycastHit.collider != null;
    }

    private bool onWall()
    {
        RaycastHit2D raycastHit = Physics2D.BoxCast(boxCollider.bounds.center, boxCollider.bounds.size, 0, new Vector2(transform.localScale.x,0), 0.1f, wallLayer);
        return raycastHit.collider != null;
    }
    public bool canAttack()
    {
        return horizontalInput == 0 && isGrounded() && !onWall();
    }
}
