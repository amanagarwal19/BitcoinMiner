// Learn more about F# at http://docs.microsoft.com/dotnet/fsharp

open System

// Define a function to construct a message to print
let from whom =
    sprintf "from %s" whom



[<EntryPoint>]
let main argv =
    printfn "hi"
    Console.Write "Enter a value"
    let a = Console.ReadLine()
    let b =  int a%2=0
    printfn "VALUE IS %b"  b
    let mutable num = 5
    printfn "%d" num
    let abc = 10
    printfn "%d" abc
    
    0