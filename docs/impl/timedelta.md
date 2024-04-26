# TimeDelta

$$
p_{n+1} = 2 p_{n} - p_{n-1} + a_{n} \Delta t^2
$$

StrandCloth では force は t二乗 が乗算済みとして実装しています。
コード中で `Time.deltaTime` を使っていません(今のところ)。

:::note
frame rate との兼ねあいなど注意
:::
