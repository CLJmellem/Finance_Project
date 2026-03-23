using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using TransactionsService.Application.Commands.UpdateTransaction;
using TransactionsService.Application.DTOs;
using TransactionsService.Application.Exceptions;
using TransactionsService.Application.Mappers;

namespace TransactionsService.Tests.Application.Commands;

/// <summary>
/// Unit tests for UpdateTransactionCommandHandler.
/// </summary>
public class UpdateTransactionCommandHandlerTests
{
    private readonly Mock<ITransactionRepository> _txRepo;
    private readonly Mock<ICardRepository> _cardRepo;
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly IMapper _mapper;
    private readonly UpdateTransactionCommandHandler _handler;

    private const string UserId = "507f1f77bcf86cd799439011";
    private const string CardId = "507f1f77bcf86cd799439022";
    private const string TxId = "507f1f77bcf86cd799439033";

    public UpdateTransactionCommandHandlerTests()
    {
        _txRepo = new Mock<ITransactionRepository>();
        _cardRepo = new Mock<ICardRepository>();
        _currentUser = new Mock<ICurrentUserService>();
        _mapper = new ServiceCollection()
            .AddLogging()
            .AddAutoMapper(cfg => cfg.AddProfile<TransactionProfile>())
            .BuildServiceProvider()
            .GetRequiredService<IMapper>();

        _handler = new UpdateTransactionCommandHandler(_txRepo.Object, _cardRepo.Object, _currentUser.Object, _mapper);
    }

    private static CardDataEntity ActiveCard(double limit = 1000) =>
        new()
        {
            Id = ObjectId.Parse(CardId),
            UserId = ObjectId.Parse(UserId),
            Name = "Nubank",
            LastFourDigits = "1234",
            CreditLimit = (decimal)limit,
            DueDay = 10,
            IsActive = true
        };

    private static TransactionsDataEntity ExistingTransaction() =>
        new()
        {
            Id = ObjectId.Parse(TxId),
            UserId = ObjectId.Parse(UserId),
            CardId = ObjectId.Parse(CardId),
            TotalAmount = 200,
            Type = TransactionType.Groceries
        };

    private static UpdateTransactionCommand ValidCommand(double? total = 150) =>
        new(CardId, TxId, total, null, null, null, null, null);

    [Fact]
    public async Task Handle_ValidUpdate_ReturnsUpdatedResponse()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard());
        _txRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync(ExistingTransaction());
        _txRepo.Setup(r => r.GetUsedCreditExcludingAsync(CardId, TxId, default)).ReturnsAsync(0);

        var result = await _handler.Handle(ValidCommand(), default);

        result.Should().NotBeNull();
        result.TotalAmount.Should().Be(150);
    }

    [Fact]
    public async Task Handle_CardNotFound_ThrowsNotFoundException()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync((CardDataEntity?)null);

        var act = async () => await _handler.Handle(ValidCommand(), default);

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

        var act = async () => await _handler.Handle(ValidCommand(), default);

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

        var act = async () => await _handler.Handle(ValidCommand(), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_NewAmountExceedsCreditLimit_ThrowsDomainException()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard(limit: 100));
        _txRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync(ExistingTransaction());

        var act = async () => await _handler.Handle(ValidCommand(total: 200), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*credit limit*");
    }

    [Fact]
    public async Task Handle_SetsLastUpdated()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard());
        _txRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync(ExistingTransaction());
        _txRepo.Setup(r => r.GetUsedCreditExcludingAsync(CardId, TxId, default)).ReturnsAsync(0);

        var before = DateTime.UtcNow;
        var result = await _handler.Handle(ValidCommand(), default);

        result.LastUpdated.Should().NotBeNull();
        result.LastUpdated.Should().BeOnOrAfter(before);
    }

    [Fact]
    public async Task Handle_DoesNotModifyCardDueDateOrCreatedAt()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard());

        var original = ExistingTransaction();
        original.CardDueDate = new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc);
        original.CreatedAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);

        _txRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync(original);
        _txRepo.Setup(r => r.GetUsedCreditExcludingAsync(CardId, TxId, default)).ReturnsAsync(0);

        var result = await _handler.Handle(ValidCommand(), default);

        result.CardDueDate.Should().Be(new DateTime(2026, 5, 10, 0, 0, 0, DateTimeKind.Utc));
        result.CreatedAt.Should().Be(new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc));
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
        _txRepo.Setup(r => r.GetUsedCreditExcludingAsync(CardId, TxId, default)).ReturnsAsync(0);

        var result = await _handler.Handle(ValidCommand(), default);

        result.UserId.Should().Be(UserId);
    }
}
