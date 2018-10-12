# Development

## Actions

You have to implement the following actions:

### sendRequest

#### Arguments

| Index | Type | Description | Example | 
| --- | --- | --- | --- | 
| 0 | string | Mac address  |  *AF:E4:34:2E:EF* | 

#### Response

| Index | Type | Description | Example | 
| --- | --- | --- | --- | 
| 0 | string | hex string  |  *AF243AA9000* | 

## Android

Notes: 
- Stackoverflow thread about setting up cordova plugin project with ide support: [https://stackoverflow.com/a/32757351/4257761](https://stackoverflow.com/a/32757351/4257761)

## Migrating from locally stored plugin :

ionic cordova plugin rm cordova-plugin-iotize-ble && npm i && ionic cordova plugin add @iotize/cordova-plugin-iotize-ble