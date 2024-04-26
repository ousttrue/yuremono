# 実装案(Strand Cloth)

Unity で実装中。

https://github.com/ousttrue/yuremono/tree/master/unity

[rocketjump(始祖)](/docs/springbone/rocketjump) をベースに発展させたものです。

オリジナルの参考にした strand(紐) から名前を頂いて、
`StrandCloth` と名付けました。
StrandCloth では、`Strand`, `CLoth`, `Spring` を次の意味で使います。

- Strand: 今迄 SpringBone と呼んでいた紐状のゆれもの
- Cloth: Strand を横に連結したゆれもの
- Spring: Cloth を横に連結するときの拘束(フック(ばね)の法則)

- 質点: transform, bone, joint, particle...

## TODO:

- Center
- Scale

