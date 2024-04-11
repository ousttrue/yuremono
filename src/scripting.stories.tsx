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
    model?.onFrame(clock, delta);
  });
  return (<>
    <color attach="background" args={[0, 0, 0]} />
    <ambientLight intensity={0.8} />
    <pointLight intensity={1} position={[0, 6, 0]} />
    <directionalLight position={[10, 10, 5]} />
    <OrbitControls makeDefault />
    <Grid cellColor="white" args={[10, 10]} />
    <axesHelper />
    <Stats />
    {model ? <primitive object={model.root} /> : ""}
  </>
  );
}

export function ScriptingScene() {
  const [model, setModel] = React.useState<Model>(null);

  React.useEffect(() => {
    // create scene
    const geometry = new THREE.BoxGeometry(1, 1, 1);
    const material = new THREE.MeshBasicMaterial({ color: 0x00ff00 });
    const cube = new THREE.Mesh(geometry, material);

    setModel({
      root: cube,
      onFrame: (clock: THREE.Clock, delta: number) => {
        cube.position.set(Math.sin(clock.elapsedTime), 0, 0);
      }
    });

  }, []);

  return (<Canvas shadows>
    <Render model={model} />
  </Canvas>);
}
