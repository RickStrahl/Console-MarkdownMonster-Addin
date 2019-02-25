# Console Addin for Markdown Monster

![](ConsoleAddin/icon.png)

This is a simple sample addin that creates a Console Powershell window and 'attaches' to the bottom of your Markdown Monster window instance. As you move the window the terminal goes along for the ride.

![](Screenshot.png)

This is just an initial quickie. 

Things to do:

* Add configuration for
    * Terminal Command Line
    * Initial Arguments
    * Initial Height

### Console Examples
The following are configuration settings for several different consoles. Note that you can use `{0}` as the placeholder to the path of the current document. If not document is open this value will be blank or you can remove the `{0}` from the arguments to not inject the path.

#### Powershell
This is the default configuration that starts up Powershell in the active document's folder.

```json
{
  "InitialHeight": 300,
  "StripWindowHeader": true,
  "TerminalExecutable": "powershell.exe",
  "TerminalArguments": "-noexit -command \"cd '{0}'\""
}
```

#### Windows Command Prompt
The following starts a classic Windows Command Prompt in the folder of the active document.

```json
{
  "InitialHeight": 300,
  "StripWindowHeader": true,
  "TerminalExecutable": "cmd.exe",
  "TerminalArguments": "cmd /K \"cd {0}\""
}
```
  
#### ConEmu
The following sets up the popular ConEmu Console application by opening in the current document's path with the `{Powershell}` subconsole configuration. You can change the configuration to any of your installed consoles or leave it out for the default console.

```json
{
  "InitialHeight": 350,
  "StripWindowHeader": false,
  "TerminalExecutable": "C:\\Program Files\\ConEmu\\ConEmu64.exe",
  "TerminalArguments": "/dir \"{0}\" /cmd {Powershell}"
}
```

> ConEmu uses a multi-window layout, so there's no easy way to kill console instances. It's best to explicitly close instances when you are done with them rather than closing through the Console Addin toggle button.