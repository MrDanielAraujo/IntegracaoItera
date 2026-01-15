using IntegracaoItera.Common;
using IntegracaoItera.Data.DTOs;
using IntegracaoItera.Data.Enums;
using IntegracaoItera.Interfaces;
using IntegracaoItera.Models;
using Microsoft.AspNetCore.Authorization;
using System;

namespace IntegracaoItera.Services;

[AllowAnonymous]
public class DocumentoValidadorService(IAnoDocumentoService anoDocumentoService) : IDocumentoValidadorService
{
    private readonly IAnoDocumentoService _anoDocumentoService = anoDocumentoService ?? throw new ArgumentNullException(nameof(anoDocumentoService));

    /// <summary>
    /// 
    /// </summary>
    /// <param name="cnpj"></param>
    /// <returns></returns>
    public bool ValidarCnpj (string? cnpj)
        => cnpj is not null && cnpj.Length != 0 && CnpjValidator.IsValid(cnpj);
           
    /// <summary>
    /// 
    /// </summary>
    /// <param name="requestDto"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public ClientArquivoDto ResolverListaDeArquivosParaEnvio(List<ClientArquivoDto> arquivos)
    {
        // Verifica se contem arquivos no objeto.
        if (arquivos is null || arquivos.Count == 0)
            throw new Exception("Nenhum arquivo foi informado.");

        // Filtrar por anos válidos
        var arquivosAnoValido = arquivos
            .Where(a => _anoDocumentoService.AnoEhValido(a.Ano)).ToList();

        // Verifica se temos aqruivos validos dentro dos anos validos.
        if (arquivosAnoValido.Count == 0)
            throw new Exception("Nenhum arquivo dentro do range de anos válidos.");

        // Obtem o ano mais recente.
        var maiorAno = arquivosAnoValido.Max(a => a.Ano);

        // Obtem os arquivos do maior ano.
        var arquivosDoAno = arquivosAnoValido
                .Where(a => a.Ano == maiorAno)
                .OrderByDescending(a => a.DataCadastro)
                .ToList();

        // Obtem os arquivos do tipo Balanço
        var tipo1 = arquivosDoAno.FirstOrDefault(a => a.Tipo == (int)ClientArquivoTipo.BalancoComDre);
        // Obtem os arquivos do tipo Balancete
        var tipo2 = arquivosDoAno.FirstOrDefault(a => a.Tipo == (int)ClientArquivoTipo.BalanceteComDre);
        // obtem os arquivos do tipo DRE
        var tipo9 = arquivosDoAno.FirstOrDefault(a => a.Tipo == (int)ClientArquivoTipo.Dre);

        // DRE não pode ser isolada
        if (tipo9 is not null && tipo1 is null && tipo2 is null)
            throw new Exception("Arquivo DRE não pode ser enviado isoladamente.");

        // Este caso retorna um tipo 1 com o tipo 9 junto (balanço com Dre)
        if (tipo9 is not null && tipo1 is not null)
        {
            tipo1.Content = PdfMeneger.MergePdfs(tipo1.Content, tipo9.Content);
            return tipo1;
        }

        // Este caso retorna um tipo 2 com o tipo 9 junto (balancete com Dre)
        if (tipo9 is not null && tipo2 is not null)
        {
            tipo2.Content = PdfMeneger.MergePdfs(tipo2.Content, tipo9.Content);
            return tipo2;
        }

        // recebe ou o tipo 1 como prioridade ou o tipo 2 caso a tipo um não exista.
        var arquivoFinal = tipo1 ?? tipo2;

        if (arquivoFinal is not null)
            return arquivoFinal!;

        throw new Exception("Não foi enviado nenum arquivo valido.");
    }
}
