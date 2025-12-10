# MCBE GDK Switcher 

Easily switch between different GDK versions of Minecraft: Bedrock Edition.

## Features

- Install any GDK version of Minecraft: Bedrock Edition.

- Automatically stop forced auto-updates when playing a downgraded version.

> [!IMPORTANT]
> Auto-update suppression is achieved by using [Pyroclastic](https://github.com/Aetopia/Pyroclastic).
> - Pyroclastic bypasses the [PC Bootstrapper provided by Gaming Runtime Services.](https://learn.microsoft.com/gaming/gdk/docs/gdk-dev/pc-dev/overviews/gr-pc-bootstrapper)

## Usage

- From the latest release, download the file called [`MCBE.GDK.Switcher.exe`](https://github.com/Aetopia/MCBE.GDK.Switcher/releases/latest/download/MCBE.GDK.Switcher.exe).

- Run the program, select your desired version & click on the download button. 

## Build
1. Install [.NET](https://dotnet.microsoft.com/download).
2. Run: 
    
    ```
    dotnet publish "src/MCBE.GDK.Switcher.csproj"
    ```