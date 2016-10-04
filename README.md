# Alteryx Omni-Bus Tools

![Alteryx OmniBus](AlteryxOmniBus.jpg?raw=true)

A set of open-source extension tools for Alteryx.

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

###  Date Time Input
A slightly expanded version of the Date Time Now tool. It will create a date field equal to one of the following values:
* Today
* Now
* Yesterday
* StartOfWeek
* StartOfMonth
* StartOfYear
* PreviousMonthEnd
* PreviousYearEnd

### Circuit Breaker
This is an expanded version of the the Test tool. The idea here is that the input data (I) is only passed onto the rest of the workflow if there is no data received in the breaker input (B). 

### Date Time Parser
This exposes the .Net DateTime parse functions to Alteryx. Parses a date from a text field and adds to the output. 

As this tool supports all formats that the DateTimeParseExact function does, it should deal with parsing all the random formats that keep come up. This tool supports different cultures allowing parsing of any supported date formats. 

### Number Parser
This exposes the .Net double parse functions to Alteryx. Parses a number from a text field and adds to the output. Again, yhis tool supports different cultures allowing parsing of any supported number formats. 

### String Formatter
This tool can be used to convert from a numerical or date or time field to a string field. It supports different cultures and number formatting.

### Hash Code Generator
This exposes the .Net System.Security.Cryptography HashAlgorithms to Alteryx. It takes and input string and computes the hash value. It support MD5, RIPEMD160, SHA1, SHA256, SHA384 and SHA512.

### Random Number
This generates a random number based of a sepcified distribution (currently Linear, Normal and LogNormal). Based of the Math.Net Numerics package. Can be seeded to allow the same sequence each time.

### HexBin
This reproduces the hexagonal binning functionality built into Tableau. It is based of the d3 HexBin plugin. It defaults to a radius 1 hexagon but you can specify different sizes.

### Sort with Culture
This tool exposes sorting with a culture from .Net. **This was an experiment and there is actually sorting with culture provided in the main Sort tool.**

### Roslyn Input
Currently a proof of concept tool. It allows you to write C# code and have the results be pushed straight into Alteryx as a data flow. It has support for syntax highlighting and pushing one or more records. **This is not a final version and will under go many changes in the next versions.** 

## Testing the ToolSet

There is a workflow in the root directory of the repository called `UnitTestRunner`. This will run all other workflows in the repository and see if they run successfully.

Please note unless you have an Api enabled copy of Alteryx you cannot run these when running Alteryx within the Visual Studio debugger as it denies the license. The workflow is dependent on the List Runner macro of the CReW macros. 
