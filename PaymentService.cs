
using Razorpay.Api;
using Microsoft.Extensions.Configuration;

public class PaymentService
{
    private readonly string _key;
    private readonly string _secret;

    public PaymentService(IConfiguration configuration)
    {
        _key = configuration["Razorpay:Key"];
        _secret = configuration["Razorpay:Secret"];
    }

    public Order CreateOrder(int amount)
    {
        RazorpayClient client = new RazorpayClient(_key, _secret);

        Dictionary<string, object> options = new Dictionary<string, object>();
        options.Add("amount", amount * 100); 
        options.Add("currency", "INR");
        options.Add("receipt", "receipt#1");
        options.Add("payment_capture", 1);

        Order order = client.Order.Create(options);
        return order;
    }
}
