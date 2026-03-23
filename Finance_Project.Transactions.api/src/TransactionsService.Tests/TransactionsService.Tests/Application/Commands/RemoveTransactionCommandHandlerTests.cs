using MediatR;
using TransactionsService.Application.Commands.RemoveTransaction;
using TransactionsService.Application.Exceptions;

namespace TransactionsService.Tests.Application.Commands;

/// <summary>
/// Unit tests for RemoveTransactionCommandHandler.
/// </summary>
public class RemoveTransactionCommandHandlerTests
{
    private readonly Mock<ITransactionRepository> _txRepo;
    private readonly Mock<ICardRepository> _cardRepo;
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly RemoveTransactionCommandHandler _handler;

    private const string UserId = "507f1f77bcf86cd799439011";
    private const string CardId = "507f1f77bcf86cd799439022";
    private const string TxId = "507f1f77bcf86cd799439033";

    public RemoveTransactionCommandHandlerTests()
    {
        _txRepo = new Mock<ITransactionRepository>();
        _cardRepo = new Mock<ICardRepository>();
        _currentUser = new Mock<ICurrentUserService>();
        _handler = new RemoveTransactionCommandHandler(_txRepo.Object, _cardRepo.Object, _currentUser.Object);
    }

    private static CardDataEntity ActiveCard() =>
        new()
        {
            Id = ObjectId.Parse(CardId),
            UserId = ObjectId.Parse(UserId),
            CreditLimit = 1000,
            DueDay = 10,
            IsActive = true
        };

    private static TransactionsDataEntity ExistingTransaction() =>
        new()
        {
            Id = ObjectId.Parse(TxId),
            UserId = ObjectId.Parse(UserId),
            CardId = ObjectId.Parse(CardId),
            TotalAmount = 200
        };

    [Fact]
    public async Task Handle_ValidDelete_CallsDeleteAsyncAndReturnsUnit()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard());
        _txRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync(ExistingTransaction());

        var result = await _handler.Handle(new RemoveTransactionCommand(CardId, TxId, null), default);

        result.Should().Be(Unit.Value);
        _txRepo.Verify(r => r.DeleteAsync(TxId, default), Times.Once);
    }

    [Fact]
    public async Task Handle_CardNotFound_ThrowsNotFoundException()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync((CardDataEntity?)null);

        var act = async () => await _handler.Handle(new RemoveTransactionCommand(CardId, TxId, null), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_InactiveCard_ThrowsDomainException()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        var inactiveCard = new CardDataEntity
        {
            Id = ObjectId.Parse(CardId), UserId = ObjectId.Parse(UserId),
            CreditLimit = 1000, DueDay = 10, IsActive = false
        };
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(inactiveCard);

        var act = async () => await _handler.Handle(new RemoveTransactionCommand(CardId, TxId, null), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*inactive*");
    }

    [Fact]
    public async Task Handle_TransactionNotFound_ThrowsNotFoundException()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard());
        _txRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync((TransactionsDataEntity?)null);

        var act = async () => await _handler.Handle(new RemoveTransactionCommand(CardId, TxId, null), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_AdminWithoutUserId_UsesTokenUserId()
    {
        _currentUser.Setup(u => u.IsAdmin).Returns(true);
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard());
        _txRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync(ExistingTransaction());

        var result = await _handler.Handle(new RemoveTransactionCommand(CardId, TxId, null), default);

        result.Should().Be(Unit.Value);
    }

    [Fact]
    public async Task Handle_TransactionNotFound_DoesNotCallDeleteAsync()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard());
        _txRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync((TransactionsDataEntity?)null);

        var act = async () => await _handler.Handle(new RemoveTransactionCommand(CardId, TxId, null), default);

        await act.Should().ThrowAsync<NotFoundException>();
        _txRepo.Verify(r => r.DeleteAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
