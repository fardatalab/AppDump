# AppDump

## Getting Started
Commands to build AppDump:

```
sudo add-apt-repository ppa:dotnet/backports
sudo apt-get update && \
  sudo apt-get install -y dotnet-sdk-8.0 dotnet-sdk-9.0
git clone https://github.com/fardatalab/AppDump --recursive

cd AppDump
dotnet build -c Release

cd Garnet
git apply ../patches/garnet.patch
dotnet build -c Release
```


For dev branch:
```

git clone https://github.com/fardatalab/AppDump

cd AppDump
git switch dev
git submodule update --init --recursive
dotnet build -c Release

cd Garnet
git apply ../patches/garnet.patch
dotnet build -c Release
```

