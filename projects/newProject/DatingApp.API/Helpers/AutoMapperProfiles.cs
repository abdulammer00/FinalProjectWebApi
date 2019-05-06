using AutoMapper;
using DatingApp.API.Dtos;
using DatingApp.API.Models;
using System.Linq;

namespace DatingApp.API.Helpers
{
    /// <summary>
    /// This will tell AutoMapper about the mappings that we need to support
    /// because AutoMapper needs to understand the source and destination of what it is
    /// mapping.
    /// In order to use the methods, we need to inherit from the Profile class
    /// </summary>
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
          
            CreateMap<User, UserForListDto>()
               
                .ForMember(destination => destination.PhotoUrl, options => {
                    options.MapFrom(source => 
                    source.Photos.FirstOrDefault(f => f.IsMain).Url);
                })
              
                .ForMember(destination => destination.Age, options => {
                    options.ResolveUsing(r => r.DateOfBirth.CalculateAge());
                });
            CreateMap<User, UserForDetailedDto>()
                .ForMember(destination => destination.PhotoUrl, options => {
                    options.MapFrom(source =>
                    source.Photos.FirstOrDefault(f => f.IsMain).Url);
                })
                .ForMember(destination => destination.Age, options => {
                    options.ResolveUsing(r => r.DateOfBirth.CalculateAge());
                });
            CreateMap<Photo, PhotoForDetailedDto>();
            CreateMap<UserForUpdateDto, User>();
            CreateMap<Photo, PhotoForReturnDto>();
            CreateMap<PhotoForCreationDto, Photo>();
            CreateMap<UserForRegisterDto, User>();

           
            CreateMap<MessageForCreationDto, Message>().ReverseMap();

            CreateMap<Message, MessageToReturnDto>()
                .ForMember(destination => destination.SenderPhotoUrl, options => {
                    options.MapFrom(source => source.Sender.Photos.FirstOrDefault(p => p.IsMain).Url);
                })
                .ForMember(destination => destination.RecipientPhotoUrl, options => {
                    options.MapFrom(source => source.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url);
                });
        }
    }
}
