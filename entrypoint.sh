#!/bin/sh
set -e

if [ -n "$PORT" ] && [ -z "$ASPNETCORE_URLS" ]; then
  export ASPNETCORE_URLS="http://*:$PORT"
fi

exec dotnet HellavorX.dll
