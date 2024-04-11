//
// Simple Cloth
//
import { vec3 } from 'gl-matrix';


// 質点
class ClothPoint {
  _pos = vec3.create(); // 現在位置
  _pre_pos = vec3.create(); // 前の位置
  _weight = 0.0; // 運動計算の重み（固定点は0.0、そうでなければ1.0）
  constructor() {
  }
}


export interface ClothParams {
  k: number, // 制約バネの特性（基本強度）
  structural_shrink: number, // 制約バネの特性（構成バネの伸び抵抗）
  structural_stretch: number, // 制約バネの特性（構成バネの縮み抵抗）
  shear_shrink: number, // 制約バネの特性（せん断バネの伸び抵抗）
  shear_stretch: number, // 制約バネの特性（せん断バネの縮み抵抗）
  bending_shrink: number, // 制約バネの特性（曲げバネの伸び抵抗）
  bending_stretch: number, // 制約バネの特性（曲げバネの縮み抵抗）
};


// 制約（バネ）
abstract class ClothConstraint {
  // バネの自然長
  _rest = 0.0;
  constructor(public readonly _p1: ClothPoint, public readonly _p2: ClothPoint,
    offsetX: number, offsetY: number,
    div: number, scale: number) {
    this._rest = scale * 2.0 / div * Math.sqrt(offsetX * offsetX + offsetY * offsetY);
  }

  protected _execute(step: number, k: number, shrink: number, stretch: number) {
    // バネの力（スカラー）
    const d = vec3.distance(this._p2._pos, this._p1._pos); // 質点間の距離
    let f = (d - this._rest) * k; // 力（フックの法則、伸びに抵抗し、縮もうとする力がプラス）
    f >= 0 ? f *= shrink : f *= stretch; // 伸び抵抗と縮み抵抗に対して、それぞれ係数をかける

    // 変位
    const dx = vec3.create();
    vec3.sub(dx, this._p2._pos, this._p1._pos); // 力（スカラー）を力（ベクトル）に変換
    vec3.normalize(dx, dx); // 
    vec3.scale(dx, dx, f); // 
    vec3.scale(dx, dx, step * step * 0.5); // 力を変位に変換

    // 位置更新（二つの質点を互いに移動させる）
    const dx_p1 = vec3.create();
    vec3.scale(dx_p1, dx, this._p1._weight / (this._p1._weight + this._p2._weight));
    vec3.add(this._p1._pos, this._p1._pos, dx_p1);
    const dx_p2 = vec3.create();
    vec3.scale(dx_p2, dx, this._p2._weight / (this._p1._weight + this._p2._weight));
    vec3.sub(this._p2._pos, this._p2._pos, dx_p2);
  }

  abstract execute(step: number, params: ClothParams): void;
}


class StructuralConstraint extends ClothConstraint {
  override execute(step: number, params: ClothParams) {
    this._execute(step, params.k, params.structural_shrink, params.structural_stretch);
  }
}

class ShearConstraint extends ClothConstraint {
  override execute(step: number, params: ClothParams) {
    this._execute(step, params.k, params.structural_shrink, params.structural_stretch);
  }
}

class BendingConstraint extends ClothConstraint {
  override execute(step: number, params: ClothParams) {
    this._execute(step, params.k, params.structural_shrink, params.structural_stretch);
  }
}


// Simple Cloth
export class Cloth {
  // 質点の作成と初期化
  _points: ClothPoint[] = [];
  // 制約の作成と初期化
  _constraints: ClothConstraint[] = [];

  constructor(
    /// 質点分割数
    div: number,
    /// スケーリング（ベースのサイズは2*2）
    scale: number,
  ) {

    // create points
    for (let y = 0; y < div + 1; y++) {
      for (let x = 0; x < div + 1; x++) {
        const point = new ClothPoint();
        point._pos[0] = x / div * 2.0 - 1.0;
        point._pos[1] = 1.0;
        point._pos[2] = y / div * 2.0;
        vec3.scale(point._pos, point._pos, scale);
        vec3.copy(point._pre_pos, point._pos);
        point._weight = (y === 0) ? 0.0 : 1.0; // 落ちないように一辺を固定する
        this._points.push(point);
      }
    }

    // create constraints
    for (let y = 0; y < div + 1; y++) {
      for (let x = 0; x < div + 1; x++) {
        // +->x
        // |
        // y
        const p = this.getPoint(div, x, y);

        // Structural
        // 左
        const left = this.getPoint(div, x - 1, y);
        if (left) {
          this._constraints.push(new StructuralConstraint(p, left, -1, 0, div, scale));
        }
        // 上
        const up = this.getPoint(div, x, y - 1);
        if (up) {
          this._constraints.push(new StructuralConstraint(p, up, 0, -1, div, scale));
        }

        // Shear
        // 左上
        const leftup = this.getPoint(div, x - 1, y - 1);
        if (leftup) {
          this._constraints.push(new ShearConstraint(p, leftup, -1, -1, div, scale));
        }
        // 右上
        const rightup = this.getPoint(div, x + 1, y - 1);
        if (rightup) {
          this._constraints.push(new ShearConstraint(p, rightup, 1, -1, div, scale));
        }

        // Bending springs
        // １つ飛ばし左
        const leftleft = this.getPoint(div, x - 2, y);
        if (leftleft) {
          this._constraints.push(new BendingConstraint(p, leftleft, -2, 0, div, scale));
        }
        // １つ飛ばし上
        const upup = this.getPoint(div, x, y - 2);
        if (upup) {
          this._constraints.push(new BendingConstraint(p, upup, 0, -2, div, scale));
        }
      }
    }
  }

  getPoint(div: number, x: number, y: number) {
    if (x >= 0 && x < div + 1 && y >= 0 && y < div + 1) {
      return this._points[y * (div + 1) + x];
    }
    return undefined; // 範囲外
  }

  // 積分フェーズ
  integrate(
    f: vec3,
    r: number
  ) {
    for (const point of this._points) {
      // 変位
      const dx = vec3.create();
      vec3.sub(dx, point._pos, point._pre_pos); // 速度分
      vec3.copy(point._pre_pos, point._pos); // 更新前の位置を記録しておく
      vec3.add(dx, dx, f); // 力の変位を足しこむ
      vec3.scale(dx, dx, r); // 抵抗

      // 位置更新
      vec3.scale(dx, dx, point._weight); // 固定点は動かさない
      vec3.add(point._pos, point._pos, dx);
    }
  }

  // 拘束フェーズ
  constraint(
    step: number,
    params: ClothParams
  ) {
    for (const constraint of this._constraints) {
      if (constraint === undefined) {
        // 無効な制約
        continue;
      }
      if (constraint._p1._weight + constraint._p2._weight === 0.0) {
        // 二つの質点がお互いに固定点であればスキップ（0除算防止）
        continue;
      }
      constraint.execute(step, params);
    }
  }

  // 衝突判定フェーズ
  collision(sphere_pos: vec3, sphere_radius: number) {
    for (const point of this._points) {
      // 球とのヒット
      const v = vec3.create();
      vec3.sub(v, point._pos, sphere_pos);
      const d = vec3.length(v);
      if (d < sphere_radius) {
        // ヒットしたので球の表面に押し出す
        vec3.scale(v, v, sphere_radius / d);
        vec3.add(point._pos, sphere_pos, v);
      }
    }
  }
}
