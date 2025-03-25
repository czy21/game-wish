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

  ssh $target_host "mkdir -p /app/${target_name}/"
  scp ${api_archive_file} $target_host:/app/${target_name}/
  ssh $target_host "cd /app/${target_name}/;tar -zxvf ${api_archive_file};chmod +x api;rm -fv ${api_archive_file}"

  ssh $target_host "bash script/start-api.sh -n ${target_name} -c dotnet -d /app/${target_name}"
  
fi