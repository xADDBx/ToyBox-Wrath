# About
ToyBox.Analyzer is currently used to make the creation of Localization easier and warn about possible unlocalized UI strings

### Setup
Black Magic

### Debugging
Visual Studio "Start Debugging" should open another instance of Visual Studio. Use this to open the normal ToyBox solution to debug the Analyzer.  
It might be necessary to remove the ToyBox.Analyzer PackageReference from the main project to debug the latest version of the Analyzer.

### Release
1. Open Solution
2. Right Click ToyBox.Analyzer.Package project
3. Click "Pack"
4. Upload to NuGet