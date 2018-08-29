using System;

public abstract class BaseEntity
    {
        /// <summary>
        /// The ID of the entity
        /// </summary>
        public Guid ID { get; protected set; }
        public byte[] Timestamp { get; set; }
    }