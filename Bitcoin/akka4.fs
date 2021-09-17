open System.Net
open System.Text
open Akka
open Akka.FSharp
open Akka.Actor
open Akka.IO
open System
open System.Security.Cryptography
open System.Threading
open System.IO

// -------------------  UTILITIES -------------------------------

let encrypt (input:string) =
    let author = input
    let inputBytes = Encoding.ASCII.GetBytes(author)
    let mySha = SHA256.Create()

    let output = mySha.ComputeHash(inputBytes)
    let stringVersion = BitConverter.ToString(output)
    // Console.WriteLine(stringVersion)
    stringVersion

let encryptRange (starting:char,ending:char):string=
    let characterRange = "ABCDEFGHIJKLMNOPQRSTUVWUXYZ"
    let numberRange = "0123456789"
    let specialChars = ":?><';/*-+=!@#$%^&*()"
    let mutable startingIndex = characterRange.IndexOf(System.Char.ToUpper(starting))
    let mutable endingIndex = characterRange.IndexOf(System.Char.ToUpper(ending))
   
    if startingIndex>endingIndex then
        let temp = startingIndex
        startingIndex <- endingIndex
        endingIndex <- temp
   
    let randomizer =  System.Random()
    let mutable str="83494778;"
    
    //Generate string of random characters
    for i=0 to 5 do
        let randomIndexChar = randomizer.Next(startingIndex,endingIndex+1)
        let randomChar = characterRange.Chars(randomIndexChar)
        str <-   str + System.Char.ToString(randomChar)
    
    // Generate string of random numbers and special characters   
    for i=0 to 3 do
        let randomNumber = numberRange.Chars(randomizer.Next(10))
        let randomSpecial = specialChars.Chars(randomizer.Next(specialChars.Length-1))
        str<-   str+
                System.Char.ToString(randomNumber)+
                System.Char.ToString(randomSpecial)
    let stringToEncrypt = str.ToLower()
    //Console.WriteLine(stringToEncrypt)
    stringToEncrypt

// --------------------------- AKKA --------------------------------

let system = System.create "system" (Configuration.defaultConfig())

type GreeterMsg =
    | Hello of string
    | Goodbye of string
    | Start of string

let greeter = spawn system "greeter" <| fun mailbox->
    let rec loop() = actor {
        let! msg = mailbox.Receive()
        let sender = mailbox.Sender()

        match msg with
        | Hello name -> sender<!sprintf "Greeter says hello and falls asleep\n"
                        System.Threading.Thread.Sleep(10000)
                        sender<!sprintf "Greeter is awake \n"
                        
        | Goodbye name -> printfn "Greeter speaking %s" name
        | Start name -> sender <!sprintf "Starting Process for Greeter \n"
                        let splitStrings = name.Split(',')
                        sender<! sprintf "found characters %c and %c\n" (char splitStrings.[0]) (char splitStrings.[1])
                        let mutable coinStrings = ""
                        let mutable coinCounter=0
                        for i=0 to 500000 do
                            let generatedString = encryptRange(char splitStrings.[0],char splitStrings.[1])
                            sender<! sprintf "Generated string : %s\n" generatedString
                            //printfn "Found string %s, encrypting now ....\n" generatedString
                            let encryptedString = encrypt(generatedString)
                            //printfn "Encrypted to: \n %s " encryptedString
                            sender<! sprintf "Encrypted String: %s\n" encryptedString
                            // let sw = new StreamWriter("coins.txt")
                            if encryptedString.Chars(0) = '0' then 
                                printfn "Counter : %d" coinCounter
                                coinStrings <- coinStrings + "\n" + generatedString + " : " + encryptedString
                                coinCounter <- coinCounter + 1    
                        // Write the set of coins to a file
                        IO.File.WriteAllText("Coins.txt",coinStrings)   
                        sender<! sprintf "Encryption Completed: \n Found %d coins \n" coinCounter
                  
        return! loop()
    }
    loop()
let greeter2 = spawn system "greeter2" <| fun mailbox->
    let rec loop() = actor {
        let! msg = mailbox.Receive()
        let sender = mailbox.Sender()

        match msg with
        | Hello name -> sender<!sprintf "Greeter says hello and falls asleep\n"
                        System.Threading.Thread.Sleep(10000)
                        sender<!sprintf "Greeter is awake \n"
                        
        | Goodbye name -> printfn "Greeter2 speaking %s" name
        | Start name -> sender <!sprintf "Starting Process for Greeter \n"
                        let splitStrings = name.Split(',')
                        sender<! sprintf "found characters %c and %c\n" (char splitStrings.[0]) (char splitStrings.[1])
                        let mutable coinStrings = ""
                        let mutable coinCounter=0
                        for i=0 to 500000 do
                            let generatedString = encryptRange(char splitStrings.[0],char splitStrings.[1])
                            sender<! sprintf "Generated string : %s\n" generatedString
                            //printfn "Found string %s, encrypting now ....\n" generatedString
                            let encryptedString = encrypt(generatedString)
                            //printfn "Encrypted to: \n %s " encryptedString
                            sender<! sprintf "Encrypted String: %s\n" encryptedString
                            // let sw = new StreamWriter("coins.txt")
                            if encryptedString.Chars(0) = '0' then 
                                printfn "Counter : %d" coinCounter
                                coinStrings <- coinStrings + "\n" + generatedString + " : " + encryptedString
                                coinCounter <- coinCounter + 1    
                        // Write the set of coins to a file
                        IO.File.WriteAllText("Coins2.txt",coinStrings)   
                        sender<! sprintf "Encryption Completed: \n Found %d coins \n" coinCounter
                        sender<! sprintf "Done" 
        return! loop()
    }
    loop()

let alice = spawn system "Alice" <| fun mailbox ->
    let rec loop() = actor {
        let! msg = mailbox.Receive()
        let sender = mailbox.Sender()

        match msg with
        | Hello name -> sender <! sprintf "Alice speaking : Hello, %s!\n" name
        | Goodbye name -> sender <! sprintf "Alice speaking : Goodbye, %s!\n" name
        | Start(_) -> failwith "Not Implemented"

        return! loop()
    }
    loop()


let handler connection (mailbox: Actor<obj>) =  
    let rec loop connection = actor {
        let! msg = mailbox.Receive()
        let sender = mailbox.Sender()

        match msg with
        | :? Tcp.Received as received ->
            let data = (Encoding.ASCII.GetString (received.Data.ToArray())).Trim().Split([|' '|], 2)

            match data with
            | [| "hello"; name |] -> greeter <! Hello (name.Trim())
            | [| "goodbye"; name |] -> alice <! Goodbye (name.Trim())
            | [| "start"; name |] -> greeter <! Start (name.Trim())
            | [| "start2"; name |] -> greeter2 <! Start (name.Trim())
            | [| "Done"; |] -> sender<! sprintf "\n\n%s\n\n this is the handler"
            | _ -> connection <! Tcp.Write.Create (ByteString.FromString "Invalid request.\n")
        | :? string as response ->
            connection <! Tcp.Write.Create (ByteString.FromString response)
        | _ -> mailbox.Unhandled()

        return! loop connection
    }

    loop connection

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
        | _ -> mailbox.Unhandled()

        return! loop()
    }
    // let byteAdress = Encoding.ASCII.GetBytes("127.0.0.1") 
    // // let objs = IPAddress(byteAdress)
    mailbox.Context.System.Tcp() <! Tcp.Bind(mailbox.Self, IPEndPoint(IPAddress.Any,8233))
    loop()    

System.Console.ReadLine() |> ignore