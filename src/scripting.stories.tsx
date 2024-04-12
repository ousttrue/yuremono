import React from "react";
import { Canvas, useFrame } from "@react-three/fiber";
import { Stats, Grid, OrbitControls } from "@react-three/drei";
import * as THREE from 'three';


interface Model {
  root: THREE.Object3D;
  onFrame: (clock: THREE.Clock, delta: number) => void;
};

function Render({ model }: { model?: Model }) {
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
    {model ? <primitive object={model.root} /> : ""}
  </>
  );
}

export function ScriptingScene() {
  const [model, setModel] = React.useState<Model>(null);

  React.useEffect(() => {
    // create scene
    // const geometry = new THREE.BoxGeometry(1, 1, 1);
    const geometry = new THREE.BufferGeometry();
    const vertices = new Float32Array([
      -1, -1, 0,
      1, -1, 0,
      1, 1, 0,
      -1, 1, 0,
    ]);
    geometry.setAttribute("position", new THREE.BufferAttribute(vertices, 3));

    const indices = [
      0, 1,
      1, 2,
      2, 3,
      3, 0,
    ]
    geometry.setIndex(indices);

    const material = new THREE.MeshBasicMaterial({ color: 0x00ff00 });
    const mesh = new THREE.LineSegments(geometry, material);

    setModel({
      root: mesh,
      onFrame: (clock: THREE.Clock, delta: number) => {
        mesh.position.set(Math.sin(clock.elapsedTime), 0, 0);
      }
    });

  }, []);

  return (<Canvas shadows>
    <Render model={model} />
  </Canvas>);
}
