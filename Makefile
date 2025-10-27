config ?= Debug
version ?= 0.0.0

build:
	dotnet build

install:
	npm ci

gen:
	dotnet run --project OpenApiGen -- Examples/OpenApiGen.config.json Examples/SampleApi.json generated

gen-buggy:
	dotnet run --project OpenApiGen -- Examples/OpenApiGen.config.json Examples/BuggyApi.json generated

gen-invest:
	dotnet run --project OpenApiGen -- Examples/InvestApi.config.json Examples/InvestApi.json generated

gen-art:
	dotnet run --project OpenApiGen -- Examples/FundApi.config.json Examples/ArtApi.json generated

gen-petstore:
	dotnet run --project OpenApiGen -- Examples/Default.config.json Examples/PetStore.json generated

help:
	dotnet run --project OpenApiGen -- --help

pack:
	dotnet pack -c $(config) -p:Version=$(version) -o .out

test:
	dotnet test -c $(config)

publish: .out/*.nupkg
	@for file in $^ ; do \
		dotnet nuget push $$file -k $(nugetkey) -s https://api.nuget.org/v3/index.json --skip-duplicate ; \
    done
