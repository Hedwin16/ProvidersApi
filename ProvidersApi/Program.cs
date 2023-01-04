using DB;
using Microsoft.EntityFrameworkCore;
using ProvidersApi;
using ProvidersApi.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ProvidersContext>(options=>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProvidersContext"))
);

builder.Services.Configure<ApiSettings>(builder.Configuration.GetSection(ApiSettings.Settings));

var app = builder.Build();

//using (var scope = app.Services.CreateScope())
//{
//    var context = scope.ServiceProvider.GetRequiredService<ProvidersContext>();
//    context.Database.Migrate();
//}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();

app.UseMiddleware<ApiSettingsMiddleware>();

app.MapControllers();

app.Run();
