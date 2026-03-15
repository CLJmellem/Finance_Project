using CardsService.Application.Commands.DeleteCard;
using CardsService.Application.Exceptions;
using CardsService.Application.Interfaces;
using CardsService.Domain.Entities;
using CardsService.Domain.Enums;
using FluentAssertions;
using MediatR;
using Moq;
using System.Linq.Expressions;

namespace CardsService.Tests.Application.Commands;

/// <summary>
/// Testes unitários para DeleteCardCommandHandler.
/// </summary>
public class DeleteCardCommandHandlerTests
{
    private readonly Mock<ICardRepository> _repositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly DeleteCardCommandHandler _handler;

    private const string CardId = "507f1f77bcf86cd799439011";
    private const string UserId = "507f1f77bcf86cd799439022";

    public DeleteCardCommandHandlerTests()
    {
        _repositoryMock = new Mock<ICardRepository>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _handler = new DeleteCardCommandHandler(_repositoryMock.Object, _currentUserMock.Object);
    }

    private static CardDataEntity BuildInactiveCard(string userId = UserId)
        => new CardDataEntity
        {
            UserId = userId,
            Name = "Cartão Inativo",
            Brand = CardBrand.Elo,
            LastFourDigits = "9876",
            CreditLimit = 2000m,
            DueDay = 5,
            IsActive = false
        };

    private static CardDataEntity BuildActiveCard(string userId = UserId)
        => new CardDataEntity
        {
            UserId = userId,
            Name = "Cartão Ativo",
            Brand = CardBrand.Elo,
            LastFourDigits = "9876",
            CreditLimit = 2000m,
            DueDay = 5,
            IsActive = true
        };

    [Fact]
    public async Task Handle_AdminUser_CallsGetByIdAsync()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        var card = BuildInactiveCard();
        _repositoryMock.Setup(r => r.GetByIdAsync(CardId, CancellationToken.None)).ReturnsAsync(card);

        await _handler.Handle(new DeleteCardCommand(CardId), CancellationToken.None);

        _repositoryMock.Verify(r => r.GetByIdAsync(CardId, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_FreeUser_CallsGetOneAsync()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(false);
        _currentUserMock.Setup(u => u.UserId).Returns(UserId);
        var card = BuildInactiveCard();
        _repositoryMock.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None)).ReturnsAsync(card);

        await _handler.Handle(new DeleteCardCommand(CardId), CancellationToken.None);

        _repositoryMock.Verify(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None), Times.Once);
        _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_AdminCardNotFound_ThrowsNotFoundException()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        _repositoryMock.Setup(r => r.GetByIdAsync(CardId, CancellationToken.None)).ReturnsAsync((CardDataEntity?)null);

        var act = async () => await _handler.Handle(new DeleteCardCommand(CardId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>().WithMessage($"*{CardId}*");
    }

    [Fact]
    public async Task Handle_FreeUserCardNotFound_ThrowsNotFoundException()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(false);
        _currentUserMock.Setup(u => u.UserId).Returns(UserId);
        _repositoryMock.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None)).ReturnsAsync((CardDataEntity?)null);

        var act = async () => await _handler.Handle(new DeleteCardCommand(CardId), CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_ActiveCard_ThrowsDomainException()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        var card = BuildActiveCard();
        _repositoryMock.Setup(r => r.GetByIdAsync(CardId, CancellationToken.None)).ReturnsAsync(card);

        var act = async () => await _handler.Handle(new DeleteCardCommand(CardId), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Card must be deactivated before it can be deleted.");
    }

    [Fact]
    public async Task Handle_ActiveCard_DoesNotCallDeleteAsync()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        var card = BuildActiveCard();
        _repositoryMock.Setup(r => r.GetByIdAsync(CardId, CancellationToken.None)).ReturnsAsync(card);

        var act = async () => await _handler.Handle(new DeleteCardCommand(CardId), CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_InactiveCard_CallsDeleteAsync()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        var card = BuildInactiveCard();
        _repositoryMock.Setup(r => r.GetByIdAsync(CardId, CancellationToken.None)).ReturnsAsync(card);

        var result = await _handler.Handle(new DeleteCardCommand(CardId), CancellationToken.None);

        result.Should().Be(Unit.Value);
        _repositoryMock.Verify(r => r.DeleteAsync(card.Id, CancellationToken.None), Times.Once);
        _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<CardDataEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_FreeUserInactiveCard_DeletesSuccessfully()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(false);
        _currentUserMock.Setup(u => u.UserId).Returns(UserId);
        var card = BuildInactiveCard(UserId);
        _repositoryMock.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None)).ReturnsAsync(card);

        var result = await _handler.Handle(new DeleteCardCommand(CardId), CancellationToken.None);

        result.Should().Be(Unit.Value);
        _repositoryMock.Verify(r => r.DeleteAsync(card.Id, CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsUnitValue()
    {
        _currentUserMock.Setup(u => u.IsAdmin).Returns(true);
        var card = BuildInactiveCard();
        _repositoryMock.Setup(r => r.GetByIdAsync(CardId, CancellationToken.None)).ReturnsAsync(card);

        var result = await _handler.Handle(new DeleteCardCommand(CardId), CancellationToken.None);

        result.Should().Be(Unit.Value);
    }
}
