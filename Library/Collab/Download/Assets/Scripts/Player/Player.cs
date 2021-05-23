using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class Player : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Other Components")]
    [SerializeField] private Text  Nickname;
    [SerializeField] private Image HPImage;

    public Sword OwnSword;

    [Header("Status")]
    [Range(0f, 100f)][SerializeField] private float          CurrentHP;
                     [SerializeField] private float          AccelSpeed;
                     [SerializeField] private float          MaxSpeed;
                     [SerializeField] private ParticleSystem HitParticle;

    public float HP
    {
        get { return CurrentHP; }
    }
    public bool IsDamaged = false;

    //Components
    private Rigidbody2D    mRigid = null;
    private Animator       mAnim = null;
    private SpriteRenderer mRenderer = null;
    private PhotonView     mView = null;

    private Vector3 mToward  = Vector3.zero;
    private bool    mFlipX   = false;
    private int     mCurSprite = 1;

    //InGameManager
    private InGameManager InGame = null;

    private void Awake()
    {
        mRigid = GetComponent<Rigidbody2D>();
        mAnim = GetComponent<Animator>();
        mRenderer = GetComponent<SpriteRenderer>();
        mView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        InGame = GameObject.Find("InGame Manager").GetComponent<InGameManager>();

        Nickname.text = mView.IsMine ? PhotonNetwork.NickName : mView.Owner.NickName;
    }

    private void Update()
    {
        if (mView.IsMine && !InGame.IsMenu)
        {
            AnimUpdate();
        }
        ValueUpdate();
    }

    private void AnimUpdate()
    {
        if (mToward == Vector3.zero)
            mView.RPC("AnimEnableRPC", RpcTarget.AllBuffered, false);
        else
            mView.RPC("AnimEnableRPC", RpcTarget.AllBuffered, true);

        float angle = OwnSword.CurrentAngle;

        if (angle >= -135 && angle < -45)
            mView.RPC("SpriteRPC", RpcTarget.AllBuffered, 1);
        if (angle >= 45 && angle < 135)
            mView.RPC("SpriteRPC", RpcTarget.AllBuffered, 2);
        if (angle >= -45 && angle < 45)
        {
            mView.RPC("SpriteRPC", RpcTarget.AllBuffered, 3);
            mView.RPC("FilpXRPC", RpcTarget.AllBuffered, true);
        }
        if (angle >= 135 || angle < -135)
        {
            mView.RPC("SpriteRPC", RpcTarget.AllBuffered, 3);
            mView.RPC("FilpXRPC", RpcTarget.AllBuffered, false);
        }
    }
    private void ValueUpdate()
    {
        mAnim.SetInteger("Walk", mCurSprite);
        HPImage.fillAmount = CurrentHP / 100f;
        mRenderer.sprite = Resources.Load<Sprite>($"Sprites/Player/Hero{mCurSprite}");
        mRenderer.flipX = mFlipX;
    }

    private void FixedUpdate()
    {
        if (mView.IsMine && !InGame.IsMenu)
        {
            Move();
        }
    }

    private void Move()
    {
        mToward = Vector3.zero;

        if (Input.GetKey(KeyCode.W))
            mToward += Vector3.up;
        if (Input.GetKey(KeyCode.S))
            mToward += Vector3.down;
        if (Input.GetKey(KeyCode.A))
            mToward += Vector3.left;
        if (Input.GetKey(KeyCode.D))
            mToward += Vector3.right;

        if (mRigid.velocity.x < -MaxSpeed)
            mRigid.velocity = new Vector2(-MaxSpeed, mRigid.velocity.y);
        if (mRigid.velocity.x > MaxSpeed)
            mRigid.velocity = new Vector2(MaxSpeed, mRigid.velocity.y);
        if (mRigid.velocity.y < -MaxSpeed)
            mRigid.velocity = new Vector2(mRigid.velocity.x, -MaxSpeed);
        if (mRigid.velocity.y > MaxSpeed)
            mRigid.velocity = new Vector2(mRigid.velocity.x, MaxSpeed);

        mRigid.AddForce(mToward * AccelSpeed * 10f * Time.deltaTime, ForceMode2D.Impulse);
    }
    public void Damage(float damage)
    {
        mView.RPC("CurrentHPRPC", RpcTarget.AllBuffered, -damage);
        if (mDamaged != null) StopCoroutine(mDamaged);
        mDamaged = StartCoroutine(Damaged());
    }
    private Coroutine mDamaged = null;
    private IEnumerator Damaged()
    {
        mView.RPC("SetColorRPC", RpcTarget.AllBuffered, 0.5f, 0.5f, 0.5f, 0.5f);

        mView.RPC("IsDamagedRPC", RpcTarget.AllBuffered, true);

        while (mRenderer.color.r < 1)
        {
            mView.RPC("AddColorRPC", RpcTarget.AllBuffered, 0.02f, 0.02f, 0.02f, 0.02f);
            yield return new WaitForSecondsRealtime(0.001f);
        }

        mView.RPC("IsDamagedRPC", RpcTarget.AllBuffered, false);

        mDamaged = null;
    }

    [PunRPC] private void AnimEnableRPC(bool isEnable) => mAnim.enabled = isEnable;
    [PunRPC] private void SpriteRPC(int anim) => mCurSprite = anim;
    [PunRPC] private void FilpXRPC(bool isFlip) => mFlipX = isFlip;
    [PunRPC] private void CurrentHPRPC(float value) => CurrentHP += value;
    [PunRPC] private void SetColorRPC(float r, float g, float b, float a) => mRenderer.color = new Color(r, g, b, a);
    [PunRPC] private void AddColorRPC(float r, float g, float b, float a) => mRenderer.color += new Color(r, g, b, a);
    [PunRPC] private void IsDamagedRPC(bool isDamaged) => IsDamaged = isDamaged;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
