#!/usr/bin/env bash

set -o xtrace,errexit

# $SONAR_TOKEN must be defined
# $GitVersion_FullSemVer can be used to specify the current version (see GitVersion)

VERSION=""
if [ -n "$GitVersion_FullSemVer" ]; then
    VERSION="/v:"$GitVersion_FullSemVer
fi

dotnet build-server shutdown
dotnet sonarscanner begin \
    /d:sonar.host.url="https://sonarcloud.io" /d:sonar.token="$SONAR_TOKEN" \
    /o:"alexeyshockov" /k:"alexeyshockov_Serilog.Sinks.FingersCrossed" "$VERSION" \
    /d:sonar.dotnet.excludeTestProjects=true \
    /d:sonar.coverage.exclusions="**/examples/**" \
    /d:sonar.cs.opencover.reportsPaths="tests/*/TestResults/*/coverage.opencover.xml" \
    /d:sonar.cs.vstest.reportsPaths="tests/*/TestResults/*.trx"

# See "Importing .NET reports" here: https://docs.sonarqube.org/latest/analysis/coverage/
dotnet build
dotnet test --no-build --collect:"XPlat Code Coverage" --settings coverlet.runsettings --logger=trx

dotnet sonarscanner end /d:sonar.token="$SONAR_TOKEN"
