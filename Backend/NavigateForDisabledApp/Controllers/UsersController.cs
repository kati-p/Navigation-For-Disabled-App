using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using NavigateForDisabledApp.Models;
using System.Security.Cryptography;

namespace NavigateForDisabledApp.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly ILogger<TokensController> _logger;

    public UsersController(ILogger<TokensController> logger)
    {
        _logger = logger;
    }

    [HttpPost]
    [Route("")]
    public IActionResult Post([FromBody] DTOs.Login data)
    {

        var db = new NavigateSoftwareDbContext();

        var hasUser = db.Users.Where(u => u.Username == data.Username).FirstOrDefault();
        if (hasUser != null) return BadRequest("Already have this username");

        // generate a 128-bit salt using a secure PRNG
        byte[] randomSalt = new byte[128 / 8];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomSalt);
        }
        var salt = Convert.ToBase64String(randomSalt);

        string hash = Convert.ToBase64String(
            KeyDerivation.Pbkdf2(
                password: data.Password,
                salt: Convert.FromBase64String(salt),
                prf: KeyDerivationPrf.HMACSHA1,
                iterationCount: 10000,
                numBytesRequested: 256 / 8
            )
        );

        var user = new Models.User
        {
            Username = data.Username,
            Password = hash,
            Salt = salt
        };

        db.Users.Add(user);
        db.SaveChanges();

        return Ok(user.Id);
    }
}