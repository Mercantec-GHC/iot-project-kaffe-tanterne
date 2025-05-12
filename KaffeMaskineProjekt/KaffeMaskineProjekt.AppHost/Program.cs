var builder = DistributedApplication.CreateBuilder(args);

var dbserver = builder.AddPostgres("KaffeDbServer");
var db = dbserver.AddDatabase("KaffeDb");
dbserver.WithPgAdmin();

builder.AddProject<Projects.KaffeMaskineProjekt_MigrationService>("MigrationService")
    .WithReference(db)
    .WaitFor(dbserver);

var apiService = builder.AddProject<Projects.KaffeMaskineProjekt_ApiService>("apiservice")
    .WithReference(db)
    .WaitFor(db);


var react = builder.AddNpmApp("KaffeMaskineProjekt-React", "../java-dashboard-delight", "dev")
    .WithReference(apiService)
    .WaitFor(apiService)
    .WithEnvironment("BROWSER", "none") // Disable opening browser on npm start
    .WithHttpEndpoint(env: "PORT", isProxied: false, port: 8080, targetPort: 8080)
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();



builder.AddProject<Projects.KaffeMaskineProjekt_Web>("webfrontend")
    .WithExternalHttpEndpoints()
    .WithReference(apiService)
    .WaitFor(apiService);

builder.Build().Run();
