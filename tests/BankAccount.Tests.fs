module BankAccountTests

open BankAccount
open System.Globalization
open BankAccount.Account
open Xunit
open Expecto
open FSharpx.Result
open System
open FSharp.Core.Result
open System.Collections.Generic

let assertThat actual expected = Expect.equal expected actual ""

let on date = DateTime.Parse($"{date}")

[<Tests>]
let tests =
  testList "All" [ 

    testList "Acceptance" [ 

      test "Should print the bank statement after some money movements in a customer account" {
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
    ]

    testList "Account" [ 

      test "Should deposit money into an account" {
        let result = Account.init [] |> Account.deposit 1000.00m (on("01/10/2012"))
                     
        assertThat result (Ok({ ledger = [(1000.00m, on("01/10/2012"), Credit)] }))
      }

      test "Should fail depositing money into an account with an invalid amount" {
        let result = Account.init [] |> Account.deposit -1000.00m (on("01/10/2012"))
                     
        assertThat result (Error InvalidNegativeAmount)
      }

      test "Should withdraw money from an account" {
        let result = Account.init [(1000.00m, on("01/10/2012"), Credit)] 
                      |> Account.withdraw 500.00m (on("02/10/2012"))
                     
        assertThat result (Ok({ ledger = [(1000.00m, on("01/10/2012"), Credit); (500.00m, on("02/10/2012"), Debit)] }))
      }

      test "Should fail withdrawing money from an account with an invalid amount" {
        let result = Account.init [(1000.00m, on("01/10/2012"), Credit)] 
                      |> Account.withdraw -500.00m (on("02/10/2012"))
                     
        assertThat result (Error InvalidNegativeAmount)
      }

      test "Should fail withdrawing money from an account when the are not enough founds" {
        let result = Account.init [(1000.00m, on("01/10/2012"), Credit)] 
                      |> Account.withdraw 1500.00m (on("02/10/2012"))
                     
        assertThat result (Error NotEnoughFounds)
      }

      test "Should calculate the balance given a list of transaction records" {
        let result = [(1000.00m, on("01/10/2012"), Credit); (500.00m, on("02/10/2012"), Debit)] |> Account.balance
                     
        assertThat result 500.00m
      }
    ]
  ]
  