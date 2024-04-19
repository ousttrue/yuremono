using UnityEngine;

namespace SpringCloth
{
    public class SpringConstraint
    {
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

        public void Draw()
        {
            Gizmos.DrawLine(_p0.transform.position, _p1.transform.position);
        }

        /// <summary>
        ///  フックの法則
        /// </summary>
        /// <returns></returns>
        public (StrandParticle, StrandParticle, Vector3) Resolve(float delta, float hookean)
        {
            // protected _execute(step: number, k: number, shrink: number, stretch: number) {
            // バネの力（スカラー）
            var d = Vector3.Distance(this._p0.transform.position, this._p1.transform.position); // 質点間の距離
            var f = (d - this._rest) * hookean; // 力（フックの法則、伸びに抵抗し、縮もうとする力がプラス）
                                                // f >= 0 ? f *= shrink : f *= stretch; // 伸び抵抗と縮み抵抗に対して、それぞれ係数をかける

            // 変位
            var dx = (this._p1.transform.position - this._p0.transform.position).normalized * f * 0.5f * delta * delta;

            // 位置更新（二つの質点を互いに移動させる）
            // const dx_p1 = new THREE.Vector3().copy(dx);
            // dx_p1.multiplyScalar(this._p1._weight / (this._p1._weight + this._p2._weight));
            // this._p1._pos.add(dx_p1);
            // const dx_p2 = new THREE.Vector3().copy(dx);
            // dx_p2.multiplyScalar(this._p2._weight / (this._p1._weight + this._p2._weight));
            // this._p2._pos.sub(dx_p2);

            return (_p0, _p1, dx);
        }
    }
}