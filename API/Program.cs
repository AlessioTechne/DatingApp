<<<<<<< HEAD
using System.Text;
using API.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

//Add services to the container
builder.Services.AddControllers();
builder.Services.AddApplicationServices(builder.Configuration);



var app = builder.Build();

//Configure the HTTP request

app.UseCors(builder => builder.AllowAnyHeader().AllowAnyMethod().WithOrigins("https://localhost:4200"));

app.UseAuthentication();
=======
using API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);      
builder.Services.AddDbContext<DataContext>(opt=> {
    opt.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddControllers();

var app = builder.Build();

app.UseHttpsRedirection();

>>>>>>> 397e4f4e700a50188659627e5d43bae6f081e4d6
app.UseAuthorization();

app.MapControllers();

app.Run();