using AutoMapper;
using CardsService.Application.DTOs;
using CardsService.Domain.Entities;

namespace CardsService.Application.Mappers;

/// <summary>
/// CardProfile
/// </summary>
/// <seealso cref="AutoMapper.Profile" />
public class CardProfile : Profile
{
    public CardProfile()
    {
        CreateMap<CardDataEntity, CardResponse>()
            .ForMember(dest => dest.Brand, opt => opt.MapFrom(src => src.Brand.ToString()));
    }
}
