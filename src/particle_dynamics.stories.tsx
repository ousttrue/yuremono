import React from "react";
import { Canvas, useFrame } from "@react-three/fiber";
import { Grid, OrbitControls, TransformControls } from "@react-three/drei";
import * as THREE from 'three';
import { Pane } from "tweakpane";

function makeSpring(i: number, len: number) {
  const x = i * 0.1;
  const points = [x, 0, 0];
  for (let i = 1; i <= len; ++i) {
    points.push(x, -i * 0.1, 0);
  }
  return points;
}

class Point {
  x: number;
  y: number;
  z: number;
  constructor(x: number, y: number, z: number) {
    this.x = x;
    this.y = y;
    this.z = z;
  }
}

interface Constraint {
  onFrame(delta: number): void;
}

/// 
/// 指定したObjectからの相対位置を維持する
/// 
class FixedParent implements Constraint {
  constructor(public readonly ref: THREE.Object3D) {
  }

  onFrame(delta: number): void {
  }
}

/// 
/// - ref からの初期方向が元に戻ろうとする(stiffness)
/// - 減速 dragForce
/// - ref からの距離を維持
/// 
class SpringBone implements Constraint {
  constructor(public readonly ref: Point) {
  }

  onFrame(delta: number): void {
  }
}

class Simulation {
  constraints: Constraint[] = [];
  constructor() {
  }

  addPoint(id: number, p: Point) {
  }

  addConstraint(id: number, c: Constraint) {
  }

  addSpring(id0: number, id1: number) {
  }

  onFrame(delta: number) {
    for (const c of this.constraints) {
      c.onFrame(delta);
    }
  }
}

class Model {
  pane: Pane;
  len: number;
  num: number;
  root: THREE.Object3D;
  simulation = new Simulation();

  constructor(container: HTMLDivElement, setRoot: Function) {
    this.pane = new Pane({
      title: "Cloth Simulation",
      container,
    });

    this.num = 1;
    this.pane.addBinding(this, 'num', {
      label: 'spring 本数',
      step: 1,
      min: 1,
      max: 11,
    }).on('change', (e) => {
      setRoot(this.makeModel());
    });

    this.len = 1
    this.pane.addBinding(this, 'len', {
      label: 'spring 長さ',
      step: 1,
      min: 1,
      max: 10,
    }).on('change', (e) => {
      setRoot(this.makeModel())
    });

    setRoot(this.makeModel())
  }

  makeModel() {
    // create scene
    // const geometry = new THREE.BoxGeometry(1, 1, 1);
    const geometry = new THREE.BufferGeometry();

    const points = []
    for (let x = 0; x < this.num; ++x) {
      const himo = makeSpring(x, this.len)
      points.push(...himo)
    }
    const vertices = new Float32Array(points);
    geometry.setAttribute("position", new THREE.BufferAttribute(vertices, 3));

    const indices = []
    const d = this.len + 1;
    for (let x = 0; x < this.num; ++x) {
      let i = x * d;
      for (let y = 0; y < this.len; ++y, ++i) {
        indices.push(i, i + 1)
        if (x > 0) {
          indices.push(i + 1, i + 1 - d);
        }
      }
    }
    geometry.setIndex(indices);

    const material = new THREE.MeshBasicMaterial({ color: 0x00ff00 });
    this.root = new THREE.LineSegments(geometry, material);
    return this.root;
  }

  onFrame(clock: THREE.Clock, delta: number) {
    this.simulation.onFrame(delta);
  }
};

function Render({ model, root }: { model?: Model, root: THREE.Object3D }) {
  useFrame(({ clock }, delta) => {
    if (delta > 0) {
      model?.onFrame(clock, delta);
    }
  });

  return (<>
    <color attach="background" args={[0, 0, 0]} />
    <ambientLight intensity={0.8} />
    <pointLight intensity={1} position={[0, 6, 0]} />
    <directionalLight position={[10, 10, 5]} />
    <OrbitControls makeDefault />
    <Grid cellColor="white" args={[10, 10]} />
    <axesHelper />
    <TransformControls >
      <primitive object={root} />
    </TransformControls>
  </>
  );
}

export function BoneParticle() {
  const [root, setRoot] = React.useState<THREE.Object3D>(new THREE.Group());
  const [model, setModel] = React.useState<Model>(null);
  const ref = React.useRef<HTMLDivElement>(null);
  React.useEffect(() => {
    setModel(new Model(ref.current, setRoot));
  }, []);

  return (<div style={{
    width: '100%',
    height: '100%',
    display: 'flex',
    flexDirection: 'column',
  }}>
    <div style={{}} ref={ref} />
    <Canvas style={{ flexGrow: 1 }} shadows>
      <Render model={model} root={root} />
    </Canvas>
  </div>);
}
