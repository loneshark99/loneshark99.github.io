---
layout: post
title:  "Protocol Buffer"
date:   2023-10-12 6:16:00 PM
categories: Development
---

[Format-Hex]: https://loneshark99.github.io/images/Format-hex.png

Protocol Buffer is a platform agnostic data transfer protocol. It is a binary protocol where data is serialized in binary format. The serialized data is very compact and takes less storage space and small data also helps with reducing the data transfer latency.

Protocol Buffer (protobuf) is created by google and it is used extensively with grpc services and also for many other scenarios. One of the biggest differences with other protocols like json is

> **It decouples the context and the data**


```json 

{
    "ShipName" : "Olympic",
    "ShipOwner" : "White Star Line",
    "ShipId"   : "1"
}

Serialized String = {"ShipName":"Olympic","ShipOwner":"White Star Line","ShipId":"1"}

Both the properties and the data is serialized together and sent over the wire, which makes it bulky.
```

```protobuf

syntax = "proto3"
namespace = "Horizon"
package = "Horizon.Ships"

message ShipMetadata 
{
    string ShipName = 1;
    string ShipOwner = 2;
    int32 ShipId = 3;
}

SeriAlized String=126Olympic2215White Star Line3211
```

> Data is serialized in 3 part      {FieldId} {FieldType} {FieldData}
> Data is written in this format    {Field Rule}{Field Type} {Field Name} {Field Tag}
> Field Rule : required, repeated...
> Field Type : string, int32, float, double ....
> Field Tag : int32 


Download the latest [protobuf compiler](https://github.com/protocolbuffers/protobuf/releases) and add it to your path in windows.

Run following command for linux.
```bash
sudo apt-get install protobuf-compiler
```

Run the following command to generate the csharp class.

```protobuf
protoc --csharp_out=. .\Messages.proto .\Messages1.proto 
```
**Google.Protobuf** package is needed in C#. This package provides the required plumbing tools for serialization and deserialization among other things.

Serialization and Deserialization of protobuf classes.

```csharp
        public static void Main()
        {
            ShipMetadata ship = new ShipMetadata();
            ship.ShipId = 1;
            ship.ShipName = "Atlantic";
            ship.ShipOwner = "Star Line";

            File.Delete("Ships.dat");

            //Serialization
            using(var s = File.Open("Ships.dat", FileMode.OpenOrCreate))
            {
                ship.WriteTo(s);
            }

            //DeSerialization
            using(var s = File.OpenRead("Ships.dat"))
            {
                ShipMetadata sObj = ShipMetadata.Parser.ParseFrom(s);
            }
        }
```
 
 To view how the serialized data, we can use the cool format-hex powershell script.

 ![format-hex][Format-Hex]

 Protobuf has many features like below to name a few

 - Nested Types
 - Splitting the classes into multiple files
 - DataTypes like OneOf, Any, Enumeration
 - Support for packages
 - Versioning
   

**Happy Learning and improving one day at a time**