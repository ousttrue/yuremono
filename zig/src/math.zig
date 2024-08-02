//------------------------------------------------------------------------------
//  math.zig
//
//  minimal vector math helper functions, just the stuff needed for
//  the sokol-samples
//
//  Ported from HandmadeMath.h
//------------------------------------------------------------------------------
const assert = @import("std").debug.assert;
const math = @import("std").math;

fn radians(deg: f32) f32 {
    return deg * (math.pi / 180.0);
}

pub const Vec2 = extern struct {
    x: f32,
    y: f32,

    pub fn zero() Vec2 {
        return Vec2{ .x = 0.0, .y = 0.0 };
    }

    pub fn new(x: f32, y: f32) Vec2 {
        return Vec2{ .x = x, .y = y };
    }
};

pub const Vec3 = extern struct {
    x: f32,
    y: f32,
    z: f32,

    pub fn zero() Vec3 {
        return Vec3{ .x = 0.0, .y = 0.0, .z = 0.0 };
    }

    pub fn new(x: f32, y: f32, z: f32) Vec3 {
        return Vec3{ .x = x, .y = y, .z = z };
    }

    pub fn up() Vec3 {
        return Vec3{ .x = 0.0, .y = 1.0, .z = 0.0 };
    }

    pub fn len(v: Vec3) f32 {
        return math.sqrt(Vec3.dot(v, v));
    }

    pub fn add(left: Vec3, right: Vec3) Vec3 {
        return Vec3{ .x = left.x + right.x, .y = left.y + right.y, .z = left.z + right.z };
    }

    pub fn sub(left: Vec3, right: Vec3) Vec3 {
        return Vec3{ .x = left.x - right.x, .y = left.y - right.y, .z = left.z - right.z };
    }

    pub fn mul(v: Vec3, s: f32) Vec3 {
        return Vec3{ .x = v.x * s, .y = v.y * s, .z = v.z * s };
    }

    pub fn norm(v: Vec3) Vec3 {
        const l = Vec3.len(v);
        if (l != 0.0) {
            return Vec3{ .x = v.x / l, .y = v.y / l, .z = v.z / l };
        } else {
            return Vec3.zero();
        }
    }

    pub fn cross(v0: Vec3, v1: Vec3) Vec3 {
        return Vec3{ .x = (v0.y * v1.z) - (v0.z * v1.y), .y = (v0.z * v1.x) - (v0.x * v1.z), .z = (v0.x * v1.y) - (v0.y * v1.x) };
    }

    pub fn dot(v0: Vec3, v1: Vec3) f32 {
        return v0.x * v1.x + v0.y * v1.y + v0.z * v1.z;
    }
};

pub const Vec4 = extern struct {
    x: f32,
    y: f32,
    z: f32,
    w: f32,

    pub fn dot(v0: Vec4, v1: Vec4) f32 {
        return v0.x * v1.x + v0.y * v1.y + v0.z * v1.z + v0.w * v1.w;
    }

    pub fn toVec3(self: @This()) Vec3 {
        return .{
            .x = self.x,
            .y = self.y,
            .z = self.z,
        };
    }
};

pub const Mat4 = extern struct {
    m: [16]f32,

    pub fn identity() Mat4 {
        return Mat4{
            .m = [_]f32{
                1.0, 0.0, 0.0, 0.0,
                0.0, 1.0, 0.0, 0.0,
                0.0, 0.0, 1.0, 0.0,
                0.0, 0.0, 0.0, 1.0,
            },
        };
    }

    pub fn zero() Mat4 {
        return Mat4{
            .m = [_]f32{
                0.0, 0.0, 0.0, 0.0,
                0.0, 0.0, 0.0, 0.0,
                0.0, 0.0, 0.0, 0.0,
                0.0, 0.0, 0.0, 0.0,
            },
        };
    }

    pub fn transpose(s: Mat4) Mat4 {
        return .{
            .m = [_]f32{
                s.m[0], s.m[4], s.m[8],  s.m[12],
                s.m[1], s.m[5], s.m[9],  s.m[13],
                s.m[2], s.m[6], s.m[10], s.m[14],
                s.m[3], s.m[7], s.m[11], s.m[15],
            },
        };
    }

    pub fn scale(s: Vec3) Mat4 {
        return Mat4{
            .m = [_]f32{
                s.x, 0.0, 0.0, 0.0,
                0.0, s.y, 0.0, 0.0,
                0.0, 0.0, s.z, 0.0,
                0.0, 0.0, 0.0, 1.0,
            },
        };
    }

    pub fn row0(self: Mat4) Vec4 {
        return .{ .x = self.m[0], .y = self.m[1], .z = self.m[2], .w = self.m[3] };
    }
    pub fn row1(self: Mat4) Vec4 {
        return .{ .x = self.m[4], .y = self.m[5], .z = self.m[6], .w = self.m[7] };
    }
    pub fn row2(self: Mat4) Vec4 {
        return .{ .x = self.m[8], .y = self.m[9], .z = self.m[10], .w = self.m[11] };
    }
    pub fn row3(self: Mat4) Vec4 {
        return .{ .x = self.m[12], .y = self.m[13], .z = self.m[14], .w = self.m[15] };
    }
    pub fn col0(self: Mat4) Vec4 {
        return .{ .x = self.m[0], .y = self.m[4], .z = self.m[8], .w = self.m[12] };
    }
    pub fn col1(self: Mat4) Vec4 {
        return .{ .x = self.m[1], .y = self.m[5], .z = self.m[9], .w = self.m[13] };
    }
    pub fn col2(self: Mat4) Vec4 {
        return .{ .x = self.m[2], .y = self.m[6], .z = self.m[10], .w = self.m[14] };
    }
    pub fn col3(self: Mat4) Vec4 {
        return .{ .x = self.m[3], .y = self.m[7], .z = self.m[11], .w = self.m[15] };
    }

    pub fn mul(left: Mat4, right: Mat4) Mat4 {
        return Mat4{
            .m = [_]f32{
                left.row0().dot(right.col0()), left.row0().dot(right.col1()), left.row0().dot(right.col2()), left.row0().dot(right.col3()),
                left.row1().dot(right.col0()), left.row1().dot(right.col1()), left.row1().dot(right.col2()), left.row1().dot(right.col3()),
                left.row2().dot(right.col0()), left.row2().dot(right.col1()), left.row2().dot(right.col2()), left.row2().dot(right.col3()),
                left.row3().dot(right.col0()), left.row3().dot(right.col1()), left.row3().dot(right.col2()), left.row3().dot(right.col3()),
            },
        };
    }

    pub fn persp(fov: f32, aspect: f32, near: f32, far: f32) Mat4 {
        var res = Mat4.identity();
        const t = math.tan(fov * (math.pi / 360.0));
        res.m[0] = 1.0 / t;
        res.m[5] = aspect / t;
        res.m[11] = -1.0;
        res.m[10] = (near + far) / (near - far);
        res.m[14] = (2.0 * near * far) / (near - far);
        res.m[15] = 0.0;
        return res;
    }

    pub fn lookat(eye: Vec3, center: Vec3, up: Vec3) Mat4 {
        var res = Mat4.zero();

        const f = Vec3.norm(Vec3.sub(center, eye));
        const s = Vec3.norm(Vec3.cross(f, up));
        const u = Vec3.cross(s, f);

        res.m[0] = s.x;
        res.m[1] = u.x;
        res.m[2] = -f.x;

        res.m[4] = s.y;
        res.m[5] = u.y;
        res.m[6] = -f.y;

        res.m[8] = s.z;
        res.m[9] = u.z;
        res.m[10] = -f.z;

        res.m[12] = -Vec3.dot(s, eye);
        res.m[13] = -Vec3.dot(u, eye);
        res.m[14] = Vec3.dot(f, eye);
        res.m[15] = 1.0;

        return res;
    }

    pub fn rotate(angle: f32, axis_unorm: Vec3) Mat4 {
        var res = Mat4.identity();

        const axis = Vec3.norm(axis_unorm);
        const sin_theta = math.sin(radians(angle));
        const cos_theta = math.cos(radians(angle));
        const cos_value = 1.0 - cos_theta;

        res.m[0] = (axis.x * axis.x * cos_value) + cos_theta;
        res.m[1] = (axis.x * axis.y * cos_value) + (axis.z * sin_theta);
        res.m[2] = (axis.x * axis.z * cos_value) - (axis.y * sin_theta);
        res.m[4] = (axis.y * axis.x * cos_value) - (axis.z * sin_theta);
        res.m[5] = (axis.y * axis.y * cos_value) + cos_theta;
        res.m[6] = (axis.y * axis.z * cos_value) + (axis.x * sin_theta);
        res.m[8] = (axis.z * axis.x * cos_value) + (axis.y * sin_theta);
        res.m[9] = (axis.z * axis.y * cos_value) - (axis.x * sin_theta);
        res.m[10] = (axis.z * axis.z * cos_value) + cos_theta;

        return res;
    }

    pub fn translate(translation: Vec3) Mat4 {
        var res = Mat4.identity();
        res.m[12] = translation.x;
        res.m[13] = translation.y;
        res.m[14] = translation.z;
        return res;
    }
};

test "Vec3.zero" {
    const v = Vec3.zero();
    assert(v.x == 0.0 and v.y == 0.0 and v.z == 0.0);
}

test "Vec3.new" {
    const v = Vec3.new(1.0, 2.0, 3.0);
    assert(v.x == 1.0 and v.y == 2.0 and v.z == 3.0);
}

test "Mat4.ident" {
    const m = Mat4.identity();
    for (m.m, 0..) |row, y| {
        for (row, 0..) |val, x| {
            if (x == y) {
                assert(val == 1.0);
            } else {
                assert(val == 0.0);
            }
        }
    }
}

test "Mat4.mul" {
    const l = Mat4.identity();
    const r = Mat4.identity();
    const m = Mat4.mul(l, r);
    for (m.m, 0..) |row, y| {
        for (row, 0..) |val, x| {
            if (x == y) {
                assert(val == 1.0);
            } else {
                assert(val == 0.0);
            }
        }
    }
}

fn eq(val: f32, cmp: f32) bool {
    const delta: f32 = 0.00001;
    return (val > (cmp - delta)) and (val < (cmp + delta));
}

test "Mat4.persp" {
    const m = Mat4.persp(60.0, 1.33333337, 0.01, 10.0);

    assert(eq(m.m[0][0], 1.73205));
    assert(eq(m.m[0][1], 0.0));
    assert(eq(m.m[0][2], 0.0));
    assert(eq(m.m[0][3], 0.0));

    assert(eq(m.m[1][0], 0.0));
    assert(eq(m.m[1][1], 2.30940));
    assert(eq(m.m[1][2], 0.0));
    assert(eq(m.m[1][3], 0.0));

    assert(eq(m.m[2][0], 0.0));
    assert(eq(m.m[2][1], 0.0));
    assert(eq(m.m[2][2], -1.00200));
    assert(eq(m.m[2][3], -1.0));

    assert(eq(m.m[3][0], 0.0));
    assert(eq(m.m[3][1], 0.0));
    assert(eq(m.m[3][2], -0.02002));
    assert(eq(m.m[3][3], 0.0));
}

test "Mat4.lookat" {
    const m = Mat4.lookat(.{ .x = 0.0, .y = 1.5, .z = 6.0 }, Vec3.zero(), Vec3.up());

    assert(eq(m.m[0][0], 1.0));
    assert(eq(m.m[0][1], 0.0));
    assert(eq(m.m[0][2], 0.0));
    assert(eq(m.m[0][3], 0.0));

    assert(eq(m.m[1][0], 0.0));
    assert(eq(m.m[1][1], 0.97014));
    assert(eq(m.m[1][2], 0.24253));
    assert(eq(m.m[1][3], 0.0));

    assert(eq(m.m[2][0], 0.0));
    assert(eq(m.m[2][1], -0.24253));
    assert(eq(m.m[2][2], 0.97014));
    assert(eq(m.m[2][3], 0.0));

    assert(eq(m.m[3][0], 0.0));
    assert(eq(m.m[3][1], 0.0));
    assert(eq(m.m[3][2], -6.18465));
    assert(eq(m.m[3][3], 1.0));
}

test "Mat4.rotate" {
    const m = Mat4.rotate(2.0, .{ .x = 0.0, .y = 1.0, .z = 0.0 });

    assert(eq(m.m[0][0], 0.99939));
    assert(eq(m.m[0][1], 0.0));
    assert(eq(m.m[0][2], -0.03489));
    assert(eq(m.m[0][3], 0.0));

    assert(eq(m.m[1][0], 0.0));
    assert(eq(m.m[1][1], 1.0));
    assert(eq(m.m[1][2], 0.0));
    assert(eq(m.m[1][3], 0.0));

    assert(eq(m.m[2][0], 0.03489));
    assert(eq(m.m[2][1], 0.0));
    assert(eq(m.m[2][2], 0.99939));
    assert(eq(m.m[2][3], 0.0));

    assert(eq(m.m[3][0], 0.0));
    assert(eq(m.m[3][1], 0.0));
    assert(eq(m.m[3][2], 0.0));
    assert(eq(m.m[3][3], 1.0));
}
