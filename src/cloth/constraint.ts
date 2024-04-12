import * as THREE from 'three';


// 質点
export class ClothPoint {
  _pos = new THREE.Vector3(); // 現在位置
  _pre_pos = new THREE.Vector3(); // 前の位置
  _weight = 0.0; // 運動計算の重み（固定点は0.0、そうでなければ1.0）
  constructor() {
  }
}


export interface SpringParams {
  k: number, // 制約バネの特性（基本強度）
  structural_shrink: number, // 制約バネの特性（構成バネの伸び抵抗）
  structural_stretch: number, // 制約バネの特性（構成バネの縮み抵抗）
  shear_shrink: number, // 制約バネの特性（せん断バネの伸び抵抗）
  shear_stretch: number, // 制約バネの特性（せん断バネの縮み抵抗）
  bending_shrink: number, // 制約バネの特性（曲げバネの伸び抵抗）
  bending_stretch: number, // 制約バネの特性（曲げバネの縮み抵抗）
};


// 制約（バネ）
export abstract class ClothConstraint {
  // バネの自然長
  _rest = 0.0;
  constructor(public readonly _p1: ClothPoint, public readonly _p2: ClothPoint,
    offsetX: number, offsetY: number,
    div: number, scale: number) {
    this._rest = scale * 2.0 / div * Math.sqrt(offsetX * offsetX + offsetY * offsetY);
  }

  protected _execute(step: number, k: number, shrink: number, stretch: number) {
    // バネの力（スカラー）
    const d = new THREE.Vector3().subVectors(this._p2._pos, this._p1._pos).length(); // 質点間の距離
    let f = (d - this._rest) * k; // 力（フックの法則、伸びに抵抗し、縮もうとする力がプラス）
    f >= 0 ? f *= shrink : f *= stretch; // 伸び抵抗と縮み抵抗に対して、それぞれ係数をかける

    // 変位
    const dx = new THREE.Vector3();
    dx.subVectors(this._p2._pos, this._p1._pos); // 力（スカラー）を力（ベクトル）に変換
    dx.normalize(); // 
    dx.multiplyScalar(f); // 
    dx.multiplyScalar(step * step * 0.5); // 力を変位に変換

    // 位置更新（二つの質点を互いに移動させる）
    const dx_p1 = new THREE.Vector3().copy(dx);
    dx_p1.multiplyScalar(this._p1._weight / (this._p1._weight + this._p2._weight));
    this._p1._pos.add(dx_p1);
    const dx_p2 = new THREE.Vector3().copy(dx);
    dx_p2.multiplyScalar(this._p2._weight / (this._p1._weight + this._p2._weight));
    this._p2._pos.sub(dx_p2);
  }

  abstract execute(step: number, params: SpringParams): void;
}


export class StructuralConstraint extends ClothConstraint {
  override execute(step: number, params: SpringParams) {
    this._execute(step, params.k, params.structural_shrink, params.structural_stretch);
  }
}

export class ShearConstraint extends ClothConstraint {
  override execute(step: number, params: SpringParams) {
    this._execute(step, params.k, params.structural_shrink, params.structural_stretch);
  }
}

export class BendingConstraint extends ClothConstraint {
  override execute(step: number, params: SpringParams) {
    this._execute(step, params.k, params.structural_shrink, params.structural_stretch);
  }
}
