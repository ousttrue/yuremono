const std = @import("std");
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

        // create a build step which invokes the Emscripten linker
        const emsdk = dep_sokol.builder.dependency("emsdk", .{});
        const link_step = try sokol.emLinkStep(b, .{
            .lib_main = lib,
            .target = target,
            .optimize = optimize,
            .emsdk = emsdk,
            .use_webgl2 = true,
            .use_emmalloc = true,
            .use_filesystem = false,
            .shell_file_path = dep_sokol.path("src/sokol/web/shell.html").getPath(b),
            .extra_args = &.{"-sUSE_OFFSET_CONVERTER=1"},
        });
        // ...and a special run step to start the web build output via 'emrun'
        const run = sokol.emRunStep(b, .{ .name = "pacman", .emsdk = emsdk });
        run.step.dependOn(&link_step.step);
        b.step("run", "Run pacman").dependOn(&run.step);

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
    b.installArtifact(compile);

    const exe_unit_tests = b.addTest(.{
        .root_source_file = b.path("src/main.zig"),
        .target = target,
        .optimize = optimize,
    });
    const run_exe_unit_tests = b.addRunArtifact(exe_unit_tests);
    const test_step = b.step("test", "Run unit tests");
    test_step.dependOn(&run_exe_unit_tests.step);
}
