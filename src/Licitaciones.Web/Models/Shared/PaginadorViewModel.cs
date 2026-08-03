using Licitaciones.Application.Common.Models;

namespace Licitaciones.Web.Models.Shared;

public sealed record PaginadorViewModel(
    IPaginaResultado Resultado,
    string Action,
    string Controller,
    int PageSize,
    IReadOnlyDictionary<string, string> RutaExtra);
