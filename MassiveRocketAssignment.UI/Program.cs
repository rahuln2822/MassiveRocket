using MassiveRocketAssignment.Processors;
using MassiveRocketAssignment.Readers;
using MassiveRocketAssignment.Storage;
using MassiveRocketAssignment;
using MassiveRocketAssignment.UI.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MassiveRocketAssignment.UI;

var builder = WebApplication.CreateBuilder(args);
var startup = new Startup(builder.Configuration);
startup.ConfigureServices(builder.Services);
var app = builder.Build();
startup.Configure(app, builder.Environment);
