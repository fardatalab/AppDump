# AppDump

## Getting Started
Commands for Garnet:

```
sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-9.0
git clone https://github.com/fardatalab/garnet_appdump --recursive

cd garnet_appdump/AppDump
dotnet build -c Release
cd ..
dotnet build -c Release
```