using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputAndMove : MonoBehaviour
{
    public Transform playerTransform;

    public PlayerSkill normalAttack;
    public PlayerSkill mainSkill;
    public PlayerSkill subSkill;

    public InputActionReference mousePositionAction; // ���콺 ��ġ �׼�

    public InputActionReference moveLeftAction;     // �� �̵� �׼�
    public InputActionReference moveRightAction;    // �� �̵� �׼�
    public InputActionReference moveUpAction;       // �� �̵� �׼�
    public InputActionReference moveDownAction;     // �� �̵� �׼�

    public InputActionReference runAction; // �ν�Ʈ �׼�

    public InputActionReference normalAttackAction; // �Ϲ� ���� �׼�
    public InputActionReference mainSkillAction;    // ���� ��ų �׼�
    public InputActionReference subSkillAction;     // ���� ��ų �׼�
    public InputActionReference equipmentAction;    // ��� ��� �׼�

    public LayerMask groundLayer; // �ٴ� ���̾� ����ũ ����

    public float maxRotationSpeed = 360f; // �ִ� ȸ�� �ӵ� (��/��)
    public float moveSpeed = 5f;
    public float maxVelocity = 5f;

    Rigidbody rb;

    Vector2 mousePosition;
    Camera mainCamera;

    bool canRotate = true;

    float moveX = 0, moveY = 0;

    float velocity = 0; // ���� �̵��ϴ� ��

    float rotationSpeed = 30f; // ȸ�� �ӵ�
    float deceleration = 10f; // ���� �ӵ�

    Vector3 currentDirection;  // ���� �̵� ����

    private void Awake()
    {
        GameManager.SetPlayerTransform(transform);
    }

    private void Start()
    {
        mainCamera = Camera.main;

        rb = GetComponent<Rigidbody>();
    }

    #region Input Action Ȱ��ȭ ��Ȱ��ȭ 
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
        subSkillAction.action.started += OnSubSkill;

        mainSkillAction.action.Enable();
        mainSkillAction.action.started += OnMainSkill;

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

        subSkillAction.action.started -= OnSubSkill;
        subSkillAction.action.Disable();

        mainSkillAction.action.started -= OnMainSkill;
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
        Debug.Log("���� ��ų ���");
        mainSkill.UseSkill();
    }
    private void OnSubSkill(InputAction.CallbackContext context)
    {
        Debug.Log("���� ��ų ���");
        subSkill.UseSkill();
    }
    private void OnRun(InputAction.CallbackContext context)
    {

    }
    private void OnWalk(InputAction.CallbackContext context)
    {

    }
    private void OnEquipment(InputAction.CallbackContext context)
    {
        // ��� ���� ��� ���� �߰� �� �� ������ �ϰ�...
        Debug.Log("��� ���");
    }
    private void FixedUpdate()
    {
        #region �̵�

        // �����¿� �Է��� �޾� ��ǥ �̵� ���� ���
        float moveX = moveRightAction.action.ReadValue<float>() - moveLeftAction.action.ReadValue<float>();
        float moveY = moveUpAction.action.ReadValue<float>() - moveDownAction.action.ReadValue<float>();

        // �̵� �� �������� ��
        if (moveX != 0 || moveY != 0)
        {
            // ���� ����
            currentDirection = new Vector3(moveX, 0, moveY).normalized;
            
            velocity = Mathf.Clamp(velocity + Time.fixedDeltaTime * moveSpeed, 0, maxVelocity);
        }
        else if (velocity > 0)
        {
            velocity = Mathf.Clamp(velocity - Time.fixedDeltaTime * moveSpeed * 3f, 0, maxVelocity);
        }

        if (velocity > 0)
        {
            // ���� ��ġ�� �ӵ� ���� �ݿ��Ͽ� �̵�
            // transform.position += currentDirection * velocity * Time.fixedDeltaTime;

            rb.linearVelocity = currentDirection * velocity;
        }
        else rb.linearVelocity = Vector3.zero;

        #endregion

        #region ȸ��
        if (canRotate)
        {
            mousePosition = mousePositionAction.action.ReadValue<Vector2>();

            Ray ray = mainCamera.ScreenPointToRay(mousePosition);
            RaycastHit hit;

            // ���̰� groundLayer�� �¾��� ���� ����
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, groundLayer))
            {
                // ��ǥ ��ġ�κ��� ���� ������Ʈ ���̸� �����ϰ� �����Ͽ� ���� ���
                Vector3 targetPosition = hit.point;
                targetPosition.y = transform.position.y; // Y�� �� ����

                // ��ǥ ��ġ���� ���� ���� ���� ���
                Vector3 direction = (targetPosition - transform.position).normalized;

                // ��ǥ ������ ���ϴ� ȸ�� �� ���
                Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);

                // ���� ȸ������ ��ǥ ȸ������ ���� �ӵ��� ȸ��
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