#!/usr/bin/env bash

version_lt() {
    ! printf '%s\n%s' "$2" "$1" | sort -C -V
}

QUERY=''
FILE_REGEX='[^/]+(?=/[^/]+\.zip$)'

if [ "$IS_TESTING" == "true" ]; then
    QUERY='.[0].TestingAssemblyVersion = $version'
    QUERY+='|.[0].DownloadLinkTesting |= sub($fileRegex; $refName)'
else
    QUERY='.[0].AssemblyVersion = $version'
    QUERY+='|.[0].DownloadLinkInstall |= sub($fileRegex; $refName)'
    QUERY+='|.[0].DownloadLinkUpdate |= sub($fileRegex; $refName)'

    TESTING_ASSEMBLY_VERSION=$(jq -r '.[0].TestingAssemblyVersion' "$REPO_JSON")
    if version_lt $TESTING_ASSEMBLY_VERSION $VERSION; then
        QUERY+='|.[0].TestingAssemblyVersion = $version'
        QUERY+='|.[0].DownloadLinkTesting |= sub($fileRegex; $refName)'
    fi
fi

jq --arg version "$VERSION" --arg refName "$REF_NAME" --arg fileRegex "$FILE_REGEX" "$QUERY" "$REPO_JSON"
