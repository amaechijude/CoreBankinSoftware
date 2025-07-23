using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using src.Infrastructure.Data;
using src.Shared.Domain.Interfaces;

namespace src.Infrastructure.Repository
{
    public class UnitOfWorkRepository(CustomerDbContext context) : IUnitOfWork
    {
        private readonly CustomerDbContext _context = context;
        private IDbContextTransaction? _currentTransaction;

        private ICustomerRepository? _customerRepository;

        // public Asessors for private repositories
        public ICustomerRepository CustomerRepository
        {
            get
            {
                return _customerRepository ??= new CustomerRepository(_context);
            }
        }


        public async Task BeginTransactionAsync()
        {
            if (_currentTransaction is not null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _currentTransaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitTransactionAsync()
        {
            if (_currentTransaction is null)
            {
                throw new InvalidOperationException("No transaction in progress to commit.");
            }

            try
            {
                await _currentTransaction.CommitAsync();
            }
            catch
            {
                await RollbackTransactionAsync();
                throw;
            }
            finally
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Handle concurrency conflicts
                throw new InvalidOperationException("The entity was modified by another user. Please refresh and try again.", ex);
            }
            catch (DbUpdateException ex)
            {
                // Handle database update errors
                throw new InvalidOperationException("An error occurred while saving changes to the database.", ex);
            }

        }


        public async Task RollbackTransactionAsync()
        {
            if (_currentTransaction is null)
            {
                throw new InvalidOperationException("No active transaction to rollback.");
            }

            try
            {
                await _currentTransaction.RollbackAsync();
            }
            finally
            {
                _currentTransaction?.Dispose();
                _currentTransaction = null;
            }

        }

        public void Dispose()
        {
            _currentTransaction?.Dispose();
            _context.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
