#!/bin/bash
cd $(cd "$(dirname "$0")"; pwd)

source build.sh

api_archive_file=api.tar.gz

if [ ! -f "${api_archive_file}" ];then
  echo "${api_archive_file} not exists"
  exit 0
fi

target_host=
target_name="game-wish"
while getopts "h:n:" opt;do
    case $opt in
        h) target_host=$OPTARG;;
        n) target_name=$OPTARG;;
    esac
done;

if [ -n "${target_host}" ];then
  (
    cd Server/build
    tar -zcf - $(find . -type f \( ! -name "appsettings*.json" -o -name "appsettings.json" \)) \
     | ssh $target_host "mkdir -p /app/${target_name}/ && tar -zxf - -C /app/${target_name}/ && ls -al /app/${target_name}/ && chmod +x /app/${target_name}/api"
  )

  ssh $target_host "chmod +x host-start-api.sh && ./host-start-api.sh -n ${target_name} -c dotnet -d /app/${target_name}"
fi