# Dragforce(速度減衰)

特に変えていません。

```cs
var velocity = (current - prev) * dragForce;
```

:::tip
値域 0 ~ 1

0.4 がよい？
:::
