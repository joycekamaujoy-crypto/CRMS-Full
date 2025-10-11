using CRMS_API.Domain.DTOs;

namespace CRMS_API.Services.Interfaces
{
    public interface IMpesaSimulation
    {
        Task<string> SimulateMpesaPaymentAsync(MpesaSimulationDto paymentDto);
    }
}
