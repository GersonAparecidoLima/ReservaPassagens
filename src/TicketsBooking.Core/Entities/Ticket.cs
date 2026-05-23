namespace TicketsBooking.Core.Entities;

public class Ticket
{
    public Guid Id { get; private set; }
    public Guid EventId { get; private set; }
    public string Description { get; private set; } = string.Empty;
    public decimal Price { get; private set; }
    public int TotalQuantity { get; private set; }
    public int AvailableQuantity { get; private set; }
    public bool IsActive { get; private set; }

    // Construtor necessário para o ORM (EF Core)
    protected Ticket() { }

    // Construtor principal para criar novos ingressos garantindo regras de negócio
    public Ticket(Guid eventId, string description, decimal price, int totalQuantity)
    {
        Id = Guid.NewGuid();
        EventId = eventId;
        Description = description;
        Price = price;
        TotalQuantity = totalQuantity;
        AvailableQuantity = totalQuantity; // Começa com o total disponível
        IsActive = true;
    }

    // Exemplo de Regra de Negócio (Comportamento da Entidade)
    public void ReserveSeats(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("A quantidade a ser reservada deve ser maior que zero.");

        if (quantity > AvailableQuantity)
            throw new InvalidOperationException("Não há ingressos suficientes disponíveis para esta reserva.");

        AvailableQuantity -= quantity;
    }

    public void CancelReservation(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentException("A quantidade a ser cancelada deve ser maior que zero.");

        if (AvailableQuantity + quantity > TotalQuantity)
            throw new InvalidOperationException("A quantidade liberada excede o limite total do ingresso.");

        AvailableQuantity += quantity;
    }
}