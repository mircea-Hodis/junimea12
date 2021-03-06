﻿using System.ComponentModel.DataAnnotations;

namespace DataModelLayer.Models.Entities
{
    public class AppAdministrationRoles
    {
        [Key]
        public int RoleId { get; set; }
        public string RoleName { get; set; }
    }

    public class UserAdministrationRoles
    {
        [Key]
        public int RoleId { get; set; }
        public string UserId { get; set; }
    }
}