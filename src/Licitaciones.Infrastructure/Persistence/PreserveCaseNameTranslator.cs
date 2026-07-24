using Npgsql;

namespace Licitaciones.Infrastructure.Persistence;

internal sealed class PreserveCaseNameTranslator : INpgsqlNameTranslator
{
    public static PreserveCaseNameTranslator Instance { get; } = new();

    private PreserveCaseNameTranslator()
    {
    }

    public string TranslateTypeName(string clrName) => clrName;

    public string TranslateMemberName(string clrName) => clrName;
}
