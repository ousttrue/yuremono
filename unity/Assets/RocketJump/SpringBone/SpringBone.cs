using UnityEngine;
using System.Collections.Generic;
using System;

public class SpringBone : MonoBehaviour
{
    //次のボーン
    public Transform _child;

    public float _radius = 0.5f;

    //バネが戻る力
    [Range(0, 1)]
    public float _stiffnessForce = 0.2f;

    //力の減衰力
    [Range(0, 1)]
    public float _dragForce = 0.1f;

    public Vector3 _springForce = new Vector3(0.0f, -0.05f, 0.0f);

    public SpringCollider[] _colliders;

    private Vector3 _boneAxis;
    private float _springLength;
    private Quaternion _localRotation;
    private Vector3 _currTipPos;
    private Vector3 _prevTipPos;

    private void Awake()
    {
        _boneAxis = _child.localPosition.normalized;
        _localRotation = transform.localRotation;
        _springLength = Vector3.Distance(transform.position, _child.position);
        _currTipPos = _child.position;
        _prevTipPos = _child.position;
    }

    public void UpdateSpring()
    {
        var restRotation = transform.parent.rotation * _localRotation;
        var restPosition = transform.position + restRotation * _boneAxis;
        var newPos = NewPosition(Time.deltaTime, restPosition, transform.position, _stiffnessForce, _springForce,
            _currTipPos, _prevTipPos, _dragForce,
            _springLength, _radius, _colliders);
        _prevTipPos = _currTipPos;
        _currTipPos = newPos;
        transform.rotation = CalcRotation(restRotation, _boneAxis, _currTipPos - transform.position);
    }

    //回転を適用；
    static Quaternion CalcRotation(Quaternion restRotation, Vector3 boneAxis, Vector3 to)
    {
        Quaternion aimRotation = Quaternion.FromToRotation(restRotation * boneAxis, to);
        return aimRotation * restRotation;
    }

    static Vector3 NewPosition(float deltaTime, Vector3 rest, Vector3 pos,
        float stiffness, Vector3 externalForce,
        Vector3 currTipPos, Vector3 prevTipPos, float dragForce,
        float constraintLen, float collisionRadius, IReadOnlyList<SpringCollider> colliders)
    {
        var f = StiffnessForce(rest, pos, stiffness);
        f += externalForce;
        var newPos = Verlet(f, currTipPos, prevTipPos, dragForce);
        newPos = Constraint(newPos, pos, constraintLen);
        foreach (var c in colliders)
        {
            if (c.TryCollide(newPos, collisionRadius, out Vector3 resolved))
            {
                newPos = Constraint(resolved, pos, constraintLen);
            }
        }
        return newPos;
    }

    static Vector3 StiffnessForceOriginal(Quaternion rotation, Vector3 boneAxis, float stiffness)
    {
        return rotation * (boneAxis * stiffness);
    }

    static Vector3 StiffnessForce(Vector3 restPosition, Vector3 currTipPos, float stiffness)
    {
        return (restPosition - currTipPos) * stiffness;
    }

    static Vector3 Verlet(Vector3 force, Vector3 currTipPos, Vector3 prevTipPos, float dragForce)
    {
        return currTipPos + (currTipPos - prevTipPos) * (1 - dragForce) + force;
    }

    //長さを元に戻す
    static Vector3 Constraint(Vector3 to, Vector3 from, float len)
    {
        return from + (to - from).normalized * len;
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
