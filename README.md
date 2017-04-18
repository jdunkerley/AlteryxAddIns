# Alteryx Omni-Bus Tools

![Alteryx OmniBus](AlteryxOmnibus.jpg?raw=true)

A set of open-source extension tools for Alteryx.

## Installation

There is an `Install.bat` script included within the zip which will set up the configuration Alteryx needs. It does this by asking for UAC permission to run `Scripts\Install.ps1`, which will install all the individual components of the OmniBus. This should work whether you have an Admin or User install of Alteryx. If you only have a User install and cannot run under UAC then you can just run the `Install.ps1` within the `Scripts` folder directly. This should install without needing UAC permission to the User install location. 

For uninstalling, there is an equivalent `Uninstall.bat` which calls `Scripts\Uninstall.ps1` to do the uninstall. Again the batch file will ask for UAC permission and then run the Uninstall.ps1.

[More Details](https://github.com/jdunkerley/AlteryxAddIns/wiki/Installation)

## Current Toolset

### Date Time Input

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

### XML Input

This tool provides an input tool much more like the JSON input tool for XML. **A tutorial walking through creating this will be availabile on the wiki.**

### Roslyn Input

Currently a proof of concept tool. It allows you to write C# code and have the results be pushed straight into Alteryx as a data flow. It has support for syntax highlighting and pushing one or more records. **This is not a final version and will under go many changes in the following versions.**

### Omnibus Regex

A proof of concept to understand the working of the Formula tool. This will appear within the Parse section of Alteryx. It uses the standard Regex engine dll to perfrom the tool but provides syntax highlighting and tests on the values. Currently no support for the Parse mode.

## Release Notes

The current [release notes](https://github.com/jdunkerley/AlteryxAddIns/wiki/Release-Notes) are contained in the [wiki](https://github.com/jdunkerley/AlteryxAddIns/wiki).

## Notices

- Some of the algorithms are based on [d3-HexBins](https://github.com/d3/d3-plugins/tree/master/hexbin) plugin.
- Numerical distributions based on [Math.Net Numerics](https://numerics.mathdotnet.com/) library.