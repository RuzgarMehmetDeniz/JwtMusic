using FluentValidation;
using JwtMusic.WebApi.Dtos.ArtistDtos;

namespace JwtMusic.WebApi.Validation.ArtistValidation
{
    public class ArtistCreateValidator : AbstractValidator<CreateArtistDto>
    {
        public ArtistCreateValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Sanatçı adı zorunludur.")
                .Length(2, 100).WithMessage("Sanatçı adı 2-100 karakter arasında olmalıdır.");

            RuleFor(x => x.Bio)
                .MaximumLength(500).WithMessage("Biyografi en fazla 500 karakter olabilir.");

            RuleFor(x => x.ImageUrl)
                .NotEmpty().WithMessage("Görsel URL'si zorunludur.")
                .Must(BeAValidUrl).WithMessage("Geçerli bir URL giriniz.")
                .When(x => !string.IsNullOrEmpty(x.ImageUrl));

            RuleFor(x => x.MonthlyListeners)
                .GreaterThanOrEqualTo(0).WithMessage("Aylık dinleyici sayısı negatif olamaz.");

            RuleFor(x => x.RequiredRole)
                .NotEmpty().WithMessage("Gerekli rol alanı zorunludur.");
        }

        private static bool BeAValidUrl(string? url)
        {
            return Uri.TryCreate(url, UriKind.Absolute, out _);
        }
    }
}