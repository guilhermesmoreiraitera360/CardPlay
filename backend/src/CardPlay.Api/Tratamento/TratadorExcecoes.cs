using CardPlay.Domain.Excecoes;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace CardPlay.Api.Tratamento;

public class TratadorExcecoes : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (status, titulo) = exception switch
        {
            RecargaInvalidaException or NomeTitularInvalidoException or SaldoInsuficienteException => (StatusCodes.Status400BadRequest, "Requisição inválida"),
            CartaoNaoEncontradoException or ProdutoNaoEncontradoException => (StatusCodes.Status404NotFound, "Recurso não encontrado"),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno")
        };

        var problema = new ProblemDetails
        {
            Status = status,
            Title = titulo,
            Detail = exception.Message,
            Type = exception.GetType().Name
        };

        httpContext.Response.StatusCode = status;
        httpContext.Response.ContentType = "application/problem+json";
        await httpContext.Response.WriteAsJsonAsync(problema, cancellationToken);
        return true;
    }
}
