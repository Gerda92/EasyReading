using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace MvcApplication3.Models
{
    public class ChapterBinding
    {
        [Key]
        public int Id { get; set; }
        public Chapter ChapterOne { get; set; }
        public Chapter ChapterTwo { get; set; }
    }
}