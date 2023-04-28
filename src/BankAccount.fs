module BankAccount

open FSharpx.Result
open System

type TransactionType = Debit | Credit

type TransationRecord = decimal * DateTime * TransactionType

type Account = { ledger: TransationRecord list }

type AccountOpError =  InvalidNegativeAmount | NotEnoughFounds

module Account =

  let init txs = { ledger = txs }

  let private validateAmount (amount: decimal) = if amount >= 0m then Ok amount else Error InvalidNegativeAmount

  let private validateDebit (amount: decimal) (balance: decimal) = if amount < balance then Ok amount else Error NotEnoughFounds

  let balance (ledger: TransationRecord list): decimal = 
    List.fold (fun acc elem -> match elem with (a,_,Credit) -> (acc + a) | (a,_,Debit) -> (acc - a)) 0m ledger
 
  let withdraw (amount:decimal) (now: DateTime) (account: Account) : Result<Account, AccountOpError> = 
    result {
      let! _ = validateAmount amount
      let! _ = validateDebit amount (balance account.ledger)
      return { account with ledger = account.ledger @ [(amount, now, Debit)]}
    }

  let deposit (amount:decimal) (now: DateTime) (account: Account) : Result<Account, AccountOpError> = 
    validateAmount amount |> Result.map (fun a -> { account with ledger = account.ledger @ [(a, now, Credit)]})

module BankStatement =

  open FSharpPlus

  let print (account: Account) : String list = 
    let cellSize = 15
    let fit (value: String) = $" {value}".PadRight(cellSize)
    let headers = $"""{fit "date"}||{fit "credit"}||{fit "debit"}||{fit "balance"}"""
    let fmtDate (d: DateTime) = d.ToString("dd/MM/yyyy") |> fit
    let fmtAmount a = sprintf "%.2f" a |> fit
    let formatTx tx balance = 
      match tx with 
        | (a,d,Credit) -> $"""{fmtDate d}||{fmtAmount a}||{fit ""}||{fmtAmount balance}"""
        | (a,d,Debit) ->  $"""{fmtDate d}||{fit ""}||{fmtAmount a}||{fmtAmount balance}"""
    let sortedLedger = List.sortBy (fun (_,ts,_) -> ts) account.ledger |> List.rev
    let content = sortedLedger |> List.mapi (fun i tx -> formatTx tx (Account.balance (List.drop i sortedLedger)))
    headers :: content
  