using Microsoft.AspNetCore.Mvc;
using TransactionService.DTOs;
using TransactionService.Services;

namespace TransactionService.Controller;

[ApiController]
[Route("")]
public class TransactionController(PerformTransaction performTransaction) : ControllerBase
{
    private readonly PerformTransaction _performTransaction = performTransaction;

    [HttpPost("name-enquiry")]
    public async Task<IActionResult> NameEnquiry([FromBody] NameEnquiryRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _performTransaction.GetBeneficiaryAccountDetails(request);
        return response.IsSuccess
            ? Ok(response)
            : BadRequest(response);
    }
}