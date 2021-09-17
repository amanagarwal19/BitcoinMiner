open System.Net
open System.Text
open Akka.FSharp
open Akka.Actor
open Akka.IO
open Akka

let system = System.create "system" (Configuration.defaultConfig())

type GreeterMsg =
    | Hello of string
    | Goodbye of string

// Greeter Actor
let greeter = spawn system "greeter" <| fun mailbox ->
    let rec loop() = actor {
        let! msg = mailbox.Receive()

        match msg with
        | Hello name -> printfn "I saw a hello, %s!\n" name
        | Goodbye name -> printfn "Tata, time to say Goodbye, %s\n" name

        return! loop()
    }
    loop()

//Listener Actor

// let listener = spawn system "listener" <| fun (mailbox: Actor<obj>) ->
//     let rec loop() = actor {
//         let! msg = mailbox.Receive()
//         let sender = mailbox.Sender()

//         match msg with
//         | :? Tcp.Bound as bound->
//             printf "Listening on %O\n" bound.LocalAddress
//         | :? Tcp.Connected as connected ->
//             printf "%O connected to the server\n" connected.RemoteAddress
//             let handlerName = "handler_" + connected.RemoteAddress.ToString().Replace("[","").Replace("]","")
//             let handlerRef = spawn mailbox handlerName (handler sender)
//             sender <! Tcp.Register handlerRef 
//         |_-> ()

//         return! loop()
//     }
//     mailbox.Context.System.Tcp() <! Tcp.Bind(mailbox.Self,IPEndPoint(IPAddress.Any,9090))
//     loop()

let server = spawn system "server" <| fun (mailbox: Actor<obj>) ->
    let rec loop() = actor {
        let! msg = mailbox.Receive()
        let sender = mailbox.Sender()
        
        match msg with
        | :? Tcp.Bound as bound ->
            printf "Listening on %O\n" bound.LocalAddress
        | :? Tcp.Connected as connected -> 
            printf "%O connected to the server\n" connected.RemoteAddress
            let handlerName = "handler_" + connected.RemoteAddress.ToString().Replace("[", "").Replace("]", "")
            let handlerRef = spawn mailbox handlerName (handler sender)
            sender <! Tcp.Register handlerRef
        | _ -> ()

        return! loop()
    }

    mailbox.Context.System.Tcp() <! Tcp.Bind(mailbox.Self, IPEndPoint(IPAddress.Any, 9090))
    loop()


let handler connection (mailbox: Actor<obj>) =
    let rec loop connection = actor {
        let! msg = mailbox.Receive()

        match msg with
        | :? Tcp.Received as received -> 
            let data = (Encoding.ASCII.GetString(received.Data.ToArray())).Trim().split([|' '|],2)

            match data with
            | [| "hello"; name |] -> greeter<! Hello (name.Trim())
            | [| "Goodbye"; name |] -> greeter<! Goodbye (name.Trim())
            | _->()
        | _ -> mailbox.Unhandled()

        return! loop connection
    }

    loop connection

greeter<! Hello "Aman"
greeter<! Goodbye "Agarwal"

System.Console.ReadLine() |>ignore


// let myActor (mailbox: Actor<_>) =
//     let rec loop() = actor{
//         let! message = mailbox.Recieve()

//         return! loop()
//     }

//     loop()

// let actorRef = spawn system "myActor" myActor

// actorRef <! "hello"

