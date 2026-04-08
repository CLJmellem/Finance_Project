using Auth.Application.DTOs.Token;
using Auth.Domain.Command.Token;
using Auth.Domain.Entities;
using AutoMapper;

namespace Auth.Application.Mappers
{
    /// <summary>
    /// TokenProfile
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class TokenProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TokenProfile"/> class.
        /// </summary>
        public TokenProfile()
        {
            CreateMap<TokenCommandResponse,  TokenResponseDTO>();
        }
    }
}