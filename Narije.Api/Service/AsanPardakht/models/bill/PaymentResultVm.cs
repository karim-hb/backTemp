using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Web;

namespace Narije.Api.Payment.AsanPardakht.models.bill
{
    public class PaymentResultVm : IResponseVm
    {
        public string CardNumber { get; set; }
        public string Rrn { get; set; } 
        public string RefId { get; set; }
        public decimal Amount { get; set; }
        public long? PayGateTranID { get; set; }
        public int ResCode { get; set; }
        public string ResMessage { get; set; }
    }

    // 2024-01-29 Moudi, add this dto for city bank transaction result, more complete
    /// <summary>
    /// TransactionResultVm
    /// </summary>
    public class TransactionResultVm
    {
        [JsonPropertyName("cardNumber")]
        public string CardNumber { get; set; }

        [JsonPropertyName("rrn")]
        public string Rrn { get; set; }

        [JsonPropertyName("refID")]
        public string RefID { get; set; }

        [JsonPropertyName("amount")]
        public string Amount { get; set; }

        [JsonPropertyName("payGateTranID")]
        public string PayGateTranID { get; set; }

        [JsonPropertyName("salesOrderID")]
        public string SalesOrderID { get; set; }

        [JsonPropertyName("hash")]
        public string Hash { get; set; }

        [JsonPropertyName("serviceTypeId")]
        public int? ServiceTypeId { get; set; }

        [JsonPropertyName("serviceStatusCode")]
        public string ServiceStatusCode { get; set; }

        [JsonPropertyName("destinationMobile")]
        public string DestinationMobile { get; set; }

        [JsonPropertyName("productId")]
        public int? ProductId { get; set; }

        [JsonPropertyName("productNameFa")]
        public string ProductNameFa { get; set; }

        [JsonPropertyName("productPrice")]
        public int? ProductPrice { get; set; }

        [JsonPropertyName("operatorId")]
        public int? OperatorId { get; set; }

        [JsonPropertyName("operatorNameFa")]
        public string OperatorNameFa { get; set; }

        [JsonPropertyName("simTypeId")]
        public int? SimTypeId { get; set; }

        [JsonPropertyName("simTypeTitleFa")]
        public string SimTypeTitleFa { get; set; }

        [JsonPropertyName("billId")]
        public string BillId { get; set; }

        [JsonPropertyName("payId")]
        public string PayId { get; set; }

        [JsonPropertyName("billOrganizationNameFa")]
        public string BillOrganizationNameFa { get; set; }

        [JsonPropertyName("payGateTranDate")]
        public DateTime? PayGateTranDate { get; set; }

        [JsonPropertyName("payGateTranDateEpoch")]
        public double? PayGateTranDateEpoch { get; set; }
    }
}