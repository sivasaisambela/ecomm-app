using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Application.DTOs
{
    public class UpdateOrderStatusDto
    {
        public string Status { get; set; } = string.Empty;
    }
}
