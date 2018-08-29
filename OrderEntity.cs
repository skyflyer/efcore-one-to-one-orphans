    public abstract class OrderEntity : BaseEntity
    {
        /// <summary>
        /// Unique order number
        /// </summary>
        public string OrderNumber { get; protected set; }
    }