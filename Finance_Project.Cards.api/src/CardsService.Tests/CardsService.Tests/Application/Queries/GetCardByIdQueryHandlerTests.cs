using AutoMapper;
using CardsService.Application.Exceptions;
using CardsService.Application.Interfaces;
using CardsService.Application.Mappers;
using CardsService.Application.Queries.GetCardById;
using CardsService.Domain.Entities;
using CardsService.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Linq.Expressions;

namespace CardsService.Tests.Application.Queries;

/// <summary>
/// Testes unitários para GetCardByIdQueryHandler.
/// </summary>
public class GetCardByIdQueryHandlerTests
{
    private readonly Mock<ICardRepository> _repositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly IMapper _mapper;
    private readonly GetCardByIdQueryHandler _handler;

    private const string CardId = "507f1f77bcf86cd799439011";
    private const string UserId = "507f1f77bcf86cd799439033";

    public GetCardByIdQueryHandlerTests()
    {
        _repositoryMock = new Mock<ICardRepository>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _mapper = new ServiceCollection().AddLogging().AddAutoMapper(cfg => cfg.AddProfile<CardProfile>()).BuildServiceProvider().GetRequiredService<IMapper>();
        _handler = new GetCardByIdQueryHandler(_repositoryMock.Object, _currentUserMock.Object, _mapper);
    }

    private static CardDataEntity BuildCard(string userId = UserId)
        => new CardDataEntity
        {
            UserId = userId,
            Name = "Cartão Teste",
            Brand = CardBrand.Visa,
            LastFourDigits = "1234",
            CreditLimit = 5000m,
            DueDay = 10
        };

    [Fact]
    public async Task Handle_AdminUser_CallsGetByIdAsync()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        var card = BuildCard("outro-user-id");
        _repositoryMock.Setup(r => r.GetByIdAsync(CardId, CancellationToken.None)).ReturnsAsync(card);

        var result = await _handler.Handle(new GetCardByIdQuery(CardId), CancellationToken.None);

        result.Should().NotBeNull();
        _repositoryMock.Verify(r => r.GetByIdAsync(CardId, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_FreeUser_CallsGetOneAsync()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(false);
        _currentUserMock.Setup(u => u.UserId).Returns(UserId);
        var card = BuildCard(UserId);
        _repositoryMock.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None)).ReturnsAsync(card);

        var result = await _handler.Handle(new GetCardByIdQuery(CardId), CancellationToken.None);

        result.Should().NotBeNull();
        _repositoryMock.Verify(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AdminCardNotFound_ThrowsNotFoundException()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        _repositoryMock.Setup(r => r.GetByIdAsync(CardId, CancellationToken.None)).ReturnsAsync((CardDataEntity?)null);

        var act = async () => await _handler.Handle(new GetCardByIdQuery(CardId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{CardId}*");
    }

    [Fact]
    public async Task Handle_FreeUserCardNotFound_ThrowsNotFoundException()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(false);
        _currentUserMock.Setup(u => u.UserId).Returns(UserId);
        _repositoryMock.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None)).ReturnsAsync((CardDataEntity?)null);

        var act = async () => await _handler.Handle(new GetCardByIdQuery(CardId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CardFound_ReturnsMappedCardResponse()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        var card = BuildCard(UserId);
        _repositoryMock.Setup(r => r.GetByIdAsync(CardId, CancellationToken.None)).ReturnsAsync(card);

        var result = await _handler.Handle(new GetCardByIdQuery(CardId), CancellationToken.None);

        result.UserId.Should().Be(UserId);
        result.Name.Should().Be("Cartão Teste");
        result.Brand.Should().Be("Visa");
        result.LastFourDigits.Should().Be("1234");
        result.CreditLimit.Should().Be(5000m);
        result.DueDay.Should().Be(10);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_FreeUserGetsOwnCard_ReturnsMappedResponse()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(false);
        _currentUserMock.Setup(u => u.UserId).Returns(UserId);
        var card = BuildCard(UserId);
        _repositoryMock.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None)).ReturnsAsync(card);

        var result = await _handler.Handle(new GetCardByIdQuery(CardId), CancellationToken.None);

        result.UserId.Should().Be(UserId);
        result.Name.Should().Be("Cartão Teste");
        result.IsActive.Should().BeTrue();
    }
}
