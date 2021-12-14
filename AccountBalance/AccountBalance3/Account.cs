using System;
using ReactiveDomain;
using ReactiveDomain.Messaging;
using ReactiveDomain.Util;

namespace AccountBalance3
{
    public class Account : EventDrivenStateMachine
    {
        private long _balance;

        private Account()
        {
            Setup();
        }

        public Account(Guid id) : this()
        {
            Raise(new AccountMsgs.AccountCreated(id));
        }

        class MySecretEvent : Message
        {
        }

        private void Setup()
        {
            Register<AccountMsgs.AccountCreated>(evt => Id = evt.AccountId);
            Register<AccountMsgs.Debit>(Apply);
            Register<AccountMsgs.Credit>(Apply);
        }

        private void Apply(AccountMsgs.Debit @event)
        {
            _balance -= @event.Amount;
        }

        private void Apply(AccountMsgs.Credit @event)
        {
            _balance += @event.Amount;
        }

        public void Credit(uint amount)
        {
            //nothing to check
            Raise(new AccountMsgs.Credit(amount));
        }

        public void Debit(uint amount)
        {
            Ensure.Nonnegative(_balance - amount, "Balance");

            Raise(new AccountMsgs.Debit(amount));
        }
    }
}