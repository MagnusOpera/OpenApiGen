version ?= 0.0.0

build:
	dotnet build OpenApiGen

pack:
	dotnet pack --project OpenAPIGen -c $(config) -p:Version=$(version) -o .out
