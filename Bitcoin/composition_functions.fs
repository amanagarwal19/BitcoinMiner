open System

let add n = n+5

let multiply n = n*2

let composition p = 
    let sum = add p
    let product = multiply sum
    product

let composition2 = add >> multiply

[<EntryPoint>]
let main argv = 
    let answer = composition 5
    let out = int answer
    Console.WriteLine out
    printfn "%d" out

    let answer2 = composition2 10
    Console.WriteLine answer2

    0
  