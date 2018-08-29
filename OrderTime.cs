using System;

public class OrderTime : BaseEntity
{
    public Guid? ServiceOrderID { get; private set; }
    public ServiceOrder ServiceOrder { get; private set; }

    private OrderTime() {}

    public OrderTime(string display)
    {
        Display = display;
    }

    public string Display { get; private set; }
}