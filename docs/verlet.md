# ベルレ積分法(Verlet integration)

[ベレの方法 - Wikipedia](https://ja.wikipedia.org/wiki/%E3%83%99%E3%83%AC%E3%81%AE%E6%96%B9%E6%B3%95)

位置更新の実装方法です。

nでの位置 $p_{n}$ 速度 $v_{n}$ 加速度 $a_{n}$ がわかっているときに $\Delta t$ 後の位置 $p_{n+1}$ は、
次式で計算できます。

$$
p_{n+1} = p_{n} + v_{n} \Delta t + \frac{1}{2} a_{n} \Delta t^2 
$$

このとき、速度ではなくて過去の位置 $p_{n-1}$ を代わりに使って次式のようします。

$$
p_{n+1} = 2 p_{n} - p_{n-1} + a_{n} \Delta t^2
$$

:::tip

$$
v_{n} \Delta t = p_{n} - p_{n-1}
$$

と置くのと似ているがちょっと違う。

:::

:::tip
3Dモデルでは親が移動することによって子のSpringBone などに速度が生じます。
:::
