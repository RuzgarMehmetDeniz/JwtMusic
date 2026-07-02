namespace JwtMusic.WebUI.Dtos.AccountDtos
{
    public class UpdateAccountDto
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string? ImageUrl { get; set; }
        public List<string> Roles { get; set; } = new List<string>();
    }
}
