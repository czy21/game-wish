#!/bin/bash
cd $(cd "$(dirname "$0")"; pwd)

rm -rf Server/build
dotnet publish -c Release Server -r linux-x64 -p:DebugType=None -p:DebugSymbols=false