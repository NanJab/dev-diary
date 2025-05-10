using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using UnityEditor;
using UnityEngine;

public class Move : NetworkBehaviour
{
    [SyncVar]
    private Vector3 syncPosition;

    public float speed = 0;
    public float moveSpeed;
    public float runSpeed;
    public float sitSpeed;
    public float minThreatRate = 0; // 위협도 추후 추가
    public float maxThreatRate = 100; // 위협도 추후 추가
    public float currentThreatRate = 0; // 위협도 추후 추가

    public bool ThreatCheck = false;

    private bool isStaminaBlocked = false;
    public bool isRunning => speed == runSpeed && !isStaminaBlocked;

    public Animator animator;
    private SpriteRenderer spriteRenderer;

    private Vector2 moveDirection;
    private Vector2 mouseDirection;
    private Vector3 mouseP;

    // 애니메이션 상태를 저장할 SyncVar 변수들
    [SyncVar]
    private float syncAnimX;
    [SyncVar]
    private float syncAnimY;
    [SyncVar]
    private bool syncIsMoving;
    [SyncVar(hook = nameof(OnFlipXChanged))] 
    private bool syncFlipX;


    void Start()
    {
        if(isServer)
        {
            Debug.Log("내가 서버다");
        }
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        // 로컬 플레이어일 경우에만 입력 처리
        if (!isLocalPlayer) return;

        mouseP = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        CheckThreat();
        Movement();
        UpdateAnimation();
    }

    public void SetStaminaBlocked(bool value)
    {
        isStaminaBlocked = value;
    }

    public void CheckThreat()
    {
        if (ThreatCheck)
        {
            if (speed == runSpeed)
            {
                currentThreatRate += 50;
                ThreatCheck = false;
            }
            else if (speed == sitSpeed)
            {
                currentThreatRate -= 30;
                ThreatCheck = false;
            }
        }

        if (currentThreatRate > 100)
        {
            currentThreatRate = maxThreatRate;
        }
        else if (currentThreatRate < 0)
        {
            currentThreatRate = minThreatRate;
        }
    }

    private void Movement()
    {
        if (!isLocalPlayer) return;

        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");

        moveDirection = new Vector2(moveX, moveY).normalized;

        // 달리기, 걷기 속도 변경
         
        if(Input.GetKey(KeyCode.LeftShift) && !isStaminaBlocked)
        {
            if(Input.GetKeyDown(KeyCode.LeftShift))
            {
                ThreatCheck = true;
            }
            speed = runSpeed;

        }
        else if(Input.GetKey(KeyCode.LeftControl))
        {
            if(Input.GetKeyDown(KeyCode.LeftControl))
            {
                ThreatCheck = true;
            }
            speed = sitSpeed;
        }
        else
        {
            ThreatCheck = false;
            speed = moveSpeed;
        }

        // 로컬에서 미리 이동
        transform.position += (Vector3)moveDirection * speed * Time.deltaTime;

        // 서버에 이동 명령 전송                
        CmdMove(moveDirection, speed);        
    }

    private void UpdateAnimation()
    {
        if (!isLocalPlayer) return;

        bool isMoving = false;
        Vector2 mousePos = (mouseP - transform.position).normalized;
        // 마우스 방향으로 이미지 이동
        animator.SetFloat("X", mousePos.x);
        animator.SetFloat("Y", mousePos.y);

        bool flipX = mousePos.x < 0;

        if(spriteRenderer.flipX != flipX)
        {
            spriteRenderer.flipX = flipX;
            CmdAnimation(mousePos.x, mousePos.y, isMoving, flipX);

        }

        if((Vector2)mouseP != Vector2.zero && moveDirection != Vector2.zero)
        {
            isMoving = true;
        }
        animator.SetBool("isMoving", isMoving);

    }

    [Command]
    void CmdMove(Vector2 direction, float currentSpeed)
    {
        // 서버에서만 이동 처리
        transform.position += (Vector3)direction * currentSpeed * Time.deltaTime;
    }

    [Command]
    void CmdAnimation(float x, float y, bool moving, bool flipX)
    {
        // SyncVar 변수에 값 저장 (자동으로 모든 클라이언트에 동기화됨)
        syncAnimX = x;
        syncAnimY = y;
        syncIsMoving = moving;
        syncFlipX = flipX;
    }

    private void OnFlipXChanged(bool oldValue, bool newValue)
    {
        if(spriteRenderer != null)
        {
            spriteRenderer.flipX = newValue;
        }
    }
}
