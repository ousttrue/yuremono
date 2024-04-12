//
// Simple Cloth
//
import * as THREE from 'three';
import {
  ClothPoint, SpringParams,
  ClothConstraint, StructuralConstraint, BendingConstraint, ShearConstraint
} from './constraint';


export interface SimulationParams {
  relaxation: number;
  collision: boolean; // 球との衝突判定
  g: number; // 重力
  w: number; // 風力
  r: number; // 抵抗
};


export interface Sphere {
  position: THREE.Vector3;
  radius: number;
}


// Simple Cloth
export class Cloth {
  /// 累積時間
  ms_acc = 0;
  /// 更新処理の余剰時間（次フレームに持ち越す分）
  ms_surplus = 0;

  // 質点の作成と初期化
  _points: ClothPoint[] = [];
  // 制約の作成と初期化
  _constraints: ClothConstraint[] = [];

  root: THREE.Object3D;

  constructor(
    /// 質点分割数
    public readonly div: number,
    /// スケーリング（ベースのサイズは2*2）
    public readonly scale: number,
  ) {

    // create points
    for (let y = 0; y < div + 1; y++) {
      for (let x = 0; x < div + 1; x++) {
        const point = new ClothPoint();
        point._pos[0] = x / div * 2.0 - 1.0;
        point._pos[1] = 1.0;
        point._pos[2] = y / div * 2.0;
        point._pos.multiplyScalar(scale);
        point._pre_pos.copy(point._pos);
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

    this.makeScene()
  }

  getPoint(div: number, x: number, y: number) {
    if (x >= 0 && x < div + 1 && y >= 0 && y < div + 1) {
      return this._points[y * (div + 1) + x];
    }
    return undefined; // 範囲外
  }

  makeScene() {
    const geometry = new THREE.BufferGeometry();

    const vertices = new Float32Array(this._points.length * 3);
    for (let i = 0; i < this._points.length; ++i) {
      const point = this._points[i];
      vertices[i * 3 + 0] = point._pos[0];
      vertices[i * 3 + 1] = point._pos[1];
      vertices[i * 3 + 2] = point._pos[2];
    }
    geometry.setAttribute("position", new THREE.BufferAttribute(vertices, 3));

    const indices: number[] = [];
    for (let y = 0; y < this.div; y++) {
      for (let x = 0; x < this.div; x++) {
        indices.push(y * (this.div + 1) + x);
        indices.push((y + 1) * (this.div + 1) + (x + 1));
        indices.push(y * (this.div + 1) + (x + 1));
        indices.push((y + 1) * (this.div + 1) + x);
      }
    }
    geometry.setIndex(indices);

    const material = new THREE.MeshBasicMaterial({ color: 0xffffff });
    this.root = new THREE.LineSegments(geometry, material);
  }

  updateVertices()
  {
  }

  // 積分フェーズ
  integrate(
    f: THREE.Vector3,
    r: number
  ) {
    for (const point of this._points) {
      // 変位
      const dx = new THREE.Vector3();
      dx.subVectors(point._pos, point._pre_pos); // 速度分
      point._pre_pos.copy(point._pos); // 更新前の位置を記録しておく
      dx.add(f); // 力の変位を足しこむ
      dx.multiplyScalar(r); // 抵抗

      // 位置更新
      dx.multiplyScalar(point._weight); // 固定点は動かさない
      point._pos.add(dx);
    }
  }

  // 拘束フェーズ
  constraint(
    step: number,
    params: SpringParams
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
  collision(sphere: Sphere) {
    for (const point of this._points) {
      // 球とのヒット
      const v = new THREE.Vector3();
      v.subVectors(point._pos, sphere.position);
      const d = v.length();
      if (d < sphere.radius) {
        // ヒットしたので球の表面に押し出す
        v.multiplyScalar(sphere.radius / d);
        point._pos.add(v);
      }
    }
  }

  onFrame(delta: number, spring_params: SpringParams, simulation: SimulationParams, collider: Sphere) {
    const ms_step = 16; // シミュレーションのタイムステップ（固定）
    const step = ms_step / 1000.0;
    const acc = this.ms_acc / 1000.0; // 累積時間（秒）

    // 質点の質量は質点分割数にかかわらず1.0固定とします
    // ただ、これだと質点分割数によって布のトータル質量が変わってしまうので、力に質量をかけて相殺しておきます
    // 実質、この実装では質量が意味をなしていないのでmは不要ですが、見通しのため残しておきます
    const m = 1.0; // 質点の質量
    const g = simulation.g * m; // 重力
    const w = simulation.w * m; // 風力
    // 重力と風力による変位（移動量）を計算しておく
    const f = new THREE.Vector3();
    f[1] -= g; // 重力
    f[2] += w * (Math.sin(acc) * 0.5 + 0.5); // 風力（適当になびかせる）
    f.multiplyScalar(step * step * 0.5); // 力を変位に変換しておく

    // 抵抗は速度に対して働く
    const r = 1.0 - simulation.r * step;

    // 更新
    let ms_delta = 1000 * delta;
    ms_delta = Math.min(ms_delta, 100); // リミッター
    while (ms_delta >= ms_step) {
      // 大きなタイムステップでシミュレーションを実行すると精度の問題で破綻が生じるため、
      // フレームの差分時間を固定のシミュレーションタイムステップで分割し、複数回処理する。
      // 余剰時間は次のフレームに持ち越す。
      this.integrate(f, r,);

      // 制約充足フェーズ
      for (let ite = 0; ite < simulation.relaxation; ite++) {
        // 反復処理して安定させる（Relaxationと呼ばれる手法）
        this.constraint(
          step,
          spring_params
        );
      }

      if (simulation.collision) {
        // 球との衝突判定
        this.collision(collider)
      }

      this.ms_acc += ms_step;
      ms_delta -= ms_step;
    }
    this.ms_surplus = ms_delta;
  }
}
