using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulkyBook.Models;
using BulkyBook.Models.ViewModels;
using Stripe;
using Stripe.Checkout;
using MailKit.Search;

namespace BulkyBookWeb.Stripe
{
    public class StripePayment
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public StripePayment(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public SessionService StripeSession(OrderHeader orderHeader, IEnumerable<OrderDetail> orderDetail = null, IEnumerable <ShoppingCart> shoppingCart = null, bool customer = true)
        {
            //Stripe settings
            var domain = _httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{domain.Scheme}://{domain.Host}";
            string successUrl, cancelUrl;
            if (customer)
            {
                successUrl = domain + $"customer/cart/OrderConfirmation?id={orderHeader.Id}";
                cancelUrl = domain + "customer/cart/index";
            }
            else
            {
                successUrl = domain + $"admin/order/PaymentConfirmation?orderHeaderId={orderHeader.Id}";
                cancelUrl = domain + $"admin/order/details?orderId={orderHeader.Id}";
            }
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string>
                    {
                        "card",
                    },
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                SuccessUrl = successUrl,
                CancelUrl = cancelUrl,
            };

            string currency = "usd";
            if (orderDetail != null)
            {
                foreach (var item in orderDetail)
                {
                    {
                        var sessionLineItem = new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(item.Price * 100), //20.00 -> 2000
                                Currency = currency,
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = item.Product.Title,
                                },
                            },
                            Quantity = item.Count,
                        };
                        options.LineItems.Add(sessionLineItem);
                    }
                }
            }
            else
            {
                foreach (var item in shoppingCart)
                {
                    {
                        var sessionLineItem = new SessionLineItemOptions
                        {
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                UnitAmount = (long)(item.Price * 100), //20.00 -> 2000
                                Currency = currency,
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = item.Product.Title,
                                },
                            },
                            Quantity = item.Count,
                        };
                        options.LineItems.Add(sessionLineItem);
                    }
                }
            }
            return new SessionService();
        }
    }
}
