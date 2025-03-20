#!/bin/bash
cd $(cd "$(dirname "$0")"; pwd)

rm -rf Server/build
dotnet publish -c Release Server -r linux-x64 --self-contained false -p:PublishSingleFile=true -p:AssemblyName=api -o Server/build
rm -rf Server/build/appsettings*.json

tar zcvf $(pwd)/api.tar.gz -C $(pwd)/Server/build/ .