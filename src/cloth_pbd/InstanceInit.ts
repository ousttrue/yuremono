import { InstancePoint } from './InstancePoint'
import { Point } from './point';
import { Stick } from './stick';


export function initInstanceObjects(width: number, height: number): { instancePoints: InstancePoint, sticks: Stick[] } {
  const points: Point[] = [];
  for (let h = 0; h < height; h++) {
    for (let w = 0; w < width; w++) {
      points.push(
        new Point(w, 0, h, false)
      );
    }
  }

  const sticks: Stick[] = []
  for (let i = 0; i < points.length; i++) {
    if (((i + 1) % width) != 0) {
      //left to right
      const stick = new Stick(points[i], points[i + 1]);
      sticks.push(stick);
    }
    //top to bottom
    if (i < ((points.length - width))) {
      const stick = new Stick(points[i], points[i + width]);
      sticks.push(stick);
    }
  }

  // top locks
  points[0].toggleLocked();
  points[width - 1].toggleLocked();
  points[(Math.round(width / 2) - 1)].toggleLocked();

  // bottom locks
  points[(width * height) - 1].toggleLocked();
  points[(width * height) - (Math.round(width / 2)) - 1].toggleLocked();
  points[(width * height) - (width - 1)].toggleLocked();

  // sidelocks
  points[(Math.round((width * height) / 2)) - Math.round(width / 2) - 1].toggleLocked();
  points[(Math.round((width * height) / 2)) + Math.round(width / 2) - 1].toggleLocked();

  const instancePoints = new InstancePoint(points, 0.1);

  return { instancePoints, sticks };
}
