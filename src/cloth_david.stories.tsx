import React from 'react'
import { Canvas, useFrame } from "@react-three/fiber";
const GL = WebGL2RenderingContext;
import * as THREE from 'three';
import * as CANNON from 'cannon-es'
import { Box, Grid, OrbitControls, TransformControls } from "@react-three/drei";
import { Stats } from '@react-three/drei'

class Simulation {
  mass = 1;
  cloth_size = 1;
  dist: number;
  particles: CANNON.Body[][] = [];
  particles_mesh: THREE.Mesh[][] = []; //Mesh of particles (to visualize particles DEBUG purposes)
  cloth: THREE.Mesh;
  //Set up physics world
  world: CANNON.World;
  timeStep = 1 / 60;
  root: THREE.Object3D;
  selected_particle: any = null;
  cloth_geometry: THREE.PlaneGeometry;

  constructor(
    public readonly Nx: number,
    public readonly Ny: number) {
    this.dist = this.cloth_size / Nx; //Distance between each pair of particles

    console.log('create cloth');

    this.root = new THREE.Group();

    //Create cloth mesh
    this.cloth_geometry = new THREE.PlaneGeometry(10, 10, this.Nx, this.Ny);
    const cloth_mat = new THREE.MeshBasicMaterial({ color: 0x9b9b9b, side: THREE.DoubleSide, wireframe: true });
    this.cloth = new THREE.Mesh(this.cloth_geometry, cloth_mat);
    this.root.add(this.cloth)

    const shape = new CANNON.Particle(); //Instance to Cannon particle class
    this.world = new CANNON.World({
      gravity: new CANNON.Vec3(0, -9.81, 0)
    });

    //Create Cannon bodies for each particle in a grid
    this.particles.length = 0;
    this.particles_mesh.length = 0;
    for (let i = 0; i < this.Nx + 1; ++i) {
      this.particles.push([]);
      this.particles_mesh.push([]);
      for (let j = 0; j < this.Ny + 1; ++j) {
        var particle_position = new THREE.Vector3((i - this.Nx * 0.5) * this.dist, (j - this.Ny * 0.5) * this.dist, 0);
        const particle = new CANNON.Body({
          mass: (j === this.Ny ? 0 : this.mass), //If attached set the first row mass to 0 as it was attached to somewhere
          shape,
          position: (particle_position as unknown as CANNON.Vec3),
          velocity: new CANNON.Vec3(0, 0, 0.5)
        });
        this.particles[i].push(particle);
        this.world.addBody(particle);

        const particleMesh = new THREE.Mesh(new THREE.SphereGeometry(0.01), new THREE.MeshBasicMaterial({ color: 0x9b9b9b }));
        particleMesh.position.copy(particle_position);
        this.root.add(particleMesh);
        this.particles_mesh[i].push(particleMesh);

        this.cloth.add(particleMesh); //Add particle mesh as cloth children
      }
    }

    for (let i = 0; i < this.Nx + 1; ++i) {
      for (let j = 0; j < this.Ny + 1; ++j) {
        if (i < this.Nx)
          this.connect(i, j, i + 1, j)
        if (j < this.Ny)
          this.connect(i, j, i, j + 1)
      }
    }
  }

  //Connect particles with Distance Constraint (between neighboring particles) to form a cloth
  connect(i1: number, j1: number, i2: number, j2: number) {
    this.world.addConstraint(new CANNON.DistanceConstraint(this.particles[i1][j1],
      this.particles[i2][j2],
      this.dist));
  }

  //Updating the particle position 
  updateParticles() {

    for (let i = 0; i < this.Nx + 1; ++i) {
      for (let j = 0; j < this.Ny + 1; ++j) {
        var index = j * (this.Nx + 1) + i; //index of which triplet of coordinates we are going to update for example i = 0 j = 0 -> index = 0 1st vertex of the 1st column

        var position_attribute = this.cloth_geometry.attributes.position; //Returns the x,y,z position of the vertex
        var position = this.particles[i][this.Ny - j].position; //Position of the cannon particle

        position_attribute.setXYZ(index, position.x, position.y, position.z); //Set position to match the cannon particle position
        position_attribute.needsUpdate = true; //Crucial when we want to change coordinates of a geometry 

        var particle = this.particles_mesh[i][this.Ny - j]; //Get particle mesh to update the position
        particle.position.copy(position as unknown as THREE.Vector3);
        particle.position.needsUpdate = true;
      }
    }

    this.world.step(this.timeStep);
  }

  setAttached(cloth_attached: boolean) {
    for (let i = 0; i < this.Nx + 1; ++i) {
      for (let j = 0; j < this.Ny + 1; ++j) {
        this.particles[i][j].mass = cloth_attached ? (j === this.Ny ? 0 : this.mass) : this.mass; //If attached set the first row mass to 0 as it was attached to somewhere
      }
    }

    // Reapply world constraints to reflect the change in particle masses
    this.world.constraints.length = 0; // Clear existing constraints

    for (let i = 0; i < this.Nx + 1; ++i) {
      for (let j = 0; j < this.Ny + 1; ++j) {
        if (i < this.Nx)
          this.connect(i, j, i + 1, j)
        if (j < this.Ny)
          this.connect(i, j, i, j + 1)
      }
    }
  }

  setWireframe(wireframe: boolean) { //Enable disable wireframe
    this.cloth.material.wireframe = wireframe;
  }

  showParticleMesh(particles_visible: boolean) { //Show particle meshes
    // this.particles_visible = particles_visible ? !particles_visible : particles_visible = true;
    const flat_particles_mesh = this.particles_mesh.flat();
    for (const particle_mesh of flat_particles_mesh) {
      particle_mesh.visible = particles_visible;
    }
  }
}


function Render({ simulation }: { simulation: Simulation }) {

  useFrame(({ gl, clock }, delta) => {
    if (!simulation) {
      // setSimulation(new Simulation(20, 20, state));
    }
    else {
      simulation.updateParticles();
    }
  },);

  return <>
    {simulation ? <primitive object={simulation.root} /> : ""}
    <color attach="background" args={[0, 0, 0]} />
    <ambientLight intensity={0.8} />
    <pointLight intensity={1} position={[0, 6, 0]} />
    <directionalLight position={[10, 10, 5]} />
    <OrbitControls makeDefault />
    <Grid cellColor="white" args={[10, 10]} />
  </>
}


export function ClothSimulation() {

  const [simulation, setSimulation] = React.useState<Simulation>(null);

  React.useEffect(() => {
    setSimulation(new Simulation(20, 20));
  }, []);

  return (<div style={{ width: "100%", height: "100%" }}>
    <div style={{ position: "absolute", display: "flex", gap: "3vw", marginTop: "6vh", marginLeft: "2vw", "zIndex": 1 }} >
      <div><input type="checkbox"
        defaultChecked
        onChange={e => simulation?.setWireframe(e.target.checked)}
      /><label style={{ color: "white" }}>Show Wireframe</label></div>

      <div><input type="checkbox"
        defaultChecked
        onChange={e => simulation?.showParticleMesh(e.target.checked)}
      /><label style={{ color: "white" }}>Show Particles</label></div>

      <div><input type="checkbox"
        defaultChecked
        onChange={e => simulation?.setAttached(e.target.checked)}
      /><label style={{ color: "white" }}>Mesh Attached</label></div>

      <div>
      <a href="https://github.com/DavidRovira18/ClothSimulation_DavidRovira">github</a>
      </div>

    </div>
    <Canvas>
      <Render simulation={simulation} />
      <Stats />
    </Canvas>
  </div>);
}
