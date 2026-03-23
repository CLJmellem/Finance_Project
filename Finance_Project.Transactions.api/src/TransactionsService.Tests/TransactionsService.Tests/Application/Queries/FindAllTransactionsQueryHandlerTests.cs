using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using TransactionsService.Application.DTOs;
using TransactionsService.Application.Exceptions;
using TransactionsService.Application.Mappers;
using TransactionsService.Application.Queries.FindAllTransactions;

namespace TransactionsService.Tests.Application.Queries;

/// <summary>
/// Unit tests for FindAllTransactionsQueryHandler.
/// </summary>
public class FindAllTransactionsQueryHandlerTests
{
    private readonly Mock<ITransactionRepository> _txRepo;
    private readonly Mock<ICardRepository> _cardRepo;
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly IMapper _mapper;
    private readonly FindAllTransactionsQueryHandler _handler;

    private const string UserId = "507f1f77bcf86cd799439011";
    private const string CardId = "507f1f77bcf86cd799439022";

    public FindAllTransactionsQueryHandlerTests()
    {
        _txRepo = new Mock<ITransactionRepository>();
        _cardRepo = new Mock<ICardRepository>();
        _currentUser = new Mock<ICurrentUserService>();
        _mapper = new ServiceCollection()
            .AddLogging()
            .AddAutoMapper(cfg => cfg.AddProfile<TransactionProfile>())
            .BuildServiceProvider()
            .GetRequiredService<IMapper>();

        _handler = new FindAllTransactionsQueryHandler(_txRepo.Object, _cardRepo.Object, _currentUser.Object, _mapper);
    }

    private static CardDataEntity ActiveCard() =>
        new()
        {
            Id = ObjectId.Parse(CardId),
            UserId = ObjectId.Parse(UserId),
            Name = "Nubank",
            LastFourDigits = "1234",
            CreditLimit = 1000,
            DueDay = 10,
            IsActive = true
        };

    private static List<TransactionsDataEntity> TwoTransactions() =>
    [
        new() { Id = ObjectId.Parse("507f1f77bcf86cd799439031"), UserId = ObjectId.Parse(UserId), CardId = ObjectId.Parse(CardId), TotalAmount = 100 },
        new() { Id = ObjectId.Parse("507f1f77bcf86cd799439032"), UserId = ObjectId.Parse(UserId), CardId = ObjectId.Parse(CardId), TotalAmount = 200 }
    ];

    [Fact]
    public async Task Handle_NoFilters_ReturnsAllUserTransactions()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        var txList = TwoTransactions().AsReadOnly();
        _txRepo.Setup(r => r.GetAllAsync(0, 0, It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync(txList);
        _txRepo.Setup(r => r.CountAllAsync(It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync(2);

        var result = await _handler.Handle(new FindAllTransactionsQuery(null, null, null, null, null, 0, 0), default);

        result.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_AdminWithoutUserId_UsesTokenUserId()
    {
        _currentUser.Setup(u => u.IsAdmin).Returns(true);
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _txRepo.Setup(r => r.GetAllAsync(0, 0, It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync(new List<TransactionsDataEntity>().AsReadOnly());
        _txRepo.Setup(r => r.CountAllAsync(It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync(0);

        var result = await _handler.Handle(
            new FindAllTransactionsQuery(null, null, null, null, null, 0, 0), default);

        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_CardNameNotFound_ReturnsEmptyList()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync((CardDataEntity?)null);

        var result = await _handler.Handle(
            new FindAllTransactionsQuery(null, null, null, "Unknown", null, 0, 0), default);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_CardNameFoundButInactive_ThrowsDomainException()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        var inactiveCard = new CardDataEntity
        {
            Id = ObjectId.Parse(CardId), UserId = ObjectId.Parse(UserId),
            Name = "Nubank", LastFourDigits = "1234", CreditLimit = 1000, DueDay = 10, IsActive = false
        };
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(inactiveCard);

        var act = async () => await _handler.Handle(
            new FindAllTransactionsQuery(null, null, null, "Nubank", null, 0, 0), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*inactive*");
    }

    [Fact]
    public async Task Handle_LastFourDigitsNotFound_ReturnsEmptyList()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync((CardDataEntity?)null);

        var result = await _handler.Handle(
            new FindAllTransactionsQuery(null, null, null, null, "9999", 0, 0), default);

        result.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_CardIdNotFound_ThrowsNotFoundException()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync((CardDataEntity?)null);

        var act = async () => await _handler.Handle(
            new FindAllTransactionsQuery(null, CardId, null, null, null, 0, 0), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_TransactionIdNotFound_ThrowsNotFoundException()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _txRepo.Setup(r => r.GetAllAsync(0, 0, It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync(new List<TransactionsDataEntity>().AsReadOnly());
        _txRepo.Setup(r => r.CountAllAsync(It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync(0);

        var act = async () => await _handler.Handle(
            new FindAllTransactionsQuery(null, null, "507f1f77bcf86cd799439099", null, null, 0, 0), default);

        await act.Should().ThrowAsync<NotFoundException>();
    }

    [Fact]
    public async Task Handle_PaginationApplied_PassesPageAndPageSizeToRepository()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _txRepo.Setup(r => r.GetAllAsync(2, 10, It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync(new List<TransactionsDataEntity>().AsReadOnly());
        _txRepo.Setup(r => r.CountAllAsync(It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default))
            .ReturnsAsync(0);

        await _handler.Handle(new FindAllTransactionsQuery(null, null, null, null, null, 2, 10), default);

        _txRepo.Verify(r => r.GetAllAsync(2, 10,
            It.IsAny<Expression<Func<TransactionsDataEntity, bool>>>(), default), Times.Once);
    }
}
