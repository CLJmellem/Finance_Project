using Auth.Application.DTOs.Login;
using Auth.Domain.Command.Login;
using AutoMapper;

namespace Auth.Application.Mappers
{
    /// <summary>
    /// LoginUserProfile
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class LoginUserProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoginUserProfile"/> class.
        /// </summary>
        public LoginUserProfile()
        {
            CreateMap<LoginUserCommandResponse, LoginResponseDTO>();
        }
    }
}