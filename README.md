# Versa

**Versa** is a NuGet package providing a versatile admin panel web interface.
It reads database metadata and uses it to generate CRUD views.

---

# Requirements

The package will target .NET 8.

Versa requires a database for internal use.
This database should be created empty and in advance (with owner permissions applied for the user that Versa will access it through).
Schema will be populated at startup accordingly to Versa's requirements.

> [!NOTE]
> Currently only SQL Server is supported.

# Quickstart guide
Provided the connection string for the internal database is stored in the `appsettings.json` (in key `VersaConnection`), you can use the following snippet to register Versa in your project.
```csharp
// add Versa services
var versaConnectionString =
    builder.Configuration.GetConnectionString("VersaConnection") ??
    throw new InvalidOperationException("Connection string 'VersaConnection' not found.");
builder.Services.AddVersa(versaConnectionString);

// ...

// invoke the initialization code
app.UseVersa(new VersaOptions
{
    ReadMetadataOnStartup = true,
    TargetDbConnectionString = connectionString
});
```

---

# Features (to-do)
- [x] Read database metadata (and save it in a persistent store);
- Generate CRUD views for tables found in the database;
- Configuration of tables to use (or omit) on startup;
- Attachable panel endpoint to an existing web app;
- Authorization and authentication with "parent's" project middleware;
- Dynamic lookups (when a table has columns that are foreign keyed to a different table);
- Audit log for all administrative actions;

# Is it ready for production use?
No.

# Why this?
- The project was started as an entrance for the ['100 commits' competition](https://100commitow.pl/).
- Inspired by the django admin site.