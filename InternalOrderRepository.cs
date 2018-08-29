using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using eftest;

public class InternalOrderRepository
{
    static int Counter=1;

    private readonly EfTestDbContext _context;

    public InternalOrderRepository(EfTestDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Retrieves all internal orders
    /// </summary>
    /// <returns>A collection of internal orders</returns>
    public IQueryable<InternalOrder> Get()
    {
        return _context.InternalOrders;
    }

    /// <summary>
    /// Checks whether the order exists
    /// </summary>
    /// <param name="id">The ID of the order to be checked</param>
    /// <returns>Whether the order with the specified ID exists</returns>
    public bool Exists(Guid id)
    {
        return _context.InternalOrders.SingleOrDefault(io => io.ID == id) != null;
    }

    /// <summary>
    /// Thetrieves an internal order by its ID
    /// </summary>
    /// <param name="id">The ID of the order to be retrieved</param>
    /// <param name="timestamp">The timestamp to be matched with the retrieved entry</param>
    /// <returns>The internal order corresponding to the provided ID</returns>
    public InternalOrder Get(Guid id, ulong? timestamp = null)
    {
        var result = _context.InternalOrders.Include(io => io.OrderTime)
                                            .Single(io => io.ID == id);

        return result;
    }

    /// <summary>
    /// Creates or updates an existing internal order
    /// </summary>
    /// <param name="order">The updated order to be saved</param>
    /// <param name="orderConflictHandler">Handler for getting order conflict notifications. First parameter is original order, second parameter is the new order</param>
    public async Task Update(InternalOrder order, Action<InternalOrder, InternalOrder> orderConflictHandler = null)
    {
        // detach if already attached
        var entry = _context.Entry(order);
        if (entry.State != EntityState.Detached) {
            entry.State = EntityState.Detached;
        }

        // check if another entity is attached
        var existingTrackedEntities = _context.ChangeTracker.Entries<InternalOrder>().Where(x => x.Entity.ID == order.ID).ToList();
        foreach(var e in existingTrackedEntities) {
            e.State = EntityState.Detached;
        }


        // find original order
        var originalOrder = await _context.InternalOrders
            .AsNoTracking()
            .Include(io => io.OrderTime)
            .SingleOrDefaultAsync(x => x.ID == order.ID);

        if(originalOrder == null) {
            // only order is in the added state
            _context.Attach(order);
            _context.Entry(order).State = EntityState.Added;
            
            if (order.OrderTime != null) {
                _context.Entry(order.OrderTime).State = EntityState.Added;
            }
            return;
        }


        _context.Attach(order);
        _context.Entry(order).State = EntityState.Modified;
        if(originalOrder.OrderTime != null) {
            // remove & add or update
            if (order.OrderTime == null) {
                var trackedOrderTime = _context.ChangeTracker.Entries<OrderTime>().Where(x => x.Entity.ID == originalOrder.OrderTimeID).SingleOrDefault();
                if (trackedOrderTime != null) {
                    trackedOrderTime.State = EntityState.Deleted;
                } else {
                    _context.Remove(originalOrder.OrderTime);
                }
            } else {
                // new or updated OrderTime
                if (order.OrderTime.ID == originalOrder.OrderTime.ID) {
                    // modify
                    _context.Entry(order.OrderTime).State = EntityState.Modified;
                } else {
                    var trackedOrderTime = _context.ChangeTracker.Entries<OrderTime>().Where(x => x.Entity.ID == originalOrder.OrderTimeID).SingleOrDefault();
                    if (trackedOrderTime != null) {
                        trackedOrderTime.State = EntityState.Deleted;
                    } else {
                        _context.Remove(originalOrder.OrderTime);
                    }
                    _context.Entry(order.OrderTime).State = EntityState.Added;
                }
            }
        }

    }
    
    /// <summary>
    /// Saves all changes done to the data asynchronously
    /// </summary>
    /// <returns>Number of updated rows in the DB</returns>
    public Task<int> SaveAsync()
    {
        return _context.SaveChangesAsync();
    }

    /// <summary>
    /// Get next order number
    /// </summary>
    public Task<int> NextOrderNumber()
    {
        return Task.FromResult(InternalOrderRepository.Counter++);
    }
}
