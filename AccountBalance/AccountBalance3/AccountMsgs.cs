using System;
using ReactiveDomain.Messaging;

namespace AccountBalance3 {
    public class AccountMsgs
    {
        public class CreateAccount : Command
        {
            public readonly Guid AccountId;

            public CreateAccount(Guid accountId)
            {
                AccountId = accountId;
            }
        }

        public class AccountCreated : Event
        {
            public readonly Guid AccountId;

            public AccountCreated(Guid accountId)
            {
                AccountId = accountId;
            }
        }

        public class DebitAccount : Command
        {
            public readonly Guid AccountId;
            public readonly uint Amount;

            public DebitAccount(
                Guid accountId,
                uint amount)
            {
                AccountId = accountId;
                Amount = amount;
            }
        }

        public class Debit : Event
        {
            public readonly uint Amount;
            public Debit(uint amount)
            {
                Amount = amount;
            }
        }

        public class CreditAccount : Command
        {
            public readonly Guid AccountId;
            public readonly uint Amount;

            public CreditAccount(
                Guid accountId,
                uint amount)
            {
                AccountId = accountId;
                Amount = amount;
            }
        }

        public class Credit : Event
        {
            public readonly uint Amount;

            public Credit(uint amount)
            {
                Amount = amount;
            }
        }
    }
}