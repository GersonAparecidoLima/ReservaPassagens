using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketsBooking.Application.DTOs
{
    internal class UpdateBookingDto
    {
        public string PassengerName { get; set; } = string.Empty;
        public string PassengerDocument { get; set; } = string.Empty;
    }
}
