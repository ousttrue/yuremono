import React from "react";
import { Canvas, useThree, useFrame } from "@react-three/fiber";
import * as THREE from 'three';
import { initInstanceObjects } from './cloth_pbd/InstanceInit';
import { InstancePoint } from './cloth_pbd/InstancePoint';
import { Stick } from './cloth_pbd/stick';
import { Stats, Box, Grid, OrbitControls, TransformControls } from "@react-three/drei";


//textures
// import Cloth from '/cloth-texture/fabric_85_basecolor-1K.png';
// import ClothRough from '/cloth-texture/fabric_85_roughness-1K.png';
// import ClothAO from '/cloth-texture/fabric_85_ambientocclusion-1K.png';
// import ClothBump from '/cloth-texture/fabric_85_height-1K.png';
// import ClothNormal from '/cloth-texture/fabric_85_normal-1K.png';
// import ClothMetallic from '/cloth-texture/fabric_85_metallic-1K.png';


class Renderer {
  instancePoints: InstancePoint;
  sticks: Stick[];
  order: number[];
  shapeGeometry: THREE.BufferGeometry;
  shape: THREE.Mesh;

  constructor(
    public readonly width: number,
    public readonly height: number,
    color: string,
    public readonly scale: number,
  ) {
    const { instancePoints, sticks } = initInstanceObjects(width, height);
    this.instancePoints = instancePoints;
    this.sticks = sticks;
    this.order = []

    // number of squares
    var squares = (width - 1) * (height - 1);
    var points = this.instancePoints.points;

    var pos = []
    for (let i = 0; i < (points.length + 0); i++) {

      if ((i + 1) < squares + (height - 1)) {
        if (((i + 1) % width) != 0) {
          pos.push(points[i].position);
          pos.push(points[i + 1].position);
          pos.push(points[i + width].position);

          this.order.push(i);
          this.order.push(i + 1);
          this.order.push(i + width);

          pos.push(points[i + 1].position);
          pos.push(points[i + width].position);
          pos.push(points[i + width + 1].position);

          this.order.push(i + 1);
          this.order.push(i + width);
          this.order.push(i + width + 1);
        }
      }
    }

    this.shapeGeometry = new THREE.BufferGeometry().setFromPoints(pos);
    this.shapeGeometry.computeVertexNormals();
    this.shapeGeometry.computeBoundingBox();

    var base =
      [
        // top left
        0.0, 0.0,
        1.0, 0.0,
        0.0, 1.0,
        // bottom right
        1.0, 0.0,
        0.0, 1.0,
        1.0, 1.0
      ];

    const quad_uvs: number[] = []
    for (let i = 0; i < ((width - 1) * (height - 1)); i++) {
      base.forEach(function(q) {
        quad_uvs.push(q)
      });
    }
    var uvs = new Float32Array(quad_uvs);
    this.shapeGeometry.setAttribute('uv', new THREE.BufferAttribute(uvs, 2));

    const loader = new THREE.TextureLoader();
    // const cloth = loader.load(Cloth);
    // const clothRough = loader.load(ClothRough);
    // const clothAO = loader.load(ClothAO);
    // const clothBump = loader.load(ClothBump);
    // const clothNormal = loader.load(ClothNormal);
    // const clothMetallic = loader.load(ClothMetallic);
    var material = new THREE.MeshStandardMaterial({
      side: THREE.DoubleSide,
      color: color,
      // roughnessMap: clothRough,
      // aoMap: clothAO,
      // bumpMap: clothBump,
      // normalMap: clothNormal,
      // metalnessMap: clothMetallic,
      normalScale: new THREE.Vector2(0.5, 0.5),
      bumpScale: 1,
      roughness: 1,
    });
    material.color.anisotropy = 16;

    this.shape = new THREE.Mesh(this.shapeGeometry, material);
    this.shape.castShadow = true;
    this.shape.receiveShadow = true;
  }

  update(delta: number) {
    this.instancePoints.updatePoints(delta);
    for (let i = 0; i < 3; i++) {
      this.sticks.forEach(function(stick) {
        stick.updateStick();
      });
    }

    for (let i = 0; i < this.order.length; i++) {
      var index = this.order[i]
      var pos = this.instancePoints.points[index].position;
      this.shapeGeometry.attributes.position.setXYZ(i, pos.x, pos.y, pos.z);
    }

    this.shapeGeometry.attributes.position.needsUpdate = true;
    this.shapeGeometry.scale(this.scale, this.scale, this.scale)
    // this.shapeGeometry.computeFaceNormals();
    this.shapeGeometry.computeVertexNormals();
  }
}

function Render() {
  const [state, setState] = React.useState<Renderer>(null);

  useFrame(({ gl, clock }, delta) => {

    if (!state) {
      // initialize
      // let color = '#403d39';
      let color = '#FFFFFF';
      let scale = 0.1;
      const width = 51;
      const height = 51;

      setState(new Renderer(width, height, color, scale));

      // function onClick(e) {
      //   instancePoints.gravity = instancePoints.gravity * -1;
      // }
    }
    else {
      state.update(delta);
    }

  })

  return <>
    {state ? <primitive object={state.shape} /> : ""}
  </>
}

export function ClothSimulationPBD({ height }: { height?: string }) {
  return (<div style={{ width: "100%", height: height ? height : "100%" }}>
    <div style={{ position: "absolute", display: "flex", gap: "3vw", marginTop: "6vh", marginLeft: "2vw", "zIndex": 1 }} >
      <div>
        <a href="https://github.com/RobertoLovece/Cloth">github</a>
      </div>
    </div>

    <Canvas>
      <color attach="background" args={[0.7, 0.7, 0.7]} />
      <ambientLight intensity={0.8} />
      <pointLight intensity={1} position={[0, 6, 0]} />
      <directionalLight position={[10, 10, 5]} />
      <OrbitControls makeDefault />
      <Grid cellColor="white" args={[10, 10]} />
      <Stats />
      <Render />
    </Canvas>
  </div>);
}
