﻿using System.ComponentModel.DataAnnotations;

namespace IeltsTestWeb.RequestModels
{
    public class LoginRequestModel
    {
        [Required(ErrorMessage = "Email is required.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        public string Password { get; set; } = null!;
    }
}
