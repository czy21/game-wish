#!/bin/bash
cd $(cd "$(dirname "$0")"; pwd)

rm -rf Server/build
dotnet publish -c Release Server -r linux-x64 --self-contained false -p:PublishSingleFile=true -p:AssemblyName=api -o Server/build
find Server/build -name "appsettings*.json" -and -not -name "appsettings.json" -delete

tar zcvf $(pwd)/api.tar.gz -C $(pwd)/Server/build/ .