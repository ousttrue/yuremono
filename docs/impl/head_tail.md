---
title: Bone Tail or Parent
---

:::tip
verlet 積分するボーンと、結果を反映するボーンがずれている。
:::

## self - tail

オリジナルでは子ボーンのひとつを tail として選択し`self - tail` で処理します。
tail の次の場所を verlet 積分や衝突判定から得ます。
確定した tail の位置から self の rotation を決定します。

## parent - self

StrandCloth では、 self の次の場所を verlet 積分や衝突判定から得ます。
確定した self の位置から `parent` の rotation を決定します。

- Gizmo の描画が自然
- 枝分かれの対応がしやすい
