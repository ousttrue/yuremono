# パラメーター

## stiffness(紐・布)

元の向きに戻ろうとする力(0 ~ 1)

:::tip
tail と rest 位置を結ぶバネを実装していて、
通常の cloth simulation の曲げバネとねじりバネの変わりに使います。

[SpringBone](/docs/springbone/rocketjump) に参考にして編み出した。

cloth simulation では四角の区画が必要ですが、紐でも使えます。
:::

## dragForce(紐・布)

verlet 積分の過去フレームとの差分から計算した速度に対する係数(0 ~ 1)

## external(紐・布)

重力など。`new Vector3(0, -0.001, 0)` などにして他の力の大きさとつりあうようにします。

:::info
紐の長さが伸縮しないように強制しているのでのびることはありません。
:::

## cloth係数(布)

紐を横に連結したときの横方向の係数です。

