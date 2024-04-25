using UnityEngine;

namespace StrandCloth
{
    struct Triangle
    {
        public Plane Plane;
        public Vector3[] Points;

        public Vector3 a => Points[0];
        public Vector3 b => Points[1];
        public Vector3 c => Points[2];

        public Triangle(Vector3 a, Vector3 b, Vector3 c)
        {
            Plane = new Plane(a, b, c);
            Points = new Vector3[] { a, b, c };
        }

        public bool SameSide(Vector3 p)
        {
            var da = Vector3.Dot(p - a, b - a);
            var db = Vector3.Dot(p - b, c - b);
            var dc = Vector3.Dot(p - c, a - c);

            if (da > 0)
            {
                if (db > 0)
                {
                    if (dc > 0)
                    {
                        return true;
                    }
                }
            }
            else if (da < 0)
            {
                if (db < 0)
                {
                    if (dc < 0)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        static float getT(in Vector3 p0, in Vector3 p1, in Vector3 p)
        {
            return (p - p0).magnitude / (p1 - p0).magnitude;
        }

        public bool TryIntersect(in Vector3 p0, in Vector3 p1, out float t)
        {
            // xa xb xc
            // xa xb
            // xb xc
            // xc xa
            // xa
            // xb
            // xc
            // 無
            var xa = IntersectSegments(p0, p1, a, b);
            if (xa.HasValue)
            {
                var xb = IntersectSegments(p0, p1, b, c);
                if (xb.HasValue)
                {
                    t = Mathf.Min(getT(p0, p1, xa.Value), getT(p0, p1, xb.Value));
                    return true;
                }
                else
                {
                    var xc = IntersectSegments(p0, p1, c, a);
                    if (xc.HasValue)
                    {
                        t = Mathf.Min(getT(p0, p1, xc.Value), getT(p0, p1, xa.Value));
                        return true;
                    }
                    else
                    {
                        // TODO: xa or 0 or 1
                        t = getT(p0, p1, xa.Value);
                        return true;
                    }
                }
            }
            else
            {
                var xb = IntersectSegments(p0, p1, b, c);
                if (xb.HasValue)
                {
                    var xc = IntersectSegments(p0, p1, c, a);
                    if (xc.HasValue)
                    {
                        t = Mathf.Min(getT(p0, p1, xb.Value), getT(p0, p1, xc.Value));
                        return true;
                    }
                    else
                    {
                        // TODO: xb or 0 or 1
                        t = getT(p0, p1, xb.Value);
                        return true;
                    }
                }
                else
                {
                    var xc = IntersectSegments(p0, p1, c, a);
                    if (xc.HasValue)
                    {
                        // TODO: xc or 0 or 1
                        t = getT(p0, p1, xc.Value);
                        return true;
                    }
                    else
                    {
                        // TODO: 内なら 0 or 1
                        t = 0;
                        return false;
                    }
                }
            }
        }

        static Vector3? IntersectSegments(in Vector3 a, in Vector3 b, in Vector3 c, in Vector3 d)
        {
            var deno = Vector3.Cross(b - a, d - c).magnitude;
            // point error = { INF, INF };
            if (deno == 0.0)
            {
                // 線分が平行
                return default;
            }
            var s = Vector3.Cross(c - a, d - c).magnitude / deno;
            var t = Vector3.Cross(b - a, a - c).magnitude / deno;
            if (s < 0.0 || 1.0 < s || t < 0.0 || 1.0 < t)
            {
                // 線分が交差していない
                return default;
            }

            return new Vector3(
                a.x + s * (b - a).x,
                a.y + s * (b - a).y,
                a.z + s * (b - a).z
             );
        }

        public void DrawGizmo()
        {
            Gizmos.DrawLineStrip(Points, true);
        }
    }

}