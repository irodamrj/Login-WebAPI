using LoginProject.Models.DTO;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using MimeKit.Text;
using MimeKit;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System;
using System.Numerics;
using static Org.BouncyCastle.Asn1.Cmp.Challenge;
using MailKit.Security;
using System.Net.Http.Headers;

namespace LoginProject.Controllers
{
	[Route("api/auth")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly UserContext _context;
		private readonly IConfiguration _configuration;
		public AuthController(UserContext context, IConfiguration configuration)
		{
			_context = context;
			_configuration = configuration;
		}


		[HttpPost("login")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status404NotFound)]
		public async Task<ActionResult<UserDTO>> Login([FromBody] UserDTO userDTO)
		{
			if (userDTO == null)
				return BadRequest(userDTO);
			User? user = await _context.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Email == userDTO.Email);
			if (user == null)
			{
				return NotFound("cannot found");
			}
			if (user.IsVerified == "False" || user.IsVerified == "True")
			{
				return BadRequest("Please verify your acount before loggin in");
			}
			bool isvalid = BCrypt.Net.BCrypt.Verify(userDTO.Password, user.Password);
			if (isvalid)
			{
				return Ok("Logged in successfully");
			}

			else
				return BadRequest("invalid credentials");
		}

		[HttpPost("signup")]
		[ProducesResponseType(StatusCodes.Status200OK)]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		public async Task<ActionResult<User>> Signup([FromBody] SignupDTO user)
		{
			bool isExist = await _context.Users.AsNoTracking().AnyAsync(u => u.Email == user.Email);
			if (isExist)
			{
				return BadRequest($"email {user.Email} already exist.");
			}
			string hashedPassword = BCrypt.Net.BCrypt.HashPassword(user.Password, 10);
			string token = GenerateToken();


			User newUser = new User()
			{
				Email = user.Email,
				Password = hashedPassword,
				Name = user.Name,
				LastName = user.LastName,
				Phone = user.Phone,
				IsVerified = "False",
				VerificationToken = token,
			};

			try
			{
				//send mail
				var email = new MimeMessage();
				email.From.Add(MailboxAddress.Parse(_configuration.GetSection("Yandex").GetSection("Username").Value));
				email.To.Add(MailboxAddress.Parse(user.Email));
				email.Subject = "Verification Email";
				email.Body = new TextPart(TextFormat.Html) { Text = $"http://localhost:5188/api/Auth/verify?token={token}" };
				using var smtp = new MailKit.Net.Smtp.SmtpClient();
				smtp.ServerCertificateValidationCallback = (s, c, h, e) => true;
				smtp.AuthenticationMechanisms.Remove("XOAUTH2");
				await smtp.ConnectAsync("smtp.yandex.com", 465, useSsl: true);
				await smtp.AuthenticateAsync(_configuration.GetSection("Yandex").GetSection("Username").Value, _configuration.GetSection("Yandex").GetSection("Password").Value);
				await smtp.SendAsync(email);
				await smtp.DisconnectAsync(true);

			}
			catch (Exception e)
			{

				return BadRequest(e);
			}
			await _context.Users.AddAsync(newUser);
			await _context.SaveChangesAsync();

			return Ok($"User created with email {user.Email} with token {token}");

		}


		[HttpGet("verify")]
		public async Task<ActionResult<User>> GetVerify([FromQuery(Name = "token")] string token)
		{
			var user = await _context.Users.FirstOrDefaultAsync(u => u.VerificationToken == token);
			if (user == null)
				return BadRequest("Wrong token");
			if (user.IsVerified == "False")
				user.IsVerified = "True";
			else if (user.IsVerified == "True")
				user.IsVerified = "True-True";
			await _context.SaveChangesAsync();
			return Ok("Mail verified successfully");

		}




		[HttpPost("resetpassword")]
		[ProducesResponseType(StatusCodes.Status400BadRequest)]
		[ProducesResponseType(StatusCodes.Status200OK)]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO resetDTO)
		{
			var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == resetDTO.Email);
			if (user == null)
			{
				return BadRequest("No user with given email");
			}
			bool isValid = BCrypt.Net.BCrypt.Verify(resetDTO.OldPassword, user.Password);
			if (!isValid)
			{
				return BadRequest("Invalid credentials");
			}
			user.Password = BCrypt.Net.BCrypt.HashPassword(resetDTO.Password, 10);
			await _context.SaveChangesAsync();
			return Ok("Password changed");
		}




		private string GenerateToken()
		{
			{
				const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
				Random random = new Random();

				char[] stringChars = new char[10];
				for (int i = 0; i < stringChars.Length; i++)
				{
					stringChars[i] = chars[random.Next(chars.Length)];
				}

				return new string(stringChars);
			}
		}




	}






}
