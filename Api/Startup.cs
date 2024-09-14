using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using minimal_api.Domain.DTOs;
using minimal_api.Domain.Entities;
using minimal_api.Domain.Enums;
using minimal_api.Domain.Interfaces;
using minimal_api.Domain.ModelViews;
using minimal_api.Domain.Services;
using minimal_api.Infrastructure.Db;

namespace minimal_api
{
    public class Startup
    {
        public IConfiguration Configuration { get; set; }
        private string _key;

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
            this._key = Configuration.GetSection("Jwt").ToString() ?? "";
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.
                AddAuthentication(option => {
                    option.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    option.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                }).
                AddJwtBearer(option => {
                option.TokenValidationParameters = new TokenValidationParameters {
                    ValidateLifetime = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._key)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };
            });

            services.AddAuthorization();

            services.AddScoped<IAdministradorServico, AdministradorServico>();
            services.AddScoped<IVeiculoServico, VeiculoServico>();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(options => {
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

            services.AddDbContext<MyDbContext>(options => {
                options.UseMySql(
                    Configuration.GetConnectionString("MySql"),
                    ServerVersion.AutoDetect(Configuration.GetConnectionString("MySql")));
            });

            services.AddCors(Options => {
                Options.AddDefaultPolicy(builder => {
                    builder.AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader();
                });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseCors();

            app.UseEndpoints(endpoints => {
                #region Home
                endpoints.MapGet("/", () => Results.Json(new Home())).WithTags("Home");
                #endregion

                #region Administrador

                string GerarTokenJwt(Administrador administrador)
                {
                    if(string.IsNullOrEmpty(this._key))
                        return string.Empty;

                    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(this._key));
                    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

                    var claims = new List<Claim>() {
                        new Claim("Email", administrador.Email),
                        new Claim("Perfil", administrador.Perfil),
                        new Claim(ClaimTypes.Role, administrador.Perfil)
                    };
                    var token = new JwtSecurityToken(
                        claims: claims,
                        expires: DateTime.Now.AddDays(1),
                        signingCredentials: credentials
                    );

                    return new JwtSecurityTokenHandler().WriteToken(token);
                }

                endpoints.MapPost("/administradores/login", ([FromBody] LoginDTO loginDTO, IAdministradorServico administrador) => {
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

                endpoints.MapPost("/administradores", ([FromBody] AdministradorDTO administradorDTO, IAdministradorServico administradorServico) => {
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
                })
                    .RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                    .WithTags("Administradores");

                endpoints.MapGet("/administradores", ([FromQuery] int? pagina, [FromQuery] string? email, [FromQuery] string? perfil, IAdministradorServico administradorServico) => {
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
                })
                    .RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                    .WithTags("Administradores");

                endpoints.MapGet("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) => {
                    var administrador = administradorServico.BuscaPorId(id);
                    return administrador == null ?
                        Results.NotFound() :
                        Results.Ok(new AdministradorModelView{
                            Id = administrador.Id,
                            Email = administrador.Email,
                            Perfil = administrador.Perfil
                    });
                })
                    .RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                    .WithTags("Administradores");

                endpoints.MapPut("/administradores/{id}", ([FromRoute] int id, AdministradorModelView administradorMV, IAdministradorServico administradorServico) => {
                    var administrador = administradorServico.BuscaPorId(id);
                    if (administrador == null)
                        return Results.NotFound();

                    administrador.Email = administradorMV.Email;
                    administrador.Perfil = administradorMV.Perfil.ToString() ?? Perfil.Vendedor.ToString();

                    administradorServico.Atualizar(administrador);
                    return Results.Ok();
                })
                    .RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                    .WithTags("Administradores");

                endpoints.MapDelete("/administradores/{id}", ([FromRoute] int id, IAdministradorServico administradorServico) => {
                    var administrador = administradorServico.BuscaPorId(id);
                    if (administrador == null)
                        return Results.NotFound();

                    administradorServico.Apagar(administrador);
                    return Results.Ok();
                })
                    .RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                    .WithTags("Administradores");
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

                endpoints.MapPost("/veiculos", ([FromBody] VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => {

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
                })
                    .RequireAuthorization()
                    .WithTags("Veículos");

                endpoints.MapGet("/veiculos", ([FromQuery] int? pagina, [FromQuery] string? nome, [FromQuery] string? marca, IVeiculoServico veiculoServico) => {
                    var veiculos = veiculoServico.Todos(pagina, nome, marca);

                    return Results.Ok(veiculos);
                })
                    .RequireAuthorization()
                    .WithTags("Veículos");

                endpoints.MapGet("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
                    var veiculo = veiculoServico.BuscaPorId(id);
                    return veiculo == null ? Results.NotFound() : Results.Ok(veiculo);
                })
                    .RequireAuthorization()
                    .WithTags("Veículos");

                endpoints.MapPut("/veiculos/{id}", ([FromRoute] int id, VeiculoDTO veiculoDTO, IVeiculoServico veiculoServico) => {
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
                })
                    .RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                    .WithTags("Veículos");

                endpoints.MapDelete("/veiculos/{id}", ([FromRoute] int id, IVeiculoServico veiculoServico) => {
                    var veiculo = veiculoServico.BuscaPorId(id);
                    if (veiculo == null)
                        return Results.NotFound();

                    veiculoServico.Apagar(veiculo);
                    return Results.Ok();
                })
                    .RequireAuthorization()
                    .RequireAuthorization(new AuthorizeAttribute { Roles = "Adm" })
                    .WithTags("Veículos");
                #endregion
            });
        }
    }
}