# AppDump

## Getting Started
Commands to run Garnet:

```
sudo add-apt-repository ppa:dotnet/backports
sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-9.0
git clone https://github.com/fardatalab/garnet_appdump --recursive

cd garnet_appdump/AppDump
dotnet build -c Release
cd ..
dotnet build -c Release
```
