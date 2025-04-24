var builder = DistributedApplication.CreateBuilder(args);

var dbserver = builder.AddPostgres("KaffeDbServer");
var db = dbserver.AddDatabase("KaffeDb");
dbserver.WithPgAdmin();

builder.AddProject<Projects.KaffeMaskineProjekt_MigrationService>("MigrationService")
    .WithReference(db)
    .WaitFor(db);

var apiService = builder.AddProject<Projects.KaffeMaskineProjekt_ApiService>("apiservice")
    .WithReference(db)
    .WaitFor(db);



builder.AddProject<Projects.KaffeMaskineProjekt_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
