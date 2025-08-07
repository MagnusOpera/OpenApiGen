config ?= Debug
version ?= 0.0.0

build:
	dotnet build

pack:
	dotnet pack -c $(config) -p:Version=$(version) -o .out

test:
	dotnet test -c $(config)

publish: .out/*.nupkg
	@for file in $^ ; do \
		dotnet nuget push $$file -k $(nugetkey) -s https://api.nuget.org/v3/index.json --skip-duplicate ; \
    done
