using CRMS_API.Domain.DTOs;
using CRMS_API.Services.Interfaces;

namespace CRMS_API.Services.Implementations
{
    public class MpesaSimulation : IMpesaSimulation
    {

        public Task<string> SimulateMpesaPaymentAsync(MpesaSimulationDto paymentDto)
        {
            var simulatedTransactionId = $"LMD{DateTime.Now.Ticks / 10000}";

            return Task.FromResult($"Simulated M-Pesa payment of KES {paymentDto.Amount:N2} successful for Booking Ref ID {paymentDto.TransactionRefId}. Transaction ID: {simulatedTransactionId}");
        }
    }
}
