# Collsion(衝突判定)

verlet 積分による位置更新の次のフェーズです。
loop で順番に衝突判定をし、衝突した場合は球の半径に応じて joint が移動します。
処理順の影響を受けます。

## オリジナル: Sphere x Sphere Collider

## StrandCloth: Sphere x Capsule Collider

球と線分の最近点を求めて、最近点に球があるとみなして判定する。

:::tip
vrm-1.0 と同じ
:::
