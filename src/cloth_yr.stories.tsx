import React from "react";
import './cloth/common.css';
import { Canvas, useFrame } from "@react-three/fiber";
import { yrGL, yrGLRenderer, yrGLMaterial } from './cloth/lib/yrGL';
import { vs_constant, fs_constant } from './cloth/lib/constant';
import { yrTimer } from './cloth/lib/yrTimer';
import { yrInput } from './cloth/lib/yrInput';
import { yrCamera } from './cloth/lib/yrCamera';
import { Cloth } from './cloth/cloth';
import { vec3, mat4, vec4 } from 'gl-matrix';
import { Stats } from '@react-three/drei'

const GL = WebGL2RenderingContext;


interface InputState {
  g: number; // 重力
  w: number; // 風力
  r: number; // 抵抗
  k: number; // 制約バネの特性（基本強度）
  structural_shrink: number; // 制約バネの特性（構成バネの伸び抵抗）
  structural_stretch: number; // 制約バネの特性（構成バネの縮み抵抗）
  shear_shrink: number; // 制約バネの特性（せん断バネの伸び抵抗）
  shear_stretch: number; // 制約バネの特性（せん断バネの縮み抵抗）
  bending_shrink: number; // 制約バネの特性（曲げバネの伸び抵抗）
  bending_stretch: number; // 制約バネの特性（曲げバネの縮み抵抗）
};


class State {
  /// 布（後で初期化する）
  cloth?: Cloth;
  /// 布の大きさに対するスケーリング（ベースのサイズは2*2）
  scale = 1.0;
  /// 累積時間
  ms_acc = 0;
  /// 更新処理の余剰時間（次フレームに持ち越す分）
  ms_surplus = 0;

  gl: yrGL;
  renderer: yrGLRenderer;
  material_constant: yrGLMaterial;
  timer = new yrTimer();
  input: yrInput;
  camera: yrCamera;

  // -------------------------------------------------------------------------------------------
  // UI（ボタンやスライダーなど）用パラメータ
  reset = true; // リセット
  div = 0; // 質点分割数
  relaxation = 0; // 制約充足の反復回数
  collision = false; // 球との衝突判定

  constructor(_gl: WebGL2RenderingContext, element: HTMLElement) {
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

  onFrame(inputState: InputState) {
    // UI（ボタンやスライダーなど）の取得
    for (let i = 0; i < document.form_ui.div.length; i++) {
      if (document.form_ui.div[i].checked) {
        const value = parseInt(document.form_ui.div[i].value);
        if (this.div !== value) {
          this.div = value;
          this.reset = true;
        }
      }
    }
    for (let i = 0; i < document.form_ui.relaxation.length; i++) {
      if (document.form_ui.relaxation[i].checked) {
        this.relaxation = parseInt(document.form_ui.relaxation[i].value);
      }
    }

    // this.g = parseFloat(document.form_ui.g.value);
    // this.w = parseFloat(document.form_ui.w.value);
    // this.r = parseFloat(document.form_ui.r.value);
    // this.k = parseFloat(document.form_ui.k.value);
    // this.structural_shrink = parseFloat(document.form_ui.structural_shrink.value);
    // this.structural_stretch = parseFloat(document.form_ui.structural_stretch.value);
    // this.shear_shrink = parseFloat(document.form_ui.shear_shrink.value);
    // this.shear_stretch = parseFloat(document.form_ui.shear_stretch.value);
    // this.bending_shrink = parseFloat(document.form_ui.bending_shrink.value);
    // this.bending_stretch = parseFloat(document.form_ui.bending_stretch.value);
    this.collision = document.form_ui.collision.checked;

    // タイマー更新
    this.timer.update();

    // 入力更新
    this.input.update();

    // カメラ更新
    this.camera.updateEditorMode(
      this.input._mouse_button_l ? this.input._mouse_nmove_x * 1.0 : 0.0,
      this.input._mouse_button_l ? this.input._mouse_nmove_y * 1.0 : 0.0,
      0.0
    );

    // 初期化（リセット）
    if (this.reset) {
      // init();
      this.cloth = undefined;
      this.ms_acc = 0;
      this.ms_surplus = 0;
      this.cloth = new Cloth(this.scale, this.div); // スケーリング, 質点分割数
      this.reset = false;
    }

    const ms_step = 16; // シミュレーションのタイムステップ（固定）
    const step = ms_step / 1000.0;
    const acc = this.ms_acc / 1000.0; // 累積時間（秒）

    // 質点の質量は質点分割数にかかわらず1.0固定とします
    // ただ、これだと質点分割数によって布のトータル質量が変わってしまうので、力に質量をかけて相殺しておきます
    // 実質、この実装では質量が意味をなしていないのでmは不要ですが、見通しのため残しておきます
    const m = 1.0; // 質点の質量
    const g = inputState.g * m; // 重力
    const w = inputState.w * m; // 風力
    // 重力と風力による変位（移動量）を計算しておく
    const f = vec3.create();
    f[1] -= g; // 重力
    f[2] += w * (Math.sin(acc) * 0.5 + 0.5); // 風力（適当になびかせる）
    vec3.scale(f, f, step * step * 0.5); // 力を変位に変換しておく

    // 抵抗は速度に対して働く
    const r = 1.0 - inputState.r * step;

    // 更新
    let ms_delta = this.timer._ms_delta + this.ms_surplus; // フレームの差分時間
    ms_delta = Math.min(ms_delta, 100); // リミッター
    while (ms_delta >= ms_step) {
      // 大きなタイムステップでシミュレーションを実行すると精度の問題で破綻が生じるため、
      // フレームの差分時間を固定のシミュレーションタイムステップで分割し、複数回処理する。
      // 余剰時間は次のフレームに持ち越す。
      this.cloth.update(
        step, // タイムステップ（秒）
        f, r,
      );

      // 制約充足フェーズ
      for (let ite = 0; ite < this.relaxation; ite++) // 反復処理して安定させる（Relaxationと呼ばれる手法）
      {
        this.cloth.constraint(
          step,
          inputState.k,
          inputState.structural_shrink,
          inputState.structural_stretch,
          inputState.shear_shrink,
          inputState.shear_stretch,
          inputState.bending_shrink,
          inputState.bending_stretch,
        );
      }

      if (this.collision) {
        // 球との衝突判定
        this.cloth.collision()
      }

      this.ms_acc += ms_step;
      ms_delta -= ms_step;
    }
    this.ms_surplus = ms_delta;

    // 描画用頂点座標を更新
    // Todo : １フレームに１回呼べばいい
    this.cloth.genVertices();

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

    // 布のジオメトリを生成
    // Todo : 毎フレームVBO/IBOを作り直すという、残念な実装になっています
    // : 動的書き換えに適したDYNAMIC_DRAW / bufferSubDataあたりに対応させるべき
    // : また、インターリーブ対応など、他にも最適化の余地があります
    const geometry_cloth = this.gl.createGeometry(this.cloth._vertices, this.cloth._indeces, GL.LINES);

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

function Render({ inputState }: { inputState: InputState }) {
  const [state, setState] = React.useState<State>(null);
  useFrame(({ gl, clock }, delta) => {
    if (!state) {
      // initialize;
      setState(new State(gl.getContext() as WebGL2RenderingContext, gl.domElement));
    }
    else {
      // render
      state.onFrame(inputState);
    }
  }, 1)

  return <Stats />
}


export function ClothSimulation() {
  const [inputState, setInputState] = React.useState<InputState>({
    g: 7.0, // 重力
    w: 7.5, // 風力
    r: 0.2, // 抵抗
    k: 3000.0, // 制約バネの特性（基本強度）
    structural_shrink: 1.0, // 制約バネの特性（構成バネの伸び抵抗）
    structural_stretch: 1.0, // 制約バネの特性（構成バネの縮み抵抗）
    shear_shrink: 1.0, // 制約バネの特性（せん断バネの伸び抵抗）
    shear_stretch: 1.0, // 制約バネの特性（せん断バネの縮み抵抗）
    bending_shrink: 1.0, // 制約バネの特性（曲げバネの伸び抵抗）
    bending_stretch: 0.5, // 制約バネの特性（曲げバネの縮み抵抗）
  });

  return (<div id="main">
    <h1>Cloth Simulation</h1>
    <a href="https://qiita.com/yunta_robo/items/0b468b65f3412554400a"
    >Qiita投稿</a
    >
    <div style={{ width: "512px", height: "512px" }} >
      <Canvas >
        <Render inputState={inputState} />
      </Canvas>
    </div>
    <form name="form_ui" style={{ fontSize: "14px" }}>
      左ドラッグまたはスワイプ操作でカメラを回転させることができます。
      <br />
      <br />
      <input type="button" value="リセット" onClick={() => { }} />
      <br />
      <br />
      ■質点分割数（低負荷→高負荷）
      <br />
      <input type="radio" name="div" value="15" />15
      <input type="radio" name="div" value="31" defaultChecked />31
      <br />
      ■制約充足の反復回数（低負荷→高負荷）
      <br />
      <input type="radio" name="relaxation" value="1" />1
      <input type="radio" name="relaxation" value="2" defaultChecked />2
      <input type="radio" name="relaxation" value="3" />3
      <input type="radio" name="relaxation" value="4" />4
      <input type="radio" name="relaxation" value="5" />5
      <input type="radio" name="relaxation" value="6" />6
      <br />
      ■重力（弱→強）
      <br />
      <input
        type="range"
        name="g"
        min="0.0"
        max="9.8"
        step="0.1"
        value={inputState.g}
        onChange={e => setInputState({ ...inputState, g: parseFloat(e.target.value) })}
      />
      <br />
      ■風力（弱→強）
      <br />
      <input
        type="range"
        name="w"
        min="0.0"
        max="20.0"
        step="0.1"
        value={inputState.w}
        onChange={e => setInputState({ ...inputState, w: parseFloat(e.target.value) })}
      />
      <br />
      ■抵抗（弱→強）
      <br />
      <input
        type="range"
        name="r"
        min="0.0"
        max="2.0"
        step="0.01"
        value={inputState.r}
        onChange={e => setInputState({ ...inputState, r: parseFloat(e.target.value) })}
      />
      <br />
      ■制約バネの特性
      <br />
      <input
        type="range"
        name="k"
        min="0.0"
        max="5000.0"
        step="10.0"
        value={inputState.k}
        onChange={e => setInputState({ ...inputState, k: parseFloat(e.target.value) })}
      />　基本強度（弱→強）
      <br />
      <input
        type="range"
        name="structural_shrink"
        min="0.0"
        max="1.0"
        step="0.01"
        value={inputState.structural_shrink}
        onChange={e => setInputState({ ...inputState, structural_shrink: parseFloat(e.target.value) })}
      />　構成バネの伸び抵抗（弱→強）
      <br />
      <input
        type="range"
        name="structural_stretch"
        min="0.0"
        max="1.0"
        step="0.01"
        value={inputState.structural_stretch}
        onChange={e => setInputState({ ...inputState, structural_stretch: parseFloat(e.target.value) })}
      />　構成バネの縮み抵抗（弱→強）
      <br />
      <input
        type="range"
        name="shear_shrink"
        min="0.0"
        max="1.0"
        step="0.01"
        value={inputState.shear_shrink}
        onChange={e => setInputState({ ...inputState, shear_shrink: parseFloat(e.target.value) })}
      />　せん断バネの伸び抵抗（弱→強）
      <br />
      <input
        type="range"
        name="shear_stretch"
        min="0.0"
        max="1.0"
        step="0.01"
        value={inputState.shear_stretch}
        onChange={e => setInputState({ ...inputState, shear_stretch: parseFloat(e.target.value) })}
      />　せん断バネの縮み抵抗（弱→強）
      <br />
      <input
        type="range"
        name="bending_shrink"
        min="0.0"
        max="1.0"
        step="0.01"
        value={inputState.bending_shrink}
        onChange={e => setInputState({ ...inputState, bending_shrink: parseFloat(e.target.value) })}
      />　曲げバネの伸び抵抗（弱→強）
      <br />
      <input
        type="range"
        name="bending_stretch"
        min="0.0"
        max="1.0"
        step="0.01"
        value={inputState.bending_stretch}
        onChange={e => setInputState({ ...inputState, bending_stretch: parseFloat(e.target.value) })}
      />　曲げバネの縮み抵抗（弱→強）
      <br />
      ■球との衝突判定
      <br />
      <input type="checkbox" name="collision" value="0" defaultChecked />
    </form>
  </div >);
}
