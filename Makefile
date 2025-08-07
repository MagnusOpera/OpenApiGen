

build:
	dotnet build


run:
	dotnet run --project OpenApiGen -- config.json Examples/RawFundApi.json RawFundApi
