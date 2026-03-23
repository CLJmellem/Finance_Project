using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using TransactionsService.Application.DTOs;
using TransactionsService.Application.Exceptions;
using TransactionsService.Application.Mappers;
using TransactionsService.Application.Queries.FindTransactionById;

namespace TransactionsService.Tests.Application.Queries;

/// <summary>
/// Unit tests for FindTransactionByIdQueryHandler.
/// </summary>
public class FindTransactionByIdQueryHandlerTests
{
    private readonly Mock<ITransactionRepository> _txRepo;
    private readonly Mock<ICardRepository> _cardRepo;
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly IMapper _mapper;
    private readonly FindTransactionByIdQueryHandler _handler;

    private const string UserId = "507f1f77bcf86cd799439011";
    private const string CardId = "507f1f77bcf86cd799439022";
    private const string TxId = "507f1f77bcf86cd799439033";

    public FindTransactionByIdQueryHandlerTests()
    {
        _txRepo = new Mock<ITransactionRepository>();
        _cardRepo = new Mock<ICardRepository>();
        _currentUser = new Mock<ICurrentUserService>();
        _mapper = new ServiceCollection()
            .AddLogging()
            .AddAutoMapper(cfg => cfg.AddProfile<TransactionProfile>())
            .BuildServiceProvider()
            .GetRequiredService<IMapper>();

        _handler = new FindTransactionByIdQueryHandler(_txRepo.Object, _cardRepo.Object, _currentUser.Object, _mapper);
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
            TotalAmount = 300,
            Type = TransactionType.Restaurants
        };

    [Fact]
    public async Task Handle_ValidQuery_ReturnsTransactionResponse()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard());
        _txRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync(ExistingTransaction());

        var result = await _handler.Handle(new FindTransactionByIdQuery(CardId, TxId, null), default);

        result.Should().NotBeNull();
        result.Id.Should().Be(TxId);
        result.TotalAmount.Should().Be(300);
    }

    [Fact]
    public async Task Handle_CardNotFound_ThrowsNotFoundException()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync((CardDataEntity?)null);

        var act = async () => await _handler.Handle(new FindTransactionByIdQuery(CardId, TxId, null), default);

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

        var act = async () => await _handler.Handle(new FindTransactionByIdQuery(CardId, TxId, null), default);

        await act.Should().ThrowAsync<DomainException>();
    }

    [Fact]
    public async Task Handle_TransactionNotFound_ThrowsNotFoundException()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard());
        _txRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync((TransactionsDataEntity?)null);

        var act = async () => await _handler.Handle(new FindTransactionByIdQuery(CardId, TxId, null), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_MapsTypeToString()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard());
        _txRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync(ExistingTransaction());

        var result = await _handler.Handle(new FindTransactionByIdQuery(CardId, TxId, null), default);

        result.Type.Should().Be("Restaurants");
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

        var result = await _handler.Handle(new FindTransactionByIdQuery(CardId, TxId, null), default);

        result.UserId.Should().Be(UserId);
    }
}
