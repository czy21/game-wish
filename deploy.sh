#!/bin/bash
cd $(cd "$(dirname "$0")"; pwd)

source build.sh

if [ ! -f "api.tar.gz" ];then
  echo "api.tar.gz not exists"
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

ssh $target_host "mkdir -p /home/app/${target_name}/"
scp api.tar.gz $target_host:/home/app/${target_name}/
ssh $target_host "cd /home/app/${target_name}/;tar -zxvf api.tar.gz;chmod +x api;rm -fv api.tar.gz"

ssh $target_host "bash script/start-api.sh -n ${target_name} -c dotnet -d /home/app/${target_name}"