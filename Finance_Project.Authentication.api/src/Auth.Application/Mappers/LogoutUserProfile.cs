using Auth.Application.DTOs.Logout;
using Auth.Domain.Command.Logout;
using AutoMapper;

namespace Auth.Application.Mappers
{
    /// <summary>
    /// LogoutUserProfile
    /// </summary>
    /// <seealso cref="AutoMapper.Profile" />
    public class LogoutUserProfile : Profile
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogoutUserProfile"/> class.
        /// </summary>
        public LogoutUserProfile()
        {
            CreateMap<LogoutUserCommandResponse, LogoutResponseDTO>();
        }
    }
}
