# 処理順

## 初期化

- T-Pose などで初期化する

## strand root の update と各 particle の回転を参照して回転が元に戻った場合の位置(restposition)計算する

:::warning
回転を使う
:::

```csharp

//
// input
//
// 各strandのrootの移動と回転を外部から入力する。
// それらを元に各 joint の方向を元に戻した場合の戻り位置を計算する
foreach (var strand in _strands)
{
    strand.UpdateRoot(_list._particleTransforms, _newPos, _restPositions);
}
```

## verlet 積分で更新位置を計算する

- strand の root の移動
- verlet による現在位置と前フレーム位置の差分を速度とした移動
- 重力などの外力
- 布の場合はばね力

:::info
回転を使わない
:::

```cs
//
// particle simulation
//
// verlet 積分
var sqDt = Time.deltaTime * Time.deltaTime;
_list.BeginFrame(Env, sqDt, _restPositions);
foreach (var (spring, collision) in _clothRects)
{
    // cloth constraint
    spring.Resolve(sqDt, _clothFactor);
}
_list.Verlet(Env, sqDt, _newPos.Init);
```

## 無さで拘束

- root から長さを強制する

:::tip
通常の cloth では縦方向のばねによって長さを維持するのだが、
本実装では位置を直接更新することにした。
:::

:::tip
長さ強制後の位置で衝突判定をするので、
このタイミング。
:::

:::warning
再帰を使う
:::

:::info
回転を使わない
:::

```cs
// 長さで拘束
foreach (var strand in _strands)
{
    strand.ForceLength(_newPos);
}
```

## 衝突判定

- srand または cloth と collider で衝突を判定して位置を更新する

:::info
回転を使わない
:::

:::warning
三角形とカプセルの衝突はだいぶ重い。
:::

```cs
// collision
if (_clothRects.Count > 0)
{
    // cloth
    foreach (var (spring, rect) in _clothRects)
    {
        rect.Collide(_newPos, _colliders);
    }
}
else
{
    // strand
    foreach (var particle in _list._particles)
    {
        if (particle.Init.Mass == 0)
        {
            continue;
        }

        var p = _newPos.Get(particle.Init.Index);
        foreach (var c in _colliders)
        {
            if (c != null && c.TryCollide(p, particle.Init.Radius, out var resolved))
            {
                _newPos.CollisionMove(particle.Init.Index, resolved, c.Radius);
            }
        }
    }
}
var result = _newPos.Resolve();
```

## 結果反映

親から順番に回転を計算しながら位置を反映させる。

:::warning
回転を使う
:::

:::warning
再帰を使う
:::


```cs
//
// apply result
//
// apply positions and
// calc rotation from positions recursive
foreach (var strand in _strands)
{
    strand.Apply(_list._particleTransforms, result);
}
``````
