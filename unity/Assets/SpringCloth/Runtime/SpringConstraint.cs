using System;
using UnityEngine;

namespace SpringCloth
{
    public class SpringConstraint
    {
        // ４つの質点を参照する。
        // d--c
        // |  |
        // a--b
        // - a=b
        // - a-c
        // - b-d
        // を拘束する
        StrandParticle _p0;

        StrandParticle _p1;

        float _rest;

        public SpringConstraint(StrandParticle p0, StrandParticle p1)
        {
            _p0 = p0;
            _p1 = p1;
            _rest = Vector3.Distance(p0.transform.position, p1.transform.position);
            Debug.Log($"SpringConstraint: {p0.transform}, {p1.transform}");
        }

        // ４つの質点を参照する。
        // d--c
        // |  |
        // a--b
        //
        // TODO: 三角形 a-b-(d+c/2) と衝突
        public Vector3 Collision(Vector3 newPos, Func<Vector3, Vector3> constraint)
        {
            return newPos;
        }

        /// <summary>
        ///  フックの法則
        /// </summary>
        /// <returns></returns>
        public void Resolve(float delta, float hookean)
        {
            // バネの力（スカラー）
            var d = Vector3.Distance(this._p0.transform.position, this._p1.transform.position); // 質点間の距離
            var f = (d - this._rest) * hookean; // 力（フックの法則、伸びに抵抗し、縮もうとする力がプラス）
                                                // f >= 0 ? f *= shrink : f *= stretch; // 伸び抵抗と縮み抵抗に対して、それぞれ係数をかける

            var dx = (this._p1.transform.position - this._p0.transform.position).normalized * f;

            // return (_p0, _p1, dx);

            // Debug.Log($"{p0.transform} <=> {p1.transform} = {f}");
            _p0.AddForce(dx);
            _p1.AddForce(-dx);
        }

        public void DrawGizmo()
        {
            Gizmos.DrawLine(_p0.transform.position, _p1.transform.position);
        }
    }
}