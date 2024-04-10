# rocketjump(始祖)

:::info 2014

https://web.archive.org/web/20190515015027/http://rocketjump.skr.jp/unity3d/109/

```cs
public void UpdateSpring()
{
	//回転をリセット
	trs.localRotation = Quaternion.identity * localRotation;

	float sqrDt = Time.deltaTime * Time.deltaTime;

	//stiffness
	Vector3 force = trs.rotation * (boneAxis * stiffnessForce) / sqrDt;

	//drag
	force += (prevTipPos - currTipPos) * dragForce / sqrDt;

	force += springForce / sqrDt;

	//前フレームと値が同じにならないように
	Vector3 temp = currTipPos;

	//verlet
	currTipPos = (currTipPos - prevTipPos) + currTipPos + (force * sqrDt); // 👈

	//長さを元に戻す
	currTipPos = ((currTipPos - trs.position).normalized * springLength) + trs.position;

	//衝突判定
	for (int i = 0; i < colliders.Length; i++)
	{
		if (Vector3.Distance(currTipPos, colliders[i].transform.position) <= (radius + colliders[i].radius))
		{
			Vector3 normal = (currTipPos - colliders[i].transform.position).normalized;
			currTipPos = colliders[i].transform.position + (normal * (radius + colliders[i].radius));
			currTipPos = ((currTipPos - trs.position).normalized * springLength) + trs.position;
		}
	}

	prevTipPos = temp;

	//回転を適用；
	Vector3 aimVector = trs.TransformDirection(boneAxis);
	Quaternion aimRotation = Quaternion.FromToRotation(aimVector, currTipPos - trs.position);
	trs.rotation = aimRotation * trs.rotation;
}
```

:::

:::tip 子ボーンの方が速度が速い？
更新中に親が移動するのに影響されて子も移動する？
そのため末端の方が加速すると思われる。
ムチのようにしなっていい感じになるかもれない。

ただし、並列処理できない。
:::
