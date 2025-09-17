using AccountServices.Application.DTO;
using AccountServices.Application.Interfaces;
using AccountServices.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace AccountServices.API.Controllers
{
  [ApiController]
  [Route("api/accounts")]
  public sealed class AccountsController : ControllerBase
  {
    private readonly IAccountRepository _repo;
    public AccountsController(IAccountRepository repo) => _repo = repo;

    [HttpPost]
    public async Task<IActionResult> Open([FromBody] OpenAccountRequest req, CancellationToken ct)
    {
      var accountNumber = $"AC{Random.Shared.Next(10000000, 99999999)}";
      var account = Account.Open(accountNumber, req.CustomerId, req.Type, req.Currency, req.OpeningBalance);
      await _repo.AddAsync(account, ct);
      await _repo.SaveChangesAsync(ct);
      return CreatedAtAction(nameof(GetById), new { id = account.Id }, new { account.Id, account.AccountNumber });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
      var acc = await _repo.GetAsync(id, ct);
      return acc is null ? NotFound() : Ok(new { acc.Id, acc.AccountNumber, acc.Status, acc.Balance, acc.Currency, acc.Type });
    }

    [HttpPost("{id:guid}/credit")]
    public async Task<IActionResult> Credit(Guid id, [FromBody] MoneyRequest req, CancellationToken ct)
    {
      var acc = await _repo.GetAsync(id, ct);
      if (acc is null) return NotFound();
      acc.Credit(req.Amount);
      await _repo.SaveChangesAsync(ct);
      return Ok(new { acc.Balance });
    }

    [HttpPost("{id:guid}/debit")]
    public async Task<IActionResult> Debit(Guid id, [FromBody] MoneyRequest req, CancellationToken ct)
    {
      var acc = await _repo.GetAsync(id, ct);
      if (acc is null) return NotFound();
      acc.Debit(req.Amount);
      await _repo.SaveChangesAsync(ct);
      return Ok(new { acc.Balance });
    }

    [HttpPost("{id:guid}/freeze")]
    public async Task<IActionResult> Freeze(Guid id, CancellationToken ct)
    {
      var acc = await _repo.GetAsync(id, ct);
      if (acc is null) return NotFound();
      acc.Freeze();
      await _repo.SaveChangesAsync(ct);
      return Ok();
    }

    [HttpPost("{id:guid}/unfreeze")]
    public async Task<IActionResult> Unfreeze(Guid id, CancellationToken ct)
    {
      var acc = await _repo.GetAsync(id, ct);
      if (acc is null) return NotFound();
      acc.Unfreeze();
      await _repo.SaveChangesAsync(ct);
      return Ok();
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, CancellationToken ct)
    {
      var acc = await _repo.GetAsync(id, ct);
      if (acc is null) return NotFound();
      acc.Close();
      await _repo.SaveChangesAsync(ct);
      return Ok();
    }
  }
}
