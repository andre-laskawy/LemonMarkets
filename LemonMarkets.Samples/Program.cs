// S    ee https://aka.ms/new-console-template for more information
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

var testActivateOrder = new Func<Task>(async () =>
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

        var result = await api.ActivateOrder(order.Result.Uuid);
        if (!result) throw new Exception("Failed to activate order");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Order Activate Example failed: {ex.Message}");
        throw;
    }
});

var testDeleteOrder = new Func<Task>(async () =>
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

        var result = await api.DeleteOrder(order.Result.Uuid);
        if (!result) throw new Exception("Failed to delete order");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Order Delete Example failed: {ex.Message}");
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

var testGetDailyOHC = new Func<Task>(async () =>
{
    try
    {
        var api = LemonApiFactory.Create(true);
        var result = await api.GetDailyOHLC("DE0005557508");
        Console.WriteLine($"OHC: {result.Open} | {result.Close}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"OHC Example failed: {ex.Message}");
        throw;
    }
});

var testGetTicker = new Func<Task>(async () =>
{
    try
    {
        var api = LemonApiFactory.Create(true);
        var result = await api.GetTicker("DE0005557508");
        Console.WriteLine($"Ticker: {result.Ask} | {result.Bid}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ticker Example failed: {ex.Message}");
        throw;
    }
});

var testGetChart = new Func<Task>(async () =>
{
    try
    {
        var api = LemonApiFactory.Create(true);
        var result = await api.GetChart("DE0005557508", DateTime.UtcNow.AddDays(-1));
        Console.WriteLine($"Chart Points: {result.Count}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Chart Example failed: {ex.Message}");
        throw;
    }
});


var testSearch = new Func<Task>(async () =>
{
    try
    {
        var api = LemonApiFactory.Create(true);
        var result = await api.Search(new LemonMarkets.Models.InstrumentSearchFilter()
        {
            InstrumentType = LemonMarkets.Models.Enums.InstrumentType.Stock,
            SearchByIsins = new List<string>() { "DE0005557508" },
        });

        Console.WriteLine($"Search Result: {result.Results.Count}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Search Example failed: {ex.Message}");
        throw;
    }
});


try
{
    Console.WriteLine("Init connection to Lemon Markets...");
    await LemonApiFactory.Init(token, true, true);
    Console.WriteLine("Lemon Api Test initialized succesfully.");

    Console.WriteLine("Subscribe to Lemon Markets User Channel...");
    await streamingExample();
    Console.WriteLine("User Channel initialized succesfully.");

    Console.WriteLine("Try to activate order...");
    await testActivateOrder();
    Console.WriteLine("Order activated succesfully.");

    Console.WriteLine("Try to delete order...");
    await testDeleteOrder();
    Console.WriteLine("Order delete succesfully.");

    Console.WriteLine("Try to get orders...");
    await testGetOrders();
    Console.WriteLine("Orders received succesfully.");
    
    Console.WriteLine("Try to get daily ohc...");
    await testGetDailyOHC();
    Console.WriteLine("OHC received succesfully.");

    Console.WriteLine("Try to get ticker...");
    await testGetTicker();
    Console.WriteLine("Ticker received succesfully.");

    Console.WriteLine("Try to get chart...");
    await testGetChart();
    Console.WriteLine("Chart received succesfully.");

    Console.WriteLine("Try to search isin...");
    await testSearch();
    Console.WriteLine("Search received succesfully.");

    Console.WriteLine("Lemon Api Test finished succesfully.");
}
catch (Exception ex)
{
    Console.WriteLine("Lemon Aoi Test failed: " + ex.Message);
}