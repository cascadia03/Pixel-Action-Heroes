using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class Sword : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] private Transform      RotatePivot;
    [SerializeField] private Transform      SpritePivot;
    [SerializeField] private ParticleSystem SwordParticle;
    [SerializeField] private TriggerSystem  Trigger;

    private PhotonView       mView = null;
    private List<GameObject> mCollideded = new List<GameObject>();

    private int   mAttackDir = 1;
    private float mCurrentAngle = 0f;
    private float mPivotAngle = -120f;
    private float mSpriteAngle = 315f;
    private float mSpritePos = 0.8f;
    private bool  mIsAttack = false;
    private bool  mIsGuard = false;

    public float CurrentAngle
    {
        get { return mCurrentAngle; }
    }
    public bool IsGuard
    {
        get { return mIsGuard; }
    }

    //InGameManager
    private InGameManager InGame = null;

    private void Awake()
    {
        mView = GetComponent<PhotonView>();
    }

    private void Start()
    {
        InGame = GameObject.Find("InGame Manager").GetComponent<InGameManager>();
    }

    private void Update()
    {
        if (mView.IsMine && !InGame.IsMenu)
        {
            AnimUpdate();
            Attack();
            Defense();
            CollisionEvent();
        }
    }

    private void AnimUpdate()
    {
        Vector3 direction = Input.mousePosition - new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2);
        mView.RPC("CurrentAngleRPC", RpcTarget.AllBuffered, Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg);

        RotatePivot.localEulerAngles = new Vector3(0, 0, mCurrentAngle + mPivotAngle);
        SpritePivot.localEulerAngles = new Vector3(0, 0, mSpriteAngle);
        SpritePivot.localPosition = new Vector3(mSpritePos, 0, 0);
    }
    private void Attack()
    {
        if (Input.GetMouseButton(0) && !Input.GetMouseButton(1) && !mIsAttack && !mIsGuard)
            StartCoroutine(Swing());
    }
    private void Defense()
    {
        if (Input.GetMouseButton(1) && !mIsAttack && !mIsGuard)
        {
            if (mGuard != null) StopCoroutine(mGuard);
            mGuard = StartCoroutine(Guard());
        }
        if (Input.GetMouseButtonUp(1) && !mIsAttack && mIsGuard)
        {
            if (mGuard != null) StopCoroutine(mGuard);
            mGuard = StartCoroutine(UnGuard());
        }
    }
    private void CollisionEvent()
    {
        if (mIsAttack)
        {
            Trigger.IsActive = true;

            if (Trigger.Collided)
            {
                GameObject @object = Trigger.Collided;

                if (@object.tag == "Player" && !@object.GetComponent<PhotonView>().IsMine)
                    if (!mCollideded.Contains(@object))
                    {
                        mCollideded.Add(Trigger.Collided);

                        Player player = @object.GetComponent<Player>();

                        if (player.OwnSword.IsGuard)
                        {
                            float angle = Mathf.Abs(mCurrentAngle - player.OwnSword.CurrentAngle + 180f);

                            if (angle > 360f) angle -= 360f;
                            if (angle < 0f) angle += 360f;

                            if (angle <= 60f) return;
                        }

                        player.Damage(12);
                    }
            }
        }
        else
            Trigger.IsActive = false;
    }
    private IEnumerator Swing()
    {
        mView.RPC("IsAttackRPC", RpcTarget.AllBuffered, true);

        float accel = 10f;

        mView.RPC("SwordPlayRPC", RpcTarget.AllBuffered);

        while (mPivotAngle >= -120f && mPivotAngle <= 120f)
        {
            mPivotAngle += (accel += 2f) * mAttackDir;

            yield return new WaitForSeconds(0.001f);
        }

        mPivotAngle = 120f * mAttackDir;

        mAttackDir = -mAttackDir;

        mCollideded.Clear();

        yield return new WaitForSeconds(0.3f);

        mView.RPC("IsAttackRPC", RpcTarget.AllBuffered, false);

        mView.RPC("SwordStopRPC", RpcTarget.AllBuffered);
    }
    private Coroutine mGuard = null;
    private IEnumerator Guard()
    {
        mView.RPC("IsGuardRPC", RpcTarget.AllBuffered, true);

        while (true)
        {
            mPivotAngle += 12f * mAttackDir;
            mSpriteAngle += 9f * mAttackDir;
            mSpritePos -= 0.03f;

            yield return new WaitForSeconds(0.001f);

            if (mAttackDir == 1f && mPivotAngle > 0f) { break; }
            if (mAttackDir == -1f && mPivotAngle < 0f) { break; }
        }

        mPivotAngle = 0f;
        mSpriteAngle = (mAttackDir == 1) ? 45f : 225f;
        mSpritePos = 0.5f;

        mGuard = null;
    }
    private IEnumerator UnGuard()
    {
        while (mPivotAngle >= -120f && mPivotAngle <= 120f)
        {
            mPivotAngle -= 12f * mAttackDir;
            mSpriteAngle -= 9f * mAttackDir;
            mSpritePos += 0.03f;

            yield return new WaitForSeconds(0.001f);
        }

        mPivotAngle = 120f * -mAttackDir;
        mSpriteAngle = 315f;
        mSpritePos = 0.8f;

        mView.RPC("IsGuardRPC", RpcTarget.AllBuffered, false);

        mGuard = null;
    }
    [PunRPC] private void SwordPlayRPC() => SwordParticle.Play();
    [PunRPC] private void SwordStopRPC() => SwordParticle.Stop();
    [PunRPC] private void CurrentAngleRPC(float angle) => mCurrentAngle = angle;
    [PunRPC] private void IsAttackRPC(bool isAttack) => mIsAttack = isAttack;
    [PunRPC] private void IsGuardRPC(bool isGuard) => mIsGuard = isGuard;

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
    }
}
