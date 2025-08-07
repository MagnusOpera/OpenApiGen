

build:
	dotnet build OpenApiGen


run:
	dotnet run --project OpenApiGen -- Examples/FundApi.config.json Examples/FundApi.json FundApi

run2:
	dotnet run --project OpenApiGen -- Examples/FundApi.config.json Examples/FundApiRedocly.json FundApiRedocly

run3:
	dotnet run --project OpenApiGen -- Examples/FundApi.config.json Examples/FundApiRedoclyPolymorphic.json FundApiRedoclyPolymorphic
