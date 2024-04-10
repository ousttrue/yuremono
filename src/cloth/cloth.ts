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


// 制約（バネ）
class ClothConstraint {
  _p1: any = undefined; // 質点1
  _p2: any = undefined; // 質点2
  _rest = 0.0; // バネの自然長
  _type = 0; // バネの種類（0..構成バネ, 1..せん断バネ, 2..曲げバネ）
  constructor() {
  }
}


// Simple Cloth
export class Cloth {
  // 質点の作成と初期化
  _points: ClothPoint[] = [];
  // 制約の作成と初期化
  _constraints: ClothConstraint[] = [];
  _vertices: number[] = []; // 頂点座標
  _indeces: number[] = []; // 頂点インデクス

  constructor(
    /// スケーリング（ベースのサイズは2*2）
    public readonly _scale: number,
    /// 質点分割数
    public readonly _div: number) {

    for (let y = 0; y < this._div + 1; y++) {
      for (let x = 0; x < this._div + 1; x++) {
        const point = new ClothPoint();
        point._pos[0] = x / this._div * 2.0 - 1.0;
        point._pos[1] = 1.0;
        point._pos[2] = y / this._div * 2.0;
        vec3.scale(point._pos, point._pos, this._scale);
        vec3.copy(point._pre_pos, point._pos);
        point._weight = (y === 0) ? 0.0 : 1.0; // 落ちないように一辺を固定する
        this._points.push(point);
      }
    }

    for (let y = 0; y < this._div + 1; y++) {
      for (let x = 0; x < this._div + 1; x++) {
        // 構成バネ（Structural springs）
        this._constraints.push(this.genConstraint(x, y, -1, 0, 0)); // 左
        this._constraints.push(this.genConstraint(x, y, 0, -1, 0)); // 上

        // せん断バネ（Shear springs）
        this._constraints.push(this.genConstraint(x, y, -1, -1, 1)); // 左上
        this._constraints.push(this.genConstraint(x, y, 1, -1, 1)); // 右上

        // 曲げバネ（Bending springs）
        this._constraints.push(this.genConstraint(x, y, -2, 0, 2)); // １つ飛ばし左
        this._constraints.push(this.genConstraint(x, y, 0, -2, 2)); // １つ飛ばし上
      }
    }

    // 描画用頂点情報
    this.genVertices();
    this.genIndeces();
  }

  // 制約生成
  genConstraint(x: number, y: number,
    offsetX: number, offsetY: number, type: number) {
    const targetX = x + offsetX;
    const targetY = y + offsetY;
    if (targetX >= 0 && targetX < this._div + 1 && targetY >= 0 && targetY < this._div + 1) {
      const constraint = new ClothConstraint();
      constraint._p1 = this._points[y * (this._div + 1) + x];
      constraint._p2 = this._points[targetY * (this._div + 1) + targetX];
      constraint._rest = this._scale * 2.0 / this._div * Math.sqrt(offsetX * offsetX + offsetY * offsetY);
      constraint._type = type;
      return constraint;
    }
    return undefined; // 範囲外
  }

  // 頂点座標生成
  genVertices() {
    this._vertices = [];
    for (const point of this._points) {
      this._vertices.push(point._pos[0]);
      this._vertices.push(point._pos[1]);
      this._vertices.push(point._pos[2]);
    }
  }

  // 頂点インデクス生成
  genIndeces() {
    this._indeces = [];
    for (let y = 0; y < this._div; y++) {
      for (let x = 0; x < this._div; x++) {
        this._indeces.push(y * (this._div + 1) + x);
        this._indeces.push((y + 1) * (this._div + 1) + (x + 1));
        this._indeces.push(y * (this._div + 1) + (x + 1));
        this._indeces.push((y + 1) * (this._div + 1) + x);
      }
    }
  }

  // 更新
  update(step: number,
    f: vec3,
    r: number
  ) {
    // 積分フェーズ
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

  constraint(
    step: number,
    k: number, // 制約バネの特性（基本強度）
    structural_shrink: number, // 制約バネの特性（構成バネの伸び抵抗）
    structural_stretch: number, // 制約バネの特性（構成バネの縮み抵抗）
    shear_shrink: number, // 制約バネの特性（せん断バネの伸び抵抗）
    shear_stretch: number, // 制約バネの特性（せん断バネの縮み抵抗）
    bending_shrink: number, // 制約バネの特性（曲げバネの伸び抵抗）
    bending_stretch: number, // 制約バネの特性（曲げバネの縮み抵抗）
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

        // 伸び抵抗と縮み抵抗
        let shrink = 0.0; // 伸び抵抗
        let stretch = 0.0; // 縮み抵抗
        if (constraint._type === 0) {
          // 構成バネ
          shrink = structural_shrink;
          stretch = structural_stretch;
        }
        else if (constraint._type === 1) {
          // せん断バネ
          shrink = shear_shrink;
          stretch = shear_stretch;
        }
        else if (constraint._type === 2) {
          // 曲げバネ
          shrink = bending_shrink;
          stretch = bending_stretch;
        }

        // バネの力（スカラー）
        const d = vec3.distance(constraint._p2._pos, constraint._p1._pos); // 質点間の距離
        let f = (d - constraint._rest) * k; // 力（フックの法則、伸びに抵抗し、縮もうとする力がプラス）
        f >= 0 ? f *= shrink : f *= stretch; // 伸び抵抗と縮み抵抗に対して、それぞれ係数をかける

        // 変位
        const dx = vec3.create();
        vec3.sub(dx, constraint._p2._pos, constraint._p1._pos); // 力（スカラー）を力（ベクトル）に変換
        vec3.normalize(dx, dx); // 
        vec3.scale(dx, dx, f); // 
        vec3.scale(dx, dx, step * step * 0.5); // 力を変位に変換

        // 位置更新（二つの質点を互いに移動させる）
        const dx_p1 = vec3.create();
        vec3.scale(dx_p1, dx, constraint._p1._weight / (constraint._p1._weight + constraint._p2._weight));
        vec3.add(constraint._p1._pos, constraint._p1._pos, dx_p1);
        const dx_p2 = vec3.create();
        vec3.scale(dx_p2, dx, constraint._p2._weight / (constraint._p1._weight + constraint._p2._weight));
        vec3.sub(constraint._p2._pos, constraint._p2._pos, dx_p2);
      }
  }

  // 衝突判定フェーズ
  collision() {
    // 衝突判定用の球を適当に定義
    const sphere_pos = vec3.fromValues(0.0, 0.0, 0.0); // 球の中心位置
    const sphere_radius = 0.75; // 球の半径

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
