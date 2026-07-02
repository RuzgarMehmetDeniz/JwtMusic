using FluentValidation;
using JwtMusic.WebApi.Dtos.SongDtos;

namespace JwtMusic.WebApi.Validation.SongValidation
{
    public class CreateSongDtoValidator : AbstractValidator<CreateSongDto>
    {
        public CreateSongDtoValidator()
        {
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Şarkı adı zorunludur.")
                .MaximumLength(150).WithMessage("Şarkı adı en fazla 150 karakter olabilir.");

            RuleFor(x => x.CoverImageUrl)
                .NotEmpty().WithMessage("Kapak görseli URL zorunludur.")
                .Must(BeAValidUrl).WithMessage("Kapak görseli için geçerli bir URL giriniz.");

            RuleFor(x => x.AudioUrl)
                .NotEmpty().WithMessage("Ses dosyası URL zorunludur.")
                .Must(BeAValidUrl).WithMessage("Ses dosyası için geçerli bir URL giriniz.");

            RuleFor(x => x.Duration)
                .NotEmpty().WithMessage("Şarkı süresi zorunludur.")
                .Matches(@"^([0-9]{2}):([0-5][0-9]):([0-5][0-9])$")
                .WithMessage("Süre formatı 'SS:DD:SS' şeklinde olmalıdır. Örn: 00:03:24");

            RuleFor(x => x.ReleaseDate)
                .NotEmpty().WithMessage("Yayın tarihi zorunludur.")
                .LessThanOrEqualTo(DateTime.Now.AddDays(1))
                .WithMessage("Yayın tarihi gelecekte bir tarih olamaz.");

            RuleFor(x => x.ArtistId)
                .GreaterThan(0).WithMessage("Geçerli bir sanatçı seçmelisiniz.");

            RuleFor(x => x.RequiredRoleId)
                .NotEmpty().WithMessage("Gerekli rol seçilmelidir.");
        }

        private static bool BeAValidUrl(string url)
        {
            return !string.IsNullOrWhiteSpace(url)
                && Uri.TryCreate(url, UriKind.Absolute, out var result)
                && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
        }
    }
}
