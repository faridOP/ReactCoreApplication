using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using Application.Core;
using Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class CreateRoleViewModel{

        
        [Required]
        public string RoleName { get; set; }
    }

    public class AddToRoleModel{
        public string UserId { get; set; }
        public string RoleId { get; set; }
    }
    public class AdministrationController :BaseApiController
    {
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly UserManager<AppUser> userManager;

        public AdministrationController(RoleManager<IdentityRole> roleManager,UserManager<AppUser> userManager){
            this.roleManager = roleManager;
            this.userManager = userManager;
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> CreateRole(CreateRoleViewModel roleViewModel){

            IdentityRole identityRole = new IdentityRole{
                Name = roleViewModel.RoleName
            };

           var result = await roleManager.CreateAsync(identityRole);

            if(result.Succeeded) return Ok();


            return BadRequest();
        }

        [HttpPost("add")]
        [AllowAnonymous]
        public async Task<IActionResult> AddToRole(AddToRoleModel model)
        {

            if (model.UserId == null || model.RoleId == null) return NotFound();

            var role = await roleManager.FindByIdAsync(model.RoleId);
            var user = await userManager.FindByIdAsync(model.UserId );

            var result = await userManager.AddToRoleAsync(user,role.Name);

            if(result.Succeeded) return Ok(); 

            return BadRequest();
        }

    }
}