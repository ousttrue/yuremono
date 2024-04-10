# VrmのSpringBone

https://github.com/vrm-c/vrm-specification/blob/master/specification/0.0/README.ja.md

:::info vrm
https://github.com/vrm-c/UniVRM/blob/master/Assets/VRM/Runtime/SpringBone/VRMSpringBone.cs

```cs
// verlet積分で次の位置を計算
var nextTail = currentTail
               + (currentTail - prevTail) * (1.0f - dragForce) // 前フレームの移動を継続する(減衰もあるよ)
               + ParentRotation * LocalRotation * m_boneAxis * stiffnessForce // 親の回転による子ボーンの移動目標
               + external; // 外力による移動量
```
:::

:::tip オリジナルのrocketjumpと同じロジック

改造する際にミスっていなければ同じロジック

:::

:::tip 3つの実装がある

- vrm-0.x
- vrm-0.x-fastspringbone(DOTS)
- vrm-1.0(DOTS)

:::
