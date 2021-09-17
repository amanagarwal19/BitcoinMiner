open System

[<EntryPoint>]
let main argv =
    printfn "hello  from if else"
    let age = Console.ReadLine()
    let citizenType = if int age >50 then "Senior citizen" elif int age >30 then "Citizen" else "Child"
    printfn "%s" citizenType
    0