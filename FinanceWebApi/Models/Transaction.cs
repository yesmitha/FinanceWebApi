//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace FinanceWebApi.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class Transaction
    {
        public int TransactionID { get; set; }
        public string UserName { get; set; }
        public int ProductID { get; set; }
        public int SchemeNo { get; set; }
        public System.DateTime PurchaseDate { get; set; }
        public decimal RemainingAmount { get; set; }
        public decimal EMIAmount { get; set; }
        public System.DateTime LastChecked { get; set; }
    
        public virtual Consumer Consumer { get; set; }
        public virtual EMI EMI { get; set; }
        public virtual Product Product { get; set; }
    }
}
