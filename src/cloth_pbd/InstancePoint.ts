import * as THREE from 'three';
import { Point } from './point';


export class InstancePoint {
  dummy = new THREE.Object3D();
  mesh: THREE.InstancedMesh;
  gravity = -9.81;
  constructor(public readonly points: Point[], radius: number) {
    var geometry = new THREE.SphereGeometry(radius, 16, 16);
    var material = new THREE.MeshBasicMaterial({ color: 0xffffff });
    this.mesh = new THREE.InstancedMesh(geometry, material, this.points.length);
    this.mesh.instanceMatrix.setUsage(THREE.DynamicDrawUsage); // will be updated every frame
  }

  updatePoints(delta: number) {
    // update points in random order
    var random = this.generateRandomIndexes(this.points.length);
    for (let i = 0; i < this.points.length; i++) {
      var index = random[i];
      var point = this.points[index];
      point.updatePoint(delta, this.gravity);
      // point.test();
      this.dummy.position.set(point.position.x, point.position.y, point.position.z);
      this.dummy.updateMatrix();
      this.mesh.setMatrixAt(index, this.dummy.matrix);
      var color = new THREE.Color(point.defaultColor);
      this.mesh.setColorAt(index, color);
    }
    this.mesh.instanceMatrix.needsUpdate = true;
    this.mesh.instanceColor.needsUpdate = true;
  }

  constrainPoints(sceneW: number, sceneH: number) {
    for (let i = 0; i < this.points.length; i++) {
      var point = this.points[i];
      point.constrainPoint(sceneW, sceneH);
      this.dummy.position.set(point.position.x, point.position.y, 0);
      this.dummy.updateMatrix();
      this.mesh.setMatrixAt(i++, this.dummy.matrix);
    }
    this.mesh.instanceMatrix.needsUpdate = true;
  }

  generateRandomIndexes(length: number) {
    var array = [];
    for (let i = 0; i < length; i++) {
      array.push(i);
    }
    var currentIndex = array.length, temporaryValue, randomIndex;
    while (0 !== currentIndex) {
      randomIndex = Math.floor(Math.random() * currentIndex);
      currentIndex -= 1;
      temporaryValue = array[currentIndex];
      array[currentIndex] = array[randomIndex];
      array[randomIndex] = temporaryValue;
    }
    return array;
  }

  randomArrayShuffle(array: any[]) {
    var currentIndex = array.length, temporaryValue, randomIndex;
    while (0 !== currentIndex) {
      randomIndex = Math.floor(Math.random() * currentIndex);
      currentIndex -= 1;
      temporaryValue = array[currentIndex];
      array[currentIndex] = array[randomIndex];
      array[randomIndex] = temporaryValue;
    }
    return array;
  }
}
