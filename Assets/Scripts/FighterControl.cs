using UnityEngine;

public class FighterControl : MonoBehaviour {

    [Header("이동관련속성.")]
    [Tooltip("기본이동속도.")]
    public float MoveSpeed = 2.0f;//이동속도.
    public float RunSpeed = 3.5f;//달리기속도.
    public float DirectionRotateSpeed = 100.0f; //이동방향을 변경하기 위한 속도.
    public float BodyRotateSpeed = 2.0f; //몸통의 방향을 변경하기 위한 속도.
    [Range(0.01f,5.0f)]
    public float VelocityChangeSpeed = 0.1f;//속도가 변경되기 위한 속도.
    private Vector3 CurrentVelocity = Vector3.zero;
    private Vector3 MoveDirection = Vector3.zero;
    private CharacterController myCharacterController = null; 
    private CollisionFlags collisionFlags = CollisionFlags.None;
    private float gravity = 9.8f; //중력값.
    private float verticalSpeed = 0.0f; //수직 속도.
    private bool CannotMove = false; //이동 불가 플래그.

    [Header("애니메이션관련속성")]
    public AnimationClip IdleAnimClip = null;
    public AnimationClip WalkAnimClip = null;
    public AnimationClip RunAnimClip = null;
    public AnimationClip Attack1AnimClip = null;
    public AnimationClip Attack2AnimClip = null;
    public AnimationClip Attack3AnimClip = null;
    public AnimationClip Attack4AnimClip = null;
    public AnimationClip SkillAnimClip = null; //스킬 애니메이션 클립.
    public AnimationClip DamageAnimClip = null;
    public AnimationClip DieAnimClip = null;


    private Animation myAnimation = null;
    //캐릭터 상태 목록.
    public enum FighterState { None, Idle, Walk, Run, Attack, Skill, Damage, Die }
    [Header("캐릭터상태")]
    public FighterState MyState = FighterState.None;

    public enum FighterAttackState { Attack1, Attack2, Attack3, Attack4 }
    public FighterAttackState AttackState = FighterAttackState.Attack1;
    //다음 공격 활성화 여부를 확인하는 플래그.
    public bool NextAttack = false;

    //[Header("전투 관련")]
    public TrailRenderer AttackTrailRenderer = null;
    public CapsuleCollider AttackCapsuleCollider = null;
    public GameObject SkillEffect = null;
    public GameObject DamageEffect = null;
    [Header("전투 속성")]
    public int HP = 570;


    // Use this for initialization
    void Start () {
        myCharacterController = GetComponent<CharacterController>();

        myAnimation = GetComponent<Animation>();
        myAnimation.playAutomatically = false; //자동 재생 끄고.
        myAnimation.Stop(); //애니메이션 정지.

        MyState = FighterState.Idle;
        myAnimation[IdleAnimClip.name].wrapMode = WrapMode.Loop; //대기 애니메이션은 반복모드.
        myAnimation[WalkAnimClip.name].wrapMode = WrapMode.Loop; //걷기 애니메이션은 반복모드.
        myAnimation[RunAnimClip.name].wrapMode = WrapMode.Loop;
        myAnimation[Attack1AnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[Attack2AnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[Attack3AnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[Attack4AnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[SkillAnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[SkillAnimClip.name].speed = 0.87f;


        myAnimation[DamageAnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[DamageAnimClip.name].layer = 10;
        myAnimation[DieAnimClip.name].wrapMode = WrapMode.Once;
        myAnimation[DieAnimClip.name].speed =0.7f;

        myAnimation[DieAnimClip.name].layer = 10;

        AddAnimationEvent(Attack1AnimClip, "OnAttackAnimFinished");
        AddAnimationEvent(Attack2AnimClip, "OnAttackAnimFinished");
        AddAnimationEvent(Attack3AnimClip, "OnAttackAnimFinished");
        AddAnimationEvent(Attack4AnimClip, "OnAttackAnimFinished");
        AddAnimationEvent(SkillAnimClip, "OnSkillAnimFinished");

        AddAnimationEvent(DamageAnimClip, "OnDamageAnimFinished");
        AddAnimationEvent(DieAnimClip, "OnDieAnimFinished");

         
    }

    // Update is called once per frame
    void Update () {
        //이동.
        Move();
        //몸통의 방향을 이동 방향으로 돌려줍니다.
        BodyDirectionChange();
        //상태에 맞추어 애니메이션을 재생시켜줍니다.
        AniamtionControl();
        //조건에 맞추어 캐릭터 상태를 변경시켜줍니다.
        CheckState();
        //마우스 왼쪽 버튼 클릭으로 공격 상태로 변경시켜 줍니다. 연속공격.
        InputControl();
        //중력적용.
        ApplyGravity();
        //공격 관련 컴포넌트 제어.
        AttackComponentControl();
	}
    /// <summary>
    /// 이동 관련 함수.
    /// </summary>
    void Move()
    {
        if(CannotMove == true)
        {
            return;
        }
        //MainCamera 게임오브젝트의 트랜스폼 컴포넌트.
        Transform CameraTransform = Camera.main.transform;
        //카메라가 바라보는 방향의 월드상에서는 어떤 방향인지 얻어옵니다.
        Vector3 forward = CameraTransform.TransformDirection(Vector3.forward);
        forward.y = 0.0f;
        Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);

        float vertical = Input.GetAxis("Vertical");//키보드의 위,아래,w,s -1 ~ 1
        float horizontal = Input.GetAxis("Horizontal");//키보드의 좌,우,a,d -1 ~1
        //우리가 이동하고자 하는 방향.
        Vector3 targetDirection = horizontal * right + vertical * forward;
        //현재 이동하는 방향에서 원하는 방향으로 조금씩 회전을하게 됩니다.
        MoveDirection = Vector3.RotateTowards(MoveDirection, targetDirection,
            DirectionRotateSpeed * Mathf.Deg2Rad * Time.deltaTime, 1000.0f);
        //방향이기때문에 크기는 없애고 방향만 가져옵니다.
        MoveDirection = MoveDirection.normalized;//
        //이동 속도.
        float speed = MoveSpeed;
        if(MyState == FighterState.Run)
        {
            speed = RunSpeed;
        }
        //중력 벡터.
        Vector3 gravityVector = new Vector3(0.0f, verticalSpeed, 0.0f);

        //이번 프레임에 움직일 양.
        Vector3 moveAmount = (MoveDirection * speed * Time.deltaTime) + gravityVector;
        //실제 이동.
        collisionFlags = myCharacterController.Move(moveAmount);

    }

    private void OnGUI()
    {
        //충돌 정보.
        GUILayout.Label("충돌 :" + collisionFlags.ToString());

        GUILayout.Label("현재 속도 : " + GetVelocitySpeed().ToString());
        //캐릭터콘트롤러 컴포넌트를 찾았고, 현재 내 캐릭터의 이동속도가 0이 아니라면.
        if(myCharacterController != null && myCharacterController.velocity != Vector3.zero)
        {
            //현재 내 캐릭터가 이동하는 방향 (+크기)
            GUILayout.Label("current Velocity Vector : " + myCharacterController.velocity.ToString());
            //현재 내 속도.
            GUILayout.Label("current Velocity Magnitude : " + myCharacterController.velocity.magnitude.ToString());
        }

    }
    /// <summary>
    /// 현재 내 캐릭터의 이동속도를 얻어옵니다.
    /// </summary>
    float GetVelocitySpeed()
    {
        //멈춰있다면.
        if(myCharacterController.velocity == Vector3.zero)
        {   //현재 속도를 0으로.
            CurrentVelocity = Vector3.zero;
        }else
        {
            Vector3 goalVelocity = myCharacterController.velocity;
            goalVelocity.y = 0.0f;
            CurrentVelocity = Vector3.Lerp(CurrentVelocity, goalVelocity,
                VelocityChangeSpeed * Time.fixedDeltaTime);
        }
        //currentVelocity의 크기를 리턴합니다.
        return CurrentVelocity.magnitude;
    }
    /// <summary>
    /// 몸통의 방향을 이동방향으로 돌려줍니다.
    /// </summary>
    void BodyDirectionChange()
    {
        //움직이고 있다면.
        if(GetVelocitySpeed() > 0.0f)
        {
            Vector3 newForward = myCharacterController.velocity;
            newForward.y = 0.0f;
            transform.forward = Vector3.Lerp(transform.forward, newForward, BodyRotateSpeed * Time.deltaTime);
        }
    }
    /// <summary>
    /// 애니메이션을 재생시키는 함수.
    /// </summary>
    void AnimationPlay(AnimationClip clip)
    {
        myAnimation.clip = clip;
        myAnimation.CrossFade(clip.name);
    }
    /// <summary>
    /// 내 상태에 맞추어 애니메이션을 재생시켜줍니다.
    /// </summary>
    void AniamtionControl()
    {
        switch(MyState)
        {
            case FighterState.Idle:
                AnimationPlay(IdleAnimClip);
                break;
            case FighterState.Walk:
                AnimationPlay(WalkAnimClip);
                break;
            case FighterState.Run:
                AnimationPlay(RunAnimClip);
                break;
            case FighterState.Attack:
                //공격상태에 맞춘 애니메이션을 재생시켜줍니다.
                AttackAnimationControl();
                break;
            case FighterState.Skill:
                AnimationPlay(SkillAnimClip);
                break;
            case FighterState.Die:
                myAnimation.CrossFade(DieAnimClip.name);
                break;
        }
    }
    /// <summary>
    /// 상태를 변경해주는 함수.
    /// </summary>
    void CheckState()
    {
        float currentSpeed = GetVelocitySpeed();
        switch(MyState)
        {
            case FighterState.Idle:
                if(currentSpeed > 0.0f)
                {
                    MyState = FighterState.Walk;
                }
                break;
            case FighterState.Walk:
                if(currentSpeed > 0.5f)
                {
                    MyState = FighterState.Run;
                }else if(currentSpeed < 0.01f)
                {
                    MyState = FighterState.Idle;
                }
                break;
            case FighterState.Run:
                if(currentSpeed < 0.5f)
                {
                    MyState = FighterState.Walk;
                }
                if(currentSpeed < 0.01f)
                {
                    MyState = FighterState.Idle;
                }

                break;
            case FighterState.Attack:
                CannotMove = true;
                break;
            case FighterState.Skill:
                CannotMove = true;
                break;
        }
    }
    /// <summary>
    /// 마우스 왼쪽 버튼으로 공격을 합니다. 
    /// </summary>
    void InputControl()
    {
        //0 마우스 왼쪽버튼 , 1 마우스 오른쪽 버튼, 2는 휠버튼.
        if(Input.GetMouseButtonDown(0) == true)
        {
            //내가 공격중이 아니라면 공격을 시작하게 되고.
            if(MyState != FighterState.Attack)
            {
                MyState = FighterState.Attack;
                AttackState = FighterAttackState.Attack1;
            }else
            {   //공격 중이라면 애니메이션이 일정 이상 재생이 돼었다면 다음 공격을 활성화.
                switch(AttackState)
                {
                    case FighterAttackState.Attack1:
                        if(myAnimation[Attack1AnimClip.name].normalizedTime > 0.1f)
                        {
                            NextAttack = true;
                        }
                        break;
                    case FighterAttackState.Attack2:
                        if(myAnimation[Attack2AnimClip.name].normalizedTime > 0.1f)
                        {
                            NextAttack = true;
                        }
                        break;
                    case FighterAttackState.Attack3:
                        if(myAnimation[Attack3AnimClip.name].normalizedTime > 0.1f)
                        {
                            NextAttack = true;
                        }
                        break;
                    case FighterAttackState.Attack4:
                        if(myAnimation[Attack4AnimClip.name].normalizedTime > 0.1f)
                        {
                            NextAttack = true;
                        }
                        break;
                }
            }
        }
        //마우스 오른쪽 버튼을 눌렀다면.
        //if(Input.GetMouseButtonDown(1) == true)
        if(Input.GetKey(KeyCode.F))
        {
            if(MyState == FighterState.Attack)
            {
                AttackState = FighterAttackState.Attack1;
                NextAttack = false;
            }
            MyState = FighterState.Skill;
        }
    }
    /// <summary>
    /// 공격 애니메이션 재생이 끝나면 호출되는 애니메이션 이벤트 함수입니다.
    /// </summary>
    void OnAttackAnimFinished()
    {
        if(NextAttack == true)
        {
            NextAttack = false;
            switch(AttackState)
            {
                case FighterAttackState.Attack1:
                    AttackState = FighterAttackState.Attack2;
                    break;
                case FighterAttackState.Attack2:
                    AttackState = FighterAttackState.Attack3;
                    break;
                case FighterAttackState.Attack3:
                    AttackState = FighterAttackState.Attack4;
                    break;
                case FighterAttackState.Attack4:
                    AttackState = FighterAttackState.Attack1;
                    break;
            }
        }else
        {
            CannotMove = false;
            MyState = FighterState.Idle;
            AttackState = FighterAttackState.Attack1;
        }
    }
    //스킬 애니메이션 재생이 끝났으면.
    void OnSkillAnimFinished()
    {
        Vector3 position = transform.position;
        position += transform.forward * 2.0f;
        Instantiate(SkillEffect, position, Quaternion.identity);
        MyState = FighterState.Idle;
        CannotMove = false;
    }

    /// <summary>
    /// 애니메이션 클립 재생이 끝날때쯤 애니메이션 이벤트 함수를 호출 시켜주도록 추가합니다.
    /// </summary>
    /// <param name="clip"></param>
    /// <param name="FuncName"></param>
    void AddAnimationEvent(AnimationClip clip, string FuncName)
    {
        AnimationEvent newEvent = new AnimationEvent();
        newEvent.functionName = FuncName;
        newEvent.time = clip.length - 0.1f;
        clip.AddEvent(newEvent);
    }
    /// <summary>
    /// 공격 애니메이션을 재생시켜줍니다.
    /// </summary>
    void AttackAnimationControl()
    {
        switch(AttackState)
        {
            case FighterAttackState.Attack1:
                AnimationPlay(Attack1AnimClip);
                break;
            case FighterAttackState.Attack2:
                AnimationPlay(Attack2AnimClip);
                break;
            case FighterAttackState.Attack3:
                AnimationPlay(Attack3AnimClip);
                break;
            case FighterAttackState.Attack4:
                AnimationPlay(Attack4AnimClip);
                break;
        }
    }
    /// <summary>
    /// 중력적용.
    /// </summary>
    void ApplyGravity()
    {
        //CollidedBelow가 세팅되었다면. -> 바닥에 붙었다면.
        if ((collisionFlags & CollisionFlags.CollidedBelow) != 0)
        {
            verticalSpeed = 0.0f;
        }else
        {
            verticalSpeed -= gravity * Time.deltaTime;
        }
    }
    /// <summary>
    /// 공격 관련 컴포넌트 제어.
    /// </summary>
    void AttackComponentControl()
    {
        switch(MyState)
        {
            //공격중일때만 트레일 컴포넌트와 충돌 컴포넌트 활성화.
            case FighterState.Attack:
            case FighterState.Skill:
                AttackTrailRenderer.enabled = true;
                AttackCapsuleCollider.enabled = true;
                break;
            default:
                AttackTrailRenderer.enabled = false;
                AttackCapsuleCollider.enabled = false;
                break;
        }
    }

    void OnDamageAnimFinished()
    {
        Debug.Log("Fighter Damage Animation finished");
    }
     
    void OnDieAnimFinished()
    {
        Debug.Log("Fighter Die Animation finished");
        //죽음 이펙트 생성.
        //Instantiate(DieEffect, myTransform.position, Quaternion.identity);
        //몬스터 삭제.
        Destroy(gameObject);
        //destroy.
    }

    /// <summary>
    /// 충돌 검출.
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        //플레이어의 공격에 맞았다면.
        if (other.gameObject.CompareTag("GoblinAttack") == true)
        {
            HP -= 12
                ;
            if (HP > 0)
            {
                //피격 이펙트 생성.
                //Instantiate(DamageEffect, transform.position, Quaternion.identity);
                //피격 애니메이션 재생.
                myAnimation.CrossFade(DamageAnimClip.name);
                //피격 트위닝 이펙트 재생.
                //DamageTweenEffect();
            }
            else
            {
                MyState = FighterState.Die;
            }
        }
    }

}
