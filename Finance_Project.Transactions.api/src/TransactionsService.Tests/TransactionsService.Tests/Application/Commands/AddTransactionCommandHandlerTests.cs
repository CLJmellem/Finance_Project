using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using TransactionsService.Application.Commands.AddTransaction;
using TransactionsService.Application.DTOs;
using TransactionsService.Application.Exceptions;
using TransactionsService.Application.Mappers;

namespace TransactionsService.Tests.Application.Commands;

/// <summary>
/// Unit tests for AddTransactionCommandHandler.
/// </summary>
public class AddTransactionCommandHandlerTests
{
    private readonly Mock<ITransactionRepository> _txRepo;
    private readonly Mock<ICardRepository> _cardRepo;
    private readonly Mock<ICurrentUserService> _currentUser;
    private readonly IMapper _mapper;
    private readonly AddTransactionCommandHandler _handler;

    private const string UserId = "507f1f77bcf86cd799439011";
    private const string CardId = "507f1f77bcf86cd799439022";

    public AddTransactionCommandHandlerTests()
    {
        _txRepo = new Mock<ITransactionRepository>();
        _cardRepo = new Mock<ICardRepository>();
        _currentUser = new Mock<ICurrentUserService>();
        _mapper = new ServiceCollection()
            .AddLogging()
            .AddAutoMapper(cfg => cfg.AddProfile<TransactionProfile>())
            .BuildServiceProvider()
            .GetRequiredService<IMapper>();

        _handler = new AddTransactionCommandHandler(_txRepo.Object, _cardRepo.Object, _currentUser.Object, _mapper);
    }

    private static CardDataEntity ActiveCard(double creditLimit = 1000) =>
        new()
        {
            Id = ObjectId.Parse(CardId),
            UserId = ObjectId.Parse(UserId),
            Name = "Nubank",
            LastFourDigits = "1234",
            CreditLimit = (decimal)creditLimit,
            DueDay = 10,
            IsActive = true
        };

    private static AddTransactionCommand ValidCommand(double total = 200, string? userId = null) =>
        new(CardId, total, null, null, TransactionType.Groceries, null, false, userId);

    [Fact]
    public async Task Handle_ValidCommand_CreatesTransactionAndReturnsResponse()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard());
        _txRepo.Setup(r => r.GetUsedCreditAsync(CardId, default)).ReturnsAsync(0);

        var result = await _handler.Handle(ValidCommand(), default);

        result.Should().NotBeNull();
        result.CardId.Should().Be(CardId);
        result.TotalAmount.Should().Be(200);
        _txRepo.Verify(r => r.CreateAsync(It.IsAny<TransactionsDataEntity>(), default), Times.Once);
    }

    [Fact]
    public async Task Handle_AdminWithUserId_UsesProvidedUserId()
    {
        _currentUser.Setup(u => u.IsAdmin).Returns(true);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard());
        _txRepo.Setup(r => r.GetUsedCreditAsync(CardId, default)).ReturnsAsync(0);

        var result = await _handler.Handle(ValidCommand(userId: UserId), default);

        result.UserId.Should().Be(UserId);
    }

    [Fact]
    public async Task Handle_AdminWithoutUserId_UsesTokenUserId()
    {
        _currentUser.Setup(u => u.IsAdmin).Returns(true);
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard());
        _txRepo.Setup(r => r.GetUsedCreditAsync(CardId, default)).ReturnsAsync(0);

        var result = await _handler.Handle(ValidCommand(), default);

        result.UserId.Should().Be(UserId);
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
    public async Task Handle_TotalAmountExceedsCreditLimit_ThrowsDomainException()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard(creditLimit: 100));

        var act = async () => await _handler.Handle(ValidCommand(total: 200), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*credit limit*");
    }

    [Fact]
    public async Task Handle_InsufficientAvailableCredit_ThrowsDomainException()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard(creditLimit: 1000));
        // 800 already used, only 200 available — trying to spend 300
        _txRepo.Setup(r => r.GetUsedCreditAsync(CardId, default)).ReturnsAsync(800);

        var act = async () => await _handler.Handle(ValidCommand(total: 300), default);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Insufficient credit*");
    }

    [Fact]
    public async Task Handle_InstallmentTransaction_SetsActualInstallmentsEqualToInstallments()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard());
        _txRepo.Setup(r => r.GetUsedCreditAsync(CardId, default)).ReturnsAsync(0);

        var command = new AddTransactionCommand(CardId, 400, 100, 4, TransactionType.OnlineShopping, null, false, null);

        TransactionsDataEntity? saved = null;
        _txRepo.Setup(r => r.CreateAsync(It.IsAny<TransactionsDataEntity>(), default))
            .Callback<TransactionsDataEntity, CancellationToken>((e, _) => saved = e);

        await _handler.Handle(command, default);

        saved.Should().NotBeNull();
        saved!.ActualInstallments.Should().Be(4);
        saved.Installments.Should().Be(4);
    }

    [Fact]
    public async Task Handle_NonInstallmentTransaction_ActualInstallmentsIsNull()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard());
        _txRepo.Setup(r => r.GetUsedCreditAsync(CardId, default)).ReturnsAsync(0);

        TransactionsDataEntity? saved = null;
        _txRepo.Setup(r => r.CreateAsync(It.IsAny<TransactionsDataEntity>(), default))
            .Callback<TransactionsDataEntity, CancellationToken>((e, _) => saved = e);

        await _handler.Handle(ValidCommand(), default);

        saved!.ActualInstallments.Should().BeNull();
    }

    [Fact]
    public async Task Handle_CardDueDateIsNextMonth()
    {
        _currentUser.Setup(u => u.UserId).Returns(UserId);
        _cardRepo.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), default))
            .ReturnsAsync(ActiveCard());
        _txRepo.Setup(r => r.GetUsedCreditAsync(CardId, default)).ReturnsAsync(0);

        TransactionsDataEntity? saved = null;
        _txRepo.Setup(r => r.CreateAsync(It.IsAny<TransactionsDataEntity>(), default))
            .Callback<TransactionsDataEntity, CancellationToken>((e, _) => saved = e);

        await _handler.Handle(ValidCommand(), default);

        var expectedMonth = DateTime.UtcNow.AddMonths(1).Month;
        saved!.CardDueDate.Month.Should().Be(expectedMonth);
    }
}
