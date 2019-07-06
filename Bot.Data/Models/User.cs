using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Bot.Data.Models
{
    public class User
    {
        public ulong UserId { get; set; }
        public string TimezoneId { get; set; }

    }
}
