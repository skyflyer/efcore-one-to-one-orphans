using System;

public class InternalOrder : OrderEntity
{
    public Guid? OrderTimeID { get; private set; }
    public OrderTime OrderTime { get; private set; }

    private InternalOrder() {}

    public InternalOrder(string orderNum)
    {
        OrderNumber = orderNum;
    }

    public void SetTime(OrderTime ot) {
        OrderTime = ot;
    }

    public void RemoveTime() {
        OrderTime = null;
    }
}