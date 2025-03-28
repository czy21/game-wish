#!/bin/bash

set -x

cd $(cd "$(dirname "$0")"; pwd)

source build.sh

target_host=
target_name="game-wish"
while getopts "h:n:" opt;do
    case $opt in
        h) target_host=$OPTARG;;
        n) target_name=$OPTARG;;
    esac
done;

export param_project_root="$(pwd)/Server"
export param_release_name="${target_name}"
export param_code_type="dotnet"

export SSH_HOST=${target_host}

curl -sSL https://raw.githubusercontent.com/czy21/script/refs/heads/master/jenkins/resources/org/ops/host-deploy.sh | bash