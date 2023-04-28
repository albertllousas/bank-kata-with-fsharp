# Bank Account Kata

https://github.com/sandromancuso/Bank-kata

## Acceptance test

```fsharp
test "Should print the bank statement after some money movements on a customer account" {
  let result = Account.init []
                |> Account.deposit 1000.00m (on("01/10/2012"))
                |> Result.bind (Account.deposit 2000.00m (on("01/13/2012")))
                |> Result.bind (Account.withdraw 500.00m (on("01/14/2012")))
                |> Result.map BankStatement.print 

  assertThat result (Ok([ " date          || credit        || debit         || balance       "      
                          " 14/01/2012    ||               || 500.00        || 2500.00       "       
                          " 13/01/2012    || 2000.00       ||               || 3000.00       "       
                          " 10/01/2012    || 1000.00       ||               || 1000.00       "]))
}
```