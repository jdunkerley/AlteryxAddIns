# AlteryxAddIns
Some C# based Alteryx Custom Tools.

## Installation

There is a batch file called `Install.bat`
This will create an ini file looking like:

```
[Settings]
x64Path=C:\Repos\AlteryxAddIns\AlteryxDateParser\bin\Debug\
x86Path=C:\Repos\AlteryxAddIns\AlteryxDateParser\bin\Debug\
ToolGroup=JDTools
```
It will copy this to `C:\Program Files\Alteryx\Settings\AdditionalPlugins\JDTools.ini`.

There is also an `Uninstall.bat` which will remove this file.

## Current Toolset

### DateTimeInputTool
A slightly expanded version of the Date Time Now tool. It will create a date field equal to one of the following values:
* Today
* Yesterday
* StartOfWeek
* StartOfMonth
* StartOfYear
* PreviousMonthEnd
* PreviousYearEnd

### CircuitBreakerTool
This is an expanded version of the the Test tool. The idea here is that the input data (I) is only passed onto the rest of the workflow if there is no data received in the breaker input (B). 

### DateTimeParseTool
This exposes the .Net DateTime parse functions to Alteryx. For this I needed to find a way to allow the user to pick an input field (see the InputFieldTypeConverter), to copy most of the values, parse a date and add to the result. As this tool supports all formats that the DateTimeParseExact function does, it should deal with parsing all the random formats that keep come up.

This tool supports different cultures allowing parsing of any supported date formats. 

### FormatterTool
This tool can be used to convert from a numerical or date or time field to a string field. It supports different cultures and number formatting.

### HashingTool
This exposes the .Net System.Security.Cryptography HashAlgorithm's to Alteryx. It takes and input string and computes the hash value. It support MD5, RIPEMD160, SHA1, SHA256, SHA384 and SHA512.
