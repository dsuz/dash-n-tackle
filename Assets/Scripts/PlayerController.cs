using UnityEngine;
using Cinemachine;
using Photon.Pun;
using System;
using System.Linq;

/// <summary>
/// プレイヤーを制御するコンポーネント
/// ・通常時は上下左右で動ける
/// ・敵に近づくとロックオンできる
/// ・ロックオン中に Fire1 を押すと、対象に向かってダッシュする
/// ・ダッシュ中は操作できない
/// ・ダッシュの対象に体当たりできたら、ダッシュ状態は解除されて操作可能な通常状態に戻る
/// </summary>
public class PlayerController : MonoBehaviour
{
    /// <summary>仮想カメラの Follow</summary>
    [SerializeField] Transform _cameraFollowTarget = default;
    /// <summary>仮想カメラの LookAt</summary>
    [SerializeField] Transform _cameraLookAtTarget = default;
    /// <summary>移動する力</summary>
    [SerializeField] float _movePower = 3;
    /// <summary>ロックオン可能な距離</summary>
    [SerializeField] float _lockOnDistance = 5;
    /// <summary>ダッシュ力</summary>
    [SerializeField] float _dashPower = 40f;
    Rigidbody _rb = default;
    Animator _anim = default;
    /// <summary>入力された方向の XZ 平面でのベクトル</summary>
    Vector3 _dir;
    PhotonView _view = default;
    /// <summary>ロックオンしているターゲット</summary>
    Transform _lockedTarget = default;
    /// <summary>ダッシュ・体当たりの対象となっているターゲット</summary>
    Transform _dashTarget = default;

    /// <summary>
    /// ロックオンしているターゲット。何もロックオンしていない時は null
    /// </summary>
    public Transform LockedTarget
    {
        get { return _lockedTarget; }
    }

    void Start()
    {
        _rb = GetComponent<Rigidbody>();
        _anim = GetComponent<Animator>();
        _view = GetComponent<PhotonView>();

        if (_view.IsMine)
        {
            SetUpVirtualCamera();
        }
    }

    /// <summary>
    /// 仮想カメラに対して、Follow, LookAt を設定する
    /// </summary>
    void SetUpVirtualCamera()
    {
        Array.ForEach(GameObject.FindObjectsOfType<CinemachineVirtualCameraBase>(), c =>
        {
            c.Follow = _cameraFollowTarget;
            c.LookAt = _cameraLookAtTarget;
        });
    }

    void Update()
    {
        // 入力を受け付ける
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        _dir = Vector3.forward * v + Vector3.right * h;
        // カメラのローカル座標系を基準に dir を変換する
        _dir = Camera.main.transform.TransformDirection(_dir);
        // カメラは斜め下に向いているので、Y 軸の値を 0 にして「XZ 平面上のベクトル」にする
        _dir.y = 0;

        // キャラクターを「現在の（XZ 平面上の）進行方向」に向ける
        Vector3 xzVelocity = _rb.velocity;
        xzVelocity.y = 0;

        if (xzVelocity != Vector3.zero)
        {
            this.transform.forward = xzVelocity;
        }

        // ロックオン
        _lockedTarget = FindLockonTarget();

        if (_lockedTarget && !_dashTarget && Input.GetButtonDown("Fire1"))
        {
            _dashTarget = _lockedTarget;
        }
    }

    void FixedUpdate()
    {
        if (_dashTarget)
        {
            // ダッシュ中は操作は利かず、ひたすらターゲットを追いかける
            Vector3 dir = _dashTarget.position - this.transform.position;
            dir.y = 0;
            _rb.AddForce(dir.normalized * _dashPower, ForceMode.Force);
        }
        else
        {
            // 通常時は操作して動かす
            _rb.AddForce(_dir.normalized * _movePower, ForceMode.Force);
        }
    }

    void LateUpdate()
    {
        if (_anim)
        {
            Vector3 xzVelocity = _rb.velocity;
            xzVelocity.y = 0;
            _anim.SetFloat("Speed", xzVelocity.magnitude);
        }
    }

    /// <summary>
    /// ロックオン対象を探す
    /// </summary>
    /// <returns>ロックオン対象。それがない場合は null</returns>
    Transform FindLockonTarget()
    {
        var go = GameObject.FindGameObjectsWithTag("Player")
            .Where(p => Vector3.Distance(this.transform.position, p.transform.position) < _lockOnDistance && !p.Equals(this.gameObject))
            .OrderBy(p => Vector3.Distance(this.transform.position, p.transform.position)).FirstOrDefault();
        return go ? go.transform : null;
    }

    void OnCollisionEnter(Collision collision)
    {
        // 体当たりの対象に体当たりできたら、ターゲットからはずす
        if (_dashTarget && collision.gameObject.Equals(_dashTarget.gameObject))
        {
            _dashTarget = null;
        }
    }
}
