using JwtMusic.WebApi.Dtos.AccountDtos;
using JwtMusic.WebApi.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace JwtMusic.WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;

        public UserController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        // Tum kullanicilari listele
        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = _userManager.Users.ToList();
            var result = new List<ResultAccountDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new ResultAccountDto
                {
                    Id = user.Id,
                    UserName = user.UserName ?? "",
                    Email = user.Email ?? "",
                    FirstName = user.Name ?? "",
                    LastName = user.Surname ?? "",
                    ImageUrl = user.ImageUrl,
                    Roles = roles.ToList()
                });
            }

            return Ok(result);
        }

        // ID'ye gore tekil kullanici getir
        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "Kullanici bulunamadi." });

            var roles = await _userManager.GetRolesAsync(user);

            var result = new ResultAccountDto
            {
                Id = user.Id,
                UserName = user.UserName ?? "",
                Email = user.Email ?? "",
                FirstName = user.Name ?? "",
                LastName = user.Surname ?? "",
                ImageUrl = user.ImageUrl,
                Roles = roles.ToList()
            };

            return Ok(result);
        }

        // Kullanici guncelle (profil bilgileri + roller)
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(string id, UpdateAccountDto dto)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "Kullanici bulunamadi." });

            user.Name = dto.Name;
            user.Surname = dto.Surname;
            user.UserName = dto.UserName;
            user.Email = dto.Email;
            user.ImageUrl = dto.ImageUrl;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
                return BadRequest(updateResult.Errors);

            // Rol senkronizasyonu: formda isaretli olmayan roller kaldirilir, yeni isaretlenenler eklenir.
            var currentRoles = await _userManager.GetRolesAsync(user);
            var incomingRoles = dto.Roles ?? new List<string>();

            var rolesToAdd = incomingRoles.Except(currentRoles).ToList();
            var rolesToRemove = currentRoles.Except(incomingRoles).ToList();

            if (rolesToAdd.Any())
            {
                var addResult = await _userManager.AddToRolesAsync(user, rolesToAdd);
                if (!addResult.Succeeded)
                    return BadRequest(addResult.Errors);
            }

            if (rolesToRemove.Any())
            {
                var removeResult = await _userManager.RemoveFromRolesAsync(user, rolesToRemove);
                if (!removeResult.Succeeded)
                    return BadRequest(removeResult.Errors);
            }

            return Ok("Kullanici guncellendi.");
        }

        // Kullanici sil
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "Kullanici bulunamadi." });

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok("Kullanici silindi.");
        }
    }
}