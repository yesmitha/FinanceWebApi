﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using FinanceWebApi.Models;
using System.Web.Http.Cors;

namespace FinanceWebApi.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class AdminController : ApiController
    {
        readonly FinanceEntities financeEntities = new FinanceEntities();

        [HttpGet]
        public HttpResponseMessage GetUnverifiedConsumers()
        {
            try
            {
                var data = from x in financeEntities.Consumers
                           where x.IsPending == true
                           select new
                           { x.UserName, x.Name, x.PhoneNumber, x.Email, x.Address };
                return Request.CreateResponse(HttpStatusCode.OK, data);
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Unverified Consumers Could Not Be Displayed");
            }
        }

        [HttpGet]
        public HttpResponseMessage GetVerifiedConsumers()
        {
            try
            {
                var data = from x in financeEntities.Consumers
                           where x.IsPending == false
                           select new { x.UserName, x.Name, x.PhoneNumber, x.Email, x.Address };
                return Request.CreateResponse(HttpStatusCode.OK, data);
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Verified Consumers Could Not Be Displayed");
            }
        }

        [HttpPut]
        public HttpResponseMessage PutConsumer(string id, [FromBody] Consumer updatedConsumer)
        {
            try
            {
                Consumer oldConsumer = financeEntities.Consumers.Find(id);
                if (oldConsumer == null)
                {
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Consumer Not Found");
                } else
                {
                    oldConsumer.Name = updatedConsumer.Name;
                    oldConsumer.PhoneNumber = updatedConsumer.PhoneNumber;
                    oldConsumer.Address = updatedConsumer.Address;
                    oldConsumer.Email = updatedConsumer.Email;

                    financeEntities.SaveChanges();
                    return Request.CreateResponse(HttpStatusCode.Accepted, "Consumer Updated Successfully");
                }
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Consumer Could Not Be Updated");
            }
        }

        [HttpDelete]
        public HttpResponseMessage DeleteConsumer(string id)
        {
            try
            {
                Consumer existingConsumer = financeEntities.Consumers.Find(id);
                if (existingConsumer == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Consumer Not Found");
                }

                CompanyCard consumerCard = financeEntities.CompanyCards.Where(c => c.UserName == id).FirstOrDefault();
                if (consumerCard == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Consumer Card Not Found");
                }

                existingConsumer.IsOpen = false;
                consumerCard.IsOpen = false;

                financeEntities.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.Accepted, "Consumer Deleted Successfully");
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Consumer Could Not Be Deleted");
            }
        }

        [HttpPut]
        public HttpResponseMessage VerifyConsumer(string id)
        {
            try
            {
                Consumer unverifiedConsumer = financeEntities.Consumers.Find(id);
                if (unverifiedConsumer == null)
                {
                    return Request.CreateResponse(HttpStatusCode.Forbidden, "Consumer Not Found To Be Verified");
                }

                CompanyCard consumerCard = financeEntities.CompanyCards.Where(c => c.UserName == unverifiedConsumer.UserName).FirstOrDefault();
                if (consumerCard == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Consumer Card Not Found");
                }

                Card card = financeEntities.Cards.Where(c => c.CardTypeNo == unverifiedConsumer.CardTypeNo).FirstOrDefault();
                if (card == null)
                {
                    return Request.CreateErrorResponse(HttpStatusCode.BadRequest, "Consumer Card Not Found");
                }

                var remainingBalance = consumerCard.Balance - card.JoiningFee;
                consumerCard.Balance = remainingBalance;
                unverifiedConsumer.IsPending = false;
                unverifiedConsumer.IsOpen = true;
                consumerCard.IsOpen = true;

                financeEntities.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.Accepted, "Consumer Verified Successfully");
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Consumer Could Not Be Verified");
            }
        }

        [HttpPut]
        public HttpResponseMessage DeductEmi()
        {
            try
            {
                var currentDate = DateTime.Now;

                foreach (var tran in financeEntities.Transactions)
                {
                    CompanyCard tranCard = financeEntities.CompanyCards.Where(c => c.UserName == tran.UserName).FirstOrDefault();
                    if (tranCard == null)
                    {
                        continue;
                    }

                    var dayDifference = (currentDate - tran.LastChecked).Days;
                    bool isEmiDate = (currentDate - tran.PurchaseDate).Days % 30 == 0;

                    if (tranCard.IsOpen && dayDifference > 0 && isEmiDate && tran.RemainingAmount > 0)
                    {
                        var currentRemainingAmount = tran.RemainingAmount - tran.EMIAmount;
                        tran.RemainingAmount = currentRemainingAmount;

                        var currentBalance = tranCard.Balance - tran.EMIAmount;
                        tranCard.Balance = currentBalance;
                    }
                    tran.LastChecked = DateTime.Now;
                }

                financeEntities.SaveChanges();
                return Request.CreateResponse(HttpStatusCode.OK, "Operation Completed Successfully");
            } catch
            {
                return Request.CreateErrorResponse(HttpStatusCode.InternalServerError, "Something Went Wrong");
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                financeEntities.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
