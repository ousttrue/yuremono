const std = @import("std");
const builtin = @import("builtin");
const sokol = @import("sokol");
const NAME = "yuremono";
const ENTRY_POINT = "src/main.zig";

pub fn build(b: *std.Build) void {
    const target = b.standardTargetOptions(.{});
    const optimize = b.standardOptimizeOption(.{});

    const dep_sokol = b.dependency("sokol", .{
        .target = target,
        .optimize = optimize,
    });

    const compile = if (target.result.isWasm()) block: {
        const lib = b.addStaticLibrary(.{
            .name = NAME,
            .target = target,
            .optimize = optimize,
            .root_source_file = b.path(ENTRY_POINT),
        });
        break :block lib;
    } else block: {
        const exe = b.addExecutable(.{
            .name = NAME,
            .root_source_file = b.path(ENTRY_POINT),
            .target = target,
            .optimize = optimize,
        });

        const run_cmd = b.addRunArtifact(exe);
        run_cmd.step.dependOn(b.getInstallStep());
        if (b.args) |args| {
            run_cmd.addArgs(args);
        }
        b.step("run", "Run the app").dependOn(&run_cmd.step);

        break :block exe;
    };
    compile.root_module.addImport("sokol", dep_sokol.module("sokol"));
    compile.step.dependOn(buildShader(b, target, "../sokol-tools-bin/bin/", "src/main.glsl"));
    b.installArtifact(compile);

    // link
    if (target.result.isWasm()) {
        // create a build step which invokes the Emscripten linker
        const emsdk = dep_sokol.builder.dependency("emsdk", .{});
        const link_step = try sokol.emLinkStep(b, .{
            .lib_main = compile,
            .target = target,
            .optimize = optimize,
            .emsdk = emsdk,
            .use_webgl2 = true,
            .use_emmalloc = true,
            .use_filesystem = false,
            .shell_file_path = dep_sokol.path("src/sokol/web/shell.html").getPath(b),
            .extra_args = &.{
                "-sTOTAL_MEMORY=500MB",
                "-sUSE_OFFSET_CONVERTER=1",
                "-sASSERTIONS",
                "-g3",
            },
        });
        const run = sokol.emRunStep(b, .{ .name = NAME, .emsdk = emsdk });
        run.step.dependOn(&link_step.step);
        b.step("run", "Run sample").dependOn(&run.step);
    }

    const exe_unit_tests = b.addTest(.{
        .root_source_file = b.path("src/main.zig"),
        .target = target,
        .optimize = optimize,
    });
    const run_exe_unit_tests = b.addRunArtifact(exe_unit_tests);
    const test_step = b.step("test", "Run unit tests");
    test_step.dependOn(&run_exe_unit_tests.step);
}

// a separate step to compile shaders, expects the shader compiler in ../sokol-tools-bin/
// TODO: install sokol-shdc via package manager
fn buildShader(
    b: *std.Build,
    target: std.Build.ResolvedTarget,
    comptime sokol_tools_bin_dir: []const u8,
    comptime shader: []const u8,
) *std.Build.Step {
    const optional_shdc: ?[:0]const u8 = comptime switch (builtin.os.tag) {
        .windows => "win32/sokol-shdc.exe",
        .linux => "linux/sokol-shdc",
        .macos => if (builtin.cpu.arch.isX86()) "osx/sokol-shdc" else "osx_arm64/sokol-shdc",
        else => null,
    };
    if (optional_shdc == null) {
        std.log.warn("unsupported host platform, skipping shader compiler step", .{});
        return;
    }
    const shdc_path = sokol_tools_bin_dir ++ optional_shdc.?;
    const glsl = if (target.result.isDarwin()) "glsl410" else "glsl430";
    const slang = glsl ++ ":metal_macos:hlsl5:glsl300es:wgsl";
    return &b.addSystemCommand(&.{
        shdc_path,
        "-i",
        shader,
        "-o",
        shader ++ ".zig",
        "-l",
        slang,
        "-f",
        "sokol_zig",
    }).step;
}
