// See https://aka.ms/new-console-template for more information
using LemonMarkets;

string token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJhdWQiOiJsZW1vbi5tYXJrZXRzIiwiaXNzIjoibGVtb24ubWFya2V0cyIsInN1YiI6InVzcl9weVBWVzIyV1dWWEt0Q1lSMHlXM2xEbUszdzBiclNoNnJzIiwiZXhwIjoxNjY4NjIwNjQzLCJpYXQiOjE2MzcwODQ2NDMsImp0aSI6ImFwa19weVBWVzMzRkZLNWs3NW5ibGtEY1d5Z1pndDQ3aDVuRndnIn0.v5JttuACxkHE6Stuh_-JLoYB1DNGueDLu68SNh7KHyU";

var streamingExample = new Func<Task>(async () =>
{
    try
    {
        bool dataReceived = false;
        var api = LemonApiFactory.Create();
        api.SubscripeToUserChannel((message) =>
        {
            Console.WriteLine($"Received streaming data: {message.Data.ToString()}");
            dataReceived = true;
        });

        api.SubscribeToIsin(new List<string>() { "DE0005557508" });

        var timeout = DateTime.UtcNow.AddSeconds(15);
        while (!dataReceived)
        {
            await Task.Delay(1000);
            if (DateTime.UtcNow > timeout) throw new Exception("No data received on stream");
        }

        api.UnsubscripeFromUserChannel();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Streaming Example failed: {ex.Message}");
        throw;
    }
});

var testMakeOrder = new Func<Task>(async () =>
{
    try
    {
        var api = LemonApiFactory.Create(true);
        var order = await api.PostOrder(new LemonMarkets.Models.PostOrderQuery()
        {
            Isin = "DE0005557508",
            Quantity = 10,
            Side = LemonMarkets.Models.Enums.OrderSide.Buy,
            ValidUntil = DateTime.UtcNow.AddHours(1),
        });

        var result = await api.ActivateOrder(order.Results.First().Uuid);
        if (!result) throw new Exception("Failed to activate order");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Order Example failed: {ex.Message}");
        throw;
    }
});

var testGetOrders = new Func<Task>(async () =>
{
    try
    {
        var api = LemonApiFactory.Create(true);
        var orders = await api.GetOrders(new LemonMarkets.Models.OrderSearchFilter() { Type = LemonMarkets.Models.Enums.OrderType.All });
        foreach (var o in orders.Results)
        {
            var order = await api.GetOrder(o.Uuid);
            Console.WriteLine($"Order: {order}");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Order Example failed: {ex.Message}");
        throw;
    }
});


try
{
    Console.WriteLine("Init connection to Lemon Markets...");
    await LemonApiFactory.Init(token, true, true);
    Console.WriteLine("Lemon Api Test initialized succesfully.");

    await testGetOrders();

    Console.WriteLine("Subscribe to Lemon Markets User Channel...");
    await streamingExample();
    Console.WriteLine("User Channel initialized succesfully.");

    Console.WriteLine("Try to make order...");
    await testMakeOrder();
    Console.WriteLine("Order made succesfully.");

    Console.WriteLine("Try to get orders...");
    await testGetOrders();
    Console.WriteLine("Orders received succesfully.");

    Console.WriteLine("Lemon Api Test finished succesfully.");
}
catch (Exception ex)
{
    Console.WriteLine("Lemon Aoi Test failed: " + ex.Message);
}