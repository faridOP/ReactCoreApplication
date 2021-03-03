using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using API.DTOs;
using API.Services;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;

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
        private readonly IConfiguration configuration;
        private readonly HttpClient httpClient;
        public AccountController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, TokenService tokenService, IConfiguration configuration)
        {
            this.configuration = configuration;
            _tokenService = tokenService;
            _userManager = userManager;
            _signInManager = signInManager;
            httpClient = new HttpClient{
            BaseAddress = new System.Uri("https://graph.facebook.com")
            };
        }

        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto loginDto)
        {
            var user = await _userManager.Users.Include(x => x.Photos).FirstOrDefaultAsync(x => x.Email == loginDto.Email);

            if (user == null) return Unauthorized();

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (result.Succeeded)
            {
                return ReturnUserObject(user);

            }

            return Unauthorized();
        }

        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDTO registerDTO)
        {
            if (await _userManager.Users.AnyAsync(u => u.Email == registerDTO.Email))
            {
                ModelState.AddModelError("email", "Email is already taken");

                return ValidationProblem();

            };
            if (await _userManager.Users.AnyAsync(u => u.UserName == registerDTO.Username))
            {

                ModelState.AddModelError("username", "Username is already taken");

                return ValidationProblem();

            };

            var user = new AppUser
            {
                DisplayName = registerDTO.DisplayName,
                Email = registerDTO.Email,
                UserName = registerDTO.Username
            };

            var result = await _userManager.CreateAsync(user, registerDTO.Password);

            if (result.Succeeded)
            {
                return ReturnUserObject(user);
            }

            return Unauthorized();

        }
        [HttpGet]
        public async Task<ActionResult<UserDto>> GetCurrentUser()
        {
            var user = await _userManager.Users.Include(x => x.Photos).FirstOrDefaultAsync(x => x.Id == User.FindFirstValue(ClaimTypes.NameIdentifier));

            return ReturnUserObject(user);

        }

        [HttpPost("fbLogin")]
        public async Task<ActionResult<UserDto>> FacebookLogin(string accessToken)
        {
            var fbVerifyKeys = configuration["Facebook:AppId"]+"|"+configuration["Facebook:AppSecret"];
            var verifyToken = await httpClient.GetAsync($"debug_token?input_token={accessToken}&access_token={fbVerifyKeys}" );

            if(!verifyToken.IsSuccessStatusCode) return Unauthorized();

            var fbUrl = $"me?access_token={accessToken}&fields=name,email,picture.width(100).height(100)";

            var response = await httpClient.GetAsync(fbUrl);

            if(!response.IsSuccessStatusCode) return Unauthorized();

            var content = await response.Content.ReadAsStringAsync();

            var fbInfo = JsonConvert.DeserializeObject<dynamic>(content);

            var username = (string)fbInfo.id;

            var user = await _userManager.Users.Include(x=>x.Photos).FirstOrDefaultAsync(x=>x.UserName==username);

            if(user!=null) return ReturnUserObject(user);

            user = new AppUser
            {
                DisplayName = (string)fbInfo.name,
                UserName = (string)fbInfo.id,
                Email =(string)fbInfo.email,
                Photos = new List<Photo>{
                    new Photo{
                        IsMain= true,
                        Id = $"fb_{(string)fbInfo.id}",
                        Url = (string)fbInfo.picture.data.url
                    }
                }
            };
            
            var result = await _userManager.CreateAsync(user);

            if(!result.Succeeded)  return BadRequest();

            return ReturnUserObject(user);

            
        }

        private UserDto ReturnUserObject(AppUser user)
        {
            return new UserDto
            {
                DisplayName = user.DisplayName,
                Image = user?.Photos?.FirstOrDefault(x => x.IsMain)?.Url, //? for in case we don't have any photos for the user.
                Token = _tokenService.CreateToken(user),
                Username = user.UserName
            };
        }

    }
}