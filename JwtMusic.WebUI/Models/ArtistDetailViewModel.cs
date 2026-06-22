using JwtMusic.WebUI.Dtos;
using JwtMusic.WebUI.Dtos.SongDtos;

namespace JwtMusic.WebUI.Models
{
    public class ArtistDetailViewModel
    {
        public ResultArtistDto Artist { get; set; }
        public List<ResultSongDto> Songs { get; set; }
    }
}
