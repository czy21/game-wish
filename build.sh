#!/bin/bash
cd $(cd "$(dirname "$0")"; pwd)

rm -rf Server/build
dotnet publish -c Release Server -r linux-x64 --self-contained false -p:PublishSingleFile=true -p:AssemblyName=api -o Server/build

tar zcvf $(pwd)/api.tar.gz -C $(pwd)/Server/build/ $(find Server/build/ -type f \( ! -name "appsettings*.json" -o -name "appsettings.json" \) | sed 's|Server/build/||')