using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.DTOs;
using API.Services;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [AllowAnonymous]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly TokenService _tokenService;

        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, TokenService tokenService)
        {
           _tokenService = tokenService;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users.Include(x=>x.Photos).FirstOrDefaultAsync(x=>x.Email==loginDto.Email);

            if (user == null) return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (result.Succeeded)
            {
                return  ReturnUserObject(user);

            }

            return Unauthorized();
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDTO registerDTO)
        {
            if(await _userManager.Users.AnyAsync(u=>u.Email==registerDTO.Email)) {
                ModelState.AddModelError("email","Email is already taken");

                return ValidationProblem();
                
            };
            if(await _userManager.Users.AnyAsync(u=>u.UserName==registerDTO.Username))
            {

                ModelState.AddModelError("username","Username is already taken");
                
                return ValidationProblem();
                 
            };

            var user = new AppUser{
                DisplayName= registerDTO.DisplayName,
                Email = registerDTO.Email,
                UserName= registerDTO.Username
            };

            var result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (result.Succeeded)
            {
                return  ReturnUserObject(user);
            }

            return Unauthorized();
           
        }
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userManager.Users.Include(x=>x.Photos).FirstOrDefaultAsync(x=>x.Id==User.FindFirstValue(ClaimTypes.NameIdentifier));

            return ReturnUserObject(user);

        }

        private UserDto ReturnUserObject(AppUser user)
        {
            return new UserDto
            {
                DisplayName = user.DisplayName,
                Image = user?.Photos?.FirstOrDefault(x=>x.IsMain)?.Url, //? for in case we don't have any photos for the user.
                Token = _tokenService.CreateToken(user),
                Username = user.UserName
            };
        }

    }
}