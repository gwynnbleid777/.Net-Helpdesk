using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace helpdesk.Models
{
    public class Request
    {
        public int Id { get; set; }

        [Required]
        [Display(Name = "Название заявки")]
        [MaxLength(50, ErrorMessage = "Превышена максимальная длина записи")]
        public string Name { get; set; }

        [Required]
        [Display(Name = "Описание")]
        [MaxLength(200, ErrorMessage = "Превышена максимальная длина записи")]
        public string Description { get; set; }

        [Display(Name = "Комментарий")]
        [MaxLength(200, ErrorMessage = "Превышена максимальная длина записи")]
        public string Comment { get; set; }

        [Display(Name = "Статус")]
        public int Status { get; set; }

        [Display(Name = "Приоритет")]
        public int Priority { get; set; }

        [Display(Name = "Кабинет")]
        public int? ActivId { get; set; }
        public Activ Activ { get; set; }

        [Display(Name = "Файл с ошибкой")]
        public string File { get; set; }

        // Внешний ключ Категория
        [Display(Name = "Категория")]
        public int? CategoryId { get; set; }
        public Category Category { get; set; }

        // Внешний ключ
        // ID Пользователя - обычное свойство
        public int? UserId { get; set; }
        // Отдел пользователя - Навигационное свойство
        public User User { get; set; }

        // Внешний ключ
        // ID Пользователя - обычное свойство
        public int? ExecutorId { get; set; }
        // Отдел пользователя - Навигационное свойство
        public User Executor { get; set; }

        // Внешний ключ
        // ID жизненного цикла заявки - обычное свойство
        public int LifecycleId { get; set; }
        // Ссылка на жизненный цикл заявки - Навигационное свойство
        public Lifecycle Lifecycle { get; set; }
    }

    public class Lifecycle
    {
        // ID 
        public int Id { get; set; }
        // Дата открытия
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy H:mm:ss}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? Opened { get; set; }
        // Дата распределения
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy H:mm:ss}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? Distributed { get; set; }
        // Дата обработки
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy H:mm:ss}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? Proccesing { get; set; }
        // Дата проверки
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy H:mm:ss}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? Checking { get; set; }
        // Дата закрытия
        [DisplayFormat(DataFormatString = "{0:dd/MM/yyyy H:mm:ss}", ApplyFormatInEditMode = true)]
        [DataType(DataType.Date)]
        public DateTime? Closed { get; set; }
    }

    // Перечисление для статуса заявки
    public enum RequestStatus
    {
        Open = 1,
        Distributed = 2,
        Proccesing = 3,
        Checking = 4,
        Closed = 5
    }
    // Перечисление для приоритета заявки
    public enum RequestPriority
    {
        Low = 1,
        Medium = 2,
        High = 3,
        Critical = 4
    }
}