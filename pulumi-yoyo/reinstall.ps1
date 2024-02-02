dotnet clean -c Release
dotnet build -c Release
dotnet pack -c Release

dotnet tool uninstall --global pulumi-yoyo
dotnet tool install --global --add-source ./nupkg pulumi-yoyo
yoyo --help