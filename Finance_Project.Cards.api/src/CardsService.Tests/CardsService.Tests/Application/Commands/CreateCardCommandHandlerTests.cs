using AutoMapper;
using CardsService.Application.Commands.CreateCard;
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
/// Testes unitários para CreateCardCommandHandler.
/// </summary>
public class CreateCardCommandHandlerTests
{
    private readonly Mock<ICardRepository> _repositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserMock;
    private readonly IMapper _mapper;
    private readonly CreateCardCommandHandler _handler;

    public CreateCardCommandHandlerTests()
    {
        _repositoryMock = new Mock<ICardRepository>();
        _currentUserMock = new Mock<ICurrentUserService>();
        _mapper = new ServiceCollection().AddLogging().AddAutoMapper(cfg => cfg.AddProfile<CardProfile>()).BuildServiceProvider().GetRequiredService<IMapper>();
        _handler = new CreateCardCommandHandler(_repositoryMock.Object, _currentUserMock.Object, _mapper);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesCardAndReturnsResponse()
    {
        _currentUserMock.Setup(u => u.UserId).Returns("user-abc");
        _repositoryMock.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync((CardDataEntity?)null);

        var command = new CreateCardCommand("Nubank", CardBrand.Mastercard, "4321", 3000m, 10);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.UserId.Should().Be("user-abc");
        result.Name.Should().Be("Nubank");
        result.Brand.Should().Be("Mastercard");
        result.LastFourDigits.Should().Be("4321");
        result.CreditLimit.Should().Be(3000m);
        result.DueDay.Should().Be(10);
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_UsesUserIdFromCurrentUserService_NotFromCommand()
    {
        _currentUserMock.Setup(u => u.UserId).Returns("jwt-user-id");
        _repositoryMock.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync((CardDataEntity?)null);

        var command = new CreateCardCommand("Cartão", CardBrand.Visa, "1111", 0m, 1);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.UserId.Should().Be("jwt-user-id");
        _currentUserMock.Verify(u => u.UserId, Times.AtLeastOnce);
    }

    [Fact]
    public async Task Handle_CallsRepositoryCreateAsync()
    {
        _currentUserMock.Setup(u => u.UserId).Returns("user-xyz");
        _repositoryMock.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync((CardDataEntity?)null);

        var command = new CreateCardCommand("Bradesco", CardBrand.Visa, "5555", 10000m, 5);

        await _handler.Handle(command, CancellationToken.None);

        _repositoryMock.Verify(
            r => r.CreateAsync(It.IsAny<CardDataEntity>(), CancellationToken.None),
            Times.Once);
    }

    [Fact]
    public async Task Handle_ReturnsCardResponseWithCorrectId()
    {
        _currentUserMock.Setup(u => u.UserId).Returns("user-id");
        _repositoryMock.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync((CardDataEntity?)null);

        var command = new CreateCardCommand("Itaú", CardBrand.Elo, "7890", 2000m, 20);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Id.Should().NotBeNullOrEmpty();
        result.Id.Should().HaveLength(24);
    }

    [Fact]
    public async Task Handle_WithAllBrands_Succeeds()
    {
        _currentUserMock.Setup(u => u.UserId).Returns("user-brand-test");
        _repositoryMock.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync((CardDataEntity?)null);

        foreach (var brand in Enum.GetValues<CardBrand>())
        {
            var command = new CreateCardCommand($"Cartão {brand}", brand, "1234", 500m, 15);
            var result = await _handler.Handle(command, CancellationToken.None);
            result.Brand.Should().Be(brand.ToString());
        }
    }

    [Fact]
    public async Task Handle_DuplicateName_ThrowsDomainException()
    {
        _currentUserMock.Setup(u => u.UserId).Returns("user-abc");
        _repositoryMock.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(new CardDataEntity { Name = "Nubank" });

        var command = new CreateCardCommand("Nubank", CardBrand.Mastercard, "4321", 3000m, 10);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>().WithMessage("*Nubank*");
    }

    [Fact]
    public async Task Handle_DuplicateName_DoesNotCallCreateAsync()
    {
        _currentUserMock.Setup(u => u.UserId).Returns("user-abc");
        _repositoryMock.Setup(r => r.GetOneAsync(It.IsAny<Expression<Func<CardDataEntity, bool>>>(), CancellationToken.None))
            .ReturnsAsync(new CardDataEntity { Name = "Nubank" });

        var command = new CreateCardCommand("Nubank", CardBrand.Mastercard, "4321", 3000m, 10);

        var act = async () => await _handler.Handle(command, CancellationToken.None);

        await act.Should().ThrowAsync<DomainException>();
        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<CardDataEntity>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
