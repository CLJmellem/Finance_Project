using AutoMapper;
using TransactionsService.Application.DTOs;
using TransactionsService.Domain.Entities;

namespace TransactionsService.Application.Mappers;

/// <summary>
/// AutoMapper profile for transaction entity to response DTO mapping.
/// </summary>
public class TransactionProfile : Profile
{
    public TransactionProfile()
    {
        CreateMap<TransactionsDataEntity, TransactionResponse>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId.ToString()))
            .ForMember(dest => dest.CardId, opt => opt.MapFrom(src => src.CardId.ToString()))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type.ToString()));
    }
}
