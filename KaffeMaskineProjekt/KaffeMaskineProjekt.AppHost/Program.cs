var builder = DistributedApplication.CreateBuilder(args);

var dbserver = builder.AddPostgres("KaffeDbServer");
var db = dbserver.AddDatabase("KaffeDb");

var apiService = builder.AddProject<Projects.KaffeMaskineProjekt_ApiService>("apiservice")
    .WithReference(db)
    .WaitFor(db);



builder.AddProject<Projects.KaffeMaskineProjekt_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
