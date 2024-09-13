using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Enums;
using minimal_api.Domain.Interfaces;
using minimal_api.Domain.ModelViews;
using minimal_api.Domain.Services;
using minimal_api.Infrastructure.Db;

#region Builder
var builder = WebApplication.CreateBuilder(args);

var key = builder.Configuration.GetSection("Jwt").ToString() ?? "";

builder.Services.
    AddAuthentication(option => {
        option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    }).
    AddJwtBearer(option => {
    option.TokenValidationParameters = new TokenValidationParameters {
        ValidateLifetime = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false
    };
});

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAdministradorServico, AdministradorServico>();
builder.Services.AddScoped<IVeiculoServico, VeiculoServico>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => {
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Insira o token JWT"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement {
        {
            new OpenApiSecurityScheme {
                Reference = new OpenApiReference {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddDbContext<DbContexto>(options => {
    options.UseMySql(
        builder.Configuration.GetConnectionString("MySql"),
        ServerVersion.AutoDetect(builder.Configuration.GetConnectionString("MySql")));
});

var app = builder.Build();
#endregion

#region Home
app.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
#endregion

#region Administrador

string GerarTokenJwt(Administrador administrador)
{
    if(string.IsNullOrEmpty(key))
        return string.Empty;

    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    var claims = new List<Claim>() {
        new Claim("Email", administrador.Email),
        new Claim("Perfil", administrador.Perfil)
    };
    var token = new JwtSecurityToken(
        claims: claims,
        expires: DateTime.Now.AddDays(1),
        signingCredentials: credentials
    );

    return new JwtSecurityTokenHandler().WriteToken(token);
}

app.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administrador) => {
    var adm = administrador.Login(loginDTO);
    if (adm != null)
    {
        string token = GerarTokenJwt(adm);
        return Results.Ok(new AdministradorLogado {
            Email = adm.Email,
            Perfil = adm.Perfil,
            Token = token
        });
    }
    else
        return Results.Unauthorized();
}).WithTags("Administradores");

app.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) => {
    var validacao = new ErrosDeValidacao {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(administradorDTO.Email))
        validacao.Mensagens.Add("O e-mail não foi preenchido.");
    
    if (string.IsNullOrEmpty(administradorDTO.Senha))
        validacao.Mensagens.Add("A senha não foi preenchida.");
    
    if (administradorDTO.Perfil.ToString() == null)
        validacao.Mensagens.Add($"O perfil não foi preenchido: {administradorDTO.Perfil}.");

    if (validacao.Mensagens.Count() > 0)
        return Results.BadRequest(validacao);

    var administrador = new Administrador {
        Email = administradorDTO.Email,
        Senha = administradorDTO.Senha,
        Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Vendedor.ToString()
    };
    administradorServico.Incluir(administrador);
    return Results.Created($"/administrador/{administrador.Id}",
        new AdministradorModelView{
            Id = administrador.Id,
            Email = administrador.Email,
            Perfil = administrador.Perfil
    });
}).RequireAuthorization().WithTags("Administradores");

app.MapGet("/administradores", ([FromQuery] int? pagina, [FromQuery] string? email, [FromQuery] string? perfil, IAdministradorServico administradorServico) => {
    var adms = new List<AdministradorModelView>();
    var administradores = administradorServico.Todos(pagina, email, perfil);
    foreach (Administrador administrador in administradores)
    {
        adms.Add(new AdministradorModelView{
            Id = administrador.Id,
            Email = administrador.Email,
            Perfil = administrador.Perfil
        });
    }
    return adms.Count() == 0 ? Results.NotFound() : Results.Ok(adms);
}).RequireAuthorization().WithTags("Administradores");

app.MapGet("/administradores/{id}", ([FromQuery] int id, IAdministradorServico administradorServico) => {
    var administrador = administradorServico.BuscaPorId(id);
    return administrador == null ?
        Results.NotFound() :
        Results.Ok(new AdministradorModelView{
            Id = administrador.Id,
            Email = administrador.Email,
            Perfil = administrador.Perfil
    });
}).RequireAuthorization().WithTags("Administradores");

app.MapPut("/administradores/{id}", ([FromQuery] int id, AdministradorDTO administradorDTO, IAdministradorServico administradorServico) => {
    var administrador = administradorServico.BuscaPorId(id);
    if (administrador == null)
        return Results.NotFound();

    administrador.Email = administradorDTO.Email;
    administrador.Senha = administradorDTO.Senha;
    administrador.Perfil = administradorDTO.Perfil.ToString() ?? Perfil.Vendedor.ToString();

    administradorServico.Atualizar(administrador);
    return Results.Ok();
}).RequireAuthorization().WithTags("Administradores");

app.MapDelete("/administradores/{id}", ([FromQuery] int id, IAdministradorServico administradorServico) => {
    var administrador = administradorServico.BuscaPorId(id);
    if (administrador == null)
        return Results.NotFound();

    administradorServico.Apagar(administrador);
    return Results.Ok();
}).RequireAuthorization().WithTags("Administradores");
#endregion

#region Veículos
ErrosDeValidacao validaDTO(VeiculoDTO dto)
{
    var validacao = new ErrosDeValidacao {
        Mensagens = new List<string>()
    };

    if (string.IsNullOrEmpty(dto.Nome))
        validacao.Mensagens.Add("O nome do veículo não foi preenchido.");
    
    if (string.IsNullOrEmpty(dto.Marca))
        validacao.Mensagens.Add("A marca do veículo não foi preenchida.");
    
    if (dto.Ano < 1970)
        validacao.Mensagens.Add("Só aceitamos veículos a partir de 1970.");

    return validacao;
}

app.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => {

    var validacao = validaDTO(veiculoDTO);

    if (validacao.Mensagens.Count() > 0)
        return Results.BadRequest(validacao);

    var veiculo = new Veiculo {
        Nome = veiculoDTO.Nome,
        Marca = veiculoDTO.Marca,
        Ano = veiculoDTO.Ano
    };
    veiculoServico.Incluir(veiculo);
    return Results.Created($"/veiculo/{veiculo.Id}", veiculo);
}).RequireAuthorization().WithTags("Veículos");

app.MapGet("/veiculos", ([FromQuery] int? pagina, [FromQuery] string? nome, [FromQuery] string? marca, IVeiculoServico veiculoServico) => {
    var veiculos = veiculoServico.Todos(pagina, nome, marca);

    return Results.Ok(veiculos);
}).RequireAuthorization().WithTags("Veículos");

app.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
    var veiculo = veiculoServico.BuscaPorId(id);
    return veiculo == null ? Results.NotFound() : Results.Ok(veiculo);
}).RequireAuthorization().WithTags("Veículos");

app.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => {
    var validacao = validaDTO(veiculoDTO);

    if (validacao.Mensagens.Count() > 0)
        return Results.BadRequest(validacao);
        
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null)
        return Results.NotFound();

    veiculo.Nome = veiculoDTO.Nome;
    veiculo.Marca = veiculoDTO.Marca;
    veiculo.Ano = veiculoDTO.Ano;

    veiculoServico.Atualizar(veiculo);
    return Results.Ok();
}).RequireAuthorization().WithTags("Veículos");

app.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
    var veiculo = veiculoServico.BuscaPorId(id);
    if (veiculo == null)
        return Results.NotFound();

    veiculoServico.Apagar(veiculo);
    return Results.Ok();
}).RequireAuthorization().WithTags("Veículos");
#endregion

#region App
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

app.Run();
#endregion
