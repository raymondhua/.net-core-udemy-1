﻿using Microsoft.AspNetCore.Http;
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
using Stripe.Issuing;

namespace BulkyBookWeb.Stripe
{
    public class StripePayment
    {
        public static SessionCreateOptions GeneratePayment(IHttpContextAccessor httpContextAccessor, int orderHeaderId, IEnumerable<OrderDetail> orderDetail, bool customer = true)
        {
            //Stripe settings
            var request = httpContextAccessor.HttpContext?.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}/";
            string successUrl, cancelUrl;
            if (customer)
            {
                successUrl = baseUrl + $"customer/cart/OrderConfirmation?id={orderHeaderId}";
                cancelUrl = baseUrl + "customer/cart/index";
            }
            else
            {
                successUrl = baseUrl + $"admin/order/PaymentConfirmation?orderHeaderId={orderHeaderId}";
                cancelUrl = baseUrl + $"admin/order/details?orderId={orderHeaderId}";
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
                CancelUrl = cancelUrl
            };

            string currency = "usd";
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
                            }
                        },
                        Quantity = item.Count,
                    };
                    options.LineItems.Add(sessionLineItem);
                }
            }
            return options;
        }

    }
}
