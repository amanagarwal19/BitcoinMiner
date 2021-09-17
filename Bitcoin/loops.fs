open System

[<EntryPoint>]
let main argv = 
    let list = [10;20;30;40;50]
    // for i in list do
    //     printfn "Hello Bello %d\n" i

    // for i = 10 downto 1 do
    //     printf "reducing i  as %d .." i

    // for i = 1 to 10 do
    //     printfn "increasing i as %d .." i

    let correctNumber = 19
    let mutable isCorrect = false
    while not isCorrect do
        printf "Enter the number to guess"
        let input = Console.ReadLine()
        let n = int input
        if n = correctNumber then 
            isCorrect <- true 
            printfn "Congratulations"
        else 
            printfn "Retry"       
    0