import { Pane } from "tweakpane";
import { Cloth, SimulationParams, Sphere } from './cloth';
import { SpringParams } from './constraint';
import * as THREE from 'three';


interface ClothConfiguration {
  /// 布の大きさに対するスケーリング（ベースのサイズは2*2）
  scale: number;
  div: number;
}


export class State {
  /// 布（後で初期化する）
  cloth?: Cloth;

  reset = true;

  collision = true; // 球との衝突判定
  // 衝突判定用の球を適当に定義
  collider = {
    position: new THREE.Vector3(0.0, 0.0, 0.0),
    radius: 0.75, // 球の半径
  } as Sphere;

  indices: number[];

  cloth_params = {
    div: 15,
    /// 布の大きさに対するスケーリング（ベースのサイズは2*2）
    scale: 1.0,
  } as ClothConfiguration;

  simulation_params = {
    relaxation: 2,
    g: 7.0, // 重力
    w: 7.5, // 風力
    r: 0.2, // 抵抗
  } as SimulationParams;

  spring_params = {
    k: 3000.0, // 制約バネの特性（基本強度）
    structural_shrink: 1.0, // 制約バネの特性（構成バネの伸び抵抗）
    structural_stretch: 1.0, // 制約バネの特性（構成バネの縮み抵抗）
    shear_shrink: 1.0, // 制約バネの特性（せん断バネの伸び抵抗）
    shear_stretch: 1.0, // 制約バネの特性（せん断バネの縮み抵抗）
    bending_shrink: 1.0, // 制約バネの特性（曲げバネの伸び抵抗）
    bending_stretch: 0.5, // 制約バネの特性（曲げバネの縮み抵抗）
  } as SpringParams;

  pane: Pane;

  constructor(container: HTMLElement) {
    this.makePane(container);
  }

  makePane(container: HTMLElement) {
    this.pane = new Pane({
      container,
      title: "Cloth Simulation",
    });

    const btn = this.pane.addButton({
      title: 'リセット',
    });
    btn.on('click', () => {
      // count += 1;
      this.reset = true;
    });
    this.pane.addBinding(
      this.cloth_params, 'div',
      {
        label: '質点分割数（低負荷→高負荷）',
        options: { '15': 15, '31': 31 }
      }
    );
    this.pane.addBinding(
      this.simulation_params, 'relaxation',
      {
        label: '制約充足の反復回数（低負荷→高負荷）',
        options: { '1': 1, '2': 2, '3': 3, '4': 4, '5': 5, '6': 6 }
      }
    );
    this.pane.addBinding(this.simulation_params, 'g', {
      label: '重力（弱→強）',
      step: 0.1,
      min: 0,
      max: 9.8,
    });
    this.pane.addBinding(this.simulation_params, 'w', {
      label: '風力（弱→強）',
      step: 0.1,
      min: 0,
      max: 20.0,
    });
    this.pane.addBinding(this.simulation_params, 'r', {
      label: '抵抗（弱→強）',
      step: 0.01,
      min: 0,
      max: 2.0,
    });
    this.pane.addBinding(this, 'collision', {
      label: '球との衝突判定',
    });

    const spring = this.pane.addFolder({
      title: '制約バネの特性',
    });

    spring.addBinding(this.spring_params, 'k', {
      label: '基本強度（弱→強）',
      step: 10,
      min: 0,
      max: 5000,
    });

    spring.addBinding(this.spring_params, 'structural_shrink', {
      label: '構成バネの伸び抵抗（弱→強）',
      step: 0.01,
      min: 0,
      max: 1,
    });
    spring.addBinding(this.spring_params, 'structural_stretch', {
      label: '構成バネの縮み抵抗（弱→強）',
      step: 0.01,
      min: 0,
      max: 1,
    });

    spring.addBinding(this.spring_params, 'shear_shrink', {
      label: 'せん断バネの伸び抵抗（弱→強）',
      step: 0.01,
      min: 0,
      max: 1,
    });
    spring.addBinding(this.spring_params, 'shear_stretch', {
      label: 'せん断バネの縮み抵抗（弱→強）',
      step: 0.01,
      min: 0,
      max: 1,
    });

    spring.addBinding(this.spring_params, 'bending_shrink', {
      label: '曲げバネの伸び抵抗（弱→強）',
      step: 0.01,
      min: 0,
      max: 1,
    });
    spring.addBinding(this.spring_params, 'bending_stretch', {
      label: '曲げバネの縮み抵抗（弱→強）',
      step: 0.01,
      min: 0,
      max: 1,
    });
  }

  makeCloth(setCloth: (cloth: Cloth) => void) {
    if (!this.cloth || this.cloth.div != this.cloth_params.div || this.cloth.scale != this.cloth_params.scale) {
      console.log('new cloth');
      this.cloth = new Cloth(this.cloth_params.div, this.cloth_params.scale)
      setCloth(this.cloth);
    }
  }
}
