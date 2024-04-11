# 実装案

## spring and cloth

spring と cloth をまとめて、質点シミュレーションの枠組みで動かす。

名称も `SpringBone` でなくて `particle dynamics` などの質点感のある名前に。

## bone or vertex

最初は、bone の方で実装する。

- SpringBone の機能拡張が当初の目的
  - SpringBone間のすり抜けを改善したい
- vertex へのパラメーター付与が煩雑(頂点カラー経由？)
- glTF は三角形 mesh なので、四角格子を推測する必要がある

## springbone 改造

- 親から子への再帰をやめて並列処理できるようにする(おそらく挙動が変わる。末端の方が加速しやすいように減速などのパラメーターを調整するとよさそうな気がする)
- stiffness を cloth の bend ロジックに起きかえてみる

## collision

質点と collider では質点側に詳細な形状がある方が、
貫通防止になると思われる。

### 四角collider

- cloth の四角格子をそのまま collider にする

