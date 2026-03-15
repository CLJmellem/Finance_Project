using AutoMapper;
using CardsService.Application.Commands.UpdateCard;
using CardsService.Application.Exceptions;
using CardsService.Application.Interfaces;
using CardsService.Application.Mappers;
using CardsService.Domain.Entities;
using CardsService.Domain.Enums;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using System.Linq.Expressions;

namespace CardsService.Tests.Application.Commands;

/// <summary>
/// Testes unitários para UpdateCardCommandHandler.
/// </summary>
public class UpdateCardCommandHandlerTests
{
    private readonly Mock<ICardRepository> _repositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly IMapper _mapper;
    private readonly UpdateCardCommandHandler _handler;

    private const string CardId = "507f1f77bcf86cd799439011";
    private const string UserId = "507f1f77bcf86cd799439012";

    public UpdateCardCommandHandlerTests()
    {
        _repositoryMock = new Mock<ICardRepository>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _mapper = new ServiceCollection().AddLogging().AddAutoMapper(cfg => cfg.AddProfile<CardProfile>()).BuildServiceProvider().GetRequiredService<IMapper>();
        _handler = new UpdateCardCommandHandler(_repositoryMock.Object, _currentUserMock.Object, _mapper);
    }

    private static CardDataEntity BuildCard(string userId = UserId)
        => new CardDataEntity
        {
            UserId = userId,
            Name = "Cartão Original",
            Brand = CardBrand.Visa,
            LastFourDigits = "1234",
            CreditLimit = 1000m,
            DueDay = 10
        };

    [Fact]
    public async Task Handle_AdminUser_CallsGetByIdAsync()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        var card = BuildCard();
        _repositoryMock.Setup(r => r.GetByIdAsync(CardId, CancellationToken.None)).ReturnsAsync(card);
        _repositoryMock.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync((CardDataEntity?)null);

        await _handler.Handle(new UpdateCardCommand(CardId, "Novo Nome", 5000m, 15, true), CancellationToken.None);

        _repositoryMock.Verify(r => r.GetByIdAsync(CardId, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FreeUser_CallsGetOneAsync()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(false);
        _currentUserMock.Setup(u => u.UserId).Returns(UserId);
        var card = BuildCard();
        _repositoryMock.SetupSequence(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(card)
            .ReturnsAsync((CardDataEntity?)null);

        await _handler.Handle(new UpdateCardCommand(CardId, "Novo Nome", 5000m, 15, true), CancellationToken.None);

        _repositoryMock.Verify(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None), Times.Exactly(2));
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AdminCardNotFound_ThrowsNotFoundException()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        _repositoryMock.Setup(r => r.GetByIdAsync(CardId, CancellationToken.None)).ReturnsAsync((CardDataEntity?)null);

        var act = async () => await _handler.Handle(new UpdateCardCommand(CardId, "Novo Nome", 1000m, 10, true), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{CardId}*");
    }

    [Fact]
    public async Task Handle_FreeUserCardNotFound_ThrowsNotFoundException()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(false);
        _currentUserMock.Setup(u => u.UserId).Returns(UserId);
        _repositoryMock.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync((CardDataEntity?)null);

        var act = async () => await _handler.Handle(new UpdateCardCommand(CardId, "Novo Nome", 1000m, 10, true), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_CardFound_CallsUpdateAndRepositoryUpdateAsync()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        var card = BuildCard();
        _repositoryMock.Setup(r => r.GetByIdAsync(CardId, CancellationToken.None)).ReturnsAsync(card);
        _repositoryMock.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync((CardDataEntity?)null);

        var result = await _handler.Handle(new UpdateCardCommand(CardId, "Nome Atualizado", 9999m, 25, true), CancellationToken.None);

        result.Name.Should().Be("Nome Atualizado");
        result.CreditLimit.Should().Be(9999m);
        result.DueDay.Should().Be(25);
        result.IsActive.Should().BeTrue();
        _repositoryMock.Verify(r => r.UpdateAsync(card, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsUpdatedCardResponse()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(false);
        _currentUserMock.Setup(u => u.UserId).Returns(UserId);
        var card = BuildCard(UserId);
        _repositoryMock.SetupSequence(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(card)
            .ReturnsAsync((CardDataEntity?)null);

        var result = await _handler.Handle(new UpdateCardCommand(CardId, "Cartão Atualizado", 7500m, 20, true), CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Cartão Atualizado");
        result.CreditLimit.Should().Be(7500m);
        result.DueDay.Should().Be(20);
        result.UserId.Should().Be(UserId);
    }

    [Fact]
    public async Task Handle_SetIsActiveFalse_DeactivatesCard()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        var card = BuildCard();
        _repositoryMock.Setup(r => r.GetByIdAsync(CardId, CancellationToken.None)).ReturnsAsync(card);
        _repositoryMock.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync((CardDataEntity?)null);

        var result = await _handler.Handle(new UpdateCardCommand(CardId, "Cartão Original", 1000m, 10, false), CancellationToken.None);

        result.IsActive.Should().BeFalse();
        _repositoryMock.Verify(r => r.UpdateAsync(card, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateName_ThrowsDomainException()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        var card = BuildCard();
        _repositoryMock.Setup(r => r.GetByIdAsync(CardId, CancellationToken.None)).ReturnsAsync(card);
        _repositoryMock.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(new CardDataEntity { Name = "Nome Duplicado" });

        var act = async () => await _handler.Handle(new UpdateCardCommand(CardId, "Nome Duplicado", 1000m, 10, true), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Nome Duplicado*");
    }

    [Fact]
    public async Task Handle_DuplicateName_DoesNotCallUpdateAsync()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(false);
        _currentUserMock.Setup(u => u.UserId).Returns(UserId);
        var card = BuildCard(UserId);
        _repositoryMock.SetupSequence(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(card)
            .ReturnsAsync(new CardDataEntity { Name = "Nome Existente" });

        var act = async () => await _handler.Handle(new UpdateCardCommand(CardId, "Nome Existente", 1000m, 10, true), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<CardDataEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
