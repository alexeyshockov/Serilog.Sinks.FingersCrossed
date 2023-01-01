#!/usr/bin/env bash

# Print a command before actually executing it
set -x
# Break the script if one of the command fails (returns non-zero status code)
set -e

# $SONAR_TOKEN should be defined
# $VERSION should also be defined to propagate the version to SonarCloud

dotnet build-server shutdown
dotnet sonarscanner begin \
    /d:sonar.host.url="https://sonarcloud.io" /d:sonar.login="$SONAR_TOKEN" \
    /o:"alexeyshockov" /k:"alexeyshockov_Serilog.Sinks.FingersCrossed" /v:"$VERSION" \
    /d:sonar.dotnet.excludeTestProjects=true \
    /d:sonar.cs.opencover.reportsPaths="tests/*/TestResults/*/coverage.opencover.xml" \
    /d:sonar.cs.vstest.reportsPaths="tests/*/TestResults/*.trx"

# See "Importing .NET reports" here: https://docs.sonarqube.org/latest/analysis/coverage/
dotnet build
dotnet test --no-build --collect:"XPlat Code Coverage" --settings coverlet.runsettings --logger=trx

dotnet sonarscanner end /d:sonar.login="$SONAR_TOKEN"
