﻿using LandingPage.ViewModels.Home;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Marketplace.SaaS;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Marketplace.SaaS.Models;
using System;
using Common.Utils.Comm;

namespace LandingPage.Controllers
{
    [Authorize(AuthenticationSchemes = OpenIdConnectDefaults.AuthenticationScheme)]
    [AuthorizeForScopes(Scopes = new string[] { "user.read" })]
    public class HomeController : Controller
    {
        private readonly IMarketplaceSaaSClient _marketplaceSaaSClient;
        private readonly GraphServiceClient _graphServiceClient;

        public HomeController(
            // Need a marketplace client to talk to the SaaS API
            IMarketplaceSaaSClient marketplaceSaaSClient,
            GraphServiceClient graphServiceClient)
        {
            _marketplaceSaaSClient = marketplaceSaaSClient;
            _graphServiceClient = graphServiceClient;
        }

        /// <summary>
        /// Shows all information associated with the user, the request, and the subscription.
        /// </summary>
        /// <param name="token">THe marketplace purchase ID token</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<IActionResult> IndexAsync(string token, CancellationToken cancellationToken)
        {
            string newGuidApp = Guid.NewGuid().ToString();
            if (string.IsNullOrEmpty(token))
            {
                this.ModelState.AddModelError(string.Empty, "Token URL parameter cannot be empty");
                this.ViewBag.Message = "Token URL parameter cannot be empty";
                return this.View();
            }
            var model = new IndexViewModel();
            if (token.Equals("testTOkenPIA"))
            {
                // build the model
                model = new IndexViewModel
                {
                    DisplayName = "TEST USER",
                    Email = "TESTEMAIL.COM",
                    SubscriptionName = "SubscriptionName",
                    
                    PlanName = "PlanName",
                    SubscriptionId = "SubscriptionId",
                    TenantId = "TenantId",
                    PurchaseIdToken = token,
                    newGuid = newGuidApp
                };
                string newGuidApp2 = HTTPUtils.ProvisionUser(model);
                model.newGuid = newGuidApp2;
            }
            else
            {

                // resolve the subscription using the marketplace purchase id token
                // this is the token that comes in on the querystring
                var resolvedSubscription = (await _marketplaceSaaSClient.Fulfillment.ResolveAsync(token, cancellationToken: cancellationToken)).Value;

                // get the plans on this subscription
                // we want these to display the plans associated with this subscription
                var subscriptionPlans = (await _marketplaceSaaSClient.Fulfillment.ListAvailablePlansAsync(resolvedSubscription.Id.Value, cancellationToken: cancellationToken)).Value;

                // find the plan that goes with this purchase
                string planName = string.Empty;
                
                foreach (var plan in subscriptionPlans.Plans)
                {
                    if (plan.PlanId == resolvedSubscription.Subscription.PlanId)
                    {
                        planName = plan.DisplayName;
                    }
                }

                // get graph current user data
                var graphApiUser = await _graphServiceClient.Me.Request().GetAsync();

                // build the model
                 model = new IndexViewModel
                {
                    DisplayName = graphApiUser.DisplayName,
                    Email = graphApiUser.Mail,
                    SubscriptionName = resolvedSubscription.SubscriptionName,
                    FulfillmentStatus = resolvedSubscription.Subscription.SaasSubscriptionStatus.GetValueOrDefault(),
                    PlanName = planName,
                    SubscriptionId = resolvedSubscription.Id.ToString(),
                    TenantId = resolvedSubscription.Subscription.Beneficiary.TenantId.ToString(),
                    PurchaseIdToken = token,
                    newGuid = newGuidApp
                };
                string newGuidApp2 = HTTPUtils.ProvisionUser(model);
                model.newGuid = newGuidApp2;
            }
            return View(model);
        }

       
    }
}