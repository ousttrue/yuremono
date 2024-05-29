# 揺れもの

3D Model の髪や服を自動でアニメーションさせます。

情報収集や実装をします。

:::tip 目標
スカートで足が貫通しない
:::

## 用語

`Strand`, `Cloth`, `Spring` を次の意味で使います。

- Strand: SpringBone と呼んでいた紐状のゆれもの
- Cloth: Strand を横に連結したゆれもの
- Spring: Cloth を横に連結するときの拘束(フック(ばね)の法則)
- Particle(質点): Strand の Joint。回転運動はしないのだけど、移動結果を回転としてフィードバックするなど限定的に回転も使いす。また、各パーティクルは親子関係を持ちます。

## 実装案1 [SpringBone](/docs/springbone/rocketjump) Base: [StrandCloth](/docs/impl)

実装案2が本命なのだけど、
安定性などの品質に問題があった場合のバックアッププラン。
こちらで、三角形とカプセルの衝突を実装した。

- 後にコリジョンは、実装案2と共通化された。
- 実装案2も verlet 後に長さ拘束を入れることにしたので似た実装になった。

## 実装案2 [ClothSimulation](/docs/cloth/impl/cloth_yr) Base: [RotateParticle](/docs/impl2/)

通常の cloth simulation 実装から出発して質点の並列処理をできることに留意して実装を進めた。
