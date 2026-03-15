using AutoMapper;
using CardsService.Application.Exceptions;
using CardsService.Application.Interfaces;
using CardsService.Application.Mappers;
using CardsService.Application.Queries.GetUserCards;
using CardsService.Domain.Entities;
using CardsService.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Linq.Expressions;

namespace CardsService.Tests.Application.Queries;

/// <summary>
/// Testes unitários para GetUserCardsQueryHandler.
/// </summary>
public class GetUserCardsQueryHandlerTests
{
    private readonly Mock<ICardRepository> _repositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly IMapper _mapper;
    private readonly GetUserCardsQueryHandler _handler;

    private const string UserId = "507f1f77bcf86cd799439044";
    private const string AdminTargetUserId = "507f1f77bcf86cd799439055";

    public GetUserCardsQueryHandlerTests()
    {
        _repositoryMock = new Mock<ICardRepository>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _mapper = new ServiceCollection().AddLogging().AddAutoMapper(cfg => cfg.AddProfile<CardProfile>()).BuildServiceProvider().GetRequiredService<IMapper>();
        _handler = new GetUserCardsQueryHandler(_repositoryMock.Object, _currentUserMock.Object, _mapper);
    }

    private static CardDataEntity BuildCard(string userId, string name = "Cartão")
        => new CardDataEntity
        {
            UserId = userId,
            Name = name,
            Brand = CardBrand.Mastercard,
            LastFourDigits = "1234",
            CreditLimit = 1000m,
            DueDay = 10
        };

    // -------------------------------------------------------------------------
    // Admin — UserId obrigatório
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_AdminUser_WithoutUserId_ThrowsDomainException()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);

        var act = async () => await _handler.Handle(new GetUserCardsQuery(), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*UserId*");
    }

    [Fact]
    public async Task Handle_AdminUser_WithEmptyUserId_ThrowsDomainException()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);

        var act = async () => await _handler.Handle(new GetUserCardsQuery(UserId: "   "), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*UserId*");
    }

    // -------------------------------------------------------------------------
    // Admin — caminho feliz
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_AdminUser_WithUserId_CallsGetAllAsyncAndCountAllAsync()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        var cards = new List<CardDataEntity> { BuildCard(AdminTargetUserId, "Cartão 1"), BuildCard(AdminTargetUserId, "Cartão 2") };
        _repositoryMock.Setup(r => r.GetAllAsync(1, 20, It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(cards.AsReadOnly());
        _repositoryMock.Setup(r => r.CountAllAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(2L);

        var result = await _handler.Handle(new GetUserCardsQuery(Page: 1, PageSize: 20, UserId: AdminTargetUserId), CancellationToken.None);

        _repositoryMock.Verify(r => r.GetAllAsync(1, 20, It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(r => r.CountAllAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None), Times.Once);
        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_AdminUser_ReturnsPaginatedResponse()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        var cards = Enumerable.Range(1, 5).Select(i => BuildCard(AdminTargetUserId, $"Cartão {i}")).ToList();
        _repositoryMock.Setup(r => r.GetAllAsync(2, 5, It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(cards.AsReadOnly());
        _repositoryMock.Setup(r => r.CountAllAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(12L);

        var result = await _handler.Handle(new GetUserCardsQuery(Page: 2, PageSize: 5, UserId: AdminTargetUserId), CancellationToken.None);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(5);
        result.TotalCount.Should().Be(12);
        result.TotalPages.Should().Be(3);
        result.HasPreviousPage.Should().BeTrue();
        result.HasNextPage.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_AdminFirstPage_HasNoPreviousPage()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        _repositoryMock.Setup(r => r.GetAllAsync(1, 10, It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(new List<CardDataEntity>().AsReadOnly());
        _repositoryMock.Setup(r => r.CountAllAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(5L);

        var result = await _handler.Handle(new GetUserCardsQuery(Page: 1, PageSize: 10, UserId: AdminTargetUserId), CancellationToken.None);

        result.HasPreviousPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_AdminLastPage_HasNoNextPage()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        _repositoryMock.Setup(r => r.GetAllAsync(1, 20, It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(new List<CardDataEntity>().AsReadOnly());
        _repositoryMock.Setup(r => r.CountAllAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(10L);

        var result = await _handler.Handle(new GetUserCardsQuery(Page: 1, PageSize: 20, UserId: AdminTargetUserId), CancellationToken.None);

        result.HasNextPage.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_AdminUser_ItemsAreMappedCorrectly()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        var card = BuildCard("user-mapped", "Cartão Mapeado");
        _repositoryMock.Setup(r => r.GetAllAsync(1, 20, It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(new List<CardDataEntity> { card }.AsReadOnly());
        _repositoryMock.Setup(r => r.CountAllAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(1L);

        var result = await _handler.Handle(new GetUserCardsQuery(Page: 1, PageSize: 20, UserId: "user-mapped"), CancellationToken.None);

        var item = result.Items.Single();
        item.Name.Should().Be("Cartão Mapeado");
        item.UserId.Should().Be("user-mapped");
        item.LastFourDigits.Should().Be("1234");
        item.Brand.Should().Be("Mastercard");
    }

    // -------------------------------------------------------------------------
    // FreeUser
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_FreeUser_CallsGetAllAsyncWithUserFilter()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(false);
        _currentUserMock.Setup(u => u.UserId).Returns(UserId);
        var cards = new List<CardDataEntity> { BuildCard(UserId, "Meu Cartão 1"), BuildCard(UserId, "Meu Cartão 2") };
        _repositoryMock.Setup(r => r.GetAllAsync(1, 20, It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(cards.AsReadOnly());
        _repositoryMock.Setup(r => r.CountAllAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(2L);

        var result = await _handler.Handle(new GetUserCardsQuery(Page: 1, PageSize: 20), CancellationToken.None);

        _repositoryMock.Verify(r => r.GetAllAsync(1, 20, It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(r => r.CountAllAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_FreeUser_ReturnsOwnCards()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(false);
        _currentUserMock.Setup(u => u.UserId).Returns(UserId);
        var cards = new List<CardDataEntity> { BuildCard(UserId, "A"), BuildCard(UserId, "B"), BuildCard(UserId, "C") };
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(cards.AsReadOnly());
        _repositoryMock.Setup(r => r.CountAllAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(3L);

        var result = await _handler.Handle(new GetUserCardsQuery(Page: 1, PageSize: 20), CancellationToken.None);

        result.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(20);
    }

    [Fact]
    public async Task Handle_FreeUserNoCards_ReturnsEmptyPaginatedResponse()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(false);
        _currentUserMock.Setup(u => u.UserId).Returns(UserId);
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(new List<CardDataEntity>().AsReadOnly());
        _repositoryMock.Setup(r => r.CountAllAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(0L);

        var result = await _handler.Handle(new GetUserCardsQuery(), CancellationToken.None);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    // -------------------------------------------------------------------------
    // InactiveCards
    // -------------------------------------------------------------------------

    [Fact]
    public async Task Handle_InactiveCardsTrue_ReturnsAllCardsIncludingInactive()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(false);
        _currentUserMock.Setup(u => u.UserId).Returns(UserId);
        var cards = new List<CardDataEntity>
        {
            BuildCard(UserId, "Ativo"),
            new CardDataEntity { UserId = UserId, Name = "Inativo", IsActive = false }
        };
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(cards.AsReadOnly());
        _repositoryMock.Setup(r => r.CountAllAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(2L);

        var result = await _handler.Handle(new GetUserCardsQuery(InactiveCards: true), CancellationToken.None);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_InactiveCardsFalse_ReturnsOnlyActiveCards()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(false);
        _currentUserMock.Setup(u => u.UserId).Returns(UserId);
        var activeCards = new List<CardDataEntity> { BuildCard(UserId, "Ativo") };
        _repositoryMock.Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(activeCards.AsReadOnly());
        _repositoryMock.Setup(r => r.CountAllAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(1L);

        var result = await _handler.Handle(new GetUserCardsQuery(InactiveCards: false), CancellationToken.None);

        result.Items.Should().HaveCount(1);
        result.TotalCount.Should().Be(1);
    }
}
