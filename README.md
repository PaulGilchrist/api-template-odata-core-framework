# odata-core-template
### REST API Template using OData, OpenAPI, and OAuth

Demo and training application showing OAuth, OData, and Swashbuckle (Swagger 3 / Open API) working together on ASP.Net Core 2.1+.  This project did have to do some workarounds due to the current version of Swashbuckle not recognizing OData controllers.  These should be standardized later when native support is added.

You can find steps on how to recreate this project from scratch at [GitHub Document Library](https://github.com/PaulGilchrist/documents)

### Feature Details

* Supports the latest versions of OData, OpenAPI/Swagger, and Entity Framework on .Net Core
* Supports both OAuth Bearer and Basic authentication
  * Supports in memory caching and persisted support for authorization roles even for Basic authentication
  * Examples of both HTTP action, and even property level access granularity
* Support for C# comments like /// <summary>, /// <remarks>, /// <params>
    * Swagger does not add much value if these comments do not exists
* Method for documenting all possible response codes and data structure returned
* Support for object model annotations enhancing Swagger documentation (ex: required properties, min/max length, friendly display names)
* Versioning (V1 has single object POST, PATCH, & PUT, V2 has bulk POST, PATCH, & PUT)
* Bulk POST, PUT, and PATCH
* Allowing for nested routes including objects exposed in API using same object name ("Notes" in this template attached to both Users and Addresses)
* Enumerations used to allow for human readable input/output while still controlling allowed choices, and tieing back to different database data types
* Microsoft Azure's Application Insights support
  * Including adding of identity data with request telemetry
  * Inlcudes example of custom exception handling responses and more detailed exception telemetry tracking
* Shows how to create many to many associations without exposing association tables/objects

### Notes
For this demo, to show how versioning can handle breaking changes, the V1 version of users has middleName, and the V1 version of addresses has streetName2, with both of them being ignored in version 2.  Both properties exist in the Entity Framework context, but are ignored by OData so a single database can support both entity models.

Also, net new capability is added in V2 to allow net new endpoints for associating users to addresses or addresses to users, showing how Swagger allows showing both versions and their differences.