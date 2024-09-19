# zig + wasm 版

[wasm](/wasm/yuremono.html)

- 再帰的に位置と方向を決定するロジックを含むため Joint 並列にできない。一瞬伸びることを許容すればJoint並列にできる。 framerate が維持できていれば微細な伸びは目立たないと思われるので、検証する。

- 衝突解決のロジックを決める。複数衝突。これも joint 並列化を試行する。

`verlet => constraint1(recursive) => collision(recursive) => constraint2(recursive)`

## TODO:

### parent constraint

### verlet + parent constraint

### verlet + local position constraint

- 親を回転させる
- 初期位置への復帰

`rocketjump仕様`

### vrm-0.x仕様

### vrm-1.0仕様

### vrm-1.0改 + joint 並列実装

### cloth 拡張
