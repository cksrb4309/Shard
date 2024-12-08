using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputAndMove : MonoBehaviour
{
    public Transform playerTransform;

    public PlayerSkill normalAttack;
    public PlayerSkill mainSkill;
    public PlayerSkill subSkill;

    public InputActionReference mousePositionAction; // 마우스 위치 액션

    public InputActionReference moveLeftAction;     // 좌 이동 액션
    public InputActionReference moveRightAction;    // 우 이동 액션
    public InputActionReference moveUpAction;       // 상 이동 액션
    public InputActionReference moveDownAction;     // 하 이동 액션

    public InputActionReference runAction; // 부스트 액션

    public InputActionReference normalAttackAction; // 일반 공격 액션
    public InputActionReference mainSkillAction;    // 메인 스킬 액션
    public InputActionReference subSkillAction;     // 서브 스킬 액션
    public InputActionReference equipmentAction;    // 장비 사용 액션

    public LayerMask groundLayer; // 바닥 레이어 마스크 설정

    public float maxRotationSpeed = 360f; // 최대 회전 속도 (도/초)

    Rigidbody rb;

    float Speed
    {
        get
        {
            return Mathf.Lerp(walkSpeed, runSpeed, boost);
        }
    }
    float boost = 0;
    float walkSpeed;
    float runSpeed;
    float moveX = 0, moveY = 0;
    float velocity = 0; // 현재 이동하는 힘
    Coroutine boostCoroutine = null;

    Vector2 mousePosition;
    Camera mainCamera;
    bool canRotate = true;
    float rotationSpeed = 30f; // 회전 속도
    float deceleration = 10f; // 감속 속도
                
    Vector3 inputDir;  // 현재 이동 방향

    private void Awake()
    {
        //GameManager.SetMyTransform(transform);
    }

    private void Start()
    {
        // GameManager.SetMyTransform(transform); TODO 이전으로 돌아갈 때 고쳐라

        mainCamera = Camera.main;

        rb = GetComponent<Rigidbody>();
    }

    #region Input Action 활성화 비활성화 
    private void OnEnable()
    {
        mousePositionAction.action.Enable();

        moveLeftAction.action.Enable();
        moveRightAction.action.Enable();
        moveUpAction.action.Enable();
        moveDownAction.action.Enable();

        runAction.action.Enable();
        runAction.action.started += OnRun;
        runAction.action.canceled += OnWalk;

        equipmentAction.action.Enable();
        equipmentAction.action.started += OnEquipment;

        subSkillAction.action.Enable();

        mainSkillAction.action.Enable();

        normalAttackAction.action.Enable();
        //normalAttackAction.action.performed += OnNormalAttack;
    }
    private void OnDisable()
    {
        mousePositionAction.action.Disable();

        moveLeftAction.action.Disable();
        moveRightAction.action.Disable();
        moveUpAction.action.Disable();
        moveDownAction.action.Disable();

        runAction.action.Disable();
        runAction.action.started -= OnRun;
        runAction.action.canceled -= OnWalk;

        equipmentAction.action.started -= OnEquipment;
        equipmentAction.action.Disable();

        subSkillAction.action.Disable();

        mainSkillAction.action.Disable();

        //normalAttackAction.action.performed -= OnNormalAttack;
        normalAttackAction.action.Disable();
    }
    #endregion

    private void Update()
    {
        if (normalAttackAction.action.IsPressed())
        {
            OnNormalAttack();
        }
        if (subSkillAction.action.IsPressed())
        {
            OnSubSkill();
        }
        if (mainSkillAction.action.IsPressed())
        {
            OnMainSkill();
        }
    }
    public void SetMoveSpeed(float speed)
    {
        walkSpeed = speed;
        runSpeed = speed * 1.5f;
    }
    private void OnNormalAttack(/*InputAction.CallbackContext context*/)
    {
        normalAttack.UseSkill();
    }
    private void OnMainSkill()
    {
        mainSkill.UseSkill();
    }
    private void OnSubSkill()
    {
        subSkill.UseSkill();
    }
    private void OnRun(InputAction.CallbackContext context)
    {
        if (boostCoroutine != null) StopCoroutine(boostCoroutine);

        boostCoroutine = StartCoroutine(SpeedUpCoroutine());
    }
    private void OnWalk(InputAction.CallbackContext context)
    {
        if (boostCoroutine != null) StopCoroutine(boostCoroutine);

        boostCoroutine = StartCoroutine(SpeedDownCoroutine());
    }
    IEnumerator SpeedUpCoroutine()
    {
        while (boost < 1f)
        {
            boost += Time.deltaTime * 3f;

            yield return null;
        }
        boost = 1;
    }
    IEnumerator SpeedDownCoroutine()
    {
        while (boost > 0)
        {
            boost -= Time.deltaTime * 3f;

            yield return null;
        }
        boost = 0;
    }
    private void OnEquipment(InputAction.CallbackContext context)
    {
        // 사실 이제 장비 없음 추가 할 수 잇으면 하고...
        Debug.Log("장비 사용");
    }
    private void FixedUpdate()
    {
        #region 이동

        // 상하좌우 입력을 받아 목표 이동 방향 계산
        float moveX = moveRightAction.action.ReadValue<float>() - moveLeftAction.action.ReadValue<float>();
        float moveY = moveUpAction.action.ReadValue<float>() - moveDownAction.action.ReadValue<float>();

        // 이동 값 가져왔을 때
        if (moveX != 0 || moveY != 0)
        {
            inputDir = new Vector3(moveX, 0, moveY).normalized;
            
            velocity = Mathf.Clamp(velocity + Time.fixedDeltaTime * Speed, 0, Speed);
        }
        else if (velocity > 0)
        {
            velocity = Mathf.Clamp(velocity - Time.fixedDeltaTime * Speed * 3f, 0, Speed);
        }

        if (velocity > 0)
        {
            // 현재 위치에 속도 값을 반영하여 이동
            // transform.position += currentDirection * velocity * Time.fixedDeltaTime;

            rb.linearVelocity = inputDir * velocity;
        }
        else rb.linearVelocity = Vector3.zero;

        #endregion

        #region 회전
        if (canRotate)
        {
            mousePosition = mousePositionAction.action.ReadValue<Vector2>();

            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            // 레이가 groundLayer에 맞았을 때만 실행
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                // 목표 위치로부터 현재 오브젝트 높이를 동일하게 유지하여 방향 계산
                Vector3 targetPosition = hit.point;
                targetPosition.y = transform.position.y; // Y축 값 고정

                // 목표 위치로의 수평 방향 벡터 계산
                Vector3 direction = (targetPosition - transform.position).normalized;

                // 목표 방향을 향하는 회전 값 계산
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

                // 현재 회전에서 목표 회전까지 일정 속도로 회전
                transform.rotation = Quaternion.RotateTowards(
                    transform.rotation,
                    targetRotation,
                    maxRotationSpeed * Time.fixedDeltaTime
                );
            }
        }
        #endregion
    }
    public void StopRotation()
    {
        canRotate = false;
    }
    public void PlayRotation()
    {
        canRotate = true;
    }
    public void ResetVelocity()
    {
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }
}