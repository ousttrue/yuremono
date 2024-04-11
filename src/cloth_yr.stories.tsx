import React from "react";
import { Canvas, useFrame } from "@react-three/fiber";
import { yrGL, yrGLRenderer, yrGLMaterial } from './cloth/lib/yrGL';
import { vs_constant, fs_constant } from './cloth/lib/constant';
import { yrInput } from './cloth/lib/yrInput';
import { yrCamera } from './cloth/lib/yrCamera';
import { Cloth, ClothParams } from './cloth/cloth';
import { vec3, mat4, vec4 } from 'gl-matrix';
import { Pane } from "tweakpane";


interface InputState {
  reset: boolean;
  /// 布の大きさに対するスケーリング（ベースのサイズは2*2）
  scale: number;
  div: number;
  relaxation: number;
  collision: boolean; // 球との衝突判定
  g: number; // 重力
  w: number; // 風力
  r: number; // 抵抗
  cloth: ClothParams;
};


class State {
  /// 布（後で初期化する）
  cloth?: Cloth;
  /// 累積時間
  ms_acc = 0;
  /// 更新処理の余剰時間（次フレームに持ち越す分）
  ms_surplus = 0;

  gl: yrGL;
  renderer: yrGLRenderer;
  material_constant: yrGLMaterial;
  input: yrInput;
  camera: yrCamera;

  // 衝突判定用の球を適当に定義
  sphere_pos = vec3.fromValues(0.0, 0.0, 0.0); // 球の中心位置
  sphere_radius = 0.75; // 球の半径

  indices: number[];

  PARAMS = {
    div: 15,
    /// 布の大きさに対するスケーリング（ベースのサイズは2*2）
    scale: 1.0,
    relaxation: 2,
    g: 7.0, // 重力
    w: 7.5, // 風力
    r: 0.2, // 抵抗
    cloth: {
      k: 3000.0, // 制約バネの特性（基本強度）
      structural_shrink: 1.0, // 制約バネの特性（構成バネの伸び抵抗）
      structural_stretch: 1.0, // 制約バネの特性（構成バネの縮み抵抗）
      shear_shrink: 1.0, // 制約バネの特性（せん断バネの伸び抵抗）
      shear_stretch: 1.0, // 制約バネの特性（せん断バネの縮み抵抗）
      bending_shrink: 1.0, // 制約バネの特性（曲げバネの伸び抵抗）
      bending_stretch: 0.5, // 制約バネの特性（曲げバネの縮み抵抗）
    },
    reset: true, // リセット
    collision: true, // 球との衝突判定
  } as InputState;

  constructor() {
  }

  lazyInitialize(_gl: WebGL2RenderingContext,
    element: HTMLElement) {

    if (!this.renderer) {
      this.renderer = new yrGLRenderer(_gl);
      // GL関係のインスタンスを生成
      this.gl = new yrGL(_gl);
      this.material_constant = this.gl.createMaterial(vs_constant, fs_constant); // マテリアル

      // カメラ
      this.camera = new yrCamera();
      this.camera._pos[0] = 1.25;
      this.camera._pos[1] = 0.0;
      this.camera._pos[2] = 5.5;
      this.camera._fov_y = 32.5 * Math.PI / 180.0; // 画角調整

      this.input = new yrInput(element);
    }
  }

  onFrame(delta: number) {
    // 入力更新
    this.input.update();

    // カメラ更新
    this.camera.updateEditorMode(
      this.input._mouse_button_l ? this.input._mouse_nmove_x * 1.0 : 0.0,
      this.input._mouse_button_l ? this.input._mouse_nmove_y * 1.0 : 0.0,
      0.0
    );

    if (this.PARAMS.reset) {
      // 初期化（リセット）
      this.ms_acc = 0;
      this.ms_surplus = 0;
      this.cloth = new Cloth(this.PARAMS.div, this.PARAMS.scale);
      this.PARAMS.reset = false;

      this.indices = [];
      const div = this.PARAMS.div;
      for (let y = 0; y < div; y++) {
        for (let x = 0; x < div; x++) {
          this.indices.push(y * (div + 1) + x);
          this.indices.push((y + 1) * (div + 1) + (x + 1));
          this.indices.push(y * (div + 1) + (x + 1));
          this.indices.push((y + 1) * (div + 1) + x);
        }
      }
    }

    const ms_step = 16; // シミュレーションのタイムステップ（固定）
    const step = ms_step / 1000.0;
    const acc = this.ms_acc / 1000.0; // 累積時間（秒）

    // 質点の質量は質点分割数にかかわらず1.0固定とします
    // ただ、これだと質点分割数によって布のトータル質量が変わってしまうので、力に質量をかけて相殺しておきます
    // 実質、この実装では質量が意味をなしていないのでmは不要ですが、見通しのため残しておきます
    const m = 1.0; // 質点の質量
    const g = this.PARAMS.g * m; // 重力
    const w = this.PARAMS.w * m; // 風力
    // 重力と風力による変位（移動量）を計算しておく
    const f = vec3.create();
    f[1] -= g; // 重力
    f[2] += w * (Math.sin(acc) * 0.5 + 0.5); // 風力（適当になびかせる）
    vec3.scale(f, f, step * step * 0.5); // 力を変位に変換しておく

    // 抵抗は速度に対して働く
    const r = 1.0 - this.PARAMS.r * step;

    // 更新
    let ms_delta = 1000 * delta;
    ms_delta = Math.min(ms_delta, 100); // リミッター
    while (ms_delta >= ms_step) {
      // 大きなタイムステップでシミュレーションを実行すると精度の問題で破綻が生じるため、
      // フレームの差分時間を固定のシミュレーションタイムステップで分割し、複数回処理する。
      // 余剰時間は次のフレームに持ち越す。
      this.cloth.integrate(f, r,);

      // 制約充足フェーズ
      for (let ite = 0; ite < this.PARAMS.relaxation; ite++) {
        // 反復処理して安定させる（Relaxationと呼ばれる手法）
        this.cloth.constraint(
          step,
          this.PARAMS.cloth
        );
      }

      if (this.PARAMS.collision) {
        // 球との衝突判定
        this.cloth.collision(this.sphere_pos, this.sphere_radius)
      }

      this.ms_acc += ms_step;
      ms_delta -= ms_step;
    }
    this.ms_surplus = ms_delta;

    this.render();

    // バッファリングされたWebGLコマンドをただちに実行する
    this.gl.flush();
  }

  // 描画
  render() {
    // カメラ行例
    const view_matrix = this.camera.getViewMatrix(); // ビュー行列
    const projection_matrix = this.camera.getProjectionMatrix(true); // プロジェクション行列

    // カラーバッファとZバッファをクリアする
    this.renderer.clearBuffer();

    const vertices = [];
    for (const point of this.cloth._points) {
      vertices.push(point._pos[0]);
      vertices.push(point._pos[1]);
      vertices.push(point._pos[2]);
    }

    // 布のジオメトリを生成
    // Todo : 毎フレームVBO/IBOを作り直すという、残念な実装になっています
    // : 動的書き換えに適したDYNAMIC_DRAW / bufferSubDataあたりに対応させるべき
    // : また、インターリーブ対応など、他にも最適化の余地があります
    const geometry_cloth = this.gl.createGeometry(vertices, this.indices, this.gl._gl.LINES);

    // 描画
    const wvp_matrix = mat4.create();
    mat4.mul(wvp_matrix, projection_matrix, view_matrix);
    this.material_constant.SetUniformFloat32Array("u_wvp", wvp_matrix);
    this.material_constant.SetUniformFloat32Array("u_color", vec4.fromValues(1.0, 1.0, 1.0, 1.0));
    this.renderer.renderGeometry(geometry_cloth, this.material_constant);

    // 布のジオメトリを破棄
    geometry_cloth.release();
  }
}

function Render({ state }: { state: State }) {
  useFrame(({ gl, clock }, delta) => {
    state.lazyInitialize(
      gl.getContext() as WebGL2RenderingContext,
      gl.domElement
    );

    if (delta > 0) {
      state.onFrame(delta);
    }
  }, 1)

  return (<></>);
}

let pane: Pane;

export function ClothSimulation(props: any) {
  const [state, setState] = React.useState<State>(null);

  const newState = new State();
  const PARAMS = newState.PARAMS;

  const ref = React.useRef(null);

  React.useEffect(() => {
    pane = new Pane({
      container: ref.current,
      title: "Cloth Simulation",
    });
    const btn = pane.addButton({
      title: 'リセット',
    });
    btn.on('click', () => {
      // count += 1;
      PARAMS.reset = true;
    });
    pane.addBinding(
      PARAMS, 'div',
      {
        label: '質点分割数（低負荷→高負荷）',
        options: { '15': 15, '31': 31 }
      }
    );
    pane.addBinding(
      PARAMS, 'relaxation',
      {
        label: '制約充足の反復回数（低負荷→高負荷）',
        options: { '1': 1, '2': 2, '3': 3, '4': 4, '5': 5, '6': 6 }
      }
    );
    pane.addBinding(PARAMS, 'g', {
      label: '重力（弱→強）',
      step: 0.1,
      min: 0,
      max: 9.8,
    });
    pane.addBinding(PARAMS, 'w', {
      label: '風力（弱→強）',
      step: 0.1,
      min: 0,
      max: 20.0,
    });
    pane.addBinding(PARAMS, 'r', {
      label: '抵抗（弱→強）',
      step: 0.01,
      min: 0,
      max: 2.0,
    });
    pane.addBinding(PARAMS, 'collision', {
      label: '球との衝突判定',
    });

    const spring = pane.addFolder({
      title: '制約バネの特性',
    });

    spring.addBinding(PARAMS.cloth, 'k', {
      label: '基本強度（弱→強）',
      step: 10,
      min: 0,
      max: 5000,
    });

    spring.addBinding(PARAMS.cloth, 'structural_shrink', {
      label: '構成バネの伸び抵抗（弱→強）',
      step: 0.01,
      min: 0,
      max: 1,
    });
    spring.addBinding(PARAMS.cloth, 'structural_stretch', {
      label: '構成バネの縮み抵抗（弱→強）',
      step: 0.01,
      min: 0,
      max: 1,
    });

    spring.addBinding(PARAMS.cloth, 'shear_shrink', {
      label: 'せん断バネの伸び抵抗（弱→強）',
      step: 0.01,
      min: 0,
      max: 1,
    });
    spring.addBinding(PARAMS.cloth, 'shear_stretch', {
      label: 'せん断バネの縮み抵抗（弱→強）',
      step: 0.01,
      min: 0,
      max: 1,
    });

    spring.addBinding(PARAMS.cloth, 'bending_shrink', {
      label: '曲げバネの伸び抵抗（弱→強）',
      step: 0.01,
      min: 0,
      max: 1,
    });
    spring.addBinding(PARAMS.cloth, 'bending_stretch', {
      label: '曲げバネの縮み抵抗（弱→強）',
      step: 0.01,
      min: 0,
      max: 1,
    });

    setState(newState);
  }, []);

  return (<>
    <Canvas style={{ ...props }}>
      <Render state={state} />
    </Canvas>
    <div ref={ref} >
    </div>
  </>);
}
