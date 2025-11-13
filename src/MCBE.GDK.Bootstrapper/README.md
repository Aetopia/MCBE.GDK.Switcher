# MCBE GDK Bootstrapper

> [!IMPORTANT]
> This project is still a work in progress hence expect issues & bugs.

Replaces the [PC Bootstrapper](https://learn.microsoft.com/en-us/gaming/gdk/docs/gdk-dev/pc-dev/overviews/gr-pc-bootstrapper) that ships with Minecraft for Windows.

## Features

- Directly launches Minecraft for Windows.

- Prevents auto-updates forced by the stock PC Bootstrapper.

## Usage

> [!CAUTION]
> Launching without the stock PC Bootstrapper might cause issues.

- From the latest release, download the file called [`GameLaunchHelper.exe`](https://github.com/Aetopia/MCBE.GDK.Switcher/releases/latest/download/GameLaunchHelper.exe).

- Locate where Minecraft: Bedrock Edition (GDK) is installed.

- Replace the stock PC Bootstrapper (`GameLaunchHelper.exe`) with the MCBE GDK Bootstrapper.

## Build
1. Install & update [MSYS2](https://www.msys2.org):

    ```bash
    pacman -Syu --noconfirm
    ```

3. Install [GCC](https://gcc.gnu.org) & [MinHook](https://github.com/TsudaKageyu/minhook):

    ```bash
    pacman -Syu mingw-w64-ucrt-x86_64-gcc mingw-w64-ucrt-x86_64-MinHook --noconfirm
    ```

3. Start MSYS2's `UCRT64` environment & run `MCBE.GDK.Bootstrapper.cmd`.