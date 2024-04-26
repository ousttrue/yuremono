# Stiffness(剛性)

SpringBone の各ジョイントは回転しかしないので、
回転が元に戻る力のことです。

```cs
// stiffnessForce を無限に大きくすることで、方向ベクトルを元の方向に向ける
Vector3 force = trs.rotation * (boneAxis * stiffnessForce) / sqrDt;
```

:::tip オリジナルの値域
0 ~ 無限大
:::

StrandCloth では、元の向きに戻る撃力？として実装しています。

```cs
public void AddStiffnessForce(float delta, float stiffness)
{
    if (Mass == 0)
    {
        return;
    }
    var restRotation = transform.parent.parent.rotation * _init.ParentLocalRotation;
    var restPosition = transform.parent.position + restRotation * _init.BoneAxis;
    var f = Stiffness(restPosition, _runtime.CurrentPosition, stiffness);
    _force += f;
}
static Vector3 Stiffness(Vector3 restPosition, Vector3 currTipPos, float stiffness)
{
    return (restPosition - currTipPos) * stiffness;
}
```

:::tip StrandCloth の値域
0 ~ 1
:::
