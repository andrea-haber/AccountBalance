using System;
using ReactiveDomain.Foundation;
using ReactiveDomain.Messaging;
using ReactiveDomain.Messaging.Bus;

namespace AccountBalance3
{
    class AccountSvc :
        IDisposable,
        IHandleCommand<AccountMsgs.CreateAccount>,
        IHandleCommand<AccountMsgs.DebitAccount>,
        IHandleCommand<AccountMsgs.CreditAccount>
    {
        private readonly IDispatcher _dispatcher;
        private readonly IRepository _repo;

        public AccountSvc(
            IDispatcher dispatcher,
            IRepository repo)
        {
            _dispatcher = dispatcher;
            _repo = repo;

            _dispatcher.Subscribe<AccountMsgs.CreateAccount>(this);
            _dispatcher.Subscribe<AccountMsgs.DebitAccount>(this);
            _dispatcher.Subscribe<AccountMsgs.CreditAccount>(this);
        }

        public CommandResponse Handle(AccountMsgs.CreateAccount command)
        {
            if (_repo.TryGetById<Account>(command.AccountId, out var account))
                return command.Fail(new Exception($"Account with ID '{command.AccountId}' already exists."));
            _repo.Save(new Account(command.AccountId));
            return command.Succeed();
        }

        public CommandResponse Handle(AccountMsgs.DebitAccount command)
        {
            var account = _repo.GetById<Account>(command.AccountId);
            account.Debit(command.Amount);
            _repo.Save(account);
            return command.Succeed();
        }

        public CommandResponse Handle(AccountMsgs.CreditAccount command)
        {
            var account = _repo.GetById<Account>(command.AccountId);
            account.Credit(command.Amount);
            _repo.Save(account);
            return command.Succeed();
        }

        public void Dispose()
        {
            Dispose(true);
        }

        private bool _disposed;

        private void Dispose(bool disposing)
        {
            if (_disposed) return;
            if (disposing)
            {
                _dispatcher.Unsubscribe<AccountMsgs.CreateAccount>(this);
                _dispatcher.Unsubscribe<AccountMsgs.DebitAccount>(this);
                _dispatcher.Unsubscribe<AccountMsgs.CreditAccount>(this);
            }
            _disposed = true;
        }
    }
}
