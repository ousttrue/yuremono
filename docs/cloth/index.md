# ClothSimulation

## constraint
格子状の四角形メッシュを、伸縮、曲げ、ねじれをばねの法則で拘束します。
3つ組合せると布っぽい動きになります。

### structural constraint

縦横の点とばね拘束する。

```
   o
   |
o--o
```

### bend constriant

上下左右の隣の隣の点をばね拘束することで、曲げに対するばねとする。

```
      o
      |
      +
      |
o--+--o
```

### shear constraint

斜めの点とばね拘束する。ねじりに対するばねとする。

```
o     o
 \   /
  \ /
   o
```

## ばねの法則

2点間の距離の変化に応じた力を発生させる。
遠ければ縮もうとし、近ければ伸びようとします。

[フックの法則 - Wikipedia](https://ja.wikipedia.org/wiki/%E3%83%95%E3%83%83%E3%82%AF%E3%81%AE%E6%B3%95%E5%89%87)

$$
F = k x
$$

## パラメーター

7つのパラメーターを使用します。

- K: ばね定数
- structural 伸/縮
- shear 伸/縮
- bend 伸/縮
