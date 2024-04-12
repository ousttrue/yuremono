import React from "react";
import { Canvas, useFrame, useThree } from "@react-three/fiber";
import { State } from './cloth/state';
import { Cloth } from './cloth/cloth';
import { Grid, OrbitControls } from "@react-three/drei";


function Render({ state, cloth }: { state?: State, cloth: Cloth }) {
  useFrame(({ clock, invalidate }, delta) => {
    if (cloth && delta > 0) {
      // console.log(cloth);
      cloth.onFrame(delta, state.spring_params, state.simulation_params, state.collider);
      // invalidate();
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
    {cloth ? <primitive object={cloth.root} /> : ""}
  </>
  );
}


export function ClothSimulation(props: any) {
  const [state, setState] = React.useState<State>(null);
  const [cloth, setCloth] = React.useState<Cloth>(null);
  const ref = React.useRef(null);
  React.useEffect(() => {
    const newState = new State(ref.current);
    newState.pane.on('change', (ev) => {
      newState.makeCloth(setCloth);
    });
    setState(newState);
    newState.makeCloth(setCloth);

  }, []);

  return (<>
    <Canvas style={{ ...props }}>
      <Render state={state} cloth={cloth} />
    </Canvas>
    <div ref={ref} >
    </div>
  </>);
}
