open System
open Akka.FSharp

let system = System.create "system" (Configuration.defaultConfig())

type ProcessorMessage = ProcessJob of int * int * int

let processor (mailbox: Actor<_>) = 
    let rec loop () = actor {
        let! ProcessJob(x,y,z) = mailbox.Receive ()
        let! message = mailbox.Receive()
        
        printfn "Processor: received ProcessJob %i %i %i" x y z
        return! loop ()
    }
    loop ()

let processorRef = spawn system "processor" processor

processorRef <! ProcessJob(1, 3, 5)

System.Console.ReadLine() |> ignore