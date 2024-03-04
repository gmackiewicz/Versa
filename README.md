# Versa

**Versa** is a NuGet package providing a versatile admin panel web interface.
It reads database metadata and uses it to generate CRUD views.

---

# Features (to-do)
- Read database metadata (and save it in a persistent store);
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