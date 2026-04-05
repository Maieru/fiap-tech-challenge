dotnet sonarscanner begin /k:"FiapFase1" /d:sonar.host.url="http://localhost:9000"  /d:sonar.token="sqp_c65ca74097e6cb83cf48141072f915825b122f7a" /d:sonar.cs.vscoveragexml.reportsPaths=coverage.xml


dotnet build
dotnet-coverage collect dotnet test -f xml  -o coverage.xml

dotnet sonarscanner end /d:sonar.token="sqp_c65ca74097e6cb83cf48141072f915825b122f7a"

del coverage.xml