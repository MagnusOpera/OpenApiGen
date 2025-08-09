config ?= Debug
version ?= 0.0.0

build:
	dotnet build

fsrun:
	dotnet run --project FsOpenApiGen -- Examples/FundApi.config.json Examples/FundApi.json FundApi

run:
	dotnet run --project OpenApiGen -- Examples/FundApi.config.json Examples/FundApi.json FundApi

run2:
	dotnet run --project OpenApiGen -- Examples/FundApi.config.json Examples/FundApi.json FundApiNew

pack:
	dotnet pack -c $(config) -p:Version=$(version) -o .out

test:
	dotnet test -c $(config)

publish: .out/*.nupkg
	@for file in $^ ; do \
		dotnet nuget push $$file -k $(nugetkey) -s https://api.nuget.org/v3/index.json --skip-duplicate ; \
    done
