using System;
using System.Collections.Generic;

namespace TicketsBooking.Core.Entities
{
    public class Trip
    {
        public Guid Id { get; set; }
        public string VehicleNumber { get; set; } = string.Empty; // Ex: Número do ônibus ou placa
        public string DeparturePlace { get; set; } = string.Empty; // Origem
        public string ArrivalPlace { get; set; } = string.Empty;   // Destino
        public DateTime DepartureTime { get; set; }                // Data/Hora de Saída
        public DateTime ArrivalTime { get; set; }                  // Data/Hora de Chegada
        public decimal Price { get; set; }                         // Preço base da passagem

        // Propriedades de Navegação
        // Uma viagem tem muitos assentos e muitas reservas
        public ICollection<Seat> Seats { get; set; } = new List<Seat>();
        public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    }
}