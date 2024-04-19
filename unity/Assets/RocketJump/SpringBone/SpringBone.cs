using UnityEngine;
using System.Collections;

public class SpringBone : MonoBehaviour
{
    //次のボーン
    public Transform _child;

    //ボーンの向き
    public Vector3 _boneAxis = new Vector3(0.0f, 1.0f, 0.0f);

    public float _radius = 0.5f;

    //バネが戻る力
    public float _stiffnessForce = 0.2f;

    //力の減衰力
    public float _dragForce = 0.1f;

    public Vector3 _springForce = new Vector3(0.0f, -0.05f, 0.0f);

    public SpringCollider[] _colliders;

    private float _springLength;
    private Quaternion _localRotation;
    private Vector3 _currTipPos;
    private Vector3 _prevTipPos;

    private void Awake()
    {
        _localRotation = transform.localRotation;
        _springLength = Vector3.Distance(transform.position, _child.position);
        _currTipPos = _child.position;
        _prevTipPos = _child.position;
    }

    public void UpdateSpring()
    {
        //回転をリセット
        transform.localRotation = Quaternion.identity * _localRotation;

        float sqrDt = Time.deltaTime * Time.deltaTime;

        //stiffness
        Vector3 force = transform.rotation * (_boneAxis * _stiffnessForce) / sqrDt;

        //drag
        force += (_prevTipPos - _currTipPos) * _dragForce / sqrDt;

        force += _springForce / sqrDt;

        //前フレームと値が同じにならないように
        Vector3 temp = _currTipPos;

        //verlet
        _currTipPos = (_currTipPos - _prevTipPos) + _currTipPos + (force * sqrDt);

        //長さを元に戻す
        _currTipPos = ((_currTipPos - transform.position).normalized * _springLength) + transform.position;

        //衝突判定
        for (int i = 0; i < _colliders.Length; i++)
        {
            if (Vector3.Distance(_currTipPos, _colliders[i].transform.position) <= (_radius + _colliders[i].radius))
            {
                Vector3 normal = (_currTipPos - _colliders[i].transform.position).normalized;
                _currTipPos = _colliders[i].transform.position + (normal * (_radius + _colliders[i].radius));
                _currTipPos = ((_currTipPos - transform.position).normalized * _springLength) + transform.position;
            }
        }

        _prevTipPos = temp;

        //回転を適用；
        Vector3 aimVector = transform.TransformDirection(_boneAxis);
        Quaternion aimRotation = Quaternion.FromToRotation(aimVector, _currTipPos - transform.position);
        transform.rotation = aimRotation * transform.rotation;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        if (Application.isPlaying)
        {
            Gizmos.DrawWireSphere(_currTipPos, _radius);
            Gizmos.DrawLine(transform.position, _currTipPos);
        }
        else if (_child != null)
        {
            Gizmos.DrawWireSphere(_child.position, _radius);
        }
    }

}
