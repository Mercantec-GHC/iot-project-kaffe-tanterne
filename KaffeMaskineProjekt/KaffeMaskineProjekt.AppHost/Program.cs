using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

builder.AddDockerComposeEnvironment("compose");

var dbserver = builder.AddPostgres("KaffeDbServer");

var db = dbserver.AddDatabase("KaffeDb");
dbserver.WithPgAdmin();

builder.AddProject<Projects.KaffeMaskineProjekt_MigrationService>("migrationservice")
    .WithReference(db)
    .WaitFor(dbserver);

var apiService = builder.AddProject<Projects.KaffeMaskineProjekt_ApiService>("apiservice")
    .WithReference(db)
    .WaitFor(db);

var react = builder.AddNpmApp("kaffemaskineprojekt-react", "../java-dashboard-delight")
    .WaitFor(apiService)
    .WithReference(apiService)
    .WithEnvironment("BROWSER", "none") // Disable opening browser on npm start
    .WithHttpEndpoint(env: "VITE_PORT")
    .WithExternalHttpEndpoints()
    .PublishAsDockerFile();


builder.Build().Run();