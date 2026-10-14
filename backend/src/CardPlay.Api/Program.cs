using CardPlay.Api.Tratamento;
using CardPlay.Application.Contratos.Repositorios;
using CardPlay.Application.Contratos.Servicos;
using CardPlay.Application.Servicos;
using CardPlay.Repository.Contexto;
using CardPlay.Repository.Repositorios;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<TratadorExcecoes>();

builder.Services.AddDbContext<CardPlayDbContext>(opcoes =>
    opcoes.UseSqlite(builder.Configuration.GetConnectionString("CardPlay")));

builder.Services.AddScoped<ICartaoRepositorio, CartaoRepositorio>();
builder.Services.AddScoped<IProdutoRepositorio, ProdutoRepositorio>();
builder.Services.AddScoped<ICartaoServico, CartaoServico>();
builder.Services.AddScoped<IProdutoServico, ProdutoServico>();
builder.Services.AddScoped<ICompraServico, CompraServico>();

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddCors(opcoes =>
    {
        opcoes.AddDefaultPolicy(politica =>
            politica.AllowAnyOrigin()
                .AllowAnyHeader()
                .AllowAnyMethod());
    });
}

var app = builder.Build();

using (var escopo = app.Services.CreateScope())
{
    var contexto = escopo.ServiceProvider.GetRequiredService<CardPlayDbContext>();
    contexto.Database.Migrate();
}

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseCors();
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.MapControllers();
app.Run();
