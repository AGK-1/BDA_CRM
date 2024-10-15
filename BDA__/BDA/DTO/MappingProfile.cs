using AutoMapper;
using BDA.Models;

public class MappingProfile : Profile
{
	public MappingProfile()
	{
		// Map Customers to CustomerDTO
		CreateMap<Customers, CustomerDTO>()
			.ForMember(dest => dest.CreatedByUser, opt => opt.MapFrom(src => src.CreatedByUser));

		// Map User to UserDTO
		CreateMap<User, UserDTO>();
	}
}
