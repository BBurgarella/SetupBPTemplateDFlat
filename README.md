
# SetupBPTemplateDFlat

A C# port of the Powershell Script to configure the [BPTemplate](https://github.com/WittleWolfie/BPCoreTemplate) from WittleWolfie (Pathfinder Modding)

I wrote this script because when I tried to use the PWSH script, it didn't work properly. I thought that having an application do the work instead would be better.
Also, upon discussing with WittleWolfie, I learned that a bug I experienced was related to Unity process potentially still running so I included a function that kills every unity process running.

## Requirements

- Windows 7 or later
- .NET 4.8.0 or later

## Usage

1. Open the `SetupBPTemplate.exe` application. As an administrator, it requires these rights because it will make sure that every Unity process is closed to avoid locked files problems
3. Select your Wrath of the Righteous install directory by clicking the "Select" button next to "Wrath of the Righteous Install Directory". The default directory is `C:\Program Files (x86)\Steam\steamapps\common\Pathfinder Second Adventure`.
4. Select your BasicTemplate directory 
5. Select your mod project directory.
5. Select your mod Unity project directory.
6. Enter the name of your mod.
7. Wait for the changes to occur
8. ...
9. Profit

## Contributing

Pull requests are welcome. For major changes, please open an issue first to discuss what you would like to change.

Please make sure to update tests as appropriate.

## License

[MIT](https://choosealicense.com/licenses/mit/)

## Recognition

Special Thank to [WittleWolfie](https://github.com/WittleWolfie) for the help on understanding several issues
