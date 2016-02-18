using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace helpdesk.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Ф.И.О")]
        [MaxLength(50, ErrorMessage = "Превышена максимальная длина записи")]
        public string Name { get; set; }

        
        [Required]
        [Display(Name = "Логин")]
        [MaxLength(50, ErrorMessage = "Превышена максимальная длина записи")]
        public string Login { get; set; }

        [Required]
        [Display(Name = "Пароль")]
        [MaxLength(50, ErrorMessage = "Превышена максимальная длина записи")]
        public string Password { get; set; }

        [Display(Name = "Должность")]
        [MaxLength(50, ErrorMessage = "Превышена максимальная длина записи")]
        public string Position { get; set; }
        // Отдел
        [Display(Name = "Отдел")]
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }
        // Статус
        [Required]
        [Display(Name = "Статус")]
        public int RoleId { get; set; }
        public Role Role { get; set; }
    }
}