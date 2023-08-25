﻿using System.ComponentModel.DataAnnotations;

namespace LoginProject.Models.DTO
{
    public class ResetPasswordDTO
    {
        [Required]
        public string Password { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        public string OldPassword { get; set; }
    }
}
