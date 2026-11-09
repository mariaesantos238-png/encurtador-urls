using Xunit;
using Moq;
using EncurtadorURLs.Services;
using EncurtadorURLs.Data;
using EncurtadorURLs.Models;

namespace EncurtadorURLs.Tests.Services;

/// <summary>
/// Testes unitários para UrlsService.
/// FASE RED do TDD: estes testes foram escritos ANTES da implementação existir.
/// Todos devem FALHAR neste momento — isso é o esperado e desejado.
/// </summary>
public class UrlsServiceTests
{
    private readonly Mock<IUrlContext> _mockContext;
    private readonly UrlsService _service;

    public UrlsServiceTests()
    {
        _mockContext = new Mock<IUrlContext>();
        _service = new UrlsService(_mockContext.Object);
    }

    // ==========================================
    // EXERCÍCIO 1 — Testes para CreateShortUrlAsync
    // ==========================================

    [Fact]
    public async Task CreateShortUrlAsync_ComUrlValida_DeveRetornarUrlItemComShortCode()
    {
        // Arrange
        var urlOriginal = "https://www.google.com";
        _mockContext.Setup(c => c.ShortCodeExistsAsync(It.IsAny<string>()))
            .ReturnsAsync(false);

        // Act
        var resultado = await _service.CreateShortUrlAsync(urlOriginal);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(urlOriginal, resultado.OriginalUrl);
        Assert.False(string.IsNullOrEmpty(resultado.ShortCode));

        // Verificação: garante que AddAsync foi chamado exatamente 1 vez
        _mockContext.Verify(c => c.AddAsync(It.IsAny<UrlItem>()), Times.Once);
    }

    [Fact]
    public async Task CreateShortUrlAsync_ComUrlVazia_DeveLancarArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateShortUrlAsync(string.Empty)
        );
    }

    [Fact]
    public async Task CreateShortUrlAsync_ComUrlNula_DeveLancarArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateShortUrlAsync(null)
        );
    }

    [Fact]
    public async Task CreateShortUrlAsync_ComEsquemaInvalido_DeveLancarArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateShortUrlAsync("ftp://arquivo-invalido.com")
        );
    }

    [Fact]
    public async Task CreateShortUrlAsync_ComUrlMuitoLonga_DeveLancarArgumentException()
    {
        // Arrange
        var urlGigante = "https://exemplo.com/" + new string('a', 2100);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.CreateShortUrlAsync(urlGigante)
        );
    }

    // ==========================================
    // EXERCÍCIO 2 — Testes para GetUrlByIdAsync
    // ==========================================

    [Fact]
    public async Task GetUrlByIdAsync_ComGuidVazio_DeveLancarArgumentException()
    {
        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => _service.GetUrlByIdAsync(Guid.Empty)
        );
    }

    [Fact]
    public async Task GetUrlByIdAsync_ComIdInexistente_DeveRetornarNull()
    {
        // Arrange
        var idInexistente = Guid.NewGuid();
        _mockContext.Setup(c => c.GetByIdAsync(idInexistente))
            .ReturnsAsync((UrlItem?)null);

        // Act
        var resultado = await _service.GetUrlByIdAsync(idInexistente);

        // Assert
        Assert.Null(resultado);
    }

    [Fact]
    public async Task GetUrlByIdAsync_ComIdExistente_DeveRetornarUrlItemCorreto()
    {
        // Arrange
        var idExistente = Guid.NewGuid();
        var urlItemEsperado = new UrlItem
        {
            Id = idExistente,
            OriginalUrl = "https://www.google.com",
            ShortCode = "abc1234"
        };

        _mockContext.Setup(c => c.GetByIdAsync(idExistente))
            .ReturnsAsync(urlItemEsperado);

        // Act
        var resultado = await _service.GetUrlByIdAsync(idExistente);

        // Assert
        Assert.NotNull(resultado);
        Assert.Equal(idExistente, resultado.Id);
        Assert.Equal(urlItemEsperado.OriginalUrl, resultado.OriginalUrl);
    }
}