# CWDemangleCs
C# port of the rust library [cwdemangle](https://github.com/encounter/cwdemangle)

## Usage

### CLI
```
CWDemangleCs.CLI 'BuildLight__9CGuiLightCFv'
```
Pass ``--help`` to se available options.

### Library
Usage:
```cs
using CWDemangleCs;

DemangleOptions options = new DemangleOptions();
string result = CWDemangler.demangle("BuildLight__9CGuiLightCFv", options);
Debug.Assert(result, "CGuiLight::BuildLight() const");
```