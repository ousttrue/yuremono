import React from "react";
import { Canvas, useFrame } from "@react-three/fiber";
import { State } from './cloth/state';


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


export function ClothSimulation(props: any) {
  const [state, setState] = React.useState<State>(null);
  const ref = React.useRef(null);
  React.useEffect(() => {
    setState(new State(ref.current));
  }, []);

  return (<>
    <Canvas style={{ ...props }}>
      <Render state={state} />
    </Canvas>
    <div ref={ref} >
    </div>
  </>);
}
