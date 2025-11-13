# MCBE GDK Switcher 

> [!IMPORTANT]
> This project is still a work in progress hence expect issues & bugs.

Easily switch between different GDK versions of Minecraft: Bedrock Edition.

## Features

- Install any GDK version of Minecraft: Bedrock Edition.

- Automatically stop forced auto-updates to prevent unattended updates.
    
    - This done by replacing the stock PC Bootstrapper with our own.
    
    - The Bootstrapper replacement that ships with MCBE GDK Switcher can be [viewed here](src/MCBE.GDK.Bootstrapper/README.md).

## Usage

- From the latest release, download the file called [`MCBE.GDK.Switcher`](https://github.com/Aetopia/MCBE.GDK.Switcher/releases/latest/download/MCBE.GDK.Switcher.exe)

- Run the program, select your desired version & click on the download button. 

## Build
1. Install & update:
    
    - [.NET](https://dotnet.microsoft.com/download)


    - [MSYS2](https://www.msys2.org)

        ```bash
        pacman -Syu --noconfirm
        ```

3. Install [GCC](https://gcc.gnu.org) & [MinHook](https://github.com/TsudaKageyu/minhook) via MSYS2:

    ```bash
    pacman -Syu mingw-w64-ucrt-x86_64-gcc mingw-w64-ucrt-x86_64-MinHook --noconfirm
    ```

3. Start MSYS2's `UCRT64` environment & run `dotnet publish`.