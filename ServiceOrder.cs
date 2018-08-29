using System.Collections.Generic;

public class ServiceOrder : OrderEntity
{
    private readonly List<OrderTime> _orderTimes = new List<OrderTime>();
    public IReadOnlyCollection<OrderTime> OrderTimes => _orderTimes;


    private ServiceOrder() {}

    public ServiceOrder(string orderNum)
    {
        OrderNumber = orderNum;
    }

    public void AddTime(OrderTime ot) {
        _orderTimes.Add(ot);
    }

    public void RemoveTime(OrderTime ot) {
        _orderTimes.Remove(ot);
    }
}