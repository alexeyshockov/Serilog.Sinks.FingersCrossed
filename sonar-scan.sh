#!/usr/bin/env bash

# Print a command before actually executing it
set -x
# Break the script if one of the command fails (returns non-zero status code)
set -e

# $SONAR_TOKEN must be defined
# $GitVersion_FullSemVer can be used to specify the current version (see GitVersion)

VERSION=""
if [ -n "$GitVersion_FullSemVer" ]; then
    VERSION="/v:"$GitVersion_FullSemVer
fi

dotnet build-server shutdown
dotnet sonarscanner begin \
    /d:sonar.host.url="https://sonarcloud.io" /d:sonar.login="$SONAR_TOKEN" \
    /o:"alexeyshockov" /k:"alexeyshockov_Serilog.Sinks.FingersCrossed" $VERSION \
    /d:sonar.dotnet.excludeTestProjects=true \
    /d:sonar.cs.opencover.reportsPaths="tests/*/TestResults/*/coverage.opencover.xml" \
    /d:sonar.cs.vstest.reportsPaths="tests/*/TestResults/*.trx"

# See "Importing .NET reports" here: https://docs.sonarqube.org/latest/analysis/coverage/
dotnet build
dotnet test --no-build --collect:"XPlat Code Coverage" --settings coverlet.runsettings --logger=trx

dotnet sonarscanner end /d:sonar.login="$SONAR_TOKEN"
