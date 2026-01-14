namespace IntegracaoItera.Common;

public class CnpjValidator
{
    public static bool IsValid(string? cnpj)
    {
        if (string.IsNullOrWhiteSpace(cnpj))
            return false;

        // Remove caracteres não numéricos
        cnpj = new string([.. cnpj.Where(char.IsDigit)]);

        // CNPJ deve ter 14 dígitos
        if (cnpj.Length != 14)
            return false;

        // Elimina CNPJs com todos os dígitos iguais
        if (cnpj.All(c => c == cnpj[0]))
            return false;

        int[] multiplicador1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplicador2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        string tempCnpj = cnpj[..12];

        int soma = 0;
        for (int i = 0; i < multiplicador1.Length; i++)
            soma += (tempCnpj[i] - '0') * multiplicador1[i];

        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;

        tempCnpj += digito1;

        soma = 0;
        for (int i = 0; i < multiplicador2.Length; i++)
            soma += (tempCnpj[i] - '0') * multiplicador2[i];

        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        return cnpj.EndsWith($"{digito1}{digito2}");
    }
}
