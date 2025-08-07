

build:
	dotnet build OpenApiGen


run:
	dotnet run --project OpenApiGen -- Examples/FundApi.config.json Examples/FundApi.json FundApi
