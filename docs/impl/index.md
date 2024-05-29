# 実装案1(Strand Cloth)

Unity で実装中。

https://github.com/ousttrue/yuremono/tree/master/unity/Assets/StrandCloth

[rocketjump(始祖)](/docs/springbone/rocketjump) をベースに発展させたものです。

オリジナルの実装が softimage の strand(紐) を参考にしたことに因んで、
`StrandCloth` と名付けました。

## 問題点

- ~~Collsion が厳しいときに振動しやすい~~ 三角形とカプセルの衝突判定が甘かった。

