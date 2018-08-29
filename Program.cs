using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace eftest
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var ctx = new EfTestDbContext())
            {
                ctx.Database.EnsureDeleted();
                ctx.Database.EnsureCreated();
            }

            // RemoveOrderTimeFromInternalOrder(); // works
            // RemoveOrderTimeFromInternalOrderWithTracking(); // breaks - orphan OrderTime
            // ReplaceOrderTimeOnInternalOrderTrackedEntities(); // breaks - orphan OrderTime
            ReplaceOrderTimeOnInternalOrder(); // breaks - foreign key violation

            Console.ReadLine();
            Console.WriteLine($"Done");
        }

        public static async void RemoveOrderTimeFromInternalOrderWithTracking()
        {
            // create internal order
            InternalOrder internalOrder;
            using(var dbContext = new EfTestDbContext()) {
                var repo = new InternalOrderRepository(dbContext);
                var seq = await repo.NextOrderNumber();

                internalOrder = new InternalOrder("W-{seq}");

                internalOrder.SetTime(new OrderTime("Test"));
                await repo.Update(internalOrder);
                await repo.SaveAsync();

                Debug.Assert(internalOrder.ID != Guid.Empty);
                Debug.Assert(null != internalOrder.OrderTimeID);
            }

            // remove order time
            Guid previousOrderTimeId;
            using(var dbContext = new EfTestDbContext()) {
                var order = dbContext.InternalOrders.Include(x => x.OrderTime)
                    .Single(x => x.ID == internalOrder.ID);
                previousOrderTimeId = order.OrderTime.ID;

                order.RemoveTime();

                Console.WriteLine($"Previous order time id {previousOrderTimeId}");

                dbContext.SaveChanges();
            }

            // check if OrderTime was deleted
            using(var dbContext = new EfTestDbContext()) {
                var order = dbContext.InternalOrders
                    .Include(x => x.OrderTime)
                    .Single(x => x.ID == internalOrder.ID);
                
                Debug.Assert(false == dbContext.OrderTimes.Any(x => x.ID == previousOrderTimeId), "OrderTime is still present");
            }
        }

        public static async void RemoveOrderTimeFromInternalOrder()
        {
            // create internal order
            InternalOrder internalOrder;
            using(var dbContext = new EfTestDbContext()) {
                var repo = new InternalOrderRepository(dbContext);
                var seq = await repo.NextOrderNumber();

                internalOrder = new InternalOrder("W-{seq}");

                internalOrder.SetTime(new OrderTime("Test"));
                await repo.Update(internalOrder);
                await repo.SaveAsync();

                Debug.Assert(internalOrder.ID != Guid.Empty);
                Debug.Assert(null != internalOrder.OrderTimeID);
            }

            // remove order time
            Guid previousOrderTimeId;
            using(var dbContext = new EfTestDbContext()) {
                var repo = new InternalOrderRepository(dbContext);
                var order = repo.Get(internalOrder.ID);
                previousOrderTimeId = order.OrderTime.ID;

                order.RemoveTime();

                Console.WriteLine($"Previous order time id {previousOrderTimeId}");

                await repo.Update(order);
                await repo.SaveAsync();
            }

            // check if OrderTime was deleted
            using(var dbContext = new EfTestDbContext()) {
                var order = dbContext.InternalOrders
                    .Include(x => x.OrderTime)
                    .Single(x => x.ID == internalOrder.ID);
                
                Debug.Assert(false == dbContext.OrderTimes.Any(x => x.ID == previousOrderTimeId), "OrderTime is still present");
            }
        }

        public static async void ReplaceOrderTimeOnInternalOrder() 
        {
            // create internal order
            InternalOrder internalOrder;
            using(var dbContext = new EfTestDbContext()) {
                var repo = new InternalOrderRepository(dbContext);
                var seq = await repo.NextOrderNumber();

                internalOrder = new InternalOrder("W-{seq}");

                internalOrder.SetTime(new OrderTime("Test"));
                await repo.Update(internalOrder);
                await repo.SaveAsync();

                Debug.Assert(internalOrder.ID != Guid.Empty);
                Debug.Assert(null != internalOrder.OrderTimeID);
            }

            // replace order time
            Guid previousOrderTimeId;
            using(var dbContext = new EfTestDbContext()) {
                var repo = new InternalOrderRepository(dbContext);
                // get untracked InternalOrder
                var order = await dbContext.InternalOrders.Include(x => x.OrderTime).SingleAsync(x => x.ID == internalOrder.ID);
                previousOrderTimeId = order.OrderTime.ID;

                order.SetTime(new OrderTime("Test 1234"));

                Console.WriteLine($"Previous order time id {previousOrderTimeId}");
                Console.WriteLine($"Current order time id {order.OrderTime.ID}");

                await repo.Update(order);
                await Task.Delay(1000); // for sensitive logging to settle
                Console.WriteLine($"-- Before save changes --");
                await repo.SaveAsync();
                Console.WriteLine($"Current order time id after save {order.OrderTime.ID}");
            }

            // check if OrderTime was deleted
            using(var dbContext = new EfTestDbContext()) {
                var repo = new InternalOrderRepository(dbContext);
                var order = repo.Get(internalOrder.ID);
                
                Debug.Assert(false == dbContext.OrderTimes.Any(x => x.ID == previousOrderTimeId), "OrderTime is still present");
                Debug.Assert(null != order.OrderTime);
                Debug.Assert("Test 1234" == order.OrderTime.Display);
            }
        }

        public static async void ReplaceOrderTimeOnInternalOrderTrackedEntities() 
        {
            // create internal order
            InternalOrder internalOrder;
            using(var dbContext = new EfTestDbContext()) {
                var repo = new InternalOrderRepository(dbContext);
                var seq = await repo.NextOrderNumber();

                internalOrder = new InternalOrder("W-{seq}");

                internalOrder.SetTime(new OrderTime("Test"));
                await repo.Update(internalOrder);
                await repo.SaveAsync();

                Debug.Assert(internalOrder.ID != Guid.Empty);
                Debug.Assert(null != internalOrder.OrderTimeID);
            }

            // replace order time
            Guid previousOrderTimeId;
            using(var dbContext = new EfTestDbContext()) {
                var order = dbContext.InternalOrders.Include(x => x.OrderTime)
                    .Single(x => x.ID == internalOrder.ID);
                previousOrderTimeId = order.OrderTime.ID;

                order.SetTime(new OrderTime("Test 1234"));

                Console.WriteLine($"Previous order time id {previousOrderTimeId}");
                Console.WriteLine($"Current order time id {order.OrderTime.ID}");

                dbContext.SaveChanges();
                Console.WriteLine($"Current order time id after save {order.OrderTime}");
            }

            // check if OrderTime was deleted
            using(var dbContext = new EfTestDbContext()) {
                var order = dbContext.InternalOrders
                    .Include(x => x.OrderTime)
                    .Single(x => x.ID == internalOrder.ID);
                
                Debug.Assert(false == dbContext.OrderTimes.Any(x => x.ID == previousOrderTimeId), "OrderTime is still present");
                Debug.Assert(null != order.OrderTime);
                Debug.Assert("Test 1234" == order.OrderTime.Display);
            }
        }
    }
}
