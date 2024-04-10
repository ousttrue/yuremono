import React from "react";
import { Canvas, useFrame, useThree } from "@react-three/fiber";
import { Stats, Grid, OrbitControls } from "@react-three/drei";
import * as THREE from 'three';
import { TransformControls } from 'three/examples/jsm/controls/TransformControls';

function cube(size: number): THREE.Mesh {
  const geometry = new THREE.BoxGeometry(size, size, size);
  const material = new THREE.MeshBasicMaterial({ color: 0x00ff00 });
  const cube = new THREE.Mesh(geometry, material);
  return cube;
}

function setWorld(scene: THREE.Scene, obj: THREE.Object3D, src: THREE.Vector3) {
  const parent = obj.parent;
  scene.attach(obj);
  obj.position.set(src.x, src.y, src.z);
  parent.attach(obj);
}

abstract class BaseModel {
  _root = new THREE.Group();
  _transform: TransformControls;
  _joints: THREE.Object3D[] = [];
  _currentPositions: THREE.Vector3[] = [];
  _prevPositions: THREE.Vector3[] = [];

  constructor() {
    this.add(cube(0.1));
    this.add(cube(0.1));
    this.add(cube(0.1));
    this.add(cube(0.1));
    this.add(cube(0.1));

    for (const joint of this._joints) {
      {
        const v = new THREE.Vector3();
        joint.getWorldPosition(v);
        this._prevPositions.push(v);
      }
      {
        const v = new THREE.Vector3();
        joint.getWorldPosition(v);
        this._currentPositions.push(v);
      }
    }
    console.log(this._prevPositions);
  }

  add(joint: THREE.Object3D) {
    if (this._joints.length == 0) {
      this._root.add(joint);
    }
    else {
      joint.position.set(0, -0.2, 0);
      this._joints[this._joints.length - 1].add(joint);
    }
    this._joints.push(joint);
  }

  get root(): THREE.Object3D {
    return this._root;
  }

  lazyInitTransform(camera: THREE.Camera, domElement: HTMLElement, orbitControls: any) {
    if (!this._transform) {
      // lazy initialization
      this._transform = new TransformControls(camera, domElement);
      // this._transform.space = 'local';
      // this._transform.addEventListener('change', tick);
      this._transform.attach(this._joints[0]);
      this._transform.addEventListener('dragging-changed', event => {
        if (orbitControls) {
          orbitControls.enabled = !event.value;
        }
      });
      this._transform.addEventListener('change', e => {
        // invalidate();
      });
      this._root.add(this._transform);
    }
  }

  abstract onFrame(
    clock: THREE.Clock,
    delta: number,
    scene: THREE.Scene
  ): void;
}

function Render({ model }: { model?: BaseModel }) {
  const transformRef = React.useRef();
  const { invalidate } = useThree();
  useFrame(({ gl, camera, clock, scene }, delta) => {
    if (model) {
      model.lazyInitTransform(camera, gl.domElement, transformRef.current);
      if (delta > 0) {
        model.onFrame(clock, delta, scene);
        invalidate();
      }
    }

  });

  return (<>
    <color attach="background" args={[0, 0, 0]} />
    <ambientLight intensity={0.8} />
    <pointLight intensity={1} position={[0, 6, 0]} />
    <directionalLight position={[10, 10, 5]} />
    <OrbitControls ref={transformRef} makeDefault />
    <Grid cellColor="white" args={[10, 10]} />
    <axesHelper />
    {model ? <primitive object={model.root} /> : ""}
  </>
  );
}

//
// Velocity
//
class VelocityModel extends BaseModel {

  onFrame(
    clock: THREE.Clock,
    delta: number,
    scene: THREE.Scene
  ) {
    // update current & detach parent
    for (let i = 1; i < this._joints.length; ++i) {
      this._joints[i].getWorldPosition(this._currentPositions[i]);
      scene.attach(this._joints[i]);
    }

    // verlet
    for (let i = 1; i < this._joints.length; ++i) {
      const joint = this._joints[i];

      const current = this._currentPositions[i];
      const prev = this._prevPositions[i];

      const velocity = new THREE.Vector3();
      velocity.subVectors(current, prev);

      prev.copy(current);
      joint.position.addVectors(current, velocity);
    }

    // restore parent
    for (let i = 1; i < this._joints.length; ++i) {
      this._joints[i].getWorldPosition(this._currentPositions[i]);
      this._joints[i - 1].attach(this._joints[i]);
    }
  }
}

export function Velocity() {
  const [model, setModel] = React.useState<VelocityModel>(null);
  React.useEffect(() => {
    setModel(new VelocityModel());
  }, []);

  return (<Canvas shadows>
    <Render model={model} />
  </Canvas>);
}

//
// Length Constraint
//
class ConstraintModel extends BaseModel {
  onFrame(
    clock: THREE.Clock,
    delta: number,
    scene: THREE.Scene
  ) {

    // update current & detach parent
    for (let i = 1; i < this._joints.length; ++i) {
      this._joints[i].getWorldPosition(this._currentPositions[i]);
      scene.attach(this._joints[i]);
    }

    // verlet
    for (let i = 1; i < this._joints.length; ++i) {
      const joint = this._joints[i];

      const current = this._currentPositions[i];
      const prev = this._prevPositions[i];

      const velocity = new THREE.Vector3();
      velocity.subVectors(current, prev);

      prev.copy(current);
      joint.position.addVectors(current, velocity);
    }

    // restore parent
    for (let i = 1; i < this._joints.length; ++i) {
      this._joints[i].getWorldPosition(this._currentPositions[i]);
      this._joints[i - 1].attach(this._joints[i]);
    }

    // constraint
    for (let i = 1; i < this._joints.length; ++i) {
      const joint = this._joints[i];
      const len = joint.position.length();
      joint.position.multiplyScalar(0.2 / len);
    }
  }
}

export function Constraint() {
  const [model, setModel] = React.useState<ConstraintModel>(null);
  React.useEffect(() => {
    setModel(new ConstraintModel());
  }, []);

  return (<Canvas shadows>
    <Render model={model} />
  </Canvas>);
}

//
// Recursive
//
class RecursiveModel extends BaseModel {
  onFrame(
    clock: THREE.Clock,
    delta: number,
    scene: THREE.Scene
  ) {
    for (let i = 1; i < this._joints.length; ++i) {
      // this._joints[i].getWorldPosition(this._currentPositions[i]);

      const joint = this._joints[i];

      const current = new THREE.Vector3();;
      joint.getWorldPosition(current);
      const prev = this._prevPositions[i];

      const velocity = new THREE.Vector3();
      velocity.subVectors(current, prev);

      prev.copy(current);
      joint.position.addVectors(current, velocity);

      const len = joint.position.length();
      joint.position.multiplyScalar(0.2 / len);
    }
  }
}

export function Recursive() {
  const [model, setModel] = React.useState<RecursiveModel>(null);
  React.useEffect(() => {
    setModel(new RecursiveModel());
  }, []);

  return (<Canvas shadows>
    <Render model={model} />
  </Canvas>);
}

//
// SpringBone
//
// https://ousttrue.github.io/yuremono/docs/springbone/rocketjump/
class SpringBoneModel extends BaseModel {
  constructor(
    public readonly stiffnessForce: number = 0.2,
    public readonly dragForce: number = 0.1,
  ) {
    super()
  }

  onFrame(
    clock: THREE.Clock,
    delta: number,
    scene: THREE.Scene
  ) {
    const sqrDt = delta * delta;

    for (let i = 1; i < this._joints.length; ++i) {
      const joint = this._joints[i];
      joint.getWorldPosition(this._currentPositions[i]);
      const currTipPos = this._currentPositions[i];
      const prevTipPos = this._prevPositions[i];
      const velocity = new THREE.Vector3();
      velocity.subVectors(currTipPos, prevTipPos);

      const dragVelocity = new THREE.Vector3();
      velocity.subVectors(prevTipPos, currTipPos);

      //stiffness
      const force = new THREE.Vector3(0, -1, 0).multiplyScalar(
        this.stiffnessForce / sqrDt);
      force.add(dragVelocity.multiplyScalar(this.dragForce / sqrDt));

      const newPosition = new THREE.Vector3();
      newPosition.add(currTipPos);
      newPosition.add(velocity);
      newPosition.add(force.multiplyScalar(sqrDt));

      prevTipPos.copy(currTipPos);
      setWorld(scene, joint, newPosition);

      // constraint
      const len = joint.position.length();
      if (len > 0) {
        joint.position.multiplyScalar(0.2 / len);
      }
    }
  }
}

export function SpringBone({ height }: { height?: string }) {
  const [model, setModel] = React.useState<SpringBoneModel>(null);
  React.useEffect(() => {
    setModel(new SpringBoneModel());
  }, []);

  return (<Canvas shadows style={{ height: height }}>
    <Render model={model} />
  </Canvas>);
}
