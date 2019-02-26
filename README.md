# odata-core-template
### REST API Template using OData, OpenAPI, and OAuth

Demo and training application showing OAuth, OData, and Swashbuckle (Swagger 3 / Open API) working together on ASP.Net Core 2.1+.  This project did have to do some workarounds due to the current version of Swashbuckle not recognizing OData controllers.  These should be standardized later when native support is added.

You can find steps on how to recreate this project from scratch at [GitHub Document Library](https://github.com/PaulGilchrist/documents)

### Feature Details

* Ability to support OData ({id}) syntax
* Ability for OData and custom parameters to be mixed (ex: GET users({id})
* Support for C# comments like /// <summary>, /// <remarks>, /// <params>
    * Swagger does not add much value if these comments do not exists
* Method for documenting all possible response codes and data structure returned
* Collapsing the initial Swagger
* Allowing for nested routes
* Showing nested/related objects
* Ability to Get single object by id without need for $filter query
* Versioning (V1 has single object POST, PATCH, & PUT, V2 has bulk POST, PATCH, & PUT)

### Notes
For this demo, to show how versioning can handle breaking changes, the V1 version of users has middleName, and the V1 version of addresses has streetName2, with both of them being ignored in version 2.  Both properties exist in the Entity Framework context, but are ignored by OData so a single database can support both entity models.

Also, net new capability is added in V2 to allow net new endpoints for associating users to addresses or addresses to users, showing how Swagger allows showing both versions and their differences.