using DemandTrack.Api.Data;
using DemandTrack.Api.Endpoints;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddValidation();

builder.AddDemandDb();//Add the DemandContext to the DI container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();//Http server

app.UseSwagger();
app.UseSwaggerUI();

app.MapDemandsEndpoints();

app.MigrateDb();//Auromatically migrate the database schema to the latest version on application startup
app.Run();
