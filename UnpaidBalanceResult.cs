namespace EnterpriseSystemApp
{
    public class UnpaidBalanceResult
    {
        public string SessionId { get; set; }
        public decimal BalanceDue { get; set; }
        public decimal AmountCollected { get; set; }

        public UnpaidBalanceResult(string sessionId, decimal balanceDue, decimal amountCollected)
        {
            SessionId = sessionId;
            BalanceDue = balanceDue;
            AmountCollected = amountCollected;
        }
    }
}
