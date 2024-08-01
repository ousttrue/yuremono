# build windows

```sh
zig build run
```

# build linux

```sh
# X11
sudo apt install libx11-dev libxcursor-dev libxi-dev libasound2-dev
zig build run
```

# run wasm

```sh
zig build -Dtarget=wasm32-emscripten run
```

http://localhost:6931
