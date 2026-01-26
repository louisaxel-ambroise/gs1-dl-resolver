# GOTO - GS1 Digital Link Resolver API

Goto is an HTTP API that conforms to the GS1 Digital Link Resolver Specification, enabling resolution of GS1 Digital Links into context-specific resources such as product information, traceability data and digital experiences.

This project provides a standards-compliant resolver that parses GS1 Digital Links, applies resolution rules, and returns appropriate responses based on client context, link type, and configuration.

## Features

- Conforms to the GS1 Digital Link Resolver Specification
- Handles uncompressed/partially compressed/fully compressed AIs
- Performs validation of GS1 AIs (Check Digit, GCP, etc.)

## Live demo

The resolver is deployed at the URL https://id.goto.it.com/

It's free to use for testing, so don't hesitate to give it a try!

There is also a [Postman Workspace](https://www.postman.com/fastnt-epcis/goto-gs1-digitallink-resolver/overview) with API request examples that you can try for a quick look at its capabilities.

## API Overview

The resolver contains endpoint to:
- Register a redirection for a DigitalLink
- Resolve a DigitalLink
- Get insights for a DigitalLink
- Compress/uncompress DigitalLinks

### Register a redirection for a DigitalLink

Registeration of a DigitalLink is made by making a POST request on the root path followed by the DigitalLink (compressed or not).

```sh
curl -X POST "https://id.goto.it.com/api/register" -H "Content-Type: application/json" -d '{
  "prefix": "/01/09506000134352",
  "title":"Description",
  "redirectUrl":"https://example.org{?gtin}",
  "linkTypes": ["gs1:pip", "gs1:defaultLink"],
  "language":"en-GB"
}'
```

The redirectUrl can follow the [UriTemplate](https://www.rfc-editor.org/rfc/rfc6570) specification, using either the AI code (digit only - 01, 10, ..) or short code (gtin, lot, ...)

### Resolve a DigitalLink

The resolution of a DigitalLink is achieved by making a GET request on the root path followed by the DigitalLink (compressed or not)

```sh
curl -X GET "https://id.goto.it.com/01/09506000134352" -H "Content-Type: application/json" -H "Accept-Language: en-GB"
```

The API supports the `linkType` parameter to redirect to the appropriate redirection, and matches the Accept-Language header based on best effort.

### Get insights for a DigitalLink

Each call to the resolve endpoint is recorded and can be retrieved usign this endpoint

```sh
curl -X GET "https://id.goto.it.com/api/insights/01/09506000134352?days=1" -H "Content-Type: application/json"
```

The days parameter restricts the number of days returned by the endpoint. The default value is 1 and maximum is 365 (1 year)

### Compress/Uncompress a DigitalLink

You can compress a DigitalLink using the `/api/compress/{digitalLink}` endpoint:

```sh
curl -X POST "https://id.goto.it.com/api/compress -H "Content-Type: application/json" -d '{
	"digitalLink": "/01/09506000134352",
	"compressionType": "Full",
	"compressQueryString": false
}'
```

`compressionType` can take the values `Full` or `Partial`. Partial compressed DigitalLink will keep the key AI uncompressed.

Similarily, decompression is done using the `/api/decompres` endpoint. The response include the type of compression used (uncompressed, fully compressed or partially compressed), as well as the canonical URL of the DigitalLink.

Example: 

```sh
curl -X POST "https://id.goto.it.com/api/decompress" -H "Content-Type: application/json" -d'{
	"digitalLink": "ARFKk4XBoA"
}'
```

## Installation

1. Clone the repo: `git clone https://github.com/louisaxel-ambroise/goto`
2. Go to the directory: `cd goto`
3. Restore nuget packages: `dotnet restore`
4. Run the project: `dotnet run --project src/Gs1DigitalLink.Api/Gs1DigitalLink.Api.csproj`

By default, the server will start on: https://localhost:7113

## Contributing

Contributions are welcome!

1. Fork the repository
2. Create a feature branch
3. Add tests for any changes
4. Submit a pull request

Or feel free to open an issue if you notice anything wrong or require a new functionality

## Authors

External contributions on this EPCIS repository are welcome from anyone.
This project was created an is primarily maintained by [Louis-Axel Ambroise](https://github.com/louisaxel-ambroise).

## License

This project is licensed under the MIT License

## Disclaimer

GS1 is a registered trademark of GS1 AISBL.
This project is an independent implementation and is not affiliated with or endorsed by GS1 unless otherwise stated.