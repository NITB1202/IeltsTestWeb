﻿using System.ComponentModel.DataAnnotations;

namespace IeltsTestWeb.RequestModels
{
    public class QueryAccountRequestModel
    {
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string? Email { get; set; }
        public int? RoleId { get; set; }
        public Boolean? IsActive { get; set; }
    }
}
