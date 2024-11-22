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

    public InputActionReference normalAttackAction; // 일반 공격 액션
    public InputActionReference mainSkillAction;    // 메인 스킬 액션
    public InputActionReference subSkillAction;     // 서브 스킬 액션
    public InputActionReference equipmentAction;    // 장비 사용 액션

    public LayerMask groundLayer; // 바닥 레이어 마스크 설정

    public float maxRotationSpeed = 360f; // 최대 회전 속도 (도/초)
    public float moveSpeed = 5f;
    public float maxVelocity = 5f;

    Rigidbody rb;

    Vector2 mousePosition;
    Camera mainCamera;

    bool canRotate = true;

    float moveX = 0, moveY = 0;

    float velocity = 0; // 현재 이동하는 힘

    float rotationSpeed = 30f; // 회전 속도
    float deceleration = 10f; // 감속 속도

    Vector3 currentDirection;  // 현재 이동 방향

    private void Start()
    {
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

        equipmentAction.action.Enable();
        equipmentAction.action.performed += OnEquipment;

        subSkillAction.action.Enable();
        subSkillAction.action.performed += OnSubSkill;

        mainSkillAction.action.Enable();
        mainSkillAction.action.performed += OnMainSkill;

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

        equipmentAction.action.performed -= OnEquipment;
        equipmentAction.action.Disable();

        subSkillAction.action.performed -= OnSubSkill;
        subSkillAction.action.Disable();

        mainSkillAction.action.performed -= OnMainSkill;
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
    }

    private void OnNormalAttack(/*InputAction.CallbackContext context*/)
    {
        normalAttack.UseSkill();
    }
    private void OnMainSkill(InputAction.CallbackContext context)
    {
        Debug.Log("메인 스킬 사용");
        mainSkill.UseSkill();
    }
    private void OnSubSkill(InputAction.CallbackContext context)
    {
        Debug.Log("서브 스킬 사용");
        subSkill.UseSkill();
    }
    private void OnEquipment(InputAction.CallbackContext context)
    {
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
            // 현재 방향
            currentDirection = new Vector3(moveX, 0, moveY).normalized;
            
            velocity = Mathf.Clamp(velocity + Time.fixedDeltaTime * moveSpeed, 0, maxVelocity);
        }
        else if (velocity > 0)
        {
            velocity = Mathf.Clamp(velocity - Time.fixedDeltaTime * moveSpeed * 3f, 0, maxVelocity);
        }

        if (velocity > 0)
        {
            // 현재 위치에 속도 값을 반영하여 이동
            // transform.position += currentDirection * velocity * Time.fixedDeltaTime;

            rb.linearVelocity = currentDirection * velocity;
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
}