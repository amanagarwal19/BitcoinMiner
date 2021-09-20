#r "nuget: Akka.FSharp"
#time "on"

// Load the utilities script to get helper functions
#load "Util.fsx"
open Util

// Load the necessary libraries
open System
open System.Threading
open Akka.Actor
open Akka.Configuration
open Akka.FSharp
open System.Diagnostics

let proc = Process.GetCurrentProcess()
let cpuTimeStamp = proc.TotalProcessorTime
let timer = Stopwatch()
timer.Start()

let stop(printTime: bool, slowDown: bool) =
    if printTime then
        let cpuTime = (proc.TotalProcessorTime-cpuTimeStamp).TotalMilliseconds
        printfn "CPU time = %dms" (int64 cpuTime)
        printfn "Absolute time = %dms" timer.ElapsedMilliseconds
    if slowDown then
        System.Threading.Thread.Sleep(3000)
    // System.Environment.Exit(0)

let zeros = fsi.CommandLineArgs.[1]|>int;
let workerCount = 5
let totalStrings = 100000
let eachWorkerEffort = totalStrings / workerCount
// printfn "Worker effort : %d" eachWorkerEffort
let mutable totalCoindFound = 0
// To keep a count of when all the workers have finished and terminate the program
let mutable workersFinished = 0


//Initialise the actor system
let system = System.create  "system" (Configuration.defaultConfig())

//Initializing message types
type MessageFormat = 
    |Hi of string
    |Hello of string
    |Convert of string
    |AssignTask of string
    |CreateWorkers of int
    |Greet of string
    |ReceiveMessageFromWorker of int*string*string
    |BlockTest of string
    |StartYourJob of int*int*int
    |Finished of int*string
    // |Finish of string

//Creating working actors
let worker (mailbox:Actor<_>)=
    let rec loop() = actor{
        let! msg = mailbox.Receive()
        let sender = mailbox.Sender()

        match msg with
        |Greet(s)->
            printfn "Okay boss, got your message, working now..." 
            // System.Threading.Thread.Sleep(5000);
            // sender<!ReceiveMessageFromWorker(0,"My job is done boss\n")
        
        |StartYourJob(ID,capacity,startCharOffset) -> 
            printfn "Worker %d Initialised\n" ID
            // System.Threading.Thread.Sleep(5000);
            //String to store all the encrypted strings and their corresponding keys
            let mutable str = ""
            let mutable invalidCounter = 0
            let mutable validCounter = 0
            let filePath = @"/Users/aman/Documents/FSharp/myFSharpApp/coins/"
            //Perform the encryption
            for i = 1 to capacity do
                if (i%50001) = 0 then printf "\nStill processing...\n"
                let stringToEncrypt = stringGenerator(startCharOffset)
                let encryptedString = encrypt(stringToEncrypt)

                //Check for validation of the encryption string
                if validCoin(encryptedString,zeros) then 
                    totalCoindFound <- totalCoindFound + 1
                    validCounter <- validCounter + 1 
                    str <- str +
                    "Input: "+  stringToEncrypt+
                    "\tOutput: "+  encryptedString + "\n" 
                    sender<!ReceiveMessageFromWorker(ID,stringToEncrypt,encryptedString)
                else 
                    invalidCounter <- invalidCounter + 1 
                    
            
            // Write all coins to a file    
            // IO.File.WriteAllText(("/coins"+"Worker "+(string ID)+"nonValid"+".txt"),string invalidCounter + "\n"+string validCounter)
            // Send completed message to boss
            
            sender<!Finished(ID,"Done");
            let appendFileName = "(" + string validCounter + "-" + string invalidCounter + ")"
            IO.File.WriteAllText((filePath+"Worker "+(string ID)+appendFileName+".txt"),str)

        | _ -> printfn "Incorrect message"
        return! loop()
    }
    loop()


//Creating the boss

let bossActor (mailbox:Actor<_> ) = 
    let rec loop() = actor{
        let! msg = mailbox.Receive()
        
        let mutable workers =[]
        
        match msg with 
        |CreateWorkers (n)->
            workers <- [for i in 0..n-1 do 
                            yield spawn system ("Worker" + (string i)) worker]
            // [for i in 1..10 do yield i]
            // printfn "workers created\n"           
            printfn "\nTotal number of workers %d \n" workers.Length

            //Assign tasks 
            // Say we will bruteforce 1M string combinations, we will split it into 5 actors of 200k each
            
            
            let mutable characterStartOffset =0
            
            //Commanding the workers to start their jobs
            let mutable workerID = 1
            for i in 0..(workerCount-1) do //minus 1 because zero index based
                
                workers.Item(i|>int)<!StartYourJob(workerID,eachWorkerEffort,characterStartOffset)
                // printfn "char offset %d" characterStartOffset
                characterStartOffset<- characterStartOffset + 5
                // printfn "Worker ID %d" workerID
                workerID <- workerID + 1
            
            // printfn "welcome to assigning task to %d people" n

        |ReceiveMessageFromWorker (workerID,generatedString,encryptedString) ->
            printfn "WorkerID : %d \nInput: %s \nOutput: %s" workerID generatedString encryptedString
            
        |Finished(workerID,message)->
            printfn "Received %s \nOkay, good job worker %d" message workerID
            //Everyone has finished processing, terminate the process
            workersFinished <- workersFinished + 1
            printfn "finished workers %d" workersFinished
            if workersFinished = workerCount then 
                printfn "TOTAL COINS MINED WITH %d zeros = %d" zeros totalCoindFound
                stop(true,false)
                mailbox.Context.System.Terminate() |>ignore

        |BlockTest(n)->
            let workerCount = 5
            let totalStrings = 1000000
            let eachWorkerEffort = totalStrings / workerCount
            printfn "Worker effort : %d" eachWorkerEffort
        | _ -> printfn "Incorrect message"

        return! loop()
    }
    loop()

let boss = spawn system "bossActor" bossActor

// boss<!Convert(zeros)
// boss<!AssignTask("5")
boss<!CreateWorkers(workerCount)
// boss<!BlockTest("hi")
system.WhenTerminated.Wait()
