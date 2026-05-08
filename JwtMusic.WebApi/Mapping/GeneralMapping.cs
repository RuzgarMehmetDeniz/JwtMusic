using AutoMapper;
using JwtMusic.WebApi.Dtos;
using JwtMusic.WebApi.Entities;

namespace JwtMusic.WebApi.Mapping
{
    public class GeneralMapping:Profile
    {
        public GeneralMapping()
        {
            CreateMap<Artist,ResultArtistDto>().ReverseMap();
            CreateMap<Artist, CreateArtistDto>().ReverseMap();
        }
    }
}
